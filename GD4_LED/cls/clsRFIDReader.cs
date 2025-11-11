using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.cls
{
    class clsRFIDReader : ClsSerialLED.ClsSerialLED    
    {  
        public override void Button_event(string Row, string Addr, string QTY)
        {
            base.Button_event(Row, Addr, QTY);

            //StrU += "ID:" + Row + "   Addr: " + Addr + "    QTY : " + QTY + "\r\n";
        }

        public override void RFID_event(string RFID_Code)
        {
            base.RFID_event(RFID_Code);
            string txt = RFID_Code;
            clsvariable.StrU_rfid = RFID_Code;
            Console.WriteLine(clsvariable.StrU_rfid);
            
        }
    }
}
