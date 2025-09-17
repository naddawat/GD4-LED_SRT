using GD4_LED.connect;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.cls
{
    internal class clsMain
    {
        clsutilDB clsINI = new clsutilDB();
        clsFillMyDB fill = new clsFillMyDB();
        clsExecuteMYSQL Execute = new clsExecuteMYSQL();
        public static string SQL;
        public  string getDeviceName()
        {
            DataTable dt = new DataTable();
            string computerName = Environment.MachineName;
            SQL = $"SELECT   shelfzone  FROM ms_shelf WHERE computername = '{computerName}'";
            dt = clsFillMyDB.GetDataSet(GD4_LED.Properties.Settings.Default.connectstring, SQL);

            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0]["shelfzone"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
