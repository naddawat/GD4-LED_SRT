using GD4_LED.cls;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace GD4_LED
{
    public partial class LoginWindow : Window
    {
        clsQuery _query = new clsQuery();

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
            if (!string.IsNullOrEmpty(scannedCode) && scannedCode != "รอการสแกน...")
            {
                DataTable dt_user = new DataTable();
                dt_user = _query.GetUser(scannedCode, "");
                if (dt_user.Rows.Count > 0)
                {
                    txtname.Text = dt_user.Rows[0]["fullname"].ToString();
                    clsvariable.user = dt_user.Rows[0]["fullname"].ToString();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void ShowError(string message)
        {
            txtLoginStatus.Text = message;
            txtLoginStatus.Visibility = Visibility.Visible;
        }
    }
}
