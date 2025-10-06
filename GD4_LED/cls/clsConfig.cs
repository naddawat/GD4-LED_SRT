using GD4_LED.connect;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;

namespace GD4_LED.cls
{    
    class clsConfig
    {
        clsutilDB clsINI = new clsutilDB();
        clsFillMyDB fill = new clsFillMyDB();
        clsExecuteMYSQL Execute = new clsExecuteMYSQL();
        public static string SQL;
        public static bool result;

        string connectst = GD4_LED.Properties.Settings.Default.connectstringlocal;
        public DataTable GetLedConfig(string comname)
        {

            SQL = $@" SELECT * FROM ms_ledconfig ms Where ms.comname = '{comname}' ";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
    }
}
