using GD4_LED.connect;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.cls
{
    class clsStock
    {
        clsutilDB clsINI = new clsutilDB();
        clsFillMyDB fill = new clsFillMyDB();
        clsExecuteMYSQL Execute = new clsExecuteMYSQL();
        public static string SQL;
        public static bool result;

        string connectst = GD4_LED.Properties.Settings.Default.connectstring;
        public DataTable GetLedStock()
        {

            SQL = @" SELECT
              ms.shelfzone AS location,
              ms.shelfname AS drugPosition,
              ms.LotNo AS lot,
              ms.orderitemcode AS drugCode,
              ml.orderitemENname AS drugName,
              ms.In_Qty AS Quantity,
              ms.Exp AS exp,
              ms.max AS max,
              ms.min AS min,
              '' AS firmname ,
              CASE 
                WHEN (ms.In_Qty / ms.max) * 100 < 0 
                    THEN 0
                ELSE ROUND((ms.In_Qty / ms.max) * 100 ,0)
            END AS Percent
            FROM
              ms_stock ms
              LEFT JOIN ms_location ml ON ms.orderitemcode = ml.orderitemcode
              ORDER BY Percent";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
        public DataTable GetLedStockByCode(string orderitemcode,string Lot)
        {

            SQL = $@" SELECT
              ms.shelfzone AS location,
              ms.shelfname AS drugPosition,
              ms.LotNo AS lot,
              ms.orderitemcode AS drugCode,
              ml.orderitemENname AS drugName,
              ms.In_Qty AS Quantity,
              ms.Exp AS exp,
              ms.max AS max,
              ms.min AS min,
              '' AS firmname ,
              CASE 
                WHEN (ms.In_Qty / ms.max) * 100 < 0 
                    THEN 0
                ELSE ROUND((ms.In_Qty / ms.max) * 100 ,0)
            END AS Percent
            FROM
              ms_stock ms
              LEFT JOIN ms_location ml ON ms.orderitemcode = ml.orderitemcode
            where ms.orderitemcode = '{orderitemcode}'
              ORDER BY Percent";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }

        public bool InsertStock(string DrugCode,int In_Qty,string LotNo,string Exp, string shelfzone, string shelfname,string max,string min)
        {
            SQL = $@" INSERT INTO ms_stock (In_Qty, LotNo, Exp, lastmodify,shelfzone,shelfname,max,min,log_refill,type_refill) VALUES ({In_Qty}, '{LotNo}','{Exp}', CURRENT_DATE),'{shelfzone}','{shelfname}','{max}','{min}',NULL,1 ; "; 

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
        public bool UpdateStockWhere(string DrugCode, int In_Qty, string LotNo, string Exp, string UserId)
        {
            SQL = $@" update ms_stock set 
                    In_Qty = {In_Qty},
                    lastmodify = CURRENT_DATE
                    where orderitemcode = '{DrugCode}' and LotNo = '{LotNo}'; ";

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
    }

    
}
