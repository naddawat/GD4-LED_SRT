using System;
using System.Windows;
using System.Windows.Input;

namespace GD4_LED
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            txtScannedCode.Focus();
            txtScannedCode.SelectAll();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void txtScannedCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformLogin();
            }
        }

        private void PerformLogin()
        {
            string scannedCode = txtScannedCode.Text.Trim();

            // Reset status
            txtLoginStatus.Visibility = Visibility.Collapsed;
            txtLoginStatus.Text = "";

            // Validate input
            if (string.IsNullOrEmpty(scannedCode) || scannedCode == "รอการสแกน...")
            {
                ShowError("กรุณาสแกนบาร์โค้ดหรือคิวอาร์โค้ด");
                txtScannedCode.SelectAll();
                txtScannedCode.Focus();
                return;
            }

            // TODO: Add actual authentication logic here
            // Validate scanned code against database
            if (ValidateScannedCode(scannedCode))
            {
                // Login successful
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ShowError("รหัสไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง");
                txtScannedCode.SelectAll();
                txtScannedCode.Focus();
            }
        }

        private bool ValidateScannedCode(string scannedCode)
        {
            // TODO: Implement actual authentication
            // This is a placeholder - connect to your database or authentication service
            // Validate the scanned code against user database
            
            // Example: Simple check (replace with actual database check)
            // return scannedCode == "USER001" || scannedCode == "ADMIN123";
            
            // For now, accept any non-empty scanned code
            return !string.IsNullOrEmpty(scannedCode) && scannedCode != "รอการสแกน...";
        }

        private void ShowError(string message)
        {
            txtLoginStatus.Text = message;
            txtLoginStatus.Visibility = Visibility.Visible;
        }
    }
}
