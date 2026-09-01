using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NORCE.Drilling.CartographicProjection.Model;
using NORCE.Drilling.CartographicProjection.ModelShared;
using OSDC.DotnetLibraries.General.DataManagement;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NORCE.Drilling.CartographicProjection.Service.Managers
{
    /// <summary>
    /// A manager for CartographicProjection. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class CartographicProjectionManager
    {
        private static CartographicProjectionManager? _instance = null;
        private readonly ILogger<CartographicProjectionManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        private CartographicProjectionManager(ILogger<CartographicProjectionManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;

            // make sure database contains default CartographicProjections
            List<Guid>? ids = GetAllCartographicProjectionId();
            if (ids != null && ids.Count < 1)
            {
                FillDefault();
            }
            ids = GetAllCartographicProjectionId();
            if (ids != null)
            {
                bool isCorrupted = false;
                try
                {
                    foreach (Guid id in ids)
                    {
                        Model.CartographicProjection? cp = GetCartographicProjectionById(id);
                        if (cp == null || cp.MetaInfo == null || cp.MetaInfo.ID != id)
                        {
                            isCorrupted = true;
                            break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    isCorrupted = true;
                }
                if (isCorrupted)
                {
                    _logger.LogWarning("The CartographicProjectionTable is corrupted: clearing it and filling it with default CartographicProjections");
                    Clear();
                    FillDefault();
                }
            }
        }

        public static CartographicProjectionManager GetInstance(ILogger<CartographicProjectionManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new CartographicProjectionManager(logger, connectionManager);
            return _instance;
        }

        internal static CartographicProjectionManager Instance { get { return _instance; } }

        public int Count
        {
            get
            {
                int count = 0;
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM CartographicProjectionTable";
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
                        _logger.LogError(ex, "Impossible to count records in the CartographicProjectionTable");
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
                    //empty CartographicProjectionTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM CartographicProjectionTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the CartographicProjectionTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM CartographicProjectionTable WHERE ID = ' {guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from CartographicProjectionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all CartographicProjection present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all CartographicProjection present in the microservice database</returns>
        public List<Guid>? GetAllCartographicProjectionId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM CartographicProjectionTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from CartographicProjectionTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from CartographicProjectionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all CartographicProjection present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all CartographicProjection present in the microservice database</returns>
        public List<ModelShared.MetaInfo?>? GetAllCartographicProjectionMetaInfo()
        {
            List<ModelShared.MetaInfo?> metaInfos = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM CartographicProjectionTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        ModelShared.MetaInfo? metaInfo = JsonSerializer.Deserialize<ModelShared.MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from CartographicProjectionTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from CartographicProjectionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns a CartographicProjection identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the CartographicProjection retrieved from the database</returns>
        public Model.CartographicProjection? GetCartographicProjectionById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.CartographicProjection? cartographicProjection = null;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT CartographicProjection FROM CartographicProjectionTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            cartographicProjection = JsonSerializer.Deserialize<Model.CartographicProjection>(data, JsonSettings.Options);
                            if (cartographicProjection != null && cartographicProjection.MetaInfo != null && !cartographicProjection.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: retrieved CartographicProjection is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No CartographicProjection of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the CartographicProjection with the given ID from CartographicProjectionTable");
                        return null;
                    }

                    // Finalizing
                    _logger.LogInformation("Returning the CartographicProjection of given ID from CartographicProjectionTable");
                    return cartographicProjection;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given CartographicProjection ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all CartographicProjection present in the microservice database 
        /// </summary>
        /// <returns>the list of all CartographicProjection present in the microservice database</returns>
        public List<Model.CartographicProjection?>? GetAllCartographicProjection()
        {
            List<Model.CartographicProjection?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT CartographicProjection FROM CartographicProjectionTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.CartographicProjection? cartographicProjection = JsonSerializer.Deserialize<Model.CartographicProjection>(data, JsonSettings.Options);
                        vals.Add(cartographicProjection);
                    }
                    _logger.LogInformation("Returning the list of existing CartographicProjection from CartographicProjectionTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get CartographicProjection from CartographicProjectionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all CartographicProjectionLight present in the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the list of CartographicProjectionLight present in the microservice database</returns>
        public List<Model.CartographicProjectionLight>? GetAllCartographicProjectionLight()
        {
            List<Model.CartographicProjectionLight>? cartographicProjectionLightList = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo, Name, Description, CreationDate, LastModificationDate FROM CartographicProjectionTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string metaInfoStr = reader.GetString(0);
                        OSDC.DotnetLibraries.General.DataManagement.MetaInfo? metaInfo = JsonSerializer.Deserialize<OSDC.DotnetLibraries.General.DataManagement.MetaInfo>(metaInfoStr, JsonSettings.Options);
                        string name = reader.GetString(1);
                        string descr = reader.GetString(2);
                        // make sure DateTimeOffset are properly instantiated when stored values are null (and parsed as empty string)
                        DateTimeOffset? creationDate = null;
                        if (DateTimeOffset.TryParse(reader.GetString(3), out DateTimeOffset cDate))
                            creationDate = cDate;
                        DateTimeOffset? lastModificationDate = null;
                        if (DateTimeOffset.TryParse(reader.GetString(4), out DateTimeOffset lDate))
                            lastModificationDate = lDate;
                        cartographicProjectionLightList.Add(new Model.CartographicProjectionLight(
                                metaInfo,
                                string.IsNullOrEmpty(name) ? null : name,
                                string.IsNullOrEmpty(descr) ? null : descr,
                                creationDate,
                                lastModificationDate,
                                false));
                    }
                    _logger.LogInformation("Returning the list of existing CartographicProjectionLight from CartographicProjectionTable");
                    return cartographicProjectionLightList;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get light datas from CartographicProjectionTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Adds the given CartographicProjection to the microservice database
        /// </summary>
        /// <param name="cartographicProjection"></param>
        /// <returns>true if the given CartographicProjection has been added successfully</returns>
        public bool AddCartographicProjection(Model.CartographicProjection? cartographicProjection)
        {
            if (cartographicProjection != null && cartographicProjection.MetaInfo != null && cartographicProjection.MetaInfo.ID != Guid.Empty)
            {
                //update CartographicProjectionTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    bool success = true;
                    try
                    {
                        //add the CartographicProjection to the CartographicProjectionTable
                        string metaInfo = JsonSerializer.Serialize(cartographicProjection.MetaInfo, JsonSettings.Options);
                        string? cDate = null;
                        if (cartographicProjection.CreationDate != null)
                            cDate = ((DateTimeOffset)cartographicProjection.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        cartographicProjection.LastModificationDate = DateTimeOffset.UtcNow;
                        string? lDate = ((DateTimeOffset)cartographicProjection.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        string data = JsonSerializer.Serialize(cartographicProjection, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = "INSERT INTO CartographicProjectionTable (" +
                            "ID, " +
                            "MetaInfo, " +
                            "Name, " +
                            "Description, " +
                            "CreationDate, " +
                            "LastModificationDate, " +
                            "CartographicProjection" +
                            ") VALUES (" +
                            $"'{cartographicProjection.MetaInfo.ID}', " +
                            $"'{metaInfo}', " +
                            $"'{cartographicProjection.Name}', " +
                            $"'{cartographicProjection.Description}', " +
                            $"'{cDate}', " +
                            $"'{lDate}', " +
                            $"'{data}'" +
                            ")";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to insert the given CartographicProjection into the CartographicProjectionTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to add the given CartographicProjection into CartographicProjectionTable");
                        success = false;
                    }
                    //finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Added the given CartographicProjection of given ID into the CartographicProjectionTable successfully");
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
                _logger.LogWarning("The CartographicProjection ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given CartographicProjection and updates it in the microservice database
        /// </summary>
        /// <param name="cartographicProjection"></param>
        /// <returns>true if the given CartographicProjection has been updated successfully</returns>
        public bool UpdateCartographicProjectionById(Guid guid, Model.CartographicProjection? cartographicProjection)
        {
            bool success = true;
            if (guid != Guid.Empty && cartographicProjection != null && cartographicProjection.MetaInfo != null && cartographicProjection.MetaInfo.ID == guid)
            {
                //update CartographicProjectionTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in CartographicProjectionTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(cartographicProjection.MetaInfo, JsonSettings.Options);
                        string? cDate = null;
                        if (cartographicProjection.CreationDate != null)
                            cDate = ((DateTimeOffset)cartographicProjection.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        cartographicProjection.LastModificationDate = DateTimeOffset.UtcNow;
                        string? lDate = ((DateTimeOffset)cartographicProjection.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        string data = JsonSerializer.Serialize(cartographicProjection, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE CartographicProjectionTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"Name = '{cartographicProjection.Name}', " +
                            $"Description = '{cartographicProjection.Description}', " +
                            $"CreationDate = '{cDate}', " +
                            $"LastModificationDate = '{lDate}', " +
                            $"CartographicProjection = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the CartographicProjection");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the CartographicProjection");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given CartographicProjection successfully");
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
                _logger.LogWarning("The CartographicProjection ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the CartographicProjection of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the CartographicProjection was deleted from the microservice database</returns>
        public bool DeleteCartographicProjectionById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete CartographicProjection from CartographicProjectionTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM CartographicProjectionTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the CartographicProjection of given ID from the CartographicProjectionTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the CartographicProjection of given ID from CartographicProjectionTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the CartographicProjection of given ID from the CartographicProjectionTable successfully");
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
                _logger.LogWarning("The CartographicProjection ID is null or empty");
            }
            return false;
        }

        /// <summary>
        /// populate database with default GeodeticDatum
        /// </summary>
        private async void FillDefault()
        {
            List<Model.CartographicProjection?> cartographicProjections = [];

            ICollection<GeodeticDatum>? geodeticDatumList = await APIUtils.ClientGeodeticDatum.GetAllGeodeticDatumAsync();
            if (geodeticDatumList == null || geodeticDatumList.Count < 1)
            {
                _logger.LogWarning("Impossible to fill default CartographicProjection: no GeodeticDatum in the GeodeticDatum microservice");
                return;
            }
            #region cartographic projections based on geodetic datum ED50
            GeodeticDatum? geodeticDatum = geodeticDatumList.FirstOrDefault<GeodeticDatum>(gd => !string.IsNullOrEmpty(gd.Name) &&
            gd.Name.ToLower().Contains("ed50") &&
            gd.Name.ToLower().Contains("norway") &&
            gd.Name.ToLower().Contains("finland"));
            if (geodeticDatum != null && geodeticDatum.MetaInfo != null && geodeticDatum.MetaInfo.ID != Guid.Empty)
            {
                #region Norway UTM 31 ED50
                Model.CartographicProjection norway_utm_31_ed50 = new()
                {
                    MetaInfo = new OSDC.DotnetLibraries.General.DataManagement.MetaInfo() { ID = new Guid("acf0f15f-8ed0-4030-ad9c-3e4e8d7cd29b") },
                    Name = "Norway UTM 31 ED50",
                    Description = "UTM Zone 31 with geodetic datum ED50 Norway, Finland",
                    CreationDate = DateTimeOffset.UtcNow,
                    LastModificationDate = DateTimeOffset.UtcNow,
                    ProjectionType = ProjectionType.UTM,
                    Zone = 31,
                    IsSouth = false,
                    GeodeticDatumID = geodeticDatum.MetaInfo.ID
                };
                cartographicProjections.Add(norway_utm_31_ed50);
                #endregion

                #region Norway UTM 32 ED50
                Model.CartographicProjection norway_utm_32_ed50 = new()
                {
                    MetaInfo = new OSDC.DotnetLibraries.General.DataManagement.MetaInfo() { ID = new Guid("90a59cb6-ea19-4eef-8aca-69af37c1acee") },
                    Name = "Norway UTM 32 ED50",
                    Description = "UTM Zone 32 with geodetic datum ED50 Norway, Finland",
                    CreationDate = DateTimeOffset.UtcNow,
                    LastModificationDate = DateTimeOffset.UtcNow,
                    ProjectionType = ProjectionType.UTM,
                    Zone = 32,
                    IsSouth = false,
                    GeodeticDatumID = geodeticDatum.MetaInfo.ID
                };
                cartographicProjections.Add(norway_utm_32_ed50);
                #endregion

                #region Norway UTM 33 ED50
                Model.CartographicProjection norway_utm_33_ed50 = new()
                {
                    MetaInfo = new OSDC.DotnetLibraries.General.DataManagement.MetaInfo() { ID = new Guid("b1706977-c475-4cd5-95a5-8ffa3a55886e") },
                    Name = "Norway UTM 33 ED50",
                    Description = "UTM Zone 33 with geodetic datum ED50 Norway, Finland",
                    CreationDate = DateTimeOffset.UtcNow,
                    LastModificationDate = DateTimeOffset.UtcNow,
                    ProjectionType = ProjectionType.UTM,
                    Zone = 33,
                    IsSouth = false,
                    GeodeticDatumID = geodeticDatum.MetaInfo.ID
                };
                cartographicProjections.Add(norway_utm_33_ed50);
                #endregion

                #region Norway UTM 34 ED50
                Model.CartographicProjection norway_utm_34_ed50 = new()
                {
                    MetaInfo = new OSDC.DotnetLibraries.General.DataManagement.MetaInfo() { ID = new Guid("1dc6d203-a644-4a21-8270-a6a6dcfdf067") },
                    Name = "Norway UTM 34 ED50",
                    Description = "UTM Zone 34 with geodetic datum ED50 Norway, Finland",
                    CreationDate = DateTimeOffset.UtcNow,
                    LastModificationDate = DateTimeOffset.UtcNow,
                    ProjectionType = ProjectionType.UTM,
                    Zone = 34,
                    IsSouth = false,
                    GeodeticDatumID = geodeticDatum.MetaInfo.ID
                };
                cartographicProjections.Add(norway_utm_34_ed50);
                #endregion

                #region Norway UTM 35 ED50
                Model.CartographicProjection norway_utm_35_ed50 = new()
                {
                    MetaInfo = new OSDC.DotnetLibraries.General.DataManagement.MetaInfo() { ID = new Guid("563cc801-cc65-4ec3-a30f-abf632e5e274") },
                    Name = "Norway UTM 35 ED50",
                    Description = "UTM Zone 35 with geodetic datum ED50 Norway, Finland",
                    CreationDate = DateTimeOffset.UtcNow,
                    LastModificationDate = DateTimeOffset.UtcNow,
                    ProjectionType = ProjectionType.UTM,
                    Zone = 35,
                    IsSouth = false,
                    GeodeticDatumID = geodeticDatum.MetaInfo.ID
                };
                cartographicProjections.Add(norway_utm_35_ed50);
                #endregion
            }
            #endregion

            #region cartographic projections based on geodetic datum European 1950 MEAN FOR Austria Denmark...
            geodeticDatum = geodeticDatumList.FirstOrDefault<GeodeticDatum>(gd => !string.IsNullOrEmpty(gd.Name) &&
                        gd.Name.ToLower().Contains("european 1950") &&
                        gd.Name.ToLower().Contains("austria") &&
                        gd.Name.ToLower().Contains("belgium"));
            if (geodeticDatum != null && geodeticDatum.MetaInfo != null && geodeticDatum.MetaInfo.ID != Guid.Empty)
            {
                Model.CartographicProjection norway_utm_31_ed50 = new()
                {
                    MetaInfo = new OSDC.DotnetLibraries.General.DataManagement.MetaInfo() { ID = new Guid("c6b47745-ff91-4804-b256-6198df5a92d0") },
                    Name = "Norway UTM 31 European 1950",
                    Description = "European 1950 \"MEAN FOR Austria; Denmark; France; W Germany; Netherlands; Switzerland",
                    CreationDate = DateTimeOffset.UtcNow,
                    LastModificationDate = DateTimeOffset.UtcNow,
                    ProjectionType = ProjectionType.UTM,
                    Zone = 31,
                    IsSouth = false,
                    GeodeticDatumID = geodeticDatum.MetaInfo.ID
                };
                cartographicProjections.Add(norway_utm_31_ed50);
            }
            #endregion
            foreach (Model.CartographicProjection? cp in cartographicProjections)
            {
                AddCartographicProjection(cp);
            }
        }
    }
}