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
using System.Windows.Forms;

namespace GD4_LED.cls
{
    public class ClsSubSerial : ClsSerialLED.ClsSerialLED
    {
        clsvariable _Variable = clsvariable.Instance;
        PrescriptionPopup prescpopup = new PrescriptionPopup("");
        //private page.DispensePage disPage = new page.DispensePage();
        clsQuery _query = new clsQuery();
        public event EventHandler<ButtonEventArgs> OnButtonReceived;
        public ClsSubSerial()
        {

        }
        //public void setUC(ref page.DispensePage c)
        //{
        //    disPage = c;
        //}
        public class ButtonEventArgs : EventArgs
        {
            public string Row { get; set; }
            public string Addr { get; set; }
            public int Qty { get; set; }
        }
        public override void Button_event(string Row, string Addr, string QTY)
        {
            //Dipen timer = new MyTimerService();
            //timer.Start();
            base.Button_event(Row, Addr, QTY);

            //if(QTY != "" && Convert.ToInt32(QTY) > 0)
            //{
            //    disPage.timerBtn.Stop();
            //    _Variable.StrU = "ID:" + Row + "   Addr: " + Addr + "    QTY : " + QTY + "\r\n";
            //    _Variable.StrU_array[0] = Row;
            //    _Variable.StrU_array[1] = Addr;
            //    _Variable.StrU_array[2] = QTY;

            //    disPage.Dispatcher.Invoke(() =>
            //    {
            //        disPage.PressBtn = true;
            //    });

            //    clsvariable.CountItem += 1;
            //    clsvariable.press_btn = true;

            //    //MessageBox.Show("ID:" + Row + "   Addr: " + Addr + "    QTY : " + QTY + clsvariable.press_btn);
            //    DataTable dt_stock = new DataTable();
            //    dt_stock = _query.GetLedStockByAddr(Row, Addr, clsvariable.shelfzone);
            //    disPage.InvenStock_Addr(Row, Addr, QTY);
            //    _Variable.StrU = "";
            //    _Variable.StrU_array = new string[3];
            //}
            //else
            //{

            //}
            //_Variable.StrU = "";
            //_Variable.StrU_array = new string[3];


            if (!int.TryParse(QTY, out int qty) || qty <= 0)
                return;


            System.Windows.MessageBox.Show("ID:" + Row + "   Addr: " + Addr + "    QTY : " + QTY + clsvariable.press_btn);
            _Variable.StrU = $"ID:{Row} Addr:{Addr} QTY:{QTY}";
            _Variable.StrU_array = new[] { Row, Addr, QTY };

            OnButtonReceived?.Invoke(this, new ButtonEventArgs
            {
                Row = Row,
                Addr = Addr,
                Qty = qty
            });
            
        }
       
        public override void RFID_event(string RFID_Code)
        {
            base.RFID_event(RFID_Code);

            _Variable.StrU = RFID_Code;
        }
    }

}
