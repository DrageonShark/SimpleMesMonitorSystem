using System;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.Services.DAL
{
    /// <summary>
    /// 基于 IDbService 的仓储实现，负责写入 SQL Server。
    /// </summary>
    public sealed class DataRepository : IDataRepository
    {
        private readonly IDbService _db;

        public DataRepository(IDbService db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<int> UpsertProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            const string sql = @"IF EXISTS (SELECT 1 FROM dbo.T_Products WHERE ProductCode = @ProductCode)
UPDATE dbo.T_Products
   SET ProductName = @ProductName,
       SetTemperature = @SetTemperature,
       SetPressure = @SetPressure,
       Description = @Description
 WHERE ProductCode = @ProductCode
ELSE
INSERT INTO dbo.T_Products(ProductCode, ProductName, SetTemperature, SetPressure, Description)
VALUES (@ProductCode, @ProductName, @SetTemperature, @SetPressure, @Description);";
            return _db.ExecuteAsync(sql, product);
        }

        public Task<int> InsertProductionOrderAsync(ProductionOrder order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            const string sql = @"INSERT INTO dbo.T_ProductionOrders (OrderNo, ProductCode, PlanQty, CompletedQty, OrderStatus, StartTime, EndTime, CreateTime)
VALUES (@OrderNo, @ProductCode, @PlanQty, @CompletedQty, @OrderStatus, @StartTime, @EndTime, @CreateTime);";
            return _db.ExecuteAsync(sql, order);
        }

        public Task<int> UpdateProductionOrderAsync(ProductionOrder order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            const string sql = @"UPDATE dbo.T_ProductionOrders
   SET ProductCode = @ProductCode,
       PlanQty = @PlanQty,
       CompletedQty = @CompletedQty,
       OrderStatus = @OrderStatus,
       StartTime = @StartTime,
       EndTime = @EndTime
 WHERE OrderNo = @OrderNo;";
            return _db.ExecuteAsync(sql, order);
        }

        public Task<int> UpdateDeviceStatusAsync(int deviceId, string status, DateTime? lastUpdateTime = null)
        {
            const string sql = @"UPDATE dbo.T_Devices
   SET Status = @Status,
       LastUpdateTime = ISNULL(@LastUpdateTime, LastUpdateTime)
 WHERE DeviceId = @DeviceId;";
            return _db.ExecuteAsync(sql, new { DeviceId = deviceId, Status = status, LastUpdateTime = lastUpdateTime });
        }

        public Task<int> InsertProductionRecordAsync(ProductionRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            const string sql = @"INSERT INTO dbo.T_ProductionRecords (DeviceId, Temperature, Pressure, Speed, RecordTime)
VALUES (@DeviceId, @Temperature, @Pressure, @Speed, @RecordTime);";
            return _db.ExecuteAsync(sql, record);
        }

        public Task<int> InsertAlarmRecordAsync(AlarmRecord alarm)
        {
            if (alarm == null) throw new ArgumentNullException(nameof(alarm));
            const string sql = @"INSERT INTO dbo.T_AlarmRecord (DeviceId, AlarmMessage, AlarmTime, IsAck)
VALUES (@DeviceId, @AlarmMessage, @AlarmTime, @IsAck);";
            return _db.ExecuteAsync(sql, alarm);
        }
    }
}
