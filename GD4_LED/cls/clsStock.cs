using GD4_LED.connect;
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

            return clsFillMyDB.GetDataSet(GD4_LED.Properties.Settings.Default.connectstring, SQL);
        }
    }
}
