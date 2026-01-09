using GD4_LED.cls;
using MySql.Data.MySqlClient;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GD4_LED.cls
{
    public class ClsSubSerial : ClsSerialLED.ClsSerialLED
    {
        clsvariable _Variable = clsvariable.Instance;
        PrescriptionPopup prescpopup = new PrescriptionPopup("");
        clsQuery _query = new clsQuery();
        public ClsSubSerial()
        {

        }
        public override void Button_event(string Row, string Addr, string QTY)
        {
            base.Button_event(Row, Addr, QTY);
            if(QTY != "" && Convert.ToInt32(QTY) > 0)
            {             
                _Variable.StrU = "ID:" + Row + "   Addr: " + Addr + "    QTY : " + QTY + "\r\n";
                _Variable.StrU_array[0] = Row;
                _Variable.StrU_array[1] = Addr;
                _Variable.StrU_array[2] = QTY;
                clsvariable.CountItem += 1;
                
                if (Convert.ToInt32(QTY) > 0)
                {
                    DataTable dt_stock = new DataTable();
                    dt_stock = _query.GetLedStockByAddr(Row, Addr, clsvariable.shelfzone);
                    prescpopup.InvenStock_Addr(Row, Addr, QTY);
                }
                _Variable.StrU = "";
                _Variable.StrU_array = new string[3];
            }
            else
            {
                _Variable.StrU = "";
                _Variable.StrU_array[0] = "";   
            }

            if (clsvariable.CountItem == clsvariable.PackItem)
            {
                //MessageBox.Show("if 1 : " + clsvariable.CountItem.ToString());
                ////timerBtn.Stop();

                //var page = new GD4_LED.page.DispensePage();
                ////var page = new GD4_LED.page.DispensePage();
                ////await page.InitializePageAsync();
                ////((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(page);

                ////this.Close();
                //await page.InitializePageAsync();
                //((MainWindow)Application.Current.MainWindow)
                //    .MainFrame.Navigate(page);

                //clsvariable.CountItem = 0;

                //MessageBox.Show("if 2 : " + clsvariable.CountItem.ToString());
                //this.Dispatcher.Invoke(() => this.Close());
                //prescpopup.Close(); // ปิดหน้าต่างก่อน

                //MessageBox.Show("if 3 : " + clsvariable.CountItem.ToString());
                //CloseButton_Click(null, null); // เรียกใช้งาน CloseButton_Click หลังจากปิดหน้าต่างแล้ว

            }

        }
       
        public override void RFID_event(string RFID_Code)
        {
            base.RFID_event(RFID_Code);

            _Variable.StrU = RFID_Code;
        }
    }
}
