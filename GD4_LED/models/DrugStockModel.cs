using System;

public class DrugStockModel
{
    public string location { get; set; }
    public string lot { get; set; }
    public string drugPosition { get; set; }
    public string drugCode { get; set; }
    public double Quantity { get; set; }
    public string drugName { get; set; }
    public string exp { get; set; }
    public string firmname { get; set; }
    public double Percent { get; set; }
    public double min { get; set; }
    public double max { get; set; }
}

public class RefillRecord
{
    public string DrugCode { get; set; }       // รหัสยา
    public string DrugName { get; set; }       // รหัสยา
    public string Location { get; set; }       // รหัสยา
    public string Quantity { get; set; }          // จำนวน
    public string LotNumber { get; set; }      // เลข Lot
    public string DrugPosition { get; set; }      // เลข Lot
    public DateTime ExpiryDate { get; set; }   // วันหมดอายุ
    public DateTime RefillDate { get; set; }   // วันที่เติมยา
    public string UserId { get; set; }         // ผู้ใช้ที่บันทึก

    public override string ToString()
    {
        return $"DrugCode={DrugCode}, Quantity={Quantity}, Lot={LotNumber}, Expiry={ExpiryDate:yyyy-MM-dd}, RefillDate={RefillDate}, UserId={UserId}";
    }
}