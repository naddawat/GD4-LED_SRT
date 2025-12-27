using GD4_LED_2.Helpers;
using GD4_LED_2.Models;
using GD4_LED_2.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GD4_LED_2.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับหน้าจัดยา - Optimized version
    /// </summary>
    public class DispenseViewModel : ViewModelBase, IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly PrescriptionService _prescriptionService;
        private readonly string _shelfZone;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly DispatcherTimer _autoRefreshTimer;
        private readonly DispatcherTimer _searchDebounceTimer;

        #region Properties

        private ObservableCollection<Prescription> _prescriptions;
        public ObservableCollection<Prescription> Prescriptions
        {
            get => _prescriptions;
            set => SetProperty(ref _prescriptions, value);
        }

        private Prescription _selectedPrescription;
        public Prescription SelectedPrescription
        {
            get => _selectedPrescription;
            set => SetProperty(ref _selectedPrescription, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Debounce search
                    _searchDebounceTimer.Stop();
                    _searchDebounceTimer.Start();
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private int _totalPrescriptions;
        public int TotalPrescriptions
        {
            get => _totalPrescriptions;
            set => SetProperty(ref _totalPrescriptions, value);
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            set => SetProperty(ref _totalItems, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SelectPrescriptionCommand { get; }
        public ICommand DispenseCommand { get; }

        #endregion

        public DispenseViewModel(DatabaseService databaseService, string deviceName)
        {
            _databaseService = databaseService;
            _prescriptionService = new PrescriptionService(databaseService);
            _shelfZone = GetShelfZoneFromDevice(deviceName);
            _cancellationTokenSource = new CancellationTokenSource();

            // Initialize collections
            Prescriptions = new ObservableCollection<Prescription>();

            // Initialize commands
            RefreshCommand = new AsyncRelayCommand(async (p) => await RefreshAsync());
            SearchCommand = new AsyncRelayCommand(async (p) => await SearchAsync());
            SelectPrescriptionCommand = new RelayCommand(OnSelectPrescription);
            DispenseCommand = new AsyncRelayCommand(async (p) => await DispenseAsync(), CanDispense);

            // Initialize search debounce timer
            _searchDebounceTimer = new DispatcherTimer();
            _searchDebounceTimer.Interval = TimeSpan.FromMilliseconds(300);
            _searchDebounceTimer.Tick += async (s, e) =>
            {
                _searchDebounceTimer.Stop();
                await SearchAsync();
            };

            // Initialize auto refresh timer (ทุก 30 วินาที แทน 15 วินาที)
            _autoRefreshTimer = new DispatcherTimer();
            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(30);
            _autoRefreshTimer.Tick += async (s, e) => await RefreshAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "กำลังโหลดข้อมูล...";

                await LoadPrescriptionsAsync();

                // Start auto refresh
                _autoRefreshTimer.Start();

                StatusMessage = "พร้อมใช้งาน";
            }
            catch (Exception ex)
            {
                StatusMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}",
                              "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshAsync()
        {
            try
            {
                // Clear cache
                await _prescriptionService.ClearCacheAsync();
                
                // Reload data
                await LoadPrescriptionsAsync();

                StatusMessage = $"อัพเดทข้อมูลเมื่อ {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"เกิดข้อผิดพลาดในการรีเฟรช: {ex.Message}";
            }
        }

        private async Task LoadPrescriptionsAsync()
        {
            try
            {
                var prescriptions = await _prescriptionService.GetPrescriptionsAsync(
                    _shelfZone, 
                    _cancellationTokenSource.Token);

                // Update UI on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Prescriptions.Clear();
                    foreach (var prescription in prescriptions)
                    {
                        Prescriptions.Add(prescription);
                    }

                    UpdateStatistics();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading prescriptions: {ex.Message}");
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadPrescriptionsAsync();
                    return;
                }

                IsLoading = true;

                var searchResults = await _prescriptionService.SearchPrescriptionsAsync(
                    SearchText,
                    _shelfZone,
                    _cancellationTokenSource.Token);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Prescriptions.Clear();
                    foreach (var prescription in searchResults)
                    {
                        Prescriptions.Add(prescription);
                    }

                    UpdateStatistics();
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"เกิดข้อผิดพลาดในการค้นหา: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSelectPrescription(object parameter)
        {
            if (parameter is Prescription prescription)
            {
                SelectedPrescription = prescription;
            }
        }

        private bool CanDispense(object parameter)
        {
            return SelectedPrescription != null && !IsLoading;
        }

        private async Task DispenseAsync()
        {
            if (SelectedPrescription == null)
                return;

            try
            {
                IsLoading = true;
                StatusMessage = $"กำลังจัดยา {SelectedPrescription.PrescriptionNo}...";

                // TODO: Implement LED control และ printing logic ที่นี่
                // await ControlLEDAsync(SelectedPrescription);
                // await PrintLabelAsync(SelectedPrescription);

                // Update status
                bool success = await _prescriptionService.UpdatePrescriptionStatusAsync(
                    SelectedPrescription.PrescriptionNo,
                    "dispensed",
                    _cancellationTokenSource.Token);

                if (success)
                {
                    StatusMessage = $"จัดยา {SelectedPrescription.PrescriptionNo} เรียบร้อย";
                    
                    // Remove from list
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Prescriptions.Remove(SelectedPrescription);
                        SelectedPrescription = null;
                        UpdateStatistics();
                    });

                    MessageBox.Show("จัดยาเรียบร้อย", "สำเร็จ", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "เกิดข้อผิดพลาดในการอัพเดทสถานะ";
                    MessageBox.Show("ไม่สามารถอัพเดทสถานะได้", "ข้อผิดพลาด",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
                MessageBox.Show($"เกิดข้อผิดพลาดในการจัดยา: {ex.Message}",
                              "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateStatistics()
        {
            TotalPrescriptions = Prescriptions.Count;
            TotalItems = Prescriptions.Sum(p => p.TotalItems);
        }

        private string GetShelfZoneFromDevice(string deviceName)
        {
            // TODO: Query จาก database ตาม device name
            // ใช้แบบเดิมหรือส่งผ่านมาจาก parameter
            return deviceName; // Placeholder
        }

        public void Dispose()
        {
            _autoRefreshTimer?.Stop();
            _searchDebounceTimer?.Stop();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
