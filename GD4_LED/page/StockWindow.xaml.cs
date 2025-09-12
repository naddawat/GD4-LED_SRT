using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json; // เพิ่ม using สำหรับ JSON
using System.ComponentModel;
namespace GD4_LED.page
{
    /// <summary>
    /// Interaction logic for StockWindow.xaml
    /// </summary>
    public partial class StockWindow : Page
    {
        public ObservableCollection<DrugStockModel> DrugStocks { get; set; }
        private ObservableCollection<DrugStockModel> AllDrugStocks { get; set; }
        public int TotalCount => DrugStocks?.Count ?? 0;
        public int LowStockCount => DrugStocks?.Count(x => x.Quantity <= 0) ?? 0; // ปรับเงื่อนไขตามที่ต้องการ
        public StockWindow()
        {
            InitializeComponent();
            string json = @"[
  { ""location"": ""LED-A1"", ""lot"": ""Robot"", ""drugPosition"": ""403"", ""drugCode"": ""1071610"", ""Quantity"": 0, ""drugName"": ""AMOXY875+CLAVULANATE125MG TAB"", ""exp"": ""19/08/2025"", ""firmname"": ""ดีทแฮล์ม เคลเลอร์ โลจิสติกส์ จำกัด\\บริษัท"" },
  { ""drugCode"": ""1050190"", ""drugPosition"": ""502"", ""location"": ""LED-A1"", ""lot"": ""83CEP"", ""Quantity"": -1437298, ""drugName"": ""GABAPENTIN 300 MG CAP."", ""exp"": ""27/04/2027"", ""firmname"": ""บี.เอ็ล.เอช. เทร็ดดิ้ง จำกัด\\บริษัท"" },
  { ""location"": ""LED-A1"", ""lot"": ""DFG2303A"", ""drugCode"": ""1071610"", ""drugPosition"": ""403"", ""Quantity"": -4809, ""drugName"": ""AMOXY875+CLAVULANATE125MG TAB"", ""exp"": ""08/04/2027"", ""firmname"": ""ดีทแฮล์ม เคลเลอร์ โลจิสติกส์ จำกัด\\บริษัท"" },
  { ""drugPosition"": ""103"", ""location"": ""LED-L"", ""lot"": ""50399510a1"", ""drugCode"": ""5010080"", ""Quantity"": 6, ""drugName"": ""BOOST OPTIMUM 800 GM"", ""exp"": ""08/11/2027"", ""firmname"": null },
  { ""location"": ""LED-A2"", ""lot"": ""A12345"", ""drugPosition"": ""201"", ""drugCode"": ""2001001"", ""Quantity"": 120, ""drugName"": ""PARACETAMOL 500MG TAB"", ""exp"": ""01/02/2026"", ""firmname"": ""สยามฟาร์มา จำกัด\\บริษัท"" },
  { ""location"": ""LED-A2"", ""lot"": ""B67452"", ""drugPosition"": ""202"", ""drugCode"": ""2001002"", ""Quantity"": 85, ""drugName"": ""IBUPROFEN 400MG TAB"", ""exp"": ""15/05/2026"", ""firmname"": ""ไทยโอสถ จำกัด\\บริษัท"" },
  { ""location"": ""LED-A2"", ""lot"": ""C88231"", ""drugPosition"": ""203"", ""drugCode"": ""2001003"", ""Quantity"": 50, ""drugName"": ""DICLOFENAC 25MG TAB"", ""exp"": ""30/09/2026"", ""firmname"": ""บางกอกดรัก จำกัด\\บริษัท"" },
  { ""location"": ""LED-A3"", ""lot"": ""D90876"", ""drugPosition"": ""301"", ""drugCode"": ""2001004"", ""Quantity"": 200, ""drugName"": ""CETIRIZINE 10MG TAB"", ""exp"": ""10/01/2027"", ""firmname"": ""Pharma International\\บริษัท"" },
  { ""location"": ""LED-A3"", ""lot"": ""E12894"", ""drugPosition"": ""302"", ""drugCode"": ""2001005"", ""Quantity"": 75, ""drugName"": ""LORATADINE 10MG TAB"", ""exp"": ""05/03/2027"", ""firmname"": ""ยูไนเต็ดเมดิคอล จำกัด\\บริษัท"" },
  { ""location"": ""LED-A3"", ""lot"": ""F55521"", ""drugPosition"": ""303"", ""drugCode"": ""2001006"", ""Quantity"": 150, ""drugName"": ""RANITIDINE 150MG TAB"", ""exp"": ""19/07/2026"", ""firmname"": ""แกรนด์ฟาร์มา จำกัด\\บริษัท"" },
  { ""location"": ""LED-A4"", ""lot"": ""G76210"", ""drugPosition"": ""401"", ""drugCode"": ""2001007"", ""Quantity"": 300, ""drugName"": ""OMEPRAZOLE 20MG CAP"", ""exp"": ""12/11/2027"", ""firmname"": ""Medline Healthcare\\บริษัท"" },
  { ""location"": ""LED-A4"", ""lot"": ""H13579"", ""drugPosition"": ""402"", ""drugCode"": ""2001008"", ""Quantity"": 95, ""drugName"": ""ATORVASTATIN 10MG TAB"", ""exp"": ""25/02/2028"", ""firmname"": ""ไบโอเมดิค จำกัด\\บริษัท"" },
  { ""location"": ""LED-A5"", ""lot"": ""J24680"", ""drugPosition"": ""501"", ""drugCode"": ""2001009"", ""Quantity"": 40, ""drugName"": ""SIMVASTATIN 20MG TAB"", ""exp"": ""03/06/2028"", ""firmname"": ""เอเชียนฟาร์มา จำกัด\\บริษัท"" },
  { ""location"": ""LED-A5"", ""lot"": ""K36912"", ""drugPosition"": ""502"", ""drugCode"": ""2001010"", ""Quantity"": 180, ""drugName"": ""METFORMIN 500MG TAB"", ""exp"": ""17/08/2027"", ""firmname"": ""ไทยเมดิคอล ซัพพลาย\\บริษัท"" },
  { ""location"": ""LED-A6"", ""lot"": ""L48223"", ""drugPosition"": ""601"", ""drugCode"": ""2001011"", ""Quantity"": 210, ""drugName"": ""GLIBENCLAMIDE 5MG TAB"", ""exp"": ""22/01/2028"", ""firmname"": ""Bangkok Pharma\\บริษัท"" },
  { ""location"": ""LED-A6"", ""lot"": ""M59433"", ""drugPosition"": ""602"", ""drugCode"": ""2001012"", ""Quantity"": 95, ""drugName"": ""INSULIN HUMAN 100IU/ML"", ""exp"": ""14/04/2026"", ""firmname"": ""อินซูลินไทย จำกัด\\บริษัท"" },
  { ""location"": ""LED-A7"", ""lot"": ""N67544"", ""drugPosition"": ""701"", ""drugCode"": ""2001013"", ""Quantity"": 500, ""drugName"": ""ASPIRIN 81MG TAB"", ""exp"": ""09/09/2027"", ""firmname"": ""ยูนิเวอร์แซล ดรักส์\\บริษัท"" },
  { ""location"": ""LED-A7"", ""lot"": ""O78655"", ""drugPosition"": ""702"", ""drugCode"": ""2001014"", ""Quantity"": 65, ""drugName"": ""WARFARIN 5MG TAB"", ""exp"": ""28/10/2026"", ""firmname"": ""ฟาร์มาไทย จำกัด\\บริษัท"" },
  { ""location"": ""LED-A8"", ""lot"": ""P89766"", ""drugPosition"": ""801"", ""drugCode"": ""2001015"", ""Quantity"": 320, ""drugName"": ""AMLODIPINE 5MG TAB"", ""exp"": ""11/11/2027"", ""firmname"": ""Global Healthcare\\บริษัท"" },
  { ""location"": ""LED-A8"", ""lot"": ""Q90877"", ""drugPosition"": ""802"", ""drugCode"": ""2001016"", ""Quantity"": 275, ""drugName"": ""LOSARTAN 50MG TAB"", ""exp"": ""19/12/2028"", ""firmname"": ""ไทยเมดิคอล จำกัด\\บริษัท"" },
  { ""location"": ""LED-A9"", ""lot"": ""R01988"", ""drugPosition"": ""901"", ""drugCode"": ""2001017"", ""Quantity"": 140, ""drugName"": ""ENALAPRIL 10MG TAB"", ""exp"": ""23/03/2029"", ""firmname"": ""บางกอกเฮลท์แคร์\\บริษัท"" },
  { ""location"": ""LED-A9"", ""lot"": ""S12099"", ""drugPosition"": ""902"", ""drugCode"": ""2001018"", ""Quantity"": 70, ""drugName"": ""CARVEDILOL 25MG TAB"", ""exp"": ""29/07/2029"", ""firmname"": ""ไบโอฟาร์มา จำกัด\\บริษัท"" },
  { ""location"": ""LED-B1"", ""lot"": ""T23110"", ""drugPosition"": ""1001"", ""drugCode"": ""2001019"", ""Quantity"": 400, ""drugName"": ""AMITRIPTYLINE 25MG TAB"", ""exp"": ""07/05/2027"", ""firmname"": ""ไทยฟาร์มาเคมีคอล\\บริษัท"" },
  { ""location"": ""LED-B1"", ""lot"": ""U34221"", ""drugPosition"": ""1002"", ""drugCode"": ""2001020"", ""Quantity"": 180, ""drugName"": ""FLUOXETINE 20MG CAP"", ""exp"": ""12/08/2028"", ""firmname"": ""Asian Drug Supply\\บริษัท"" },
  { ""location"": ""LED-B2"", ""lot"": ""V45332"", ""drugPosition"": ""1101"", ""drugCode"": ""2001021"", ""Quantity"": 75, ""drugName"": ""SERTRALINE 50MG TAB"", ""exp"": ""03/02/2027"", ""firmname"": ""ยูโรฟาร์มา จำกัด\\บริษัท"" },
  { ""location"": ""LED-B2"", ""lot"": ""W56443"", ""drugPosition"": ""1102"", ""drugCode"": ""2001022"", ""Quantity"": 230, ""drugName"": ""ALPRAZOLAM 0.5MG TAB"", ""exp"": ""26/06/2028"", ""firmname"": ""Global Pharma\\บริษัท"" },
  { ""location"": ""LED-B3"", ""lot"": ""X67554"", ""drugPosition"": ""1201"", ""drugCode"": ""2001023"", ""Quantity"": 90, ""drugName"": ""DIAZEPAM 5MG TAB"", ""exp"": ""14/04/2027"", ""firmname"": ""บางกอกดรักส์\\บริษัท"" },
  { ""location"": ""LED-B3"", ""lot"": ""Y78665"", ""drugPosition"": ""1202"", ""drugCode"": ""2001024"", ""Quantity"": 360, ""drugName"": ""CLONAZEPAM 2MG TAB"", ""exp"": ""19/09/2029"", ""firmname"": ""Pharma Siam\\บริษัท"" },
  { ""location"": ""LED-B4"", ""lot"": ""Z89776"", ""drugPosition"": ""1301"", ""drugCode"": ""2001025"", ""Quantity"": 60, ""drugName"": ""LEVOTHYROXINE 50MCG TAB"", ""exp"": ""28/12/2026"", ""firmname"": ""ไทยไบโอฟาร์ม\\บริษัท"" }
]";

            AllDrugStocks = JsonConvert.DeserializeObject<ObservableCollection<DrugStockModel>>(json);

            DrugStocks = JsonConvert.DeserializeObject<ObservableCollection<DrugStockModel>>(json);
            this.DataContext = this;
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // กรองเมื่อกด Enter หรือทุก Key ก็ได้
            //if (e.Key == Key.Enter || e.Key == Key.Return)
            //{
            var textBox = sender as TextBox;
            string keyword = textBox.Text.Trim().ToLower();

            var filtered = AllDrugStocks.Where(x =>
                (x.drugCode != null && x.drugCode.ToLower().Contains(keyword)) ||
                (x.drugName != null && x.drugName.ToLower().Contains(keyword))
            ).ToList();

            DrugStocks.Clear();
            foreach (var item in filtered)
                DrugStocks.Add(item);

            // อัปเดตสรุป
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(LowStockCount));
            //}
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }
}
