using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.models
{
    internal class DispensePage
    {

    }

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
        public List<Package> Package { get; set; }
    }

    public class Package
    {
        [JsonProperty("orderitemcode")]
        public string OrderItemCode { get; set; }

        [JsonProperty("orderitemname")]
        public string OrderItemName { get; set; }

        [JsonProperty("orderqty")]
        public int OrderQty { get; set; }
    }
}
