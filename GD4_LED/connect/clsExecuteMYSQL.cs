using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.connect
{
    class clsExecuteMYSQL : Logger
    {
        public bool dataExecuteNonQuery(string ConnectionString, MySqlCommand cmd)
        {
            bool ret = false;
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                cmd.Connection = conn;
                cmd.Transaction = trans;
                try
                {
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    ret = true;
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString() + Environment.NewLine + cmd.CommandText, "ERROR");
                    trans.Rollback();
                    ret = false;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return ret;
        }
    }
}
