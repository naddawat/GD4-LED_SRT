using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GD4_LED
{
    /// <summary>
    /// Interaction logic for ModernCalendarApp.xaml
    /// </summary>
    public partial class ModernCalendarApp : Window
    {
        private DateTime _currentDate;
        private DateTime _selectedDate;
        private ThaiBuddhistCalendar _thaiCalendar;
        private CultureInfo _thaiCulture;

        public DateTime SelectedDate => _selectedDate;


        public ModernCalendarApp()
        {
            InitializeComponent();
            this.Topmost = true;

            //Point mousePosition = Mouse.GetPosition(Application.Current.MainWindow);

            //// ตั้งค่าตำแหน่งหน้าต่างให้อยู่ที่ตำแหน่งเมาส์
            //this.Left = mousePosition.X;
            //this.Top = mousePosition.Y;

            _thaiCalendar = new ThaiBuddhistCalendar();
            _thaiCulture = new CultureInfo("th-TH");
            _currentDate = DateTime.Today;
            _selectedDate = DateTime.Today;
            InitializeCalendar();
            InitializeYearMonthComboBoxes();
        }

        private void InitializeYearMonthComboBoxes()
        {
            // Initialize Years (10 years back and 10 years forward)
            var currentYear = _thaiCalendar.GetYear(DateTime.Today);
            for (int year = currentYear - 10; year <= currentYear + 10; year++)
            {
                CboYear.Items.Add(year);
            }
            CboYear.SelectedItem = _thaiCalendar.GetYear(_currentDate);

            // Initialize Months
            for (int month = 1; month <= 12; month++)
            {
                CboMonth.Items.Add(new ComboBoxItem
                {
                    Content = _thaiCulture.DateTimeFormat.MonthNames[month - 1],
                    Tag = month
                });
            }
            CboMonth.SelectedIndex = _currentDate.Month - 1;
        }

        private void InitializeCalendar()
        {
            UpdateMonthDisplay();
            GenerateCalendarDays();
        }

        private void UpdateMonthDisplay()
        {
            var monthName = _thaiCulture.DateTimeFormat.MonthNames[_currentDate.Month - 1];
            var year = _thaiCalendar.GetYear(_currentDate);
            TxtCurrentMonth.Text = $"{monthName} {year}";
        }

        private void GenerateCalendarDays()
        {
            CalendarGrid.Children.Clear();

            var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);

            // Calculate starting day (0 = Sunday, 1 = Monday, etc.)
            int startDay = (int)firstDayOfMonth.DayOfWeek;

            // Add empty cells for days before the first day of month
            for (int i = 0; i < startDay; i++)
            {
                var emptyBorder = new Border();
                CalendarGrid.Children.Add(emptyBorder);
            }

            // Add day buttons
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(_currentDate.Year, _currentDate.Month, day);
                var button = CreateDayButton(day, date);
                CalendarGrid.Children.Add(button);
            }

            // Fill remaining cells to complete 6 rows
            int totalCells = 42; // 6 rows x 7 columns
            int currentCells = CalendarGrid.Children.Count;

            for (int i = currentCells; i < totalCells; i++)
            {
                var emptyBorder = new Border();
                CalendarGrid.Children.Add(emptyBorder);
            }
        }

        private Button CreateDayButton(int day, DateTime date)
        {
            var button = new Button
            {
                Content = day.ToString(),
                Tag = date
            };

            // Apply appropriate style
            if (date.Date == DateTime.Today.Date)
            {
                button.Style = (Style)FindResource("TodayButtonStyle");
            }
            else if (date.Date == _selectedDate.Date)
            {
                button.Style = (Style)FindResource("SelectedDayStyle");
            }
            else
            {
                button.Style = (Style)FindResource("DayButtonStyle");
            }

            button.Click += DayButton_Click;
            return button;
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is DateTime date)
            {
                _selectedDate = date;
                GenerateCalendarDays(); // Refresh to update selection
            }
        }

        private void CboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboYear.SelectedItem != null && CboMonth.SelectedItem != null)
            {
                int year = (int)CboYear.SelectedItem;
                int month = ((ComboBoxItem)CboMonth.SelectedItem).Tag as int? ?? 1;

                // Convert from Thai Buddhist year to Gregorian year
                int gregorianYear = year - 543;
                _currentDate = new DateTime(gregorianYear, month, 1);
                InitializeCalendar();
            }
        }

        private void CboMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboYear.SelectedItem != null && CboMonth.SelectedItem != null)
            {
                int year = (int)CboYear.SelectedItem;
                int month = ((ComboBoxItem)CboMonth.SelectedItem).Tag as int? ?? 1;

                // Convert from Thai Buddhist year to Gregorian year
                int gregorianYear = year - 543;
                _currentDate = new DateTime(gregorianYear, month, 1);
                InitializeCalendar();
            }
        }

        private void BtnPrevMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            UpdateComboBoxesFromCurrentDate();
            InitializeCalendar();
        }

        private void BtnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            UpdateComboBoxesFromCurrentDate();
            InitializeCalendar();
        }

        private void UpdateComboBoxesFromCurrentDate()
        {
            var thaiYear = _thaiCalendar.GetYear(_currentDate);
            CboYear.SelectedItem = thaiYear;
            CboMonth.SelectedIndex = _currentDate.Month - 1;
        }

        private void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = DateTime.Today;
            _selectedDate = DateTime.Today;
            UpdateComboBoxesFromCurrentDate();
            InitializeCalendar();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"เลือกวันที่: {_selectedDate:dd/MM/yyyy}", "การยืนยัน",
                          MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // เพิ่ม method สำหรับใช้งานจากภายนอก
        public void SetSelectedDate(DateTime date)
        {
            _selectedDate = date;
            _currentDate = date;
            UpdateComboBoxesFromCurrentDate();
            InitializeCalendar();
        }

        public DateTime GetSelectedDate()
        {
            return _selectedDate;
        }
    }
}