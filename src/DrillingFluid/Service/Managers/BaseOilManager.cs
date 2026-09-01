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
    /// A manager for BaseOil. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class BaseOilManager
    {
        private static BaseOilManager? _instance = null;
        private readonly ILogger<BaseOilManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        private BaseOilManager(ILogger<BaseOilManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static BaseOilManager GetInstance(ILogger<BaseOilManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new BaseOilManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM BaseOilTable";
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
                        _logger.LogError(ex, "Impossible to count records in the BaseOilTable");
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
                    //empty BaseOilTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM BaseOilTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the BaseOilTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM BaseOilTable WHERE ID = ' {guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from BaseOilTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all BaseOil present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all BaseOil present in the microservice database</returns>
        public List<Guid>? GetAllBaseOilId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM BaseOilTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from BaseOilTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from BaseOilTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all BaseOil present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all BaseOil present in the microservice database</returns>
        public List<MetaInfo?>? GetAllBaseOilMetaInfo()
        {
            List<MetaInfo?> metaInfos = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM BaseOilTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from BaseOilTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from BaseOilTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns a BaseOil identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the BaseOil retrieved from the database</returns>
        public Model.BaseOil? GetBaseOilById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.BaseOil? baseOil = null;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT BaseOil FROM BaseOilTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            baseOil = JsonSerializer.Deserialize<Model.BaseOil>(data, JsonSettings.Options);
                            if (baseOil != null && baseOil.MetaInfo != null && !baseOil.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: retrieved BaseOil is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No BaseOil of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the BaseOil with the given ID from BaseOilTable");
                        return null;
                    }

                    // Finalizing
                    _logger.LogInformation("Returning the BaseOil of given ID from BaseOilTable");
                    return baseOil;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given BaseOil ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all BaseOil present in the microservice database 
        /// </summary>
        /// <returns>the list of all BaseOil present in the microservice database</returns>
        public List<Model.BaseOil?>? GetAllBaseOil()
        {
            List<Model.BaseOil?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT BaseOil FROM BaseOilTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.BaseOil? baseOil = JsonSerializer.Deserialize<Model.BaseOil>(data, JsonSettings.Options);
                        vals.Add(baseOil);
                    }
                    _logger.LogInformation("Returning the list of existing BaseOil from BaseOilTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get BaseOil from BaseOilTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Adds the given BaseOil to the microservice database
        /// </summary>
        /// <param name="baseOil"></param>
        /// <returns>true if the given BaseOil has been added successfully</returns>
        public bool AddBaseOil(Model.BaseOil? baseOil)
        {
            if (baseOil != null && baseOil.MetaInfo != null && baseOil.MetaInfo.ID != Guid.Empty)
            {
                //update BaseOilTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    bool success = true;
                    try
                    {
                        //add the BaseOil to the BaseOilTable
                        string metaInfo = JsonSerializer.Serialize(baseOil.MetaInfo, JsonSettings.Options);
                        string data = JsonSerializer.Serialize(baseOil, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = "INSERT INTO BaseOilTable (" +
                            "ID, " +
                            "MetaInfo, " +
                            "BaseOil" +
                            ") VALUES (" +
                            $"'{baseOil.MetaInfo.ID}', " +
                            $"'{metaInfo}', " +
                            $"'{data}'" +
                            ")";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to insert the given BaseOil into the BaseOilTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to add the given BaseOil into BaseOilTable");
                        success = false;
                    }
                    //finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Added the given BaseOil of given ID into the BaseOilTable successfully");
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
                _logger.LogWarning("The BaseOil ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given BaseOil and updates it in the microservice database
        /// </summary>
        /// <param name="baseOil"></param>
        /// <returns>true if the given BaseOil has been updated successfully</returns>
        public bool UpdateBaseOilById(Guid guid, Model.BaseOil? baseOil)
        {
            bool success = true;
            if (guid != Guid.Empty && baseOil != null && baseOil.MetaInfo != null && baseOil.MetaInfo.ID == guid)
            {
                //update BaseOilTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in BaseOilTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(baseOil.MetaInfo, JsonSettings.Options);
                        baseOil.LastModificationDate = DateTimeOffset.UtcNow;
                        string data = JsonSerializer.Serialize(baseOil, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE BaseOilTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"BaseOil = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the BaseOil");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the BaseOil");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given BaseOil successfully");
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
                _logger.LogWarning("The BaseOil ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the BaseOil of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the BaseOil was deleted from the microservice database</returns>
        public bool DeleteBaseOilById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete BaseOil from BaseOilTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM BaseOilTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the BaseOil of given ID from the BaseOilTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the BaseOil of given ID from BaseOilTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the BaseOil of given ID from the BaseOilTable successfully");
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
                _logger.LogWarning("The BaseOil ID is null or empty");
            }
            return false;
        }
    }
}