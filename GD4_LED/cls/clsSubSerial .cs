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
                clsvariable.CountItem += 1;
                //MessageBox.Show(Row+"|"+Addr+"|"+QTY);
                if (Convert.ToInt32(QTY) > 0)
                {
                    //MessageBox.Show(Row + "|" + Addr + "|" + QTY);
                    DataTable dt_stock = new DataTable();
                    dt_stock = _query.GetLedStockByAddr(Row, Addr, clsvariable.shelfzone);
                    MessageBox.Show(dt_stock.Rows.Count.ToString());

                    //if (dt_stock.Rows.Count > 0)
                    //{
                    //    MessageBox.Show(dt_stock.Rows[0]["drugCode"].ToString() + "|" + QTY);
                    //    prescpopup.InvenStock(dt_stock.Rows[0]["drugCode"].ToString(), QTY);                       
                    //    MessageBox.Show(clsvariable.CountItem.ToString());
                    //}
                    //else
                    //{
                    //    _Variable.StrU = "";
                    //}

                    prescpopup.InvenStock_Addr(Row, Addr, QTY);
                    //MessageBox.Show(clsvariable.CountItem.ToString());
                }
                _Variable.StrU = "";
                _Variable.StrU_array = new string[3];
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
