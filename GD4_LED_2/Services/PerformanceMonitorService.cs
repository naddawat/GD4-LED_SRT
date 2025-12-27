using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GD4_LED_2.Services
{
    /// <summary>
    /// Service สำหรับ monitor performance แบบ optimized
    /// ใช้ resource น้อยกว่าเดิม
    /// </summary>
    public class PerformanceMonitorService : IDisposable
    {
        private readonly Process _currentProcess;
        private DateTime _lastCpuTime;
        private TimeSpan _lastTotalProcessorTime;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _monitorTask;
        private bool _isRunning;

        public event EventHandler<PerformanceData> PerformanceUpdated;

        public PerformanceMonitorService()
        {
            _currentProcess = Process.GetCurrentProcess();
            _lastCpuTime = DateTime.UtcNow;
            _lastTotalProcessorTime = _currentProcess.TotalProcessorTime;
        }

        /// <summary>
        /// เริ่ม monitor performance
        /// </summary>
        public void Start(int intervalSeconds = 5)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _monitorTask = MonitorAsync(intervalSeconds, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// หยุด monitor
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Monitor loop แบบ Async
        /// </summary>
        private async Task MonitorAsync(int intervalSeconds, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var perfData = await GetPerformanceDataAsync();
                    PerformanceUpdated?.Invoke(this, perfData);

                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Performance monitor error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// ดึงข้อมูล Performance แบบ Async
        /// </summary>
        private async Task<PerformanceData> GetPerformanceDataAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Calculate CPU usage
                    DateTime currentTime = DateTime.UtcNow;
                    _currentProcess.Refresh();
                    TimeSpan currentTotalProcessorTime = _currentProcess.TotalProcessorTime;

                    double cpuUsedMs = (currentTotalProcessorTime - _lastTotalProcessorTime).TotalMilliseconds;
                    double totalMsPassed = (currentTime - _lastCpuTime).TotalMilliseconds;
                    double cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                    double cpuUsage = cpuUsageTotal * 100;

                    _lastCpuTime = currentTime;
                    _lastTotalProcessorTime = currentTotalProcessorTime;

                    // Get RAM usage (in MB)
                    double ramUsageMB = _currentProcess.WorkingSet64 / 1024.0 / 1024.0;

                    return new PerformanceData
                    {
                        CpuUsagePercent = Math.Round(cpuUsage, 1),
                        RamUsageMB = Math.Round(ramUsageMB, 0),
                        Timestamp = DateTime.Now
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error calculating performance: {ex.Message}");
                    return new PerformanceData();
                }
            });
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
            _currentProcess?.Dispose();
        }
    }

    /// <summary>
    /// ข้อมูล Performance
    /// </summary>
    public class PerformanceData
    {
        public double CpuUsagePercent { get; set; }
        public double RamUsageMB { get; set; }
        public DateTime Timestamp { get; set; }

        public string CpuDisplay => $"{CpuUsagePercent:F1}%";
        public string RamDisplay => $"{RamUsageMB:F0} MB";
        
        public PerformanceLevel CpuLevel
        {
            get
            {
                if (CpuUsagePercent > 80) return PerformanceLevel.Critical;
                if (CpuUsagePercent > 60) return PerformanceLevel.Warning;
                return PerformanceLevel.Normal;
            }
        }

        public PerformanceLevel RamLevel
        {
            get
            {
                if (RamUsageMB > 1024) return PerformanceLevel.Critical;
                if (RamUsageMB > 512) return PerformanceLevel.Warning;
                return PerformanceLevel.Normal;
            }
        }
    }

    public enum PerformanceLevel
    {
        Normal,
        Warning,
        Critical
    }
}
