# GD4_LED_2 - Optimized Version

## 🎯 เป้าหมายของการปรับปรุง

โปรเจ็คนี้เป็นเวอร์ชันที่ปรับปรุงจาก GD4_LED เดิม เพื่อแก้ไขปัญหา:
- ✅ โปรแกรมค้าง CPU ขึ้นสูง
- ✅ การทำงานติดๆ ขัด
- ✅ UI ไม่ลื่นไหล

## 🚀 การปรับปรุงหลัก

### 1. **Architecture แบบ MVVM Pattern**
```
GD4_LED_2/
├── Models/          → Data models (Prescription, Package, DrugStock)
├── ViewModels/      → Business logic with INotifyPropertyChanged
├── Views/           → XAML UI
├── Services/        → Async operations, database, performance monitoring
├── Helpers/         → Utilities (ViewModelBase, RelayCommand)
└── Converters/      → Value converters for data binding
```

### 2. **Async/Await Pattern**
- ใช้ `async/await` ทุกการเรียก database
- ใช้ `Task.Run()` สำหรับงานที่ใช้เวลานาน
- ใช้ `CancellationToken` เพื่อยกเลิกงานที่ไม่จำเป็น
- ป้องกัน UI thread block

**ตัวอย่าง:**
```csharp
// เดิม (Blocking UI)
DataTable dt = _query.GetPrescription(shelfZone);

// ใหม่ (Non-blocking)
var prescriptions = await _prescriptionService.GetPrescriptionsAsync(
    shelfZone, 
    cancellationToken);
```

### 3. **Performance Optimization**

#### 3.1 Database Connection Pool
```csharp
// จำกัด concurrent connections = 10
private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10);
```

#### 3.2 Caching System
```csharp
// Cache expire ทุก 30 วินาที แทนการ query ซ้ำๆ
private Dictionary<string, Prescription> _prescriptionCache;
private DateTime _lastCacheUpdate;
```

#### 3.3 Reduced Timer Frequency
```csharp
// เดิม: PerformanceTimer ทุก 2 วินาที
// ใหม่: PerformanceMonitor ทุก 5 วินาที (ลด CPU usage)

// เดิม: AutoRefresh ทุก 15 วินาที
// ใหม่: AutoRefresh ทุก 30 วินาที
```

#### 3.4 Search Debouncing
```csharp
// หน่วงเวลา 300ms ก่อนค้นหา
// ป้องกันการ query database ทุกครั้งที่พิมพ์
private readonly DispatcherTimer _searchDebounceTimer;
```

### 4. **Memory Management**
```csharp
// Proper disposal pattern
public void Dispose()
{
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
    _performanceMonitor?.Dispose();
}
```

### 5. **UI Improvements**
- ใช้ `ObservableCollection` แทน `List` สำหรับ data binding
- Loading indicators ที่ชัดเจน
- Smooth animations
- Proper error handling

## 📊 การเปรียบเทียบ

| ฟีเจอร์ | GD4_LED (เดิม) | GD4_LED_2 (ใหม่) |
|---------|----------------|------------------|
| **Architecture** | Code-behind | MVVM Pattern |
| **Database Call** | Synchronous | Asynchronous |
| **Caching** | ไม่มี | มี (30 sec expire) |
| **Performance Monitor** | 2 sec | 5 sec |
| **Auto Refresh** | 15 sec | 30 sec |
| **Search** | Immediate | Debounced (300ms) |
| **Connection Pool** | ไม่จำกัด | จำกัด 10 concurrent |
| **Memory Management** | Basic | Proper Disposal |

## 🔧 การติดตั้งและใช้งาน

### Prerequisites
- .NET 7.0 หรือสูงกว่า
- MySQL Database
- NuGet Packages:
  - `MySql.Data`
  - `Newtonsoft.Json`

### การตั้งค่า Connection String
แก้ไขใน `MainWindow.xaml.cs`:
```csharp
string connectionString = "Server=localhost;Database=gd4_led;Uid=root;Pwd=your_password;";
```

หรือเพิ่มใน `App.config`:
```xml
<connectionStrings>
    <add name="GD4_LED" 
         connectionString="Server=localhost;Database=gd4_led;Uid=root;Pwd=your_password;" 
         providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

### การ Build
```powershell
dotnet build
```

### การรัน
```powershell
dotnet run
```

## 📝 โครงสร้างฐานข้อมูล (ตัวอย่าง)

```sql
-- ตาราง prescriptions
CREATE TABLE prescriptions (
    prescriptionno VARCHAR(50) PRIMARY KEY,
    hn VARCHAR(20),
    an VARCHAR(20),
    patientname VARCHAR(100),
    ward VARCHAR(50),
    bed VARCHAR(20),
    shelfzone VARCHAR(50),
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ตาราง prescription_details
CREATE TABLE prescription_details (
    id INT AUTO_INCREMENT PRIMARY KEY,
    prescriptionno VARCHAR(50),
    orderitemcode VARCHAR(50),
    orderitemname VARCHAR(200),
    orderqty INT,
    addr VARCHAR(50),
    position_id VARCHAR(50),
    shelfname VARCHAR(100),
    FOREIGN KEY (prescriptionno) REFERENCES prescriptions(prescriptionno)
);

-- ตาราง ms_shelf (สำหรับ device mapping)
CREATE TABLE ms_shelf (
    id INT AUTO_INCREMENT PRIMARY KEY,
    computername VARCHAR(100),
    shelfzone VARCHAR(50),
    detail TEXT
);
```

## 🎨 UI Features

### MainWindow
- Header แสดง device name, version, CPU/RAM usage, datetime, database status
- Content area สำหรับแสดง views
- Bottom navigation (จ่ายยา, ประวัติ, Stock, ตั้งค่า)

### DispenseView
- Search bar พร้อม debouncing
- Statistics cards (ใบสั่งยาทั้งหมด, รายการยา, สถานะ)
- Prescription list with card design
- Loading indicator
- Refresh button

## 🔐 Best Practices ที่ใช้

1. **Separation of Concerns**: แยก UI, Business Logic, และ Data Access
2. **Async/Await**: ทุกการเรียก I/O operations
3. **Error Handling**: Try-catch with proper logging
4. **Resource Management**: Using statements และ Dispose pattern
5. **Data Binding**: Two-way binding with INotifyPropertyChanged
6. **Dependency Injection**: Ready for IoC container
7. **Cancellation**: ใช้ CancellationToken ทุก async operations

## 📈 Performance Metrics

**คาดหวัง:**
- CPU Usage: ลดลง 40-50%
- Memory Usage: ลดลง 20-30%
- UI Response Time: เร็วขึ้น 60-70%
- Database Query Time: เร็วขึ้น 50% (จาก caching)

## 🐛 Known Issues & TODO

- [ ] เพิ่ม Login Window
- [ ] เพิ่ม LED Control service
- [ ] เพิ่ม Printing service
- [ ] เพิ่ม History page
- [ ] เพิ่ม Stock management page
- [ ] เพิ่ม Settings page
- [ ] เพิ่ม Unit Tests
- [ ] เพิ่ม Logging system (NLog, Serilog)
- [ ] เพิ่ม Configuration management
- [ ] เพิ่ม Barcode scanning

## 📞 Support

หากพบปัญหาหรือต้องการความช่วยเหลือ กรุณาติดต่อทีมพัฒนา

## 📄 License

Copyright © 2025 GD4 LED System. All rights reserved.
