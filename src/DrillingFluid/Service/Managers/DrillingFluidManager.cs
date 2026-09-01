using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace NORCE.Drilling.DrillingFluid.Service.Managers
{
    /// <summary>
    /// A manager for DrillingFluid. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class DrillingFluidManager
    {
        private static DrillingFluidManager? _instance = null;
        private readonly ILogger<DrillingFluidManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        private DrillingFluidManager(ILogger<DrillingFluidManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static DrillingFluidManager GetInstance(ILogger<DrillingFluidManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new DrillingFluidManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM DrillingFluidTable";
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
                        _logger.LogError(ex, "Impossible to count records in the DrillingFluidTable");
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
                    //empty DrillingFluidTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM DrillingFluidTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the DrillingFluidTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM DrillingFluidTable WHERE ID = '{guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from DrillingFluidTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all DrillingFluid present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all DrillingFluid present in the microservice database</returns>
        public List<Guid>? GetAllDrillingFluidId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM DrillingFluidTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from DrillingFluidTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from DrillingFluidTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all DrillingFluid present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all DrillingFluid present in the microservice database</returns>
        public List<MetaInfo?>? GetAllDrillingFluidMetaInfo()
        {
            List<MetaInfo?> metaInfos = new();
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM DrillingFluidTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from DrillingFluidTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from DrillingFluidTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the DrillingFluid identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the DrillingFluid identified by its Guid from the microservice database</returns>
        public Model.DrillingFluid? GetDrillingFluidById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.DrillingFluid? drillingFluid;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT DrillingFluid FROM DrillingFluidTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            drillingFluid = JsonSerializer.Deserialize<Model.DrillingFluid>(data, JsonSettings.Options);
                            if (drillingFluid != null && drillingFluid.MetaInfo != null && !drillingFluid.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: returned DrillingFluid is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No DrillingFluid of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the DrillingFluid with the given ID from DrillingFluidTable");
                        return null;
                    }
                    _logger.LogInformation("Returning the DrillingFluid of given ID from DrillingFluidTable");
                    return drillingFluid;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluid ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all DrillingFluid present in the microservice database 
        /// </summary>
        /// <returns>the list of all DrillingFluid present in the microservice database</returns>
        public List<Model.DrillingFluid?>? GetAllDrillingFluid()
        {
            List<Model.DrillingFluid?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT DrillingFluid FROM DrillingFluidTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.DrillingFluid? drillingFluid = JsonSerializer.Deserialize<Model.DrillingFluid>(data, JsonSettings.Options);
                        vals.Add(drillingFluid);
                    }
                    _logger.LogInformation("Returning the list of existing DrillingFluid from DrillingFluidTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get DrillingFluid from DrillingFluidTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all DrillingFluidLight present in the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the list of DrillingFluidLight present in the microservice database</returns>
        public List<Model.DrillingFluidLight>? GetAllDrillingFluidLight()
        {
            List<Model.DrillingFluidLight>? drillingFluidLightList = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo, Name, Description, CreationDate, LastModificationDate FROM DrillingFluidTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string metaInfoStr = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(metaInfoStr, JsonSettings.Options);
                        string name = reader.GetString(1);
                        string descr = reader.GetString(2);
                        // make sure DateTimeOffset are properly instantiated when stored values are null (and parsed as empty string)
                        DateTimeOffset? creationDate = null;
                        if (DateTimeOffset.TryParse(reader.GetString(3), out DateTimeOffset cDate))
                            creationDate = cDate;
                        DateTimeOffset? lastModificationDate = null;
                        if (DateTimeOffset.TryParse(reader.GetString(4), out DateTimeOffset lDate))
                            lastModificationDate = lDate;
                        drillingFluidLightList.Add(new Model.DrillingFluidLight(
                                metaInfo,
                                string.IsNullOrEmpty(name) ? null : name,
                                string.IsNullOrEmpty(descr) ? null : descr,
                                creationDate,
                                lastModificationDate));
                    }
                    _logger.LogInformation("Returning the list of existing DrillingFluidLight from DrillingFluidTable");
                    return drillingFluidLightList;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get light datas from DrillingFluidTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Performs calculation on the given DrillingFluid and adds it to the microservice database
        /// </summary>
        /// <param name="drillingFluid"></param>
        /// <returns>true if the given DrillingFluid has been added successfully to the microservice database</returns>
        public bool AddDrillingFluid(Model.DrillingFluid? drillingFluid)
        {
            if (drillingFluid != null && drillingFluid.MetaInfo != null && drillingFluid.MetaInfo.ID != Guid.Empty)
            {
                //calculate outputs
                if (!drillingFluid.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given DrillingFluid");
                    return false;
                }

                //if successful, check if another parent data with the same ID was calculated/added during the calculation time
                Model.DrillingFluid? newDrillingFluid = GetDrillingFluidById(drillingFluid.MetaInfo.ID);
                if (newDrillingFluid == null)
                {
                    //update DrillingFluidTable
                    var connection = _connectionManager.GetConnection();
                    if (connection != null)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        bool success = true;
                        try
                        {
                            //add the DrillingFluid to the DrillingFluidTable
                            string metaInfo = JsonSerializer.Serialize(drillingFluid.MetaInfo, JsonSettings.Options);
                            string? cDate = null;
                            if (drillingFluid.CreationDate != null)
                                cDate = ((DateTimeOffset)drillingFluid.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string? lDate = null;
                            if (drillingFluid.LastModificationDate != null)
                                lDate = ((DateTimeOffset)drillingFluid.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string data = JsonSerializer.Serialize(drillingFluid, JsonSettings.Options);
                            var command = connection.CreateCommand();
                            command.CommandText = "INSERT INTO DrillingFluidTable (" +
                                "ID, " +
                                "MetaInfo, " +
                                "Name, " +
                                "Description, " +
                                "CreationDate, " +
                                "LastModificationDate, " +
                                "DrillingFluid" +
                                ") VALUES (" +
                                $"'{drillingFluid.MetaInfo.ID}', " +
                                $"'{metaInfo}', " +
                                $"'{drillingFluid.Name}', " +
                                $"'{drillingFluid.Description}', " +
                                $"'{cDate}', " +
                                $"'{lDate}', " +
                                $"'{data}'" +
                                ")";
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to insert the given DrillingFluid into the DrillingFluidTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to add the given DrillingFluid into DrillingFluidTable");
                            success = false;
                        }
                        //finalizing SQL transaction
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Added the given DrillingFluid of given ID into the DrillingFluidTable successfully");
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
                    _logger.LogWarning("Impossible to post DrillingFluid. ID already found in database.");
                    return false;
                }

            }
            else
            {
                _logger.LogWarning("The DrillingFluid ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given DrillingFluid and updates it in the microservice database
        /// </summary>
        /// <param name="drillingFluid"></param>
        /// <returns>true if the given DrillingFluid has been updated successfully</returns>
        public bool UpdateDrillingFluidById(Guid guid, Model.DrillingFluid? drillingFluid)
        {
            bool success = true;
            if (guid != Guid.Empty && drillingFluid != null && drillingFluid.MetaInfo != null && drillingFluid.MetaInfo.ID == guid)
            {
                //calculate outputs
                if (!drillingFluid.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs of the given DrillingFluid");
                    return false;
                }
                //update DrillingFluidTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in DrillingFluidTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(drillingFluid.MetaInfo, JsonSettings.Options);
                        string? cDate = null;
                        if (drillingFluid.CreationDate != null)
                            cDate = ((DateTimeOffset)drillingFluid.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        drillingFluid.LastModificationDate = DateTimeOffset.UtcNow;
                        string? lDate = ((DateTimeOffset)drillingFluid.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        string data = JsonSerializer.Serialize(drillingFluid, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE DrillingFluidTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"Name = '{drillingFluid.Name}', " +
                            $"Description = '{drillingFluid.Description}', " +
                            $"CreationDate = '{cDate}', " +
                            $"LastModificationDate = '{lDate}', " +
                            $"DrillingFluid = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the DrillingFluid");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the DrillingFluid");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given DrillingFluid successfully");
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
                _logger.LogWarning("The DrillingFluid ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the DrillingFluid of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the DrillingFluid was deleted from the microservice database</returns>
        public bool DeleteDrillingFluidById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete DrillingFluid from DrillingFluidTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM DrillingFluidTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the DrillingFluid of given ID from the DrillingFluidTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the DrillingFluid of given ID from DrillingFluidTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the DrillingFluid of given ID from the DrillingFluidTable successfully");
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
                _logger.LogWarning("The DrillingFluid ID is null or empty");
            }
            return false;
        }
    }
}