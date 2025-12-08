```sql
-- 1. 创建数据库 (如果不存在)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SimpleMES_DB')
BEGIN
    CREATE DATABASE SimpleMES_DB;
END
GO

USE SimpleMES_DB;
GO

-- 2. 创建设备表 (Devices)
-- 存储生产线上的设备基础信息及当前状态
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Devices]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[T_Devices](
        [DeviceId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, -- 设备ID
        [DeviceName] [nvarchar](50) NOT NULL,                -- 设备名称
        [IpAddress] [nvarchar](20) NULL,                     -- IP地址 (Modbus TCP)
        [Port] [int] DEFAULT 502,                            -- 端口
        [SlaveId] [tinyint] NOT NULL DEFAULT 1,                  -- 从站地址
        [Status] [nvarchar](20) DEFAULT 'Stopped',           -- 状态: Running/Stopped/Fault
        [LastUpdateTime] [datetime] DEFAULT GETDATE()        -- 最后通信时间
    );
    

    -- 插入模拟数据 (3台设备)
    INSERT INTO T_Devices (DeviceName, IpAddress, Status) VALUES 
    ('注塑机-A01', '127.0.0.1', 'Stopped'),
    ('冲压机-B02', '127.0.0.1', 'Stopped'),
    ('包装机-C03', '127.0.0.1', 'Stopped');

END
GO

-- 3. 创建生产订单表 (ProductionOrders)
-- 管理生产任务，包含计划数和完成数
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_ProductionOrders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[T_ProductionOrders](
        [OrderNo] [nvarchar](50) NOT NULL PRIMARY KEY,       -- 订单号
        [ProductModel] [nvarchar](50) NOT NULL,              -- 产品型号
        [PlanQty] [int] NOT NULL,                            -- 计划数量
        [CompletedQty] [int] DEFAULT 0,                      -- 已完成数量
        [DefectQty] [int] DEFAULT 0,                         -- 不良品数量
        [OrderStatus] [nvarchar](20) DEFAULT 'Pending',      -- 状态: Pending/Producing/Completed
        [StartTime] [datetime] NULL,                         -- 开始时间
        [EndTime] [datetime] NULL,                           -- 结束时间
        [CreateTime] [datetime] DEFAULT GETDATE()
    );

    -- 插入一条测试订单
    INSERT INTO ProductionOrders (OrderNo, ProductModel, PlanQty, OrderStatus) 
    VALUES ('MO-20231027001', 'Widget-X', 1000, 'Pending');

END
GO

-- 4. 创建生产过程记录表 (ProductionRecords)
-- 用于生成历史趋势图，记录核心参数
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_ProductionRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[T_ProductionRecords](
        [RecordId] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [DeviceId] [int] NOT NULL,                           -- 关联设备ID
        [Temperature] [decimal](10, 2) NULL,                 -- 温度
        [Pressure] [decimal](10, 2) NULL,                    -- 压力
        [Speed] [int] NULL,                                  -- 转速
        [RecordTime] [datetime] DEFAULT GETDATE(),           -- 记录时间
        

        -- 外键约束 (可选，为了数据完整性建议加上)
        CONSTRAINT [FK_Records_Devices] FOREIGN KEY([DeviceId]) REFERENCES [dbo].[Devices] ([DeviceId])
    );

END
GO

-- 5. 创建报警记录表 (AlarmRecords)
-- 记录设备故障历史
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AlarmRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AlarmRecords](
        [AlarmId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [DeviceId] [int] NOT NULL,                           -- 关联设备ID
        [AlarmMessage] [nvarchar](200) NOT NULL,             -- 报警内容
        [AlarmTime] [datetime] DEFAULT GETDATE(),            -- 报警时间
        [IsAck] [bit] DEFAULT 0,                             -- 是否确认 (扩展功能)
        

        CONSTRAINT [FK_Alarms_Devices] FOREIGN KEY([DeviceId]) REFERENCES [dbo].[Devices] ([DeviceId])
    );

END
GO
```

