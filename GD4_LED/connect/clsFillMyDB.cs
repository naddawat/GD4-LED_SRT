using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.connect
{
    class clsFillMyDB : Logger
    {
        public static DataTable GetDataSet(string ConnectionString, string SQL)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            MySqlDataAdapter da = new MySqlDataAdapter();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = SQL;
            da.SelectCommand = cmd;
            DataTable dt = new DataTable();
            try
            {
                conn.Open();
                da.Fill(dt);
                conn.Close();
            }
            catch (Exception ex)
            {
                //WriteLog(ex.ToString(), "Error");
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }
    }
}
