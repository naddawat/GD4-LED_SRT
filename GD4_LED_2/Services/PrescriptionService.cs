using GD4_LED_2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD4_LED_2.Services
{
    /// <summary>
    /// Service สำหรับจัดการใบสั่งยา ที่ optimize แล้ว (ใช้วิธีโหลดข้อมูลเหมือน GD4_LED)
    /// </summary>
    public class PrescriptionService
    {
        private readonly DatabaseService _dbService;
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        private Dictionary<string, Prescription> _prescriptionCache = new Dictionary<string, Prescription>();
        private List<Prescription> _allPrescriptions = new List<Prescription>();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private const int CACHE_EXPIRY_SECONDS = 30; // Cache expire ทุก 30 วินาที

        public PrescriptionService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// โหลดใบสั่งยาทั้งหมดแบบ Async พร้อม Cache (ใช้วิธีเดียวกับ GD4_LED)
        /// </summary>
        public async Task<List<Prescription>> GetPrescriptionsAsync(string shelfZone, CancellationToken cancellationToken = default)
        {
            try
            {
                // ตรวจสอบ cache ก่อน
                if (_allPrescriptions.Count > 0 && 
                    (DateTime.Now - _lastCacheUpdate).TotalSeconds < CACHE_EXPIRY_SECONDS)
                {
                    return _allPrescriptions.ToList();
                }

                // Query database ด้วย GetPrescriptionAsync จาก DatabaseService
                var dataTable = await _dbService.GetPrescriptionAsync(shelfZone, cancellationToken);

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    _allPrescriptions.Clear();
                    _prescriptionCache.Clear();
                    return new List<Prescription>();
                }

                // Process data แบบเดียวกับ GD4_LED - Group by prescriptionno
                var prescriptions = dataTable.AsEnumerable()
                    .GroupBy(r => r["prescriptionno"].ToString())
                    .Select(g => new
                    {
                        prescriptionno = g.Key,
                        hn = g.First()["hn"].ToString(),
                        an = g.First()["an"].ToString(),
                        patientname = g.First()["patientname"].ToString(),
                        ward = g.First()["ward"].ToString(),
                        bed = g.First()["bed"].ToString(),
                        status = "รอจัด",
                        package = g.Select(r => new
                        {
                            orderitemcode = r["orderitemcode"].ToString(),
                            orderitemname = r["orderitemname"].ToString(),
                            orderqty = Convert.ToInt32(r["orderqty"]),
                            addr = r["addr"].ToString(),
                            id = r["position_id"].ToString(),
                            location = r["shelfname"].ToString()
                        }).ToList()
                    })
                    .ToList();

                // Serialize to JSON and deserialize back to Prescription objects (เหมือน GD4_LED)
                string jsonResult = JsonConvert.SerializeObject(prescriptions, Formatting.None);
                _allPrescriptions = JsonConvert.DeserializeObject<List<Prescription>>(jsonResult);

                // Update cache
                await _cacheLock.WaitAsync(cancellationToken);
                try
                {
                    _prescriptionCache.Clear();
                    foreach (var p in _allPrescriptions)
                    {
                        _prescriptionCache[p.PrescriptionNo] = p;
                    }
                    _lastCacheUpdate = DateTime.Now;
                }
                finally
                {
                    _cacheLock.Release();
                }

                return _allPrescriptions.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting prescriptions: {ex.Message}");
                return new List<Prescription>();
            }
        }

        /// <summary>
        /// โหลดใบสั่งยาเฉพาะ prescription number (ใช้วิธีเดียวกับ GD4_LED)
        /// </summary>
        public async Task<Prescription> GetPrescriptionByNumberAsync(string prescriptionNo, string shelfZone, CancellationToken cancellationToken = default)
        {
            // ตรวจสอบ cache ก่อน
            if (_prescriptionCache.ContainsKey(prescriptionNo))
            {
                return _prescriptionCache[prescriptionNo];
            }

            try
            {
                var dataTable = await _dbService.GetPrescriptionByCodeAsync(prescriptionNo, shelfZone, cancellationToken);

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    return null;
                }

                // Process data แบบเดียวกับ GD4_LED
                var prescriptions = dataTable.AsEnumerable()
                    .GroupBy(r => r["prescriptionno"].ToString())
                    .Select(g => new
                    {
                        prescriptionno = g.Key,
                        hn = g.First()["hn"].ToString(),
                        an = g.First()["an"].ToString(),
                        patientname = g.First()["patientname"].ToString(),
                        ward = g.First()["wardname"].ToString(),
                        bed = g.First()["bedcode"].ToString(),
                        status = "รอจัด",
                        package = g.Select(r => new
                        {
                            orderitemcode = r["orderitemcode"].ToString(),
                            orderitemname = r["orderitemname"].ToString(),
                            orderqty = Convert.ToInt32(r["orderqty"]),
                            addr = r["addr"].ToString(),
                            id = r["position_id"].ToString(),
                            location = r["shelfname"].ToString()
                        }).ToList()
                    })
                    .ToList();

                string jsonResult = JsonConvert.SerializeObject(prescriptions, Formatting.None);
                var result = JsonConvert.DeserializeObject<List<Prescription>>(jsonResult);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting prescription {prescriptionNo}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// อัพเดทสถานะใบสั่งยา
        /// </summary>
        public async Task<bool> UpdatePrescriptionStatusAsync(string prescriptionNo, string status, CancellationToken cancellationToken = default)
        {
            try
            {
                string query = $@"
                    UPDATE prescriptions 
                    SET status = '{status}',
                        updated_at = NOW()
                    WHERE prescriptionno = '{prescriptionNo}'";

                int result = await _dbService.ExecuteNonQueryAsync(query, cancellationToken);

                if (result > 0)
                {
                    // อัพเดท cache
                    if (_prescriptionCache.ContainsKey(prescriptionNo))
                    {
                        _prescriptionCache[prescriptionNo].Status = status;
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating prescription status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ค้นหาใบสั่งยา (ใช้วิธีเดียวกับ GD4_LED - ค้นหาจาก cache หรือโหลดจาก database)
        /// </summary>
        public async Task<List<Prescription>> SearchPrescriptionsAsync(string keyword, string shelfZone, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _allPrescriptions.ToList();
            }

            keyword = keyword.Trim();

            // ค้นหาใน cache ก่อน
            var filtered = _allPrescriptions
                .Where(p => 
                    p.PrescriptionNo.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.HN.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.PatientName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // ถ้าไม่เจอ ให้โหลดจาก database ด้วย prescription code (เหมือน GD4_LED)
            if (filtered.Count == 0)
            {
                var dataTable = await _dbService.GetPrescriptionByCodeAsync(keyword, shelfZone, cancellationToken);
                
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    // Process data แบบเดียวกับ LoadPrescriptionsFromDatabaseAsync
                    var prescriptions = dataTable.AsEnumerable()
                        .GroupBy(r => r["prescriptionno"].ToString())
                        .Select(g => new
                        {
                            prescriptionno = g.Key,
                            hn = g.First()["hn"].ToString(),
                            an = g.First()["an"].ToString(),
                            patientname = g.First()["patientname"].ToString(),
                            ward = g.First()["wardname"].ToString(),
                            bed = g.First()["bedcode"].ToString(),
                            status = "รอจัด",
                            package = g.Select(r => new
                            {
                                orderitemcode = r["orderitemcode"].ToString(),
                                orderitemname = r["orderitemname"].ToString(),
                                orderqty = Convert.ToInt32(r["orderqty"]),
                                addr = r["addr"].ToString(),
                                id = r["position_id"].ToString(),
                                location = r["shelfname"].ToString()
                            }).ToList()
                        })
                        .ToList();

                    string jsonResult = JsonConvert.SerializeObject(prescriptions, Formatting.None);
                    filtered = JsonConvert.DeserializeObject<List<Prescription>>(jsonResult);
                }
            }

            return filtered;
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public async Task ClearCacheAsync()
        {
            await _cacheLock.WaitAsync();
            try
            {
                _prescriptionCache.Clear();
                _allPrescriptions.Clear();
                _lastCacheUpdate = DateTime.MinValue;
            }
            finally
            {
                _cacheLock.Release();
            }
        }
    }
}
