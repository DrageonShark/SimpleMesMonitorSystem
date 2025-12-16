using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;

namespace WPF9SimpleMesMonitorSystem.Services.Device.States
{
    /// <summary>
    /// 状态上下文：负责在运行/停止/故障之间切换，并统一输出日志与报警。
    /// </summary>
    public sealed class DeviceStateContext
    {
        private readonly IDeviceState _runningState = new RunningDeviceState();
        private readonly IDeviceState _stoppedState = new StoppedDeviceState();
        private readonly IDeviceState _faultState = new FaultDeviceState();
        private readonly Action<string> _logAction;
        private readonly Action<string> _alarmAction;
        private IDeviceState _currentState;

        public DeviceStateContext(Models.Device device, Action<string>? logAction = null, Action<string>? alarmAction = null)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            _currentState = _stoppedState;
            _logAction = logAction ?? (message => Debug.WriteLine(message));
            _alarmAction = alarmAction ?? (message => Debug.WriteLine(message));
        }

        public Models.Device Device { get; }
        public IDeviceState CurrentState => _currentState;
        public double HighTemperatureThreshold { get; set; } = 85d;
        public double HighPressureThreshold { get; set; } = 15d;
        public int LowSpeedThreshold { get; set; } = 5;

        public void ApplySnapshot(DeviceTelemetrySnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            var targetState = ResolveState(snapshot.Status);
            if (!ReferenceEquals(_currentState, targetState))
            {
                _currentState?.OnExit(this);
                _currentState = targetState;
                _currentState.OnEnter(this,snapshot);
            }

            _currentState.OnTelemetry(this, snapshot);
        }

        private IDeviceState ResolveState(string status)
        {
            var normalized = (status ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "running" => _runningState,
                "fault" => _faultState,
                _ => _stoppedState
            };
        }

        public void RaiseLog(string message)
        {
            if(string.IsNullOrWhiteSpace(message))
                return;
            _logAction($"[{Device.DeviceName}]{message}");
        }

        public void RaiseAlarm(string message)
        {
            if(string.IsNullOrWhiteSpace(message))
                return;
            _alarmAction($"[{Device.DeviceName}]{message}");
        }
    }
}
