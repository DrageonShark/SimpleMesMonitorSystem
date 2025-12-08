using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace WPF9SimpleMesMonitorSystem.Services.DAL
{
    public class SqlDbService:IDbService
    {
        //数据库连接字符串
        // 在实际生产中，这个字符串应该放在 App.config 或 appsettings.json 中
        private readonly string _connectionString;
        
        public SqlDbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        //获取数据库连接对象（工厂模式的体现：按需生产连接）
        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                //Dapper 自动打开连接，查询，映射对象，关闭连接
                return await conn.QueryAsync<T>(sql, param);
            }
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                return await conn.QueryFirstOrDefaultAsync<T>(sql, param);
            }
        }

        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                return await conn.ExecuteAsync(sql, param);
            }
        }

        public async Task<T?> ExecuteScalarAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                return await conn.ExecuteScalarAsync<T>(sql, param);
            }
        }
    }
}
