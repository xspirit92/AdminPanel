namespace CubArt.Infrastructure.Interfaces
{
    public interface IDapperService
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, int? commandTimeout = null);
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, int? commandTimeout = null);
        Task<T> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, int? commandTimeout = null);
        Task<int> ExecuteAsync(string sql, object? parameters = null, int? commandTimeout = null);
        Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, int? commandTimeout = null);
    }

}
