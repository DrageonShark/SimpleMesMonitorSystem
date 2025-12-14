using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Services.DAL
{
    /// <summary>
    /// 简易的内存桩实现，便于单元测试或离线演示。
    /// </summary>
    internal sealed class InMemoryDbService:IDbService
    {
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null) =>
            await Task.FromResult(Enumerable.Empty<T>());

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object param = null) =>
            await Task.FromResult(default(T));

        public async Task<int> ExecuteAsync(string sql, object param = null) =>
            await Task.FromResult(0);

        public async Task<T?> ExecuteScalarAsync<T>(string sql, object param = null) =>
            await Task.FromResult(default(T));
    }
}
