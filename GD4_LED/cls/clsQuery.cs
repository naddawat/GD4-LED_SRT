using GD4_LED.connect;
using MySql.Data.MySqlClient;
using Mysqlx.Expr;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.cls
{
    class clsQuery
    {
        clsutilDB clsINI = new clsutilDB();
        clsFillMyDB fill = new clsFillMyDB();
        clsExecuteMYSQL Execute = new clsExecuteMYSQL();
        public static string SQL;
        public static bool result;

        string connectst = GD4_LED.Properties.Settings.Default.connectstringlocal;
        public DataTable GetLedStock(string shelfzone)
        {

            SQL = $@" SELECT
                      ml.shelfzone AS location,
                      ml.shelfname AS drugPosition,
                      ml.position_id,
                      ms.LotNo AS lot,
                      ml.orderitemcode AS drugCode,
                      ml.orderitemENname AS drugName,
                      COALESCE(ms.In_Qty ,0) AS Quantity,
                      ms.Exp AS exp,
                      COALESCE(ml.max ,0) AS max,
                      COALESCE(ml.min ,0) AS min,
                      '' AS firmname,
                    CASE
    
                        WHEN COALESCE ( ms.In_Qty, 0.0 ) / COALESCE ( ms.max, 1 ) * 100 < 0 THEN
                        0 ELSE ROUND( COALESCE ( ms.In_Qty, 0.0 ) / COALESCE ( ms.max, 1 ) * 100, 0.0 ) 
                      END AS Percent 
                    FROM
                      ms_stock ms
                      RIGHT JOIN ms_location ml ON ms.orderitemcode = ml.orderitemcode 
                    WHERE
                      ml.shelfzone = '{shelfzone}' 
                    GROUP BY
                      ms.orderitemcode,
                      ms.LotNo,
                      ms.Exp 
                    ORDER BY
                      Percent";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
        public DataTable GetPrescription(string shelfzone)
        {

            SQL = $@" SELECT
                      prescriptionno,
                      hn,
                      an,
                      patientname,
                      wardname As ward,
                      bedcode as bed,
                      orderitemcode,
                      orderitemname,
                      orderqty,
                      shelfzone,
                      shelfname
                    FROM
                      packagemaster_ipd 
                    WHERE
                      shelfzone = '{shelfzone}' 
                      AND leddatetime IS NULL
                      AND voiddatetime is null
                      AND ordercreatedate > CURRENT_DATE();";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
        public DataTable GetPrescriptionByCode(string prescriptionno)
        {

            SQL = $@" SELECT
                      *
                    FROM
                      packagemaster_ipd 
                    WHERE
                      prescriptionno = '{prescriptionno}' 
                      AND leddatetime IS NULL
                      AND voiddatetime is null
                      AND shelfzone = '{clsvariable.shelfzone}' ";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
        public DataTable GetPrescrfinished(string shelfzone)
        {

            SQL = $@" SELECT
                      prescriptionno,
                      hn,
                      an,
                      patientname,
                      wardname As ward,
                      bedcode as bed,
                      orderitemcode,
                      orderitemname,
                      orderqty
                    FROM
                      packagemaster_ipd 
                    WHERE
                      shelfzone = '{shelfzone}' 
                      AND leddatetime IS not NULL
                      AND voiddatetime is null";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
        public DataTable GetStockByCode(string orderitemcode)
        {

            SQL = $@" SELECT
                    st.*,
                      STR_TO_DATE( st.Exp, '%Y-%m-%d' ) AS ExpDate ,
                      ml.position_id
                    FROM
                      ms_stock st LEFT JOIN ms_location ml on st.orderitemcode = ml.orderitemcode
                    Where 
                      st.orderitemcode = '{orderitemcode}'
                    ORDER BY
                      ExpDate;";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
        public DataTable GetLedInfo(string comname)
        {

            SQL = $@" SELECT *
            FROM
              ms_shelf ms
              
            Where ms.computername = '{comname}' ";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }        
        public DataTable GetLocation(string code,string device)
        {

            SQL = $@"  SELECT *
            FROM
              ms_location ml
            WHERE
              ml.orderitemcode = '{code}' 
              AND ml.shelfzone = '{device}'";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
        public DataTable GetLedStockByCode(string orderitemcode,string shelfzone)
        {

            SQL = $@"  SELECT
              ml.shelfzone AS location,
              ml.shelfname AS drugPosition,
              ms.LotNo AS lot,
              ml.orderitemcode AS drugCode,
              ml.orderitemENname AS drugName,
              ms.In_Qty AS Quantity,
              ms.Exp AS exp,
              ml.max AS max,
              ml.min AS min,
              '' AS firmname ,
              CASE 
                WHEN (ms.In_Qty / ml.max) * 100 < 0 
                    THEN 0
                ELSE ROUND((ms.In_Qty / ml.max) * 100 ,0)
            END AS Percent
            FROM
              ms_stock ms
              RIGHT JOIN ms_location ml ON ms.orderitemcode = ml.orderitemcode
            where ml.orderitemcode = '{orderitemcode}' and ml.shelfzone = '{shelfzone}'
              ORDER BY Percent";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }
        public DataTable GetLedStockByAddr(string Addr, string _id,string shelfzone)
        {

            SQL = $@"  SELECT
              ml.shelfzone AS location,
              ml.shelfname AS drugPosition,
              ms.LotNo AS lot,
              ml.orderitemcode AS drugCode,
              ml.orderitemENname AS drugName,
              ms.In_Qty AS Quantity,
              ms.Exp AS exp,
              ml.max AS max,
              ml.min AS min,
              '' AS firmname ,
              CASE 
                WHEN (ms.In_Qty / ml.max) * 100 < 0 
                    THEN 0
                ELSE ROUND((ms.In_Qty / ml.max) * 100 ,0)
            END AS Percent
            FROM
              ms_stock ms
              RIGHT JOIN ms_location ml ON ms.orderitemcode = ml.orderitemcode
            where ml.addr = '{Addr}' and ml.position_id = '{_id}' and ml.shelfzone = '{shelfzone}'
              ORDER BY Percent";

            return clsFillMyDB.GetDataSet(connectst, SQL);
        }

        public bool InsertStock(string DrugCode,int In_Qty,string LotNo,string Exp, string shelfzone, string shelfname,string max,string min)
        {
            SQL = $@" INSERT INTO ms_stock (orderitemcode,In_Qty, LotNo, Exp, lastmodify,shelfzone,shelfname,max,min,log_refill,type_refill) 
                        VALUES ('{DrugCode}',{In_Qty}, '{LotNo}','{Exp}',CURRENT_TIMESTAMP(),'{shelfzone}','{shelfname}','{max}','{min}',NULL,1) ; "; 

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
        public bool InsertLog(string eventstr, string user, string location)
        {
            SQL = $@" INSERT INTO logevent (event,createdate, user, location) 
                        VALUES ('{eventstr}',CURRENT_TIMESTAMP(), '{user}','{location}') ; ";

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
        public bool UpdateStockWhere(string DrugCode, int In_Qty, string LotNo, string Exp, string UserId)
        {
            SQL = $@" update ms_stock set 
                    In_Qty = {In_Qty},
                    lastmodify = CURRENT_TIMESTAMP()
                    where orderitemcode = '{DrugCode}' and LotNo = '{LotNo}'; ";

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
        public bool UpdatePrintStatus(string status,string comname)
        {
            SQL = $@" update ms_ledconfig set 
                    print_isenable = '{status}'
                    where comname = '{comname}'; ";

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
        public bool UpdateTrigger(string status, string comname)
        {
            SQL = $@" update ms_ledconfig set 
                    trigger_isenable = '{status}'
                    where comname = '{comname}'; ";

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
        public bool UpdateJob(string leduserid, string prescriptionno,string seq,string orderitemcode)
        {
            SQL = $@" UPDATE packagemaster_ipd 
                        SET leddatetime = CURRENT_TIMESTAMP(),
                        leduserid = '{leduserid}' 
                        WHERE
                        prescriptionno = '{prescriptionno}'
                        AND seq = '{seq}'
                        AND orderitemcode = '{orderitemcode}' ";

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
        public bool UpdateDisStock(string new_qty, string orderitem,string lot,string exp)
        {
            SQL = $@" UPDATE ms_stock SET
                                    In_Qty = '{new_qty}',
                                    lastmodify = NOW()
                                    WHERE
                                    orderitemcode = '{orderitem}'
                                    AND LotNo = '{lot}'
                                    AND Exp = '{exp}'
                                    AND shelfzone = '{clsvariable.shelfzone}' "; ;

            using (MySqlCommand cmd = new MySqlCommand(SQL))
            {
                return Execute.dataExecuteNonQuery(connectst, cmd);
            }
        }
        public static bool CheckConnection(string ConnectionString)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    return true; // ต่อได้
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false; // ต่อไม่ได้
            }
        }
    }

    
}
