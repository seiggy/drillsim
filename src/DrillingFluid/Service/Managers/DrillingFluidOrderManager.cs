using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace NORCE.Drilling.DrillingFluid.Service.Managers
{
    /// <summary>
    /// A manager for DrillingFluidOrder. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class DrillingFluidOrderManager
    {
        private static DrillingFluidOrderManager? _instance = null;
        private readonly ILogger<DrillingFluidOrderManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        private DrillingFluidOrderManager(ILogger<DrillingFluidOrderManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static DrillingFluidOrderManager GetInstance(ILogger<DrillingFluidOrderManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new DrillingFluidOrderManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM DrillingFluidOrderTable";
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
                        _logger.LogError(ex, "Impossible to count records in the DrillingFluidOrderTable");
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
                    //empty DrillingFluidOrderTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM DrillingFluidOrderTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the DrillingFluidOrderTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM DrillingFluidOrderTable WHERE ID = '{guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from DrillingFluidOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all DrillingFluidOrder present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all DrillingFluidOrder present in the microservice database</returns>
        public List<Guid>? GetAllDrillingFluidOrderId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM DrillingFluidOrderTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from DrillingFluidOrderTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from DrillingFluidOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all DrillingFluidOrder present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all DrillingFluidOrder present in the microservice database</returns>
        public List<MetaInfo?>? GetAllDrillingFluidOrderMetaInfo()
        {
            List<MetaInfo?> metaInfos = new();
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM DrillingFluidOrderTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from DrillingFluidOrderTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from DrillingFluidOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the DrillingFluidOrder identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the DrillingFluidOrder identified by its Guid from the microservice database</returns>
        public Model.DrillingFluidOrder? GetDrillingFluidOrderById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.DrillingFluidOrder? drillingFluidOrder;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT DrillingFluidOrder FROM DrillingFluidOrderTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            drillingFluidOrder = JsonSerializer.Deserialize<Model.DrillingFluidOrder>(data, JsonSettings.Options);
                            if (drillingFluidOrder != null && drillingFluidOrder.MetaInfo != null && !drillingFluidOrder.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: returned DrillingFluidOrder is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No DrillingFluidOrder of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the DrillingFluidOrder with the given ID from DrillingFluidOrderTable");
                        return null;
                    }
                    _logger.LogInformation("Returning the DrillingFluidOrder of given ID from DrillingFluidOrderTable");
                    return drillingFluidOrder;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluidOrder ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all DrillingFluidOrder present in the microservice database 
        /// </summary>
        /// <returns>the list of all DrillingFluidOrder present in the microservice database</returns>
        public List<Model.DrillingFluidOrder?>? GetAllDrillingFluidOrder()
        {
            List<Model.DrillingFluidOrder?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT DrillingFluidOrder FROM DrillingFluidOrderTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.DrillingFluidOrder? drillingFluidOrder = JsonSerializer.Deserialize<Model.DrillingFluidOrder>(data, JsonSettings.Options);
                        vals.Add(drillingFluidOrder);
                    }
                    _logger.LogInformation("Returning the list of existing DrillingFluidOrder from DrillingFluidOrderTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get DrillingFluidOrder from DrillingFluidOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all DrillingFluidOrderLight present in the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the list of DrillingFluidOrderLight present in the microservice database</returns>
       
        /// <summary>
        /// Performs calculation on the given DrillingFluidOrder and adds it to the microservice database
        /// </summary>
        /// <param name="drillingFluidOrder"></param>
        /// <returns>true if the given DrillingFluidOrder has been added successfully to the microservice database</returns>
        public bool AddDrillingFluidOrder(Model.DrillingFluidOrder? drillingFluidOrder)
        {
            if (drillingFluidOrder != null && drillingFluidOrder.MetaInfo != null && drillingFluidOrder.MetaInfo.ID != Guid.Empty)
            {
                //calculate outputs
                if (!drillingFluidOrder.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given DrillingFluidOrder");
                    return false;
                }

                //if successful, check if another parent data with the same ID was calculated/added during the calculation time
                Model.DrillingFluidOrder? newDrillingFluidOrder = GetDrillingFluidOrderById(drillingFluidOrder.MetaInfo.ID);
                if (newDrillingFluidOrder == null)
                {
                    //update DrillingFluidOrderTable
                    var connection = _connectionManager.GetConnection();
                    if (connection != null)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        bool success = true;
                        try
                        {
                            //add the DrillingFluidOrder to the DrillingFluidOrderTable
                            string metaInfo = JsonSerializer.Serialize(drillingFluidOrder.MetaInfo, JsonSettings.Options);
                            string? cDate = null;
                            if (drillingFluidOrder.CreationDate != null)
                                cDate = ((DateTimeOffset)drillingFluidOrder.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string? lDate = null;
                            if (drillingFluidOrder.LastModificationDate != null)
                                lDate = ((DateTimeOffset)drillingFluidOrder.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string data = JsonSerializer.Serialize(drillingFluidOrder, JsonSettings.Options);
                            var command = connection.CreateCommand();
                            command.CommandText = "INSERT INTO DrillingFluidOrderTable (" +
                                "ID, " +
                                "MetaInfo, " +
                                "Name, " +
                                "Description, " +
                                "CreationDate, " +
                                "LastModificationDate, " +
                                "DrillingFluidOrder" +
                                ") VALUES (" +
                                $"'{drillingFluidOrder.MetaInfo.ID}', " +
                                $"'{metaInfo}', " +
                                $"'{drillingFluidOrder.Name}', " +
                                $"'{drillingFluidOrder.Description}', " +
                                $"'{cDate}', " +
                                $"'{lDate}', " +
                                $"'{data}'" +
                                ")";
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to insert the given DrillingFluidOrder into the DrillingFluidOrderTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to add the given DrillingFluidOrder into DrillingFluidOrderTable");
                            success = false;
                        }
                        //finalizing SQL transaction
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Added the given DrillingFluidOrder of given ID into the DrillingFluidOrderTable successfully");
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
                    _logger.LogWarning("Impossible to post DrillingFluidOrder. ID already found in database.");
                    return false;
                }

            }
            else
            {
                _logger.LogWarning("The DrillingFluidOrder ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given DrillingFluidOrder and updates it in the microservice database
        /// </summary>
        /// <param name="drillingFluidOrder"></param>
        /// <returns>true if the given DrillingFluidOrder has been updated successfully</returns>
        public bool UpdateDrillingFluidOrderById(Guid guid, Model.DrillingFluidOrder? drillingFluidOrder)
        {
            bool success = true;
            if (guid != Guid.Empty && drillingFluidOrder != null && drillingFluidOrder.MetaInfo != null && drillingFluidOrder.MetaInfo.ID == guid)
            {
                //calculate outputs
                if (!drillingFluidOrder.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs of the given DrillingFluidOrder");
                    return false;
                }
                //update DrillingFluidOrderTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in DrillingFluidOrderTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(drillingFluidOrder.MetaInfo, JsonSettings.Options);
                        string? cDate = null;
                        if (drillingFluidOrder.CreationDate != null)
                            cDate = ((DateTimeOffset)drillingFluidOrder.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        drillingFluidOrder.LastModificationDate = DateTimeOffset.UtcNow;
                        string? lDate = ((DateTimeOffset)drillingFluidOrder.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        string data = JsonSerializer.Serialize(drillingFluidOrder, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE DrillingFluidOrderTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"Name = '{drillingFluidOrder.Name}', " +
                            $"Description = '{drillingFluidOrder.Description}', " +
                            $"CreationDate = '{cDate}', " +
                            $"LastModificationDate = '{lDate}', " +
                            $"DrillingFluidOrder = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the DrillingFluidOrder");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the DrillingFluidOrder");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given DrillingFluidOrder successfully");
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
                _logger.LogWarning("The DrillingFluidOrder ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the DrillingFluidOrder of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the DrillingFluidOrder was deleted from the microservice database</returns>
        public bool DeleteDrillingFluidOrderById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete DrillingFluidOrder from DrillingFluidOrderTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM DrillingFluidOrderTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the DrillingFluidOrder of given ID from the DrillingFluidOrderTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the DrillingFluidOrder of given ID from DrillingFluidOrderTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the DrillingFluidOrder of given ID from the DrillingFluidOrderTable successfully");
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
                _logger.LogWarning("The DrillingFluidOrder ID is null or empty");
            }
            return false;
        }
    }
}