using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.UnitConversion.Model;

namespace OSDC.UnitConversion.Service
{
    /// <summary>
    /// A manager for UnitSystemConversionSet. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class UnitSystemConversionSetManager
    {
        private static UnitSystemConversionSetManager? _instance = null;
        private readonly ILogger<UnitSystemConversionSetManager> _logger;
        private readonly object _lock = new();
        private readonly SqlConnectionManager _connectionManager;

        private UnitSystemConversionSetManager(ILogger<UnitSystemConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static UnitSystemConversionSetManager GetInstance(ILogger<UnitSystemConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new UnitSystemConversionSetManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM UnitSystemConversionSetTable";
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
                        _logger.LogError(ex, "Impossible to count records in the UnitSystemConversionSetTable");
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
                lock (_lock)
                {
                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        //empty UnitSystemConversionSetTable
                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM UnitSystemConversionSetTable";
                        command.ExecuteNonQuery();

                        transaction.Commit();
                        success = true;
                    }
                    catch (SqliteException ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Impossible to clear the UnitSystemConversionSetTable");
                    }
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
                command.CommandText = $"SELECT COUNT(*) FROM UnitSystemConversionSetTable WHERE ID = ' {guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from UnitSystemConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all UnitSystemConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all UnitSystemConversionSet present in the microservice database</returns>
        public List<Guid>? GetAllUnitSystemConversionSetId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM UnitSystemConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from UnitSystemConversionSetTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from UnitSystemConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all UnitSystemConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all UnitSystemConversionSet present in the microservice database</returns>
        public List<MetaInfo?>? GetAllUnitSystemConversionSetMetaInfo()
        {
            List<MetaInfo?> metaInfos = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM UnitSystemConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from UnitSystemConversionSetTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from UnitSystemConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return metaInfos;
        }

        /// <summary>
        /// Returns a UnitSystemConversionSet identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the UnitSystemConversionSet retrieved from the database</returns>
        public UnitSystemConversionSet? GetUnitSystemConversionSetById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    UnitSystemConversionSet? unitSystemConversionSet;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT UnitSystemConversionSet FROM UnitSystemConversionSetTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            unitSystemConversionSet = JsonSerializer.Deserialize<UnitSystemConversionSet>(data);
                            if (unitSystemConversionSet != null && unitSystemConversionSet.MetaInfo != null && !unitSystemConversionSet.MetaInfo.ID.Equals(guid))
                                throw (new SqliteException("SQLite database corrupted: retrieved UnitSystemConversionSet is null or has been jsonified with the wrong ID.", 1));
                        }
                        else
                        {
                            _logger.LogInformation("No UnitSystemConversionSet of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the UnitSystemConversionSet with the given ID from UnitSystemConversionSetTable");
                        return null;
                    }

                    // Finalizing
                    _logger.LogInformation("Returning the UnitSystemConversionSet of given ID from UnitSystemConversionSetTable");
                    return unitSystemConversionSet;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given UnitSystemConversionSet ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all UnitSystemConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of all UnitSystemConversionSet present in the microservice database</returns>
        public List<UnitSystemConversionSet?>? GetAllUnitSystemConversionSet()
        {
            List<UnitSystemConversionSet?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT UnitSystemConversionSet FROM UnitSystemConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        UnitSystemConversionSet? unitSystemConversionSet = JsonSerializer.Deserialize<UnitSystemConversionSet>(data);
                        vals.Add(unitSystemConversionSet);
                    }
                    _logger.LogInformation("Returning the list of existing UnitSystemConversionSet from UnitSystemConversionSetTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get UnitSystemConversionSet from UnitSystemConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Adds the given UnitSystemConversionSet to the microservice database
        /// </summary>
        /// <param name="unitSystemConversionSet"></param>
        /// <returns>true if the given UnitSystemConversionSet has been added successfully</returns>
        public bool AddUnitSystemConversionSet(UnitSystemConversionSet? unitSystemConversionSet)
        {
            if (unitSystemConversionSet != null && unitSystemConversionSet.MetaInfo != null && unitSystemConversionSet.MetaInfo.ID != Guid.Empty)
            {
                //calculate outputs
                if (!unitSystemConversionSet.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given UnitSystemConversionSet");
                    return false;
                }
                //update UnitSystemConversionSetTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    lock (_lock)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        bool success = true;
                        try
                        {
                            //add the UnitSystemConversionSet to the UnitSystemConversionSetTable
                            string metaInfo = JsonSerializer.Serialize(unitSystemConversionSet.MetaInfo);
                            string? cDate = null;
                            if (unitSystemConversionSet.CreationDate != null)
                                cDate = ((DateTimeOffset)unitSystemConversionSet.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string? lDate = null;
                            if (unitSystemConversionSet.LastModificationDate != null)
                                lDate = ((DateTimeOffset)unitSystemConversionSet.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string data = JsonSerializer.Serialize(unitSystemConversionSet);
                            var command = connection.CreateCommand();
                            command.CommandText = "INSERT INTO UnitSystemConversionSetTable " +
                                "(ID," +
                                "MetaInfo, " +
                                "CreationDate, " +
                                "LastModificationDate, " +
                                "UnitSystemConversionSet" +
                                ") VALUES (" +
                                $"'{unitSystemConversionSet.MetaInfo.ID}', " +
                                $"'{metaInfo}', " +
                                $"'{cDate}', " +
                                $"'{lDate}', " +
                                $"'{data}'" +
                                ")";
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to insert the given UnitSystemConversionSet into the UnitSystemConversionSetTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to add the given UnitSystemConversionSet into UnitSystemConversionSetTable");
                            success = false;
                        }
                        // Finalizing
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Added the given UnitSystemConversionSet of given ID into the UnitSystemConversionSetTable successfully");
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                        return success;
                    }
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The UnitSystemConversionSet ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given UnitSystemConversionSet and updates it in the microservice database
        /// </summary>
        /// <param name="unitSystemConversionSet"></param>
        /// <returns>true if the given UnitSystemConversionSet has been updated successfully</returns>
        public bool UpdateUnitSystemConversionSetById(Guid guid, UnitSystemConversionSet? unitSystemConversionSet)
        {
            bool success = true;
            if (guid != Guid.Empty && unitSystemConversionSet != null && unitSystemConversionSet.MetaInfo != null && unitSystemConversionSet.MetaInfo.ID == guid)
            {
                //calculate outputs
                if (!unitSystemConversionSet.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given UnitSystemConversionSet");
                    return false;
                }
                //update UnitSystemConversionSetTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    lock (_lock)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        //update fields in UnitSystemConversionSetTable
                        try
                        {
                            string metaInfo = JsonSerializer.Serialize(unitSystemConversionSet.MetaInfo);
                            string? cDate = null;
                            if (unitSystemConversionSet.CreationDate != null)
                                cDate = ((DateTimeOffset)unitSystemConversionSet.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string? lDate = null;
                            if (unitSystemConversionSet.LastModificationDate != null)
                                lDate = ((DateTimeOffset)unitSystemConversionSet.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string data = JsonSerializer.Serialize(unitSystemConversionSet);
                            var command = connection.CreateCommand();
                            command.CommandText = $"UPDATE UnitSystemConversionSetTable SET " +
                                $"MetaInfo = '{metaInfo}', " +
                                $"CreationDate = '{cDate}', " +
                                $"LastModificationDate = '{lDate}', " +
                                $"UnitSystemConversionSet = '{data}' " +
                                $"WHERE ID = '{guid}'";
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to update the UnitSystemConversionSet");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to update the UnitSystemConversionSet");
                            success = false;
                        }

                        // Finalizing
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Updated the given UnitSystemConversionSet successfully");
                            return true;
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The UnitSystemConversionSet ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the UnitSystemConversionSet of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the UnitSystemConversionSet was deleted from the microservice database</returns>
        public bool DeleteUnitSystemConversionSetById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    lock (_lock)
                    {
                        using var transaction = connection.BeginTransaction();
                        bool success = true;
                        //delete UnitSystemConversionSet from UnitSystemConversionSetTable
                        try
                        {
                            var command = connection.CreateCommand();
                            command.CommandText = $"DELETE FROM UnitSystemConversionSetTable WHERE ID = '{guid}'";
                            int count = command.ExecuteNonQuery();
                            if (count < 0)
                            {
                                _logger.LogWarning("Impossible to delete the UnitSystemConversionSet of given ID from the UnitSystemConversionSetTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to delete the UnitSystemConversionSet of given ID from UnitSystemConversionSetTable");
                            success = false;
                        }
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Removed the UnitSystemConversionSet of given ID from the UnitSystemConversionSetTable successfully");
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                        return success;
                    }
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The UnitSystemConversionSet ID is null or empty");
            }
            return false;
        }
    }
}