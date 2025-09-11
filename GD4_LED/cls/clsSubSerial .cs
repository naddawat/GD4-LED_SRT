using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD4_LED.cls;

namespace GD4_LED.cls
{
    public class ClsSubSerial : ClsSerialLED.ClsSerialLED
    {
        public ClsSubSerial()
        {

        }
        public override void Button_event(string Row, string Addr, string QTY)
        {
            base.Button_event(Row, Addr, QTY);

            //_Variable.StrU += "ID:" + Row + "   Addr: " + Addr + "    QTY : " + QTY + "\r\n";
        }

        public override void RFID_event(string RFID_Code)
        {
            base.RFID_event(RFID_Code);

            //_Variable.StrU += "RFID:" + RFID_Code + "\r\n";
        }
    }
}
