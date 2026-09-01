using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using NORCE.Drilling.GeothermalProperties.Model;

namespace NORCE.Drilling.GeothermalProperties.Service.Managers
{

    /// <summary>
    /// A manager for GeothermalPropertiesCompletionOrder. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class GeothermalPropertiesCompletionOrderManager
    {
        private static GeothermalPropertiesCompletionOrderManager? _instance = null;
        private readonly ILogger<GeothermalPropertiesCompletionOrderManager> _logger;
        private readonly SqlConnectionManager _connectionManager;
        private static GeothermalPropertiesManager? geothermalPropertiesManager = null;
        private GeothermalPropertiesCompletionOrderManager(ILogger<GeothermalPropertiesCompletionOrderManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static GeothermalPropertiesCompletionOrderManager GetInstance(ILogger<GeothermalPropertiesCompletionOrderManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new GeothermalPropertiesCompletionOrderManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM GeothermalPropertiesCompletionOrderTable";
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
                        _logger.LogError(ex, "Impossible to count records in the GeothermalPropertiesCompletionOrderTable");
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
                    //empty GeothermalPropertiesCompletionOrderTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM GeothermalPropertiesCompletionOrderTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the GeothermalPropertiesCompletionOrderTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM GeothermalPropertiesCompletionOrderTable WHERE ID = '{guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from GeothermalPropertiesCompletionOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }
        private static Model.GeothermalPropertiesCompletionOrderLight CreateDataLightInstance(Model.GeothermalPropertiesCompletionOrder geothermalPropertiesCompletionOrder)
        {
            return new Model.GeothermalPropertiesCompletionOrderLight()
                {
                    MetaInfo = geothermalPropertiesCompletionOrder.MetaInfo,
                    Name = geothermalPropertiesCompletionOrder.Name,
                    Description = geothermalPropertiesCompletionOrder.Description,
                    CreationDate = geothermalPropertiesCompletionOrder.CreationDate,
                    LastModificationDate = geothermalPropertiesCompletionOrder.LastModificationDate
                };
        }
        /// <summary>
        /// Returns the list of Guid of all GeothermalPropertiesCompletionOrder present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all GeothermalPropertiesCompletionOrder present in the microservice database</returns>
        public List<Guid>? GetAllGeothermalPropertiesCompletionOrderId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM GeothermalPropertiesCompletionOrderTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from GeothermalPropertiesCompletionOrderTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from GeothermalPropertiesCompletionOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all GeothermalPropertiesCompletionOrder present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all GeothermalPropertiesCompletionOrder present in the microservice database</returns>
        public List<MetaInfo?>? GetAllGeothermalPropertiesCompletionOrderMetaInfo()
        {
            List<MetaInfo?> metaInfos = new();
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM GeothermalPropertiesCompletionOrderTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from GeothermalPropertiesCompletionOrderTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from GeothermalPropertiesCompletionOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the GeothermalPropertiesCompletionOrder identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the GeothermalPropertiesCompletionOrder identified by its Guid from the microservice database</returns>
        public Model.GeothermalPropertiesCompletionOrder? GetGeothermalPropertiesCompletionOrderById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.GeothermalPropertiesCompletionOrder? geothermalPropertiesCompletionOrder;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT GeothermalPropertiesCompletionOrder FROM GeothermalPropertiesCompletionOrderTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            geothermalPropertiesCompletionOrder = JsonSerializer.Deserialize<Model.GeothermalPropertiesCompletionOrder>(data, JsonSettings.Options);
                            if (geothermalPropertiesCompletionOrder != null && geothermalPropertiesCompletionOrder.MetaInfo != null && !geothermalPropertiesCompletionOrder.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: returned GeothermalPropertiesCompletionOrder is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No GeothermalPropertiesCompletionOrder of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the GeothermalPropertiesCompletionOrder with the given ID from GeothermalPropertiesCompletionOrderTable");
                        return null;
                    }
                    _logger.LogInformation("Returning the GeothermalPropertiesCompletionOrder of given ID from GeothermalPropertiesCompletionOrderTable");
                    return geothermalPropertiesCompletionOrder;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given GeothermalPropertiesCompletionOrder ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all GeothermalPropertiesCompletionOrder present in the microservice database 
        /// </summary>
        /// <returns>the list of all GeothermalPropertiesCompletionOrder present in the microservice database</returns>
        public List<Model.GeothermalPropertiesCompletionOrder?>? GetAllGeothermalPropertiesCompletionOrder()
        {
            List<Model.GeothermalPropertiesCompletionOrder?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT GeothermalPropertiesCompletionOrder FROM GeothermalPropertiesCompletionOrderTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.GeothermalPropertiesCompletionOrder? geothermalPropertiesCompletionOrder = JsonSerializer.Deserialize<Model.GeothermalPropertiesCompletionOrder>(data, JsonSettings.Options);
                        vals.Add(geothermalPropertiesCompletionOrder);
                    }
                    _logger.LogInformation("Returning the list of existing GeothermalPropertiesCompletionOrder from GeothermalPropertiesCompletionOrderTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get GeothermalPropertiesCompletionOrder from GeothermalPropertiesCompletionOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all GeothermalPropertiesCompletionOrderLight present in the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the list of GeothermalPropertiesCompletionOrderLight present in the microservice database</returns>
        public List<Model.GeothermalPropertiesCompletionOrderLight>? GetAllGeothermalPropertiesCompletionOrderLight()
        {
            List<Model.GeothermalPropertiesCompletionOrderLight>? geothermalPropertiesCompletionOrderLightList = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo, GeothermalPropertiesCompletionOrderLight FROM GeothermalPropertiesCompletionOrderTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string metaInfoStr = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(metaInfoStr, JsonSettings.Options);
                        Model.GeothermalPropertiesCompletionOrderLight? geothermalPropertiesCompletionOrderLight = JsonSerializer.Deserialize<Model.GeothermalPropertiesCompletionOrderLight>(reader.GetString(1), JsonSettings.Options);
                        if (geothermalPropertiesCompletionOrderLight != null)
                        {
                            geothermalPropertiesCompletionOrderLightList.Add(geothermalPropertiesCompletionOrderLight);                            
                        }
                    }
                    _logger.LogInformation("Returning the list of existing GeothermalPropertiesCompletionOrderLight from GeothermalPropertiesCompletionOrderTable");
                    return geothermalPropertiesCompletionOrderLightList;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get light datas from GeothermalPropertiesCompletionOrderTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Performs calculation on the given GeothermalPropertiesCompletionOrder and adds it to the microservice database
        /// </summary>
        /// <param name="geothermalPropertiesCompletionOrder"></param>
        /// <returns>true if the given GeothermalPropertiesCompletionOrder has been added successfully to the microservice database</returns>
        public bool AddGeothermalPropertiesCompletionOrder(Model.GeothermalPropertiesCompletionOrder? geothermalPropertiesCompletionOrder)
        {
            if (geothermalPropertiesCompletionOrder != null && geothermalPropertiesCompletionOrder.MetaInfo != null && geothermalPropertiesCompletionOrder.MetaInfo.ID != Guid.Empty)
            {
                //calculate outputs
                if (!geothermalPropertiesCompletionOrder.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given GeothermalPropertiesCompletionOrder");
                    return false;
                }

                //if successful, check if another parent data with the same ID was calculated/added during the calculation time
                Model.GeothermalPropertiesCompletionOrder? newGeothermalPropertiesCompletionOrder = GetGeothermalPropertiesCompletionOrderById(geothermalPropertiesCompletionOrder.MetaInfo.ID);
                if (newGeothermalPropertiesCompletionOrder == null)
                {
                    //update GeothermalPropertiesCompletionOrderTable
                    var connection = _connectionManager.GetConnection();
                    if (connection != null)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        bool success = true;
                        try
                        {
                            //add the GeothermalPropertiesCompletionOrder to the GeothermalPropertiesCompletionOrderTable
                            string metaInfo = JsonSerializer.Serialize(geothermalPropertiesCompletionOrder.MetaInfo, JsonSettings.Options);
                      
                            Model.GeothermalPropertiesCompletionOrderLight geothermalPropertiesCompletionOrderLight = CreateDataLightInstance(geothermalPropertiesCompletionOrder);
                            string dataLight = JsonSerializer.Serialize(geothermalPropertiesCompletionOrderLight, JsonSettings.Options);                           

                            string? cDate = null;
                            if (geothermalPropertiesCompletionOrder.CreationDate != null)
                                cDate = ((DateTimeOffset)geothermalPropertiesCompletionOrder.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string? lDate = null;
                            if (geothermalPropertiesCompletionOrder.LastModificationDate != null)
                                lDate = ((DateTimeOffset)geothermalPropertiesCompletionOrder.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string data = JsonSerializer.Serialize(geothermalPropertiesCompletionOrder, JsonSettings.Options);
                            
                            var command = connection.CreateCommand();
                            command.CommandText = "INSERT INTO GeothermalPropertiesCompletionOrderTable (" +
                                "ID, " +
                                "MetaInfo, " +
                                "GeothermalPropertiesCompletionOrderLight, " +                                
                                "CreationDate, " +
                                "LastModificationDate, " +
                                "GeothermalPropertiesCompletionOrder" +
                                ") VALUES (" +
                                $"'{geothermalPropertiesCompletionOrder.MetaInfo.ID}', " +
                                $"'{metaInfo}', " +
                                $"'{dataLight}', " +
                                $"'{cDate}', " +
                                $"'{lDate}', " +
                                $"'{data}'" +
                                ")";
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to insert the given GeothermalPropertiesCompletionOrder into the GeothermalPropertiesCompletionOrderTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to add the given GeothermalPropertiesCompletionOrder into GeothermalPropertiesCompletionOrderTable");
                            success = false;
                        }
                        try
                        {
                            Model.GeothermalProperties? geoData = geothermalPropertiesCompletionOrder.CompletedGeothermalProperties;       
                            if (geoData != null)
                            {
                                //add the GeothermalProperties to the GeothermalPropertiesTable
                                string metaInfo = JsonSerializer.Serialize(geoData.MetaInfo, JsonSettings.Options);
                                string data = JsonSerializer.Serialize(geoData, JsonSettings.Options);
                                var command = connection.CreateCommand();
                                command.CommandText = "INSERT INTO GeothermalPropertiesTable (" +
                                    "ID, " +
                                    "MetaInfo, " +
                                    "GeothermalProperties" +
                                    ") VALUES (" +
                                    $"'{geoData.MetaInfo!.ID}', " +
                                    $"'{metaInfo}', " +
                                    $"'{data}'" +
                                    ")";
                                int count = command.ExecuteNonQuery();
                                if (count != 1)
                                {
                                    _logger.LogWarning("Impossible to insert the given Geothermal Properties into the GeothermalPropertiesTable");
                                    success = false;
                                }                                
                            }
                            else
                            {
                                _logger.LogError("Impossible to updated interpolated data");
                                success = false;                                
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to updated interpolated data");
                            success = false;
                        }
                        //finalizing SQL transaction
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Added the given GeothermalPropertiesCompletionOrder of given ID into the GeothermalPropertiesCompletionOrderTable successfully");
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
                    _logger.LogWarning("Impossible to post GeothermalPropertiesCompletionOrder. ID already found in database.");
                    return false;
                }

            }
            else
            {
                _logger.LogWarning("The GeothermalPropertiesCompletionOrder ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given GeothermalPropertiesCompletionOrder and updates it in the microservice database
        /// </summary>
        /// <param name="geothermalPropertiesCompletionOrder"></param>
        /// <returns>true if the given GeothermalPropertiesCompletionOrder has been updated successfully</returns>
        public bool UpdateGeothermalPropertiesCompletionOrderById(Guid guid, Model.GeothermalPropertiesCompletionOrder? geothermalPropertiesCompletionOrder)
        {
            bool success = true;
            if (guid != Guid.Empty && geothermalPropertiesCompletionOrder != null && geothermalPropertiesCompletionOrder.MetaInfo != null && geothermalPropertiesCompletionOrder.MetaInfo.ID == guid)
            {
                //calculate outputs
                if (!geothermalPropertiesCompletionOrder.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs of the given GeothermalPropertiesCompletionOrder");
                    return false;
                }
                //update GeothermalPropertiesCompletionOrderTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in GeothermalPropertiesCompletionOrderTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(geothermalPropertiesCompletionOrder.MetaInfo, JsonSettings.Options);
                        Model.GeothermalPropertiesCompletionOrderLight geothermalPropertiesCompletionOrderLight = CreateDataLightInstance(geothermalPropertiesCompletionOrder);
                        string dataLight = JsonSerializer.Serialize(geothermalPropertiesCompletionOrderLight, JsonSettings.Options);                           
                        string? cDate = null;
                        if (geothermalPropertiesCompletionOrder.CreationDate != null)
                            cDate = ((DateTimeOffset)geothermalPropertiesCompletionOrder.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        geothermalPropertiesCompletionOrder.LastModificationDate = DateTimeOffset.UtcNow;
                        string? lDate = ((DateTimeOffset)geothermalPropertiesCompletionOrder.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        string data = JsonSerializer.Serialize(geothermalPropertiesCompletionOrder, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE GeothermalPropertiesCompletionOrderTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"GeothermalPropertiesCompletionOrderLight = '{dataLight}', " +                              
                            $"CreationDate = '{cDate}', " +
                            $"LastModificationDate = '{lDate}', " +
                            $"GeothermalPropertiesCompletionOrder = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the GeothermalPropertiesCompletionOrder");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the GeothermalPropertiesCompletionOrder");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given GeothermalPropertiesCompletionOrder successfully");
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
                _logger.LogWarning("The GeothermalPropertiesCompletionOrder ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the GeothermalPropertiesCompletionOrder of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the GeothermalPropertiesCompletionOrder was deleted from the microservice database</returns>
        public bool DeleteGeothermalPropertiesCompletionOrderById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete GeothermalPropertiesCompletionOrder from GeothermalPropertiesCompletionOrderTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM GeothermalPropertiesCompletionOrderTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the GeothermalPropertiesCompletionOrder of given ID from the GeothermalPropertiesCompletionOrderTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the GeothermalPropertiesCompletionOrder of given ID from GeothermalPropertiesCompletionOrderTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the GeothermalPropertiesCompletionOrder of given ID from the GeothermalPropertiesCompletionOrderTable successfully");
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
                _logger.LogWarning("The GeothermalPropertiesCompletionOrder ID is null or empty");
            }
            return false;
        }
    }
}