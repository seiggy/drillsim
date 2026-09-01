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
    /// A manager for Brine. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class BrineManager
    {
        private static BrineManager? _instance = null;
        private readonly ILogger<BrineManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        private BrineManager(ILogger<BrineManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static BrineManager GetInstance(ILogger<BrineManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new BrineManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM BrineTable";
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
                        _logger.LogError(ex, "Impossible to count records in the BrineTable");
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
                    //empty BrineTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM BrineTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the BrineTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM BrineTable WHERE ID = ' {guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from BrineTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all Brine present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all Brine present in the microservice database</returns>
        public List<Guid>? GetAllBrineId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM BrineTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from BrineTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from BrineTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all Brine present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all Brine present in the microservice database</returns>
        public List<MetaInfo?>? GetAllBrineMetaInfo()
        {
            List<MetaInfo?> metaInfos = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM BrineTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from BrineTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from BrineTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns a Brine identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the Brine retrieved from the database</returns>
        public Model.Brine? GetBrineById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.Brine? brine = null;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT Brine FROM BrineTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            brine = JsonSerializer.Deserialize<Model.Brine>(data, JsonSettings.Options);
                            if (brine != null && brine.MetaInfo != null && !brine.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: retrieved Brine is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No Brine of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the Brine with the given ID from BrineTable");
                        return null;
                    }

                    // Finalizing
                    _logger.LogInformation("Returning the Brine of given ID from BrineTable");
                    return brine;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given Brine ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all Brine present in the microservice database 
        /// </summary>
        /// <returns>the list of all Brine present in the microservice database</returns>
        public List<Model.Brine?>? GetAllBrine()
        {
            List<Model.Brine?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Brine FROM BrineTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.Brine? brine = JsonSerializer.Deserialize<Model.Brine>(data, JsonSettings.Options);
                        vals.Add(brine);
                    }
                    _logger.LogInformation("Returning the list of existing Brine from BrineTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get Brine from BrineTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Adds the given Brine to the microservice database
        /// </summary>
        /// <param name="brine"></param>
        /// <returns>true if the given Brine has been added successfully</returns>
        public bool AddBrine(Model.Brine? brine)
        {
            if (brine != null && brine.MetaInfo != null && brine.MetaInfo.ID != Guid.Empty)
            {
                //update BrineTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    bool success = true;
                    try
                    {
                        //add the Brine to the BrineTable
                        string metaInfo = JsonSerializer.Serialize(brine.MetaInfo, JsonSettings.Options);
                        string data = JsonSerializer.Serialize(brine, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = "INSERT INTO BrineTable (" +
                            "ID, " +
                            "MetaInfo, " +
                            "Brine" +
                            ") VALUES (" +
                            $"'{brine.MetaInfo.ID}', " +
                            $"'{metaInfo}', " +
                            $"'{data}'" +
                            ")";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to insert the given Brine into the BrineTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to add the given Brine into BrineTable");
                        success = false;
                    }
                    //finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Added the given Brine of given ID into the BrineTable successfully");
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
                _logger.LogWarning("The Brine ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given Brine and updates it in the microservice database
        /// </summary>
        /// <param name="brine"></param>
        /// <returns>true if the given Brine has been updated successfully</returns>
        public bool UpdateBrineById(Guid guid, Model.Brine? brine)
        {
            bool success = true;
            if (guid != Guid.Empty && brine != null && brine.MetaInfo != null && brine.MetaInfo.ID == guid)
            {
                //update BrineTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in BrineTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(brine.MetaInfo, JsonSettings.Options);
                        brine.LastModificationDate = DateTimeOffset.UtcNow;
                        string data = JsonSerializer.Serialize(brine, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE BrineTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"Brine = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the Brine");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the Brine");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given Brine successfully");
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
                _logger.LogWarning("The Brine ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the Brine of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the Brine was deleted from the microservice database</returns>
        public bool DeleteBrineById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete Brine from BrineTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM BrineTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the Brine of given ID from the BrineTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the Brine of given ID from BrineTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the Brine of given ID from the BrineTable successfully");
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
                _logger.LogWarning("The Brine ID is null or empty");
            }
            return false;
        }
    }
}