using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Services.DAL
{
    /// <summary>
    /// 支持的数据库类型。
    /// </summary>
    public enum DatabaseProviderType
    {
        SqlServer = 0,
        InMemoryMock = 1
    }
}
