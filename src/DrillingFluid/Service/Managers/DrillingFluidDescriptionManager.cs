using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.DrillingFluid.Model;

namespace NORCE.Drilling.DrillingFluid.Service.Managers
{
    /// <summary>
    /// A manager for DrillingFluidDescription. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class DrillingFluidDescriptionManager
    {
        private static DrillingFluidDescriptionManager? _instance = null;
        private readonly ILogger<DrillingFluidDescriptionManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        private DrillingFluidDescriptionManager(ILogger<DrillingFluidDescriptionManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static DrillingFluidDescriptionManager GetInstance(ILogger<DrillingFluidDescriptionManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new DrillingFluidDescriptionManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM DrillingFluidDescriptionTable";
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
                        _logger.LogError(ex, "Impossible to count records in the DrillingFluidDescriptionTable");
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
                    //empty DrillingFluidDescriptionTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM DrillingFluidDescriptionTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the DrillingFluidDescriptionTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM DrillingFluidDescriptionTable WHERE ID = ' {guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from DrillingFluidDescriptionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all DrillingFluidDescription present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all DrillingFluidDescription present in the microservice database</returns>
        public List<Guid>? GetAllDrillingFluidDescriptionId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM DrillingFluidDescriptionTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from DrillingFluidDescriptionTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from DrillingFluidDescriptionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all DrillingFluidDescription present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all DrillingFluidDescription present in the microservice database</returns>
        public List<MetaInfo?>? GetAllDrillingFluidDescriptionMetaInfo()
        {
            List<MetaInfo?> metaInfos = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM DrillingFluidDescriptionTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from DrillingFluidDescriptionTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from DrillingFluidDescriptionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns a DrillingFluidDescription identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the DrillingFluidDescription retrieved from the database</returns>
        public Model.DrillingFluidDescription? GetDrillingFluidDescriptionById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.DrillingFluidDescription? drillingFluidDescription = null;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT DrillingFluidDescription FROM DrillingFluidDescriptionTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            drillingFluidDescription = JsonSerializer.Deserialize<Model.DrillingFluidDescription>(data, JsonSettings.Options);
                            if (drillingFluidDescription != null && drillingFluidDescription.MetaInfo != null && !drillingFluidDescription.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: retrieved DrillingFluidDescription is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No DrillingFluidDescription of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the DrillingFluidDescription with the given ID from DrillingFluidDescriptionTable");
                        return null;
                    }

                    // Finalizing
                    _logger.LogInformation("Returning the DrillingFluidDescription of given ID from DrillingFluidDescriptionTable");
                    return drillingFluidDescription;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluidDescription ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all DrillingFluidDescription present in the microservice database 
        /// </summary>
        /// <returns>the list of all DrillingFluidDescription present in the microservice database</returns>
        public List<Model.DrillingFluidDescription?>? GetAllDrillingFluidDescription()
        {
            List<Model.DrillingFluidDescription?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT DrillingFluidDescription FROM DrillingFluidDescriptionTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.DrillingFluidDescription? drillingFluidDescription = JsonSerializer.Deserialize<Model.DrillingFluidDescription>(data, JsonSettings.Options);
                        vals.Add(drillingFluidDescription);
                    }
                    _logger.LogInformation("Returning the list of existing DrillingFluidDescription from DrillingFluidDescriptionTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get DrillingFluidDescription from DrillingFluidDescriptionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Adds the given DrillingFluidDescription to the microservice database
        /// </summary>
        /// <param name="drillingFluidDescription"></param>
        /// <returns>true if the given DrillingFluidDescription has been added successfully</returns>
        public bool AddDrillingFluidDescription(Model.DrillingFluidDescription? drillingFluidDescription)
        {
            if (drillingFluidDescription != null && drillingFluidDescription.MetaInfo != null && drillingFluidDescription.MetaInfo.ID != Guid.Empty)
            {
                //update DrillingFluidDescriptionTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    bool success = true;
                    try
                    {
                        //add the DrillingFluidDescription to the DrillingFluidDescriptionTable
                        string metaInfo = JsonSerializer.Serialize(drillingFluidDescription.MetaInfo, JsonSettings.Options);
                        string data = JsonSerializer.Serialize(drillingFluidDescription, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = "INSERT INTO DrillingFluidDescriptionTable (" +
                            "ID, " +
                            "MetaInfo, " +
                            "DrillingFluidDescription" +
                            ") VALUES (" +
                            $"'{drillingFluidDescription.MetaInfo.ID}', " +
                            $"'{metaInfo}', " +
                            $"'{data}'" +
                            ")";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to insert the given DrillingFluidDescription into the DrillingFluidDescriptionTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to add the given DrillingFluidDescription into DrillingFluidDescriptionTable");
                        success = false;
                    }
                    //finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Added the given DrillingFluidDescription of given ID into the DrillingFluidDescriptionTable successfully");
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
                _logger.LogWarning("The DrillingFluidDescription ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given DrillingFluidDescription and updates it in the microservice database
        /// </summary>
        /// <param name="drillingFluidDescription"></param>
        /// <returns>true if the given DrillingFluidDescription has been updated successfully</returns>
        public bool UpdateDrillingFluidDescriptionById(Guid guid, Model.DrillingFluidDescription? drillingFluidDescription)
        {
            bool success = true;
            if (guid != Guid.Empty && drillingFluidDescription != null && drillingFluidDescription.MetaInfo != null && drillingFluidDescription.MetaInfo.ID == guid)
            {
                //update DrillingFluidDescriptionTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in DrillingFluidDescriptionTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(drillingFluidDescription.MetaInfo, JsonSettings.Options);
                        drillingFluidDescription.LastModificationDate = DateTimeOffset.UtcNow;
                        string data = JsonSerializer.Serialize(drillingFluidDescription, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE DrillingFluidDescriptionTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"DrillingFluidDescription = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the DrillingFluidDescription");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the DrillingFluidDescription");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given DrillingFluidDescription successfully");
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
                _logger.LogWarning("The DrillingFluidDescription ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the DrillingFluidDescription of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the DrillingFluidDescription was deleted from the microservice database</returns>
        public bool DeleteDrillingFluidDescriptionById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete DrillingFluidDescription from DrillingFluidDescriptionTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM DrillingFluidDescriptionTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the DrillingFluidDescription of given ID from the DrillingFluidDescriptionTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the DrillingFluidDescription of given ID from DrillingFluidDescriptionTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the DrillingFluidDescription of given ID from the DrillingFluidDescriptionTable successfully");
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
                _logger.LogWarning("The DrillingFluidDescription ID is null or empty");
            }
            return false;
        }
    }
}