using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Services.DAL
{
    /// <summary>
    /// 根据配置生产具体的 IDbService 实例，目前实现了 SqlServer + 内存桩两个选项。
    /// </summary>
    public class DbServiceFactory:IDbServiceFactory
    {
        private readonly DbServiceFactoryOptions _options;

        public DbServiceFactory(DbServiceFactoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IDbService CreateService()
        {
            return _options.ProviderType switch
            {
                DatabaseProviderType.SqlServer => new SqlDbService(_options.ConnectionString),
                DatabaseProviderType.InMemoryMock => new InMemoryDbService(),
                _ => throw new NotSupportedException($"提供者 {_options.ProviderType} 未实现。")
            };
        }
    }
}
