using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GD4_LED_2.Models
{
    /// <summary>
    /// Model สำหรับใบสั่งยา
    /// </summary>
    public class Prescription
    {
        [JsonProperty("prescriptionno")]
        public string PrescriptionNo { get; set; }

        [JsonProperty("hn")]
        public string HN { get; set; }

        [JsonProperty("an")]
        public string AN { get; set; }

        [JsonProperty("patientname")]
        public string PatientName { get; set; }

        [JsonProperty("ward")]
        public string Ward { get; set; }

        [JsonProperty("bed")]
        public string Bed { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "รอจัด";

        [JsonProperty("package")]
        public List<Package> Package { get; set; } = new List<Package>();

        // Helper properties
        public int TotalItems => Package?.Count ?? 0;
        public string DisplayInfo => $"{PatientName} (HN: {HN})";
    }

    /// <summary>
    /// Model สำหรับรายการยา
    /// </summary>
    public class Package
    {
        [JsonProperty("orderitemcode")]
        public string OrderItemCode { get; set; }

        [JsonProperty("orderitemname")]
        public string OrderItemName { get; set; }

        [JsonProperty("orderqty")]
        public int OrderQty { get; set; }

        [JsonProperty("addr")]
        public string Addr { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }

    /// <summary>
    /// Model สำหรับสต็อกยา
    /// </summary>
    public class DrugStock
    {
        [JsonProperty("location")]
        public string Location { get; set; }
        
        [JsonProperty("lot")]
        public string Lot { get; set; }
        
        [JsonProperty("drugPosition")]
        public string DrugPosition { get; set; }
        
        [JsonProperty("led_id")]
        public int LedId { get; set; }
        
        [JsonProperty("drugCode")]
        public string DrugCode { get; set; }
        
        [JsonProperty("quantity")]
        public double Quantity { get; set; }
        
        [JsonProperty("drugName")]
        public string DrugName { get; set; }
        
        [JsonProperty("exp")]
        public string Exp { get; set; }
        
        [JsonProperty("firmname")]
        public string FirmName { get; set; }
        
        [JsonProperty("percent")]
        public double Percent { get; set; }
        
        [JsonProperty("min")]
        public double Min { get; set; }
        
        [JsonProperty("max")]
        public double Max { get; set; }

        [JsonProperty("itemcode")]
        public string ItemCode { get; set; }
        
        [JsonProperty("itemname")]
        public string ItemName { get; set; }
        
        [JsonProperty("addr")]
        public string Addr { get; set; }

        public bool IsLowStock => Quantity <= Min;
        public double StockPercentage => Max > 0 ? (double)Quantity / Max * 100 : 0;
    }

    /// <summary>
    /// Model สำหรับ Lot Detail
    /// </summary>
    public class LotDetail
    {
        public string Lot { get; set; }
        public string Exp { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Model สำหรับกลุ่มยาที่มี code เดียวกัน
    /// </summary>
    public class DrugStockGroupModel
    {
        public string DrugCode { get; set; }
        public string DrugName { get; set; }
        public string DrugPosition { get; set; }
        public string Location { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public List<LotDetail> LotDetails { get; set; } = new List<LotDetail>();
        public int TotalQuantity { get; set; }
        public double Percent { get; set; }
    }

    /// <summary>
    /// Model สำหรับ Refill Record
    /// </summary>
    public class RefillRecord
    {
        public string DrugCode { get; set; }
        public string DrugName { get; set; }
        public string Location { get; set; }
        public string LedId { get; set; }
        public string Quantity { get; set; }
        public string LotNumber { get; set; }
        public string DrugPosition { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime RefillDate { get; set; }
        public string UserId { get; set; }

        public override string ToString()
        {
            return $"DrugCode={DrugCode}, Quantity={Quantity}, Lot={LotNumber}, Expiry={ExpiryDate:yyyy-MM-dd}, RefillDate={RefillDate}, UserId={UserId}";
        }
    }
}
