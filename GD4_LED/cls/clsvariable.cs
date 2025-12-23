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
        public static string comport = "";
        public static string crp_report = "";
        public static string printname = "";
        public static bool print_isenable = false;
        public static bool trigger_isenable = false;
        public static string sever = "";
        public static string database = "";
        public static string port = "";
        public static string username = "";
        public static string password = "";
        public static string connectionST = "";
        private static clsvariable _instance;
        public static string[] RGD_dispense;
        public static string Button_event = "";
        public static int CountItem = 0;
        public static int PackItem = 0;
        public string[] StrU_array = new string[3];
        public static string StrU_rfid { get; set; } = "";
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
        public static DataTable dt_LedConfig = new DataTable();
        public static DataTable dt_Prescr = new DataTable();
        public static DataTable dt_Prescr_his = new DataTable();
    }
}
