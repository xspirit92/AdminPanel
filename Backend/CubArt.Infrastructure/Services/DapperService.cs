using CubArt.Infrastructure.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace CubArt.Infrastructure.Services
{
    public class DapperService : IDapperService
    {
        private readonly string _connectionString;

        public DapperService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DBConnectionString");
        }

        private IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, int? commandTimeout = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<T>(sql, parameters, commandTimeout: commandTimeout);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, int? commandTimeout = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandTimeout: commandTimeout);
        }

        public async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, int? commandTimeout = null)
        {
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, commandTimeout: commandTimeout);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null, int? commandTimeout = null)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, parameters, commandTimeout: commandTimeout);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, int? commandTimeout = null)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<T>(sql, parameters, commandTimeout: commandTimeout);
        }
    }

}
