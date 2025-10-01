using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.cls
{
    class clsvariable 
    {
        public string StrU = "";
        public ClsSubSerial SerialCan;
        public static string comname = "";
        public static string shelfzone = "";
        private static clsvariable _instance;
        public static clsvariable Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new clsvariable();
                return _instance;
            }
        }

        private clsvariable() { }

        public static DataTable dt_Ledinfo = new DataTable();
    }
}
