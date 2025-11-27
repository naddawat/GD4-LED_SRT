using System;
using System.Collections.Generic;
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
