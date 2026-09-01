using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using NORCE.Drilling.CartographicProjection.ModelShared;
using NORCE.Drilling.CartographicProjection.Model;
using System.Linq;
using System.Threading.Tasks;

namespace NORCE.Drilling.CartographicProjection.Service.Managers
{
    /// <summary>
    /// A manager for CartographicConversionSet. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class CartographicConversionSetManager
    {
        private static CartographicConversionSetManager? _instance = null;
        private readonly ILogger<CartographicConversionSetManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        private CartographicConversionSetManager(ILogger<CartographicConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static CartographicConversionSetManager GetInstance(ILogger<CartographicConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new CartographicConversionSetManager(logger, connectionManager);
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
                    command.CommandText = "SELECT COUNT(*) FROM CartographicConversionSetTable";
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
                        _logger.LogError(ex, "Impossible to count records in the CartographicConversionSetTable");
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
                    //empty CartographicConversionSetTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM CartographicConversionSetTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the CartographicConversionSetTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM CartographicConversionSetTable WHERE ID = '{guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from CartographicConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all CartographicConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all CartographicConversionSet present in the microservice database</returns>
        public List<Guid>? GetAllCartographicConversionSetId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM CartographicConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from CartographicConversionSetTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from CartographicConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all CartographicConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all CartographicConversionSet present in the microservice database</returns>
        public List<ModelShared.MetaInfo?>? GetAllCartographicConversionSetMetaInfo()
        {
            List<ModelShared.MetaInfo?> metaInfos = new();
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM CartographicConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        ModelShared.MetaInfo? metaInfo = JsonSerializer.Deserialize<ModelShared.MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from CartographicConversionSetTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from CartographicConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the CartographicConversionSet identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the CartographicConversionSet identified by its Guid from the microservice database</returns>
        public Model.CartographicConversionSet? GetCartographicConversionSetById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.CartographicConversionSet? cartographicConversionSet;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT CartographicConversionSet FROM CartographicConversionSetTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            cartographicConversionSet = JsonSerializer.Deserialize<Model.CartographicConversionSet>(data, JsonSettings.Options);
                            if (cartographicConversionSet != null && cartographicConversionSet.MetaInfo != null && !cartographicConversionSet.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: returned CartographicConversionSet is null or has been jsonified with the wrong ID.", 1);
                        }
                        else
                        {
                            _logger.LogInformation("No CartographicConversionSet of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the CartographicConversionSet with the given ID from CartographicConversionSetTable");
                        return null;
                    }
                    _logger.LogInformation("Returning the CartographicConversionSet of given ID from CartographicConversionSetTable");
                    return cartographicConversionSet;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given CartographicConversionSet ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all CartographicConversionSet present in the microservice database 
        /// </summary>
        /// <returns>the list of all CartographicConversionSet present in the microservice database</returns>
        public List<Model.CartographicConversionSet?>? GetAllCartographicConversionSet()
        {
            List<Model.CartographicConversionSet?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT CartographicConversionSet FROM CartographicConversionSetTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.CartographicConversionSet? cartographicConversionSet = JsonSerializer.Deserialize<Model.CartographicConversionSet>(data, JsonSettings.Options);
                        vals.Add(cartographicConversionSet);
                    }
                    _logger.LogInformation("Returning the list of existing CartographicConversionSet from CartographicConversionSetTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get CartographicConversionSet from CartographicConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all CartographicConversionSetLight present in the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the list of CartographicConversionSetLight present in the microservice database</returns>
        public List<Model.CartographicConversionSetLight>? GetAllCartographicConversionSetLight()
        {
            List<Model.CartographicConversionSetLight>? cartographicConversionSetLightList = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo, Name, Description, CreationDate, LastModificationDate, CartographicProjectionName, CartographicProjectionDescription FROM CartographicConversionSetTable";
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
                        string cartographicProjectionName = reader.GetString(5);
                        string cartographicProjectionDescr = reader.GetString(6);
                        cartographicConversionSetLightList.Add(new Model.CartographicConversionSetLight(
                                metaInfo,
                                string.IsNullOrEmpty(name) ? null : name,
                                string.IsNullOrEmpty(descr) ? null : descr,
                                creationDate,
                                lastModificationDate,
                                cartographicProjectionName,
                                cartographicProjectionDescr));
                    }
                    _logger.LogInformation("Returning the list of existing CartographicConversionSetLight from CartographicConversionSetTable");
                    return cartographicConversionSetLightList;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get light datas from CartographicConversionSetTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Performs calculation on the given CartographicConversionSet and adds it to the microservice database
        /// </summary>
        /// <param name="cartographicConversionSet"></param>
        /// <returns>true if the given CartographicConversionSet has been added successfully to the microservice database</returns>
        public async Task<bool> AddCartographicConversionSet(Model.CartographicConversionSet? cartographicConversionSet)
        {
            if (cartographicConversionSet != null && cartographicConversionSet.MetaInfo != null && cartographicConversionSet.MetaInfo.ID != Guid.Empty)
            {
                // calculate outputs: part of the computation is run through CartographicProjection µS, part through GeodeticDatum µS
                cartographicConversionSet = await ManageCartographicConversionSetAsync(cartographicConversionSet);
                if (cartographicConversionSet == null || cartographicConversionSet.MetaInfo == null)
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given CartographicConversionSet");
                    return false;
                }
                //if successful, check if another parent data with the same ID was calculated/added during the calculation time
                Model.CartographicConversionSet? newCartographicConversionSet = GetCartographicConversionSetById(cartographicConversionSet.MetaInfo.ID);
                if (newCartographicConversionSet == null)
                {
                    Model.CartographicProjection? cartographicProjection = null;
                    if (CartographicProjectionManager.Instance != null && cartographicConversionSet.CartographicProjectionID != null)
                    {
                        cartographicProjection = CartographicProjectionManager.Instance.GetCartographicProjectionById(cartographicConversionSet.CartographicProjectionID.Value);
                    }
                    if (cartographicProjection == null)
                    {
                        _logger.LogWarning("Impossible to get the CartographicProjection of given ID from CartographicProjectionManager");
                        return false;
                    }
                    //update CartographicConversionSetTable
                    var connection = _connectionManager.GetConnection();
                    if (connection != null)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        bool success = true;
                        try
                        {
                            //add the CartographicConversionSet to the CartographicConversionSetTable
                            string metaInfo = JsonSerializer.Serialize(cartographicConversionSet.MetaInfo, JsonSettings.Options);
                            string? cDate = null;
                            if (cartographicConversionSet.CreationDate != null)
                                cDate = ((DateTimeOffset)cartographicConversionSet.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            cartographicConversionSet.LastModificationDate = DateTimeOffset.UtcNow;
                            string? lDate = ((DateTimeOffset)cartographicConversionSet.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string data = JsonSerializer.Serialize(cartographicConversionSet, JsonSettings.Options);
                            var command = connection.CreateCommand();
                            command.CommandText = "INSERT INTO CartographicConversionSetTable (" +
                               "ID, " +
                               "MetaInfo, " +
                               "Name, " +
                               "Description, " +
                               "CreationDate, " +
                               "LastModificationDate, " +
                               "CartographicProjectionName, " +
                               "CartographicProjectionDescription, " +
                               "CartographicConversionSet" +
                               ") VALUES (" +
                               $"'{cartographicConversionSet!.MetaInfo!.ID}', " +
                               $"'{metaInfo}', " +
                               $"'{cartographicConversionSet.Name}', " +
                               $"'{cartographicConversionSet.Description}', " +
                               $"'{cDate}', " +
                               $"'{lDate}', " +
                               $"'{cartographicProjection.Name}', " +
                               $"'{cartographicProjection.Description}', " +
                               $"'{data}'" +
                               ")";
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to insert the given CartographicConversionSet into the CartographicConversionSetTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to add the given CartographicConversionSet into CartographicConversionSetTable");
                            success = false;
                        }
                        //finalizing SQL transaction
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Added the given CartographicConversionSet of given ID into the CartographicConversionSetTable successfully");
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
                    _logger.LogWarning("Impossible to post CartographicConversionSet. ID already found in database.");
                    return false;
                }

            }
            else
            {
                _logger.LogWarning("The CartographicConversionSet ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given CartographicConversionSet and updates it in the microservice database
        /// </summary>
        /// <param name="cartographicConversionSet"></param>
        /// <returns>true if the given CartographicConversionSet has been updated successfully</returns>
        public async Task<bool> UpdateCartographicConversionSetById(Guid guid, Model.CartographicConversionSet? cartographicConversionSet)
        {
            bool success = true;
            if (guid != Guid.Empty && cartographicConversionSet != null && cartographicConversionSet.MetaInfo != null && cartographicConversionSet.MetaInfo.ID == guid)
            {
                // calculate outputs: part of the computation is run through CartographicProjection µS, part through GeodeticDatum µS
                cartographicConversionSet = await ManageCartographicConversionSetAsync(cartographicConversionSet);
                if (cartographicConversionSet == null || cartographicConversionSet.MetaInfo == null)
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given CartographicConversionSet");
                    return false;
                }
                Model.CartographicProjection? cartographicProjection = null;
                if (CartographicProjectionManager.Instance != null && cartographicConversionSet.CartographicProjectionID != null)
                {
                    cartographicProjection = CartographicProjectionManager.Instance.GetCartographicProjectionById(cartographicConversionSet.CartographicProjectionID.Value);
                }
                if (cartographicConversionSet == null)
                {
                    _logger.LogWarning("Impossible to get the CartographicProjection of given ID from CartographicProjectionManager");
                    return false;
                }
                //update CartographicConversionSetTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in CartographicConversionSetTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(cartographicConversionSet.MetaInfo, JsonSettings.Options);
                        string? cDate = null;
                        if (cartographicConversionSet.CreationDate != null)
                            cDate = ((DateTimeOffset)cartographicConversionSet.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        cartographicConversionSet.LastModificationDate = DateTimeOffset.UtcNow;
                        string? lDate = ((DateTimeOffset)cartographicConversionSet.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        string data = JsonSerializer.Serialize(cartographicConversionSet, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE CartographicConversionSetTable SET " +
                            $"MetaInfo = '{metaInfo}', " +
                            $"Name = '{cartographicConversionSet.Name}', " +
                            $"Description = '{cartographicConversionSet.Description}', " +
                            $"CartographicConversionSet = '{data}', " +
                            $"CreationDate = '{cDate}', " +
                            $"LastModificationDate = '{lDate}', " +
                            $"CartographicProjectionName = '{cartographicProjection!.Name}', " +
                            $"CartographicProjectionDescription = '{cartographicProjection.Description}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the CartographicConversionSet");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the CartographicConversionSet");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given CartographicConversionSet successfully");
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
                _logger.LogWarning("The CartographicConversionSet ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the CartographicConversionSet of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the CartographicConversionSet was deleted from the microservice database</returns>
        public bool DeleteCartographicConversionSetById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete CartographicConversionSet from CartographicConversionSetTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM CartographicConversionSetTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the CartographicConversionSet of given ID from the CartographicConversionSetTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the CartographicConversionSet of given ID from CartographicConversionSetTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the CartographicConversionSet of given ID from the CartographicConversionSetTable successfully");
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
                _logger.LogWarning("The CartographicConversionSet ID is null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given CartographicConversionSet by running through CartographicProjection µS (projections) and through GeodeticDatum µS (transformations)
        /// - cartographic conversion set is separated into 2 subsets depending on user's inputs (from cartographic coordinates, or from geodetic coordinates)
        /// - if cartographic coordinates (Northing, Easting, TVD) are provided
        ///     - then de-projection to the geodetic coordinates relative to the reference geodetic datum is done first
        ///     - then transformation from resulting geodetic coordinates to other geodetic coordinates (WGS84, octree code) are performed
        /// - if geodetic coordinates (datum, WGS84, octree code) are provided
        ///     - then transformation from whichever is set to the others is done first
        ///     - then projection to cartographic coordinates is performed
        /// Geodetic coordinates transformation call is implemented two ways:
        /// - best practice: call to GeodeticDatum µS (requires GeodeticDatum µS dependency)
        /// - best performance: direct call to GeodeticDatum.Model calculation method (requires GeodeticDatum.Model nuget install)
        /// </summary>
        /// <param name="cartographicConversionSet"></param>
        /// <returns>true if the given CartographicConversionSet has been updated successfully</returns>
        private async Task<Model.CartographicConversionSet?> ManageCartographicConversionSetAsync(Model.CartographicConversionSet? cartographicConversionSet)
        {
            if (cartographicConversionSet != null && cartographicConversionSet.CartographicProjectionID != null && 
                cartographicConversionSet.CartographicCoordinateList != null && cartographicConversionSet.CartographicCoordinateList.Count != 0)
            {
                Model.CartographicProjection? cartographicProjection = null;
                if (CartographicProjectionManager.Instance != null)
                {
                    cartographicProjection = CartographicProjectionManager.Instance.GetCartographicProjectionById(cartographicConversionSet.CartographicProjectionID.Value);
                }
                if (cartographicProjection == null || cartographicProjection.GeodeticDatumID == null || cartographicProjection.GeodeticDatumID == Guid.Empty)
                {
                    _logger.LogWarning("Impossible to get the CartographicProjection of given ID from CartographicProjection microservice");
                    return null;
                }
                GeodeticDatum geodeticDatum = await APIUtils.ClientGeodeticDatum.GetGeodeticDatumByIdAsync(cartographicProjection.GeodeticDatumID!.Value);
                if (geodeticDatum == null)
                {
                    _logger.LogWarning("Impossible to get the GeodeticDatum of given ID from GeodeticDatum microservice");
                    return null;
                }
                #region split coordinates
                // split the list of cartographic coordinates into those set from cartographic coordinates (Northing, Easting, TVD) and those set from geodetic coordinates
                List<CartographicCoordinate> cartographicValueList = [];
                List<CartographicCoordinate> geodeticValueList = [];
                // keep track of indices to eventually consolidate the given cartographic conversion set
                List<int> cartographicIndexList = [];
                List<int> geodeticIndexList = [];
                for (int i = 0; i < cartographicConversionSet.CartographicCoordinateList.Count; ++i)
                {
                    CartographicCoordinate coord = cartographicConversionSet.CartographicCoordinateList[i];
                    if (coord != null)
                    {
                        if (coord.Northing != null && coord.Easting != null && coord.VerticalDepth != null)
                        {
                            cartographicValueList.Add(coord);
                            cartographicIndexList.Add(i);
                        }
                        else if (coord.GeodeticCoordinate != null && (
                            (coord.GeodeticCoordinate.LatitudeDatum != null && coord.GeodeticCoordinate.LongitudeDatum != null && coord.GeodeticCoordinate.VerticalDepthDatum != null) ||
                            (coord.GeodeticCoordinate.LatitudeWGS84 != null && coord.GeodeticCoordinate.LongitudeWGS84 != null && coord.GeodeticCoordinate.VerticalDepthWGS84 != null) ||
                            (coord.GeodeticCoordinate.OctreeCode != null && coord.GeodeticCoordinate.OctreeDepth > 0)))
                        {
                            geodeticValueList.Add(coord);
                            geodeticIndexList.Add(i);
                        }
                        else
                        {
                            _logger.LogWarning("Impossible to update the given CartographicConversionSet for its cartographic coordinates are corrupted");
                            return null;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Impossible to update the given CartographicConversionSet for its cartographic coordinates are corrupted");
                        return null;
                    }
                }
                #endregion

                GeodeticConversionSet? gdcsCarto = null;

                #region processing the cartographic coordinates based subset
                if (cartographicValueList.Count != 0)
                {
                    #region run CartographicProjection computations
                    // assemble a cartographic conversion subset from cartographic coordinate values
                    Model.CartographicConversionSet? cartographicConversionSubSetCarto = new()
                    {
                        CartographicProjectionID = cartographicConversionSet.CartographicProjectionID,
                        CartographicCoordinateList = cartographicValueList
                    };
                    if (!cartographicConversionSubSetCarto.CalculateProjection(cartographicProjection, geodeticDatum))
                    {
                        _logger.LogWarning("Impossible to calculate outputs for the cartographic conversion subset based on cartographic coordinates");
                        return null;
                    }
                    // assemble cartographic coordinates into geodetic coordinates
                    List<GeodeticCoordinate> geodeticCoordinatesCarto = [];
                    foreach (var coordinate in cartographicConversionSubSetCarto.CartographicCoordinateList)
                    {
                        geodeticCoordinatesCarto.Add(new GeodeticCoordinate()
                        {
                            LatitudeDatum = coordinate.GeodeticCoordinate!.LatitudeDatum,
                            LongitudeDatum = coordinate.GeodeticCoordinate!.LongitudeDatum,
                            VerticalDepthDatum = coordinate.GeodeticCoordinate!.VerticalDepthDatum,
                            LatitudeWGS84 = coordinate.GeodeticCoordinate!.LatitudeWGS84,
                            LongitudeWGS84 = coordinate.GeodeticCoordinate!.LongitudeWGS84,
                            VerticalDepthWGS84 = coordinate.GeodeticCoordinate!.VerticalDepthWGS84,
                            OctreeCode = coordinate.GeodeticCoordinate!.OctreeCode,
                            OctreeDepth = coordinate.GeodeticCoordinate!.OctreeDepth
                        });
                    }
                    // create a geodetic datum conversion set to compute those geodetic coordinates that are still not defined
                    Guid guidCarto = Guid.NewGuid();
                    GeodeticConversionSet geodeticConversionSubSetCarto = new()
                    {
                        MetaInfo = new ModelShared.MetaInfo() { ID = guidCarto },
                        GeodeticDatum = geodeticDatum,
                        GeodeticCoordinates = geodeticCoordinatesCarto
                    };
                    #endregion
                    #region run GeodeticDatum computations
                    // Post the geodetic datum conversion set
                    await APIUtils.ClientGeodeticDatum.PostGeodeticConversionSetAsync(geodeticConversionSubSetCarto);
                    double timeOutInSecond = 5.0;
                    double retryAfterInMillisecond = 200;
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    // Get the computed geodetic datum conversion set, until its geodetic coordinates are completely set
                    while (watch.ElapsedMilliseconds < (long)(timeOutInSecond * 1000))
                    {
                        gdcsCarto = await APIUtils.ClientGeodeticDatum.GetGeodeticConversionSetByIdAsync(guidCarto);
                        if (gdcsCarto != null)
                        {
                            if (gdcsCarto.GeodeticCoordinates != null && gdcsCarto.GeodeticCoordinates.Count != 0 &&
                                gdcsCarto.GeodeticCoordinates.ElementAt(0).LatitudeDatum != null && gdcsCarto.GeodeticCoordinates.ElementAt(0).LongitudeDatum != null && gdcsCarto.GeodeticCoordinates.ElementAt(0).VerticalDepthDatum != null &&
                                gdcsCarto.GeodeticCoordinates.ElementAt(0).LatitudeWGS84 != null && gdcsCarto.GeodeticCoordinates.ElementAt(0).LongitudeWGS84 != null && gdcsCarto.GeodeticCoordinates.ElementAt(0).VerticalDepthWGS84 != null &&
                                gdcsCarto.GeodeticCoordinates.ElementAt(0).OctreeCode != null && gdcsCarto.GeodeticCoordinates.ElementAt(0).OctreeCode.CodeHigh > 0 && gdcsCarto.GeodeticCoordinates.ElementAt(0).OctreeDepth > 0)
                            {
                                _logger.LogInformation("Geodetic coordinates conversion completed");
                                await APIUtils.ClientGeodeticDatum.DeleteGeodeticConversionSetByIdAsync(guidCarto);
                                break;
                            }
                            else
                            {
                                _logger.LogWarning("Retrying after {retryAfter}ms", retryAfterInMillisecond);
                                await Task.Delay((int)(retryAfterInMillisecond));
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Impossible to retrieve for GeodeticConversionSet from GeodeticDatum µS, retrying after {retryAfter}ms", retryAfterInMillisecond);
                            await Task.Delay((int)(retryAfterInMillisecond));
                        }
                    }
                    watch.Stop();

                    #endregion
                }
                #endregion

                Model.CartographicConversionSet? ccsgeod = null;

                #region processing the geodetic coordinates based subset
                if (geodeticValueList.Count != 0)
                {
                    #region assemble cartographic coordinates into geodetic coordinates
                    List<GeodeticCoordinate> geodeticCoordinatesGeod = [];
                    foreach (var coordinate in geodeticValueList)
                    {
                        geodeticCoordinatesGeod.Add(new GeodeticCoordinate()
                        {
                            LatitudeDatum = coordinate.GeodeticCoordinate!.LatitudeDatum,
                            LongitudeDatum = coordinate.GeodeticCoordinate!.LongitudeDatum,
                            VerticalDepthDatum = coordinate.GeodeticCoordinate!.VerticalDepthDatum,
                            LatitudeWGS84 = coordinate.GeodeticCoordinate!.LatitudeWGS84,
                            LongitudeWGS84 = coordinate.GeodeticCoordinate!.LongitudeWGS84,
                            VerticalDepthWGS84 = coordinate.GeodeticCoordinate!.VerticalDepthWGS84,
                            OctreeCode = coordinate.GeodeticCoordinate!.OctreeCode,
                            OctreeDepth = coordinate.GeodeticCoordinate!.OctreeDepth
                        });
                    }
                    // create a geodetic datum conversion set to compute those geodetic coordinates that are still not defined
                    Guid guidGeod = Guid.NewGuid();
                    GeodeticConversionSet geodeticConversionSubSetGeod = new()
                    {
                        MetaInfo = new ModelShared.MetaInfo() { ID = guidGeod },
                        Name = "CartographicProjection",
                        Description = "CartographicConversionSet calculation sent from CartographicProjection µS",
                        CreationDate = DateTimeOffset.UtcNow,
                        GeodeticDatum = geodeticDatum,
                        GeodeticCoordinates = geodeticCoordinatesGeod
                    };
                    #endregion

                    #region run GeodeticDatum computations
                    // Post the geodetic datum conversion set
                    GeodeticConversionSet? gdcsGeod = null;
                    await APIUtils.ClientGeodeticDatum.PostGeodeticConversionSetAsync(geodeticConversionSubSetGeod);
                    double timeOutInSecondGeod = 5.0;
                    double retryAfterInMillisecondGeod = 200;
                    var watchGeod = System.Diagnostics.Stopwatch.StartNew();
                    // Get the computed geodetic datum conversion set, until its geodetic coordinates are completely set
                    while (watchGeod.ElapsedMilliseconds < (long)(timeOutInSecondGeod * 1000))
                    {
                        gdcsGeod = await APIUtils.ClientGeodeticDatum.GetGeodeticConversionSetByIdAsync(guidGeod);
                        if (gdcsGeod != null)
                        {
                            if (gdcsGeod.GeodeticCoordinates != null && gdcsGeod.GeodeticCoordinates.Count != 0 &&
                                gdcsGeod.GeodeticCoordinates.ElementAt(0).LatitudeDatum != null && gdcsGeod.GeodeticCoordinates.ElementAt(0).LongitudeDatum != null && gdcsGeod.GeodeticCoordinates.ElementAt(0).VerticalDepthDatum != null &&
                                gdcsGeod.GeodeticCoordinates.ElementAt(0).LatitudeWGS84 != null && gdcsGeod.GeodeticCoordinates.ElementAt(0).LongitudeWGS84 != null && gdcsGeod.GeodeticCoordinates.ElementAt(0).VerticalDepthWGS84 != null &&
                                gdcsGeod.GeodeticCoordinates.ElementAt(0).OctreeCode != null && gdcsGeod.GeodeticCoordinates.ElementAt(0).OctreeCode.CodeHigh > 0 && gdcsGeod.GeodeticCoordinates.ElementAt(0).OctreeDepth > 0)
                            {
                                _logger.LogInformation("Geodetic coordinates conversion completed");
                                await APIUtils.ClientGeodeticDatum.DeleteGeodeticConversionSetByIdAsync(guidGeod);
                                break;
                            }
                            else
                            {
                                _logger.LogWarning("Retrying after {retryAfter}ms", retryAfterInMillisecondGeod);
                                await Task.Delay((int)(retryAfterInMillisecondGeod));
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Impossible to retrieve for GeodeticConversionSet from GeodeticDatum µS, retrying after {retryAfter}ms", retryAfterInMillisecondGeod);
                            await Task.Delay((int)(retryAfterInMillisecondGeod));
                        }
                    }
                    watchGeod.Stop();

                    #endregion

                    #region run CartographicProjection computations
                    if (gdcsGeod != null && gdcsGeod.GeodeticCoordinates != null && gdcsGeod.GeodeticCoordinates.Count != 0)
                    {
                        // assemble a cartographic conversion subset from geodetic coordinate values
                        List<int> idxList = Enumerable.Range(0, gdcsGeod.GeodeticCoordinates.Count).ToList();
                        List<CartographicCoordinate> cartoCoordList = [];
                        cartoCoordList.AddRange(from int i in idxList
                                                select new CartographicCoordinate());
                        ccsgeod = new()
                        {
                            CartographicProjectionID = cartographicConversionSet.CartographicProjectionID,
                            CartographicCoordinateList = cartoCoordList
                        };
                        SetGeodeticCoordinate(ref ccsgeod, gdcsGeod, idxList);
                        if (ccsgeod == null || !ccsgeod.CalculateProjection(cartographicProjection, geodeticDatum))
                        {
                            _logger.LogWarning("Impossible to calculate outputs for the cartographic conversion subset based on geodetic coordinates");
                            return null;
                        }
                    }
                    #endregion
                }
                #endregion

                #region consolidating results
                SetGeodeticCoordinate(ref cartographicConversionSet, gdcsCarto, cartographicIndexList);
                SetCartographicCoordinate(ref cartographicConversionSet, ccsgeod, geodeticIndexList);
                #endregion
            }
            return cartographicConversionSet;
        }

        private static void SetGeodeticCoordinate(ref Model.CartographicConversionSet? ccs, GeodeticConversionSet? gdcs, List<int> idxList)
        {
            if (ccs != null && ccs.CartographicCoordinateList != null && gdcs != null && gdcs.GeodeticCoordinates != null && gdcs.GeodeticCoordinates.Count != 0)
            {
                for (int i = 0; i < idxList.Count; ++i)
                {
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate ??= new GeodeticCoordinate();
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.LatitudeWGS84 = gdcs.GeodeticCoordinates.ElementAt(i).LatitudeWGS84;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.LongitudeWGS84 = gdcs.GeodeticCoordinates.ElementAt(i).LongitudeWGS84;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.VerticalDepthWGS84 = gdcs.GeodeticCoordinates.ElementAt(i).VerticalDepthWGS84;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.LatitudeDatum = gdcs.GeodeticCoordinates.ElementAt(i).LatitudeDatum;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.LongitudeDatum = gdcs.GeodeticCoordinates.ElementAt(i).LongitudeDatum;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.VerticalDepthDatum = gdcs.GeodeticCoordinates.ElementAt(i).VerticalDepthDatum;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.OctreeCode ??= new OctreeCodeLong();
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.OctreeCode = gdcs.GeodeticCoordinates.ElementAt(i).OctreeCode;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.OctreeDepth = gdcs.GeodeticCoordinates.ElementAt(i).OctreeDepth;
                }
            }
        }

        private static void SetCartographicCoordinate(ref Model.CartographicConversionSet? ccs, Model.CartographicConversionSet? ccsubSet, List<int> idxList)
        {
            if (ccs != null && ccs.CartographicCoordinateList != null && ccsubSet != null && ccsubSet.CartographicCoordinateList != null && ccsubSet.CartographicCoordinateList.Count != 0)
            {
                for (int i = 0; i < idxList.Count; ++i)
                {
                    ccs.CartographicCoordinateList[idxList[i]].Northing = ccsubSet.CartographicCoordinateList[i].Northing;
                    ccs.CartographicCoordinateList[idxList[i]].Easting = ccsubSet.CartographicCoordinateList[i].Easting;
                    ccs.CartographicCoordinateList[idxList[i]].VerticalDepth = ccsubSet.CartographicCoordinateList[i].VerticalDepth;
                    ccs.CartographicCoordinateList[idxList[i]].GridConvergenceDatum = ccsubSet.CartographicCoordinateList[i].GridConvergenceDatum;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.LatitudeDatum = ccsubSet.CartographicCoordinateList[i].GeodeticCoordinate!.LatitudeDatum;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.LongitudeDatum = ccsubSet.CartographicCoordinateList[i].GeodeticCoordinate!.LongitudeDatum;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.VerticalDepthDatum = ccsubSet.CartographicCoordinateList[i].GeodeticCoordinate!.VerticalDepthDatum;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.LatitudeWGS84 = ccsubSet.CartographicCoordinateList[i].GeodeticCoordinate!.LatitudeWGS84;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.LongitudeWGS84 = ccsubSet.CartographicCoordinateList[i].GeodeticCoordinate!.LongitudeWGS84;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.VerticalDepthWGS84 = ccsubSet.CartographicCoordinateList[i].GeodeticCoordinate!.VerticalDepthWGS84;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.OctreeCode ??= new OctreeCodeLong();
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.OctreeCode = ccsubSet.CartographicCoordinateList[i].GeodeticCoordinate!.OctreeCode;
                    ccs.CartographicCoordinateList[idxList[i]].GeodeticCoordinate!.OctreeDepth = ccsubSet.CartographicCoordinateList[i].GeodeticCoordinate!.OctreeDepth;
                }
            }
        }
    }
}