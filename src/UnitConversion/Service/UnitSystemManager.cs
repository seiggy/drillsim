using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering;

namespace OSDC.UnitConversion.Service
{
    /// <summary>
    /// A manager for UnitSystem. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class UnitSystemManager
    {
        private static UnitSystemManager? _instance = null;
        private readonly ILogger _logger;
        private static readonly object _lock = new();
        private readonly SqlConnectionManager _connectionManager;

        private UnitSystemManager(ILogger<UnitSystemManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;

            // make sure database contains default UnitSystems
            List<Guid>? ids = GetAllUnitSystemId();
            if (ids != null && ids.Count < 4)
            {
                FillDefault();
            }
        }

        public static UnitSystemManager GetInstance(ILogger<UnitSystemManager> logger, SqlConnectionManager connectionManager)
        {
            lock (_lock)
            {
                _instance ??= new UnitSystemManager(logger, connectionManager);
            }
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
                    command.CommandText = "SELECT COUNT(*) FROM UnitSystemTable";
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
                        _logger.LogError(ex, "Impossible to count records in the UnitSystemTable");
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
                        //empty UnitSystemTable
                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM UnitSystemTable";
                        command.ExecuteNonQuery();

                        transaction.Commit();
                        success = true;
                    }
                    catch (SqliteException ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Impossible to clear the UnitSystemTable");
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
                command.CommandText = $"SELECT COUNT(*) FROM UnitSystemTable WHERE ID = ' {guid}'";
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
                    _logger.LogError(ex, "Impossible to count rows from UnitSystemTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all UnitSystem present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all UnitSystem present in the microservice database</returns>
        public List<Guid>? GetAllUnitSystemId()
        {
            List<Guid> ids = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM UnitSystemTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from UnitSystemTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from UnitSystemTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns a UnitSystem identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the UnitSystem retrieved from the database</returns>
        public DrillingUnitSystem? GetUnitSystemById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    DrillingUnitSystem? unitSystem;
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT UnitSystem FROM UnitSystemTable WHERE ID = '{guid}'";
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            unitSystem = JsonSerializer.Deserialize<DrillingUnitSystem>(data);
                            if (unitSystem != null && !unitSystem.ID.Equals(guid))
                                throw (new SqliteException("SQLite database corrupted: retrieved UnitSystem is null or has been jsonified with the wrong ID.", 1));
                        }
                        else
                        {
                            _logger.LogInformation("No UnitSystem of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the UnitSystem with the given ID from UnitSystemTable");
                        return null;
                    }

                    // Finalizing
                    _logger.LogInformation("Returning the UnitSystem of given ID from UnitSystemTable");
                    return unitSystem;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given UnitSystem ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all UnitSystem present in the microservice database 
        /// </summary>
        /// <returns>the list of all UnitSystem present in the microservice database</returns>
        public List<DrillingUnitSystem?>? GetAllUnitSystem()
        {
            List<DrillingUnitSystem?> vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT UnitSystem FROM UnitSystemTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        DrillingUnitSystem? unitSystem = JsonSerializer.Deserialize<DrillingUnitSystem>(data);
                        vals.Add(unitSystem);
                    }
                    _logger.LogInformation("Returning the list of existing UnitSystem from UnitSystemTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get UnitSystem from UnitSystemTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all UnitSystemLight present in the microservice database 
        /// </summary>
        /// <returns>the list of all UnitSystemLight present in the microservice database</returns>
        public List<UnitSystemLight>? GetAllUnitSystemLight()
        {
            List<UnitSystemLight>? vals = [];
            var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID, Name, Description, IsDefault, IsSI FROM UnitSystemTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        string name = reader.GetString(1);
                        string descr = reader.GetString(2);
                        bool isDef = bool.Parse(reader.GetString(3));
                        bool si = bool.Parse(reader.GetString(4));
                        UnitSystemLight unitSystemLight = new()
                        {
                            ID = id,
                            Name = name,
                            Description = descr,
                            IsDefault = isDef,
                            IsSI = si
                        };
                        vals.Add(unitSystemLight);
                    }
                    _logger.LogInformation("Returning the list of existing UnitSystemLight from UnitSystemTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get UnitSystemLight from UnitSystemTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Adds the given UnitSystem to the microservice database
        /// </summary>
        /// <param name="unitSystem"></param>
        /// <returns>true if the given UnitSystem has been added successfully</returns>
        public bool AddUnitSystem(DrillingUnitSystem unitSystem)
        {
            if (unitSystem != null && !unitSystem.ID.Equals(Guid.Empty))
            {
                //update UnitSystemTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    bool success = true;
                    try
                    {
                        //add the UnitSystem to the UnitSystemTable
                        string data = JsonSerializer.Serialize(unitSystem);
                        var command = connection.CreateCommand();
                        command.CommandText = "INSERT INTO UnitSystemTable " +
                            "(ID, " +
                            "Name, " +
                            "Description, " +
                            "IsDefault, " +
                            "IsSI, " +
                            "UnitSystem" +
                            ") VALUES (" +
                            $"'{unitSystem.ID}', " +
                            $"'{unitSystem.Name}', " +
                            $"'{unitSystem.Description}', " +
                            $"'{unitSystem.IsDefault}', " +
                            $"'{unitSystem.IsSI}', " +
                            $"'{data}'" +
                            ")";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to insert the given UnitSystem into the UnitSystemTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to add the given UnitSystem into UnitSystemTable");
                        success = false;
                    }
                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Added the given UnitSystem of given ID into the UnitSystemTable successfully");
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
                _logger.LogWarning("The UnitSystem ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given UnitSystem and updates it in the microservice database
        /// </summary>
        /// <param name="unitSystem"></param>
        /// <returns>true if the given UnitSystem has been updated successfully</returns>
        public bool UpdateUnitSystemById(Guid guid, DrillingUnitSystem unitSystem)
        {
            bool success = true;
            if (!guid.Equals(Guid.Empty) && unitSystem != null && unitSystem.ID.Equals(guid))
            {
                //update UnitSystemTable
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in UnitSystemTable
                    try
                    {
                        string data = JsonSerializer.Serialize(unitSystem);
                        var command = connection.CreateCommand();
                        command.CommandText = $"UPDATE UnitSystemTable SET " +
                            $"Name = '{unitSystem.Name}', " +
                            $"Description = '{unitSystem.Description}', " +
                            $"IsDefault = '{unitSystem.IsDefault}', " +
                            $"IsSI = '{unitSystem.IsSI}', " +
                            $"UnitSystem = '{data}' " +
                            $"WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the UnitSystem");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the UnitSystem");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given UnitSystem successfully");
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
                _logger.LogWarning("The UnitSystem ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Deletes the UnitSystem of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the UnitSystem was deleted from the microservice database</returns>
        public bool DeleteUnitSystemById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete UnitSystem from UnitSystemTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM UnitSystemTable WHERE ID = '{guid}'";
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the UnitSystem of given ID from the UnitSystemTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the UnitSystem of given ID from UnitSystemTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the UnitSystem of given ID from the UnitSystemTable successfully");
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
                _logger.LogWarning("The UnitSystem ID is null or empty");
            }
            return false;
        }

        /// <summary>
        /// populate database with default UnitSystems
        /// </summary>
        private void FillDefault()
        {
            List<DrillingUnitSystem> unitSystems =
            [
                DrillingUnitSystem.SIUnitSystem,
                DrillingUnitSystem.MetricUnitSystem,
                DrillingUnitSystem.USUnitSystem,
                DrillingUnitSystem.ImperialUnitSystem
            ];


            foreach (DrillingUnitSystem uSys in unitSystems)
            {
                if (GetUnitSystemById(uSys.ID) == null)
                {
                    AddUnitSystem(uSys);
                }
                else
                {
                    UpdateUnitSystemById(uSys.ID, uSys);
                }
            }
        }
    }
}