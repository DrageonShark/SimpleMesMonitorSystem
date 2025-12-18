using System;
using System.Collections.Generic;

namespace WPF9SimpleMesMonitorSystem.Services.Orders
{
    /// <summary>
    /// 订单统计快照：用于驱动看板（LiveCharts）显示。
    /// </summary>
    public sealed record OrderBoardSnapshot(int Pending, int Producing, int Paused, int Completed, DateTime Timestamp)
    {
        public int Total => Pending + Producing + Paused + Completed;
    }

    /// <summary>
    /// 观察者接口：接收订单统计的实时快照。
    /// </summary>
    public interface IOrderBoardObserver
    {
        void OnOrderBoardUpdated(OrderBoardSnapshot snapshot);
    }

    /// <summary>
    /// 主体接口：发布订单统计快照。
    /// </summary>
    public interface IOrderBoardSubject
    {
        void Subscribe(IOrderBoardObserver observer);
        void Unsubscribe(IOrderBoardObserver observer);
        void Publish(OrderBoardSnapshot snapshot);
    }

    /// <summary>
    /// 简单的订单看板事件中心，负责向观察者广播最新的统计数据。
    /// </summary>
    public sealed class OrderBoardHub : IOrderBoardSubject
    {
        private readonly List<IOrderBoardObserver> _observers = new();
        private readonly object _syncRoot = new();
        private OrderBoardSnapshot? _latest;

        public void Subscribe(IOrderBoardObserver observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            lock (_syncRoot)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }

            if (_latest != null)
            {
                observer.OnOrderBoardUpdated(_latest);
            }
        }

        public void Unsubscribe(IOrderBoardObserver observer)
        {
            if (observer == null) return;
            lock (_syncRoot)
            {
                _observers.Remove(observer);
            }
        }

        public void Publish(OrderBoardSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            _latest = snapshot;
            IOrderBoardObserver[] observers;
            lock (_syncRoot)
            {
                observers = _observers.ToArray();
            }

            foreach (var observer in observers)
            {
                try
                {
                    observer.OnOrderBoardUpdated(snapshot);
                }
                catch
                {
                    // 观察者内部异常不阻塞其他监听者
                }
            }
        }
    }
}
