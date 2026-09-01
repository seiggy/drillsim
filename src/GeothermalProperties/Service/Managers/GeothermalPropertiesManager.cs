using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.GeothermalProperties.Model;

namespace NORCE.Drilling.GeothermalProperties.Service.Managers
{
    /// <summary>
    /// A manager for GeothermalProperties. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class GeothermalPropertiesManager
    {
        private static GeothermalPropertiesManager? _instance = null;
        private readonly ILogger<GeothermalPropertiesManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        private GeothermalPropertiesManager(ILogger<GeothermalPropertiesManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static GeothermalPropertiesManager GetInstance(ILogger<GeothermalPropertiesManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new GeothermalPropertiesManager(logger, connectionManager);
            return _instance;
        }

        public int Count
        {
            get
            {
                int count = 0;
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM GeothermalPropertiesTable";
                    try
                    {
                        using SqliteDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            count = (int)reader.GetInt64(0);
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to count records in the GeothermalPropertiesTable");
                    }
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
                return count;
            }
        }

        public bool Clear()
        {
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                bool success = false;
                using var transaction = connection.BeginTransaction();
                try
                {
                    //empty GeothermalPropertiesTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM GeothermalPropertiesTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the GeothermalPropertiesTable");
                }
                return success;
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
                return false;
            }
        }

        public bool Contains(Guid guid)
        {
            int count = 0;
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT COUNT(*) FROM GeothermalPropertiesTable WHERE ID = ' {guid}'";
                try
                {
                    using SqliteDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        count = (int)reader.GetInt64(0);
                    }
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to count rows from GeothermalPropertiesTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all GeothermalProperties present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all GeothermalProperties present in the microservice database</returns>
        public List<Guid>? GetAllGeothermalPropertiesId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM GeothermalPropertiesTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from GeothermalPropertiesTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from GeothermalPropertiesTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all GeothermalProperties present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all GeothermalProperties present in the microservice database</returns>
        public List<MetaInfo?>? GetAllGeothermalPropertiesMetaInfo()
        {
            List<MetaInfo?> metaInfos = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM GeothermalPropertiesTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from GeothermalPropertiesTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from GeothermalPropertiesTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns a GeothermalProperties identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the GeothermalProperties retrieved from the database</returns>
        public Model.GeothermalProperties? GetGeothermalPropertiesById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.GeothermalProperties? geothermalProperties = null;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT GeothermalProperties FROM GeothermalPropertiesTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            geothermalProperties = JsonSerializer.Deserialize<Model.GeothermalProperties>(data, JsonSettings.Options);
                            if (geothermalProperties != null && geothermalProperties.MetaInfo != null && !geothermalProperties.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: retrieved GeothermalProperties is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No GeothermalProperties of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the GeothermalProperties with the given ID from GeothermalPropertiesTable");
                        return null;
                    }

                    // Finalizing
                    _logger.LogInformation("Returning the GeothermalProperties of given ID from GeothermalPropertiesTable");
                    return geothermalProperties;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given GeothermalProperties ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all GeothermalProperties present in the microservice database 
        /// </summary>
        /// <returns>the list of all GeothermalProperties present in the microservice database</returns>
        public List<Model.GeothermalProperties?>? GetAllGeothermalProperties()
        {
            List<Model.GeothermalProperties?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT GeothermalProperties FROM GeothermalPropertiesTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.GeothermalProperties? geothermalProperties = JsonSerializer.Deserialize<Model.GeothermalProperties>(data, JsonSettings.Options);
                        vals.Add(geothermalProperties);
                    }
                    _logger.LogInformation("Returning the list of existing GeothermalProperties from GeothermalPropertiesTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get GeothermalProperties from GeothermalPropertiesTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Adds the given GeothermalProperties to the microservice database
        /// </summary>
        /// <param name="geothermalProperties"></param>
        /// <returns>true if the given GeothermalProperties has been added successfully</returns>
        public bool AddGeothermalProperties(Model.GeothermalProperties? geothermalProperties)
        {
            if (geothermalProperties != null && geothermalProperties.MetaInfo != null && geothermalProperties.MetaInfo.ID != Guid.Empty)
            {
                //update GeothermalPropertiesTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    bool success = true;
                    try
                    {
                        //add the GeothermalProperties to the GeothermalPropertiesTable
                        string metaInfo = JsonSerializer.Serialize(geothermalProperties.MetaInfo, JsonSettings.Options);
                        string data = JsonSerializer.Serialize(geothermalProperties, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = "INSERT INTO GeothermalPropertiesTable (" +
                            "ID, " +
                            "MetaInfo, " +
                            "GeothermalProperties" +
                            ") VALUES (" +
                            $"'{geothermalProperties.MetaInfo.ID}', " +
                            $"'{metaInfo}', " +
                            $"'{data}'" +
                            ")";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to insert the given GeothermalProperties into the GeothermalPropertiesTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to add the given GeothermalProperties into GeothermalPropertiesTable");
                        success = false;
                    }
                    //finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Added the given GeothermalProperties of given ID into the GeothermalPropertiesTable successfully");
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                    return success;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The GeothermalProperties ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given GeothermalProperties and updates it in the microservice database
        /// </summary>
        /// <param name="geothermalProperties"></param>
        /// <returns>true if the given GeothermalProperties has been updated successfully</returns>
        public bool UpdateGeothermalPropertiesById(Guid guid, Model.GeothermalProperties? geothermalProperties)
        {
            bool success = true;
            if (guid != Guid.Empty && geothermalProperties != null && geothermalProperties.MetaInfo != null && geothermalProperties.MetaInfo.ID == guid)
            {
                //update GeothermalPropertiesTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in GeothermalPropertiesTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(geothermalProperties.MetaInfo, JsonSettings.Options);
                        geothermalProperties.LastModificationDate = DateTimeOffset.UtcNow;
                        string data = JsonSerializer.Serialize(geothermalProperties, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE GeothermalPropertiesTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"GeothermalProperties = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the GeothermalProperties");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the GeothermalProperties");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given GeothermalProperties successfully");
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The GeothermalProperties ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the GeothermalProperties of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the GeothermalProperties was deleted from the microservice database</returns>
        public bool DeleteGeothermalPropertiesById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete GeothermalProperties from GeothermalPropertiesTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM GeothermalPropertiesTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the GeothermalProperties of given ID from the GeothermalPropertiesTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the GeothermalProperties of given ID from GeothermalPropertiesTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the GeothermalProperties of given ID from the GeothermalPropertiesTable successfully");
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                    return success;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The GeothermalProperties ID is null or empty");
            }
            return false;
        }
    }
}