using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GD4_LED.cls;

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
                //MessageBox.Show(_Variable.StrU);
                // Auto-verify after brief pause in scanning
                //if (!string.IsNullOrEmpty(_Variable.StrU_array[2]))
                //{
                //    if (Convert.ToInt32(_Variable.StrU_array[2]) > 0)
                //    {
                //        DataTable dt_stock = new DataTable();
                //        dt_stock = _query.GetLedStockByAddr(_Variable.StrU_array[0], _Variable.StrU_array[1], clsvariable.shelfzone);
                //        if (dt_stock.Rows.Count != 0)
                //        {
                //            prescpopup.InvenStock(dt_stock.Rows[0]["drugCode"].ToString(), _Variable.StrU_array[2]);

                //            _Variable.StrU = "";
                //            _Variable.StrU_array = new string[3];


                //        }
                //        else
                //        {
                //            _Variable.StrU = "";
                //        }
                //    }
                //}
                //_Variable.StrU = "";
                //_Variable.StrU_array = new string[3];
            }
            else
            {
                _Variable.StrU = "";
                _Variable.StrU_array[0] = "";   
            }

        }

        public override void RFID_event(string RFID_Code)
        {
            base.RFID_event(RFID_Code);

            _Variable.StrU = RFID_Code;
        }
    }
}
