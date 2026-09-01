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
    /// A manager for UnitConversionSet. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class UnitConversionSetManager
    {
        private static UnitConversionSetManager? _instance = null;
        private readonly ILogger<UnitConversionSetManager> _logger;
        private readonly object _lock = new();
        private readonly SqlConnectionManager _connectionManager;

        private UnitConversionSetManager(ILogger<UnitConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static UnitConversionSetManager GetInstance(ILogger<UnitConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new UnitConversionSetManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM UnitConversionSetTable";
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
                        _logger.LogError(ex, "Impossible to count records in the UnitConversionSetTable");
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
                        //empty UnitConversionSetTable
                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM UnitConversionSetTable";
                        command.ExecuteNonQuery();

                        transaction.Commit();
                        success = true;
                    }
                    catch (SqliteException ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Impossible to clear the UnitConversionSetTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM UnitConversionSetTable WHERE ID = ' {guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from UnitConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all UnitConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all UnitConversionSet present in the microservice database</returns>
        public List<Guid>? GetAllUnitConversionSetId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM UnitConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from UnitConversionSetTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from UnitConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all UnitConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all UnitConversionSet present in the microservice database</returns>
        public List<MetaInfo?>? GetAllUnitConversionSetMetaInfo()
        {
            List<MetaInfo?> metaInfos = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM UnitConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from UnitConversionSetTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from UnitConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns a UnitConversionSet identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the UnitConversionSet retrieved from the database</returns>
        public UnitConversionSet? GetUnitConversionSetById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    UnitConversionSet? unitConversionSet = null;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT UnitConversionSet FROM UnitConversionSetTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            unitConversionSet = JsonSerializer.Deserialize<UnitConversionSet>(data);
                            if (unitConversionSet != null && unitConversionSet.MetaInfo != null && !unitConversionSet.MetaInfo.ID.Equals(guid))
                                throw (new SqliteException("SQLite database corrupted: retrieved UnitConversionSet is null or has been jsonified with the wrong ID.", 1));
                        }
                        else
                        {
                            _logger.LogInformation("No UnitConversionSet of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the UnitConversionSet with the given ID from UnitConversionSetTable");
                        return null;
                    }

                    // Finalizing
                    _logger.LogInformation("Returning the UnitConversionSet of given ID from UnitConversionSetTable");
                    return unitConversionSet;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given UnitConversionSet ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all UnitConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of all UnitConversionSet present in the microservice database</returns>
        public List<UnitConversionSet?>? GetAllUnitConversionSet()
        {
            List<UnitConversionSet?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT UnitConversionSet FROM UnitConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        UnitConversionSet? unitConversionSet = JsonSerializer.Deserialize<UnitConversionSet>(data);
                        vals.Add(unitConversionSet);
                    }
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get UnitConversionSet from UnitConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return vals;
        }

        /// <summary>
        /// Adds the given UnitConversionSet to the microservice database
        /// </summary>
        /// <param name="unitConversionSet"></param>
        /// <returns>true if the given UnitConversionSet has been added successfully</returns>
        public bool AddUnitConversionSet(UnitConversionSet? unitConversionSet)
        {
            if (unitConversionSet != null && unitConversionSet.MetaInfo != null && unitConversionSet.MetaInfo.ID != Guid.Empty)
            {
                //calculate outputs
                if (!unitConversionSet.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given UnitConversionSet");
                    return false;
                }
                //update UnitConversionSetTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    lock (_lock)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        bool success = true;
                        try
                        {
                            //add the UnitConversionSet to the UnitConversionSetTable
                            string metaInfo = JsonSerializer.Serialize(unitConversionSet.MetaInfo);
                            string? cDate = null;
                            if (unitConversionSet.CreationDate != null)
                                cDate = ((DateTimeOffset)unitConversionSet.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string? lDate = null;
                            if (unitConversionSet.LastModificationDate != null)
                                lDate = ((DateTimeOffset)unitConversionSet.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string data = JsonSerializer.Serialize(unitConversionSet);
                            var command = connection.CreateCommand();
                            command.CommandText = "INSERT INTO UnitConversionSetTable " +
                                "(ID, " +
                                "MetaInfo, " +
                                "CreationDate, " +
                                "LastModificationDate, " +
                                "UnitConversionSet" +
                                ") VALUES (" +
                                $"'{unitConversionSet.MetaInfo.ID}', " +
                                $"'{metaInfo}', " +
                                $"'{cDate}', " +
                                $"'{lDate}', " +
                                $"'{data}'" +
                                ")";
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to insert the given UnitConversionSet into the UnitConversionSetTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to add the given UnitConversionSet into UnitConversionSetTable");
                            success = false;
                        }
                        // Finalizing
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Added the given UnitConversionSet of given ID into the UnitConversionSetTable successfully");
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
                _logger.LogWarning("The UnitConversionSet ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given UnitConversionSet and updates it in the microservice database
        /// </summary>
        /// <param name="unitConversionSet"></param>
        /// <returns>true if the given UnitConversionSet has been updated successfully</returns>
        public bool UpdateUnitConversionSetById(Guid guid, UnitConversionSet? unitConversionSet)
        {
            bool success = true;
            if (guid != Guid.Empty && unitConversionSet != null && unitConversionSet.MetaInfo != null && unitConversionSet.MetaInfo.ID == guid)
            {
                //calculate outputs
                if (!unitConversionSet.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given UnitConversionSet");
                    return false;
                }
                //update UnitConversionSetTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    lock (_lock)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        //update fields in UnitConversionSetTable
                        try
                        {
                            string metaInfo = JsonSerializer.Serialize(unitConversionSet.MetaInfo);
                            string? cDate = null;
                            if (unitConversionSet.CreationDate != null)
                                cDate = ((DateTimeOffset)unitConversionSet.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string? lDate = null;
                            if (unitConversionSet.LastModificationDate != null)
                                lDate = ((DateTimeOffset)unitConversionSet.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string data = JsonSerializer.Serialize(unitConversionSet);
                            var command = connection.CreateCommand();
                            command.CommandText = $"UPDATE UnitConversionSetTable SET " +
                                $"MetaInfo = '{metaInfo}', " +
                                $"CreationDate = '{cDate}', " +
                                $"LastModificationDate = '{lDate}', " +
                                $"UnitConversionSet = '{data}' " +
                                $"WHERE ID = '{guid}'";
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to update the UnitConversionSet");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to update the UnitConversionSet");
                            success = false;
                        }

                        // Finalizing
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Updated the given UnitConversionSet successfully");
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
                _logger.LogWarning("The UnitConversionSet ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the UnitConversionSet of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the UnitConversionSet was deleted from the microservice database</returns>
        public bool DeleteUnitConversionSetById(Guid guid)
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
                        //delete UnitConversionSet from UnitConversionSetTable
                        try
                        {
                            var command = connection.CreateCommand();
                            command.CommandText = $"DELETE FROM UnitConversionSetTable WHERE ID = '{guid}'";
                            int count = command.ExecuteNonQuery();
                            if (count < 0)
                            {
                                _logger.LogWarning("Impossible to delete the UnitConversionSet of given ID from the UnitConversionSetTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to delete the UnitConversionSet of given ID from UnitConversionSetTable");
                            success = false;
                        }
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Removed the UnitConversionSet of given ID from the UnitConversionSetTable successfully");
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
                _logger.LogWarning("The UnitConversionSet ID is null or empty");
            }
            return false;
        }
    }
}