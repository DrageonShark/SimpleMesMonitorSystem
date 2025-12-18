using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.Services.DAL
{
    /// <summary>
    /// 数据仓储：封装对 SQL Server 的写入操作（基于 sql/sql.md 的表结构）。
    /// </summary>
    public interface IDataRepository
    {
        Task<int> UpsertProductAsync(Product product);
        Task<int> InsertProductionOrderAsync(ProductionOrder order);
        Task<int> UpdateProductionOrderAsync(ProductionOrder order);
        Task<int> UpdateDeviceStatusAsync(int deviceId, string status, DateTime? lastUpdateTime = null);
        Task<int> InsertProductionRecordAsync(ProductionRecord record);
        Task<int> InsertAlarmRecordAsync(AlarmRecord alarm);
    }
}
