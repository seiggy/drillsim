using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using DrillSim.PublicationGate;

namespace NORCE.Drilling.WellBoreArchitecture.Service.Managers
{
    public class DatabaseCleanerService : BackgroundService
    {
        private readonly ILogger<DatabaseCleanerService> _logger;
        private readonly SqlConnectionManager _connectionManager;
        private readonly ScenarioPublicationGateStore _publicationGate;
        private readonly string[] _dataTables = ["WellBoreArchitectureTable"];
        private static readonly TimeSpan _cleaningInterval = TimeSpan.FromDays(1);
        private static readonly TimeSpan _retirementTime = TimeSpan.FromDays(90);

        public DatabaseCleanerService(ILogger<DatabaseCleanerService> logger, SqlConnectionManager connectionManager,
            ScenarioPublicationGateStore publicationGate)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _publicationGate = publicationGate;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DatabaseCleanerService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (string table in _dataTables)
                    {
                        if (!await CleanDatabaseAsync(table, stoppingToken))
                            _logger.LogError("{table} was not cleaned successfully at time {time}", table, DateTimeOffset.UtcNow);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while cleaning the database");
                }
                await Task.Delay(_cleaningInterval, stoppingToken);
            }
            _logger.LogInformation("DatabaseCleanerService is stopping.");
        }

        /// <summary>
        /// Removes expired unregistered records. Scenario-owned records are never retired.
        /// </summary>
        /// <returns>true if older MyBaseData were successfully deleted</returns>
        private async Task<bool> CleanDatabaseAsync(string dataTable, CancellationToken cancellationToken)
        {
            using var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                try
                {
                    _logger.LogInformation("Looking in {table} for records older than {timeSpan} days, at time {time}", dataTable, _retirementTime.TotalDays, DateTimeOffset.UtcNow);
                    int count = await _publicationGate.DeleteExpiredUnregisteredAsync(connection, dataTable,
                        DateTimeOffset.UtcNow - _retirementTime, cancellationToken);
                    if (count < 0)
                    {
                        _logger.LogWarning("Impossible to delete the data from {table} at time {time}", dataTable, DateTimeOffset.UtcNow);
                    }
                    else if (count > 0)
                    {
                        _logger.LogInformation("{count} records removed successfully from {table} at time {time}", count, dataTable, DateTimeOffset.UtcNow);
                    }
                    return true;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to clean old MyBaseData from {table}", dataTable);
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return false;
        }
    }
}
