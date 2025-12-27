using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace GD4_LED_2.Services
{
    /// <summary>
    /// Service สำหรับจัดการ Database ที่ optimize แล้ว
    /// ใช้ async/await และ CancellationToken เพื่อป้องกันการค้าง
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10); // จำกัด concurrent connections

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// ตรวจสอบการเชื่อมต่อ Database แบบ Async
        /// </summary>
        public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                try
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        await connection.OpenAsync(cancellationToken);
                        return connection.State == ConnectionState.Open;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Execute query และคืนค่า DataTable แบบ Async
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string query, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = 30; // Timeout 30 วินาที
                    await connection.OpenAsync(cancellationToken);

                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        // Run Fill in background thread
                        await Task.Run(() => adapter.Fill(dataTable), cancellationToken);
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Query execution error: {ex.Message}");
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Execute non-query command (INSERT, UPDATE, DELETE) แบบ Async
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string query, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = 30;
                    await connection.OpenAsync(cancellationToken);
                    return await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Non-query execution error: {ex.Message}");
                return -1;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Execute scalar query แบบ Async
        /// </summary>
        public async Task<object> ExecuteScalarAsync(string query, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandTimeout = 30;
                    await connection.OpenAsync(cancellationToken);
                    return await command.ExecuteScalarAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scalar execution error: {ex.Message}");
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// ดึงข้อมูล Prescription จาก database ตาม shelfzone (จากโปรเจ็ค GD4_LED)
        /// </summary>
        public async Task<DataTable> GetPrescriptionAsync(string shelfzone, CancellationToken cancellationToken = default)
        {
            string sql = $@" SELECT
                      p.prescriptionno,
                      p.hn,
                      p.an,
                      p.patientname,
                      p.wardname As ward,
                      p.bedcode as bed,
                      p.orderitemcode,
                      p.orderitemname,
                      TRUNCATE(p.orderqty,0) as orderqty,
                      p.shelfzone,
                      p.shelfname,
                      ms.addr,
                      ms.position_id
                    FROM
                      packagemaster_ipd  p INNER JOIN ms_location ms on p.orderitemcode = ms.orderitemcode and p.shelfname = ms.shelfname
                    WHERE
                      p.shelfzone = '{shelfzone}' 
                      AND p.leddatetime IS NULL
                      AND p.voiddatetime is null
                      AND p.lastmodified  > CURRENT_DATE();";

            return await ExecuteQueryAsync(sql, cancellationToken);
        }

        /// <summary>
        /// ดึงข้อมูล Prescription ตาม prescription number (จากโปรเจ็ค GD4_LED)
        /// </summary>
        public async Task<DataTable> GetPrescriptionByCodeAsync(string prescriptionno, string shelfzone, CancellationToken cancellationToken = default)
        {
            string sql = $@" SELECT
                      *
                    FROM
                      packagemaster_ipd 
                    WHERE
                      prescriptionno = '{prescriptionno}' 
                      AND leddatetime IS NULL
                      AND voiddatetime is null
                      AND shelfzone = '{shelfzone}' ";

            return await ExecuteQueryAsync(sql, cancellationToken);
        }
    }
}
