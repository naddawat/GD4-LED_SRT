using GD4_LED_2.Helpers;
using GD4_LED_2.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GD4_LED_2.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับ MainWindow
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly PerformanceMonitorService _performanceMonitor;
        private readonly CancellationTokenSource _cancellationTokenSource;

        #region Properties

        private string _deviceName;
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }

        private string _versionText;
        public string VersionText
        {
            get => _versionText;
            set => SetProperty(ref _versionText, value);
        }

        private string _currentDateTime;
        public string CurrentDateTime
        {
            get => _currentDateTime;
            set => SetProperty(ref _currentDateTime, value);
        }

        private string _cpuUsage;
        public string CpuUsage
        {
            get => _cpuUsage;
            set => SetProperty(ref _cpuUsage, value);
        }

        private string _ramUsage;
        public string RamUsage
        {
            get => _ramUsage;
            set => SetProperty(ref _ramUsage, value);
        }

        private bool _isDatabaseConnected;
        public bool IsDatabaseConnected
        {
            get => _isDatabaseConnected;
            set => SetProperty(ref _isDatabaseConnected, value);
        }

        private string _cpuColor = "White";
        public string CpuColor
        {
            get => _cpuColor;
            set => SetProperty(ref _cpuColor, value);
        }

        private string _ramColor = "White";
        public string RamColor
        {
            get => _ramColor;
            set => SetProperty(ref _ramColor, value);
        }

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        #endregion

        #region Commands

        public ICommand CloseCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        public MainViewModel(string connectionString)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _databaseService = new DatabaseService(connectionString);
            _performanceMonitor = new PerformanceMonitorService();

            // Initialize commands
            CloseCommand = new RelayCommand(OnClose);
            RefreshCommand = new AsyncRelayCommand(async (p) => await RefreshAsync());

            // Initialize
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Get version
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                DeviceName = Environment.MachineName;
                VersionText = $"Medicine Management System | ver: {version} | {DeviceName}";

                // Check database connection
                IsDatabaseConnected = await _databaseService.CheckConnectionAsync(_cancellationTokenSource.Token);

                // Start datetime timer
                StartDateTimeTimer();

                // Start performance monitoring (ทุก 5 วินาที แทน 2 วินาที เพื่อลด CPU usage)
                _performanceMonitor.PerformanceUpdated += OnPerformanceUpdated;
                _performanceMonitor.Start(5);

                // Load default view (DispenseViewModel)
                await LoadDispenseViewAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการเริ่มต้นโปรแกรม: {ex.Message}",
                              "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDispenseViewAsync()
        {
            try
            {
                var dispenseViewModel = new DispenseViewModel(_databaseService, DeviceName);
                CurrentViewModel = dispenseViewModel;
                await dispenseViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดหน้าจัดยา: {ex.Message}",
                              "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartDateTimeTimer()
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                CurrentDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };
            timer.Start();
            CurrentDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void OnPerformanceUpdated(object sender, PerformanceData perfData)
        {
            // Update UI on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                CpuUsage = perfData.CpuDisplay;
                RamUsage = perfData.RamDisplay;

                // Update colors based on level
                CpuColor = perfData.CpuLevel switch
                {
                    PerformanceLevel.Critical => "Red",
                    PerformanceLevel.Warning => "Yellow",
                    _ => "White"
                };

                RamColor = perfData.RamLevel switch
                {
                    PerformanceLevel.Critical => "Red",
                    PerformanceLevel.Warning => "Yellow",
                    _ => "White"
                };
            });
        }

        private async Task RefreshAsync()
        {
            IsDatabaseConnected = await _databaseService.CheckConnectionAsync(_cancellationTokenSource.Token);
            
            if (CurrentViewModel is DispenseViewModel dispenseVM)
            {
                await dispenseVM.RefreshAsync();
            }
        }

        private void OnClose(object parameter)
        {
            Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _performanceMonitor?.Dispose();

            if (CurrentViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
