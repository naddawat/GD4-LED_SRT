using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.cls
{
    class clsutilDB
    {
        public string ConStrmysql = GD4_LED.Properties.Settings.Default.connectstring;

        public static string convertdate_DD_Mon_YYYY_en(DateTime val)
        {
            return val.ToString("yyyyMMddhhmm ", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

            //  return val.ToString("dd MMM yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("th-TH"));

        }

        public static string convertdate_YYYY_MM_DD(DateTime val)
        {
            return val.ToString("yyyy-MM-dd ", System.Globalization.CultureInfo.GetCultureInfo("en-US"));



        }
        public static string convertdate_YYYYMMDD(DateTime val)
        {
            return val.ToString("yyyyMMdd ", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        }

        public static string convertdate_DD_Mon_YYYY_(DateTime val)
        {
            return val.ToString("dd/MM/yyyy");

        }
        public static string convertdate_DD_Mon_YYYY_TH(DateTime val)
        {
            return val.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("th-TH"));

        }
        public static string convertdatetime_DD_Mon_YYYY_TH(DateTime val)
        {

            return val.ToString("dd/MM/yyyy HH:mm ", System.Globalization.CultureInfo.CreateSpecificCulture("th-TH"));

        }
        public static string convertdatetime_YY(DateTime val)
        {
            return val.ToString("yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

        }
        public static string convertdatetime_DD_Mon(DateTime val)
        {
            return val.ToString("dd-MM-", System.Globalization.CultureInfo.CreateSpecificCulture("th-TH"));

        }
        public static string convertdatetime_DDMMYYYY_TH(DateTime val)
        {
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            exp = val.ToString().Split(' ');
            year = exp[0].Split('/');
            date = year[0].ToString() + "-" + year[1].ToString();
            //date = val.ToString().Substring(1, 6);

            if (Convert.ToInt32(year[2]) > 2000 && Convert.ToInt32(year[2]) < 2500)
            {
                buddhistYear = Convert.ToInt32(year[2]) + 543;
                date = date + "-" + buddhistYear.ToString();


            }
            else if (Convert.ToInt32(year[2]) > 2500)
            {
                date = exp[0].ToString();
            }

            return date;

        }
        public static string convertdate_DDMMYYYY_HHmmSS_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(8, 2);
                DT += val_.Substring(5, 2);
                DT += val_.Substring(0, 4);
                if (Convert.ToInt32(DT) > 2000 && Convert.ToInt32(DT) < 2500)
                {
                    buddhistYear = Convert.ToInt32(DT) + 543;
                    DT = buddhistYear.ToString();

                }
                DT += val_.Substring(9, 9);


                //DT += DateTime.Now.ToString(" HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }
            return DT;
        }
        public static string convertdate_DDMMYYYY_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(8, 2);
                DT += val_.Substring(5, 2);
                //DT += val_.Substring(0, 4);

                if (Convert.ToInt32(val_.Substring(0, 4)) > 2000 && Convert.ToInt32(val_.Substring(0, 4)) < 2500)
                {
                    buddhistYear = Convert.ToInt32(val_.Substring(0, 4)) + 543;
                    DT += buddhistYear.ToString();

                }
                //DT += val_.Substring(9, 9);


                //DT += DateTime.Now.ToString(" HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }
            return DT;
        }
        public static string convertdate_YYYY_MM_DD_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(0, 4);
                if (Convert.ToInt32(DT) > 2000 && Convert.ToInt32(DT) < 2500)
                {
                    buddhistYear = Convert.ToInt32(DT) + 543;
                    DT = buddhistYear.ToString();

                }
                DT += "-";
                DT += val_.Substring(5, 2) + "-";
                DT += val_.Substring(8, 2);
                //DT += DateTime.Now.ToString(" HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }
            return DT;
        }
        public static string convertdate_YYYYMMDD_EN_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(0, 4);
                //if (Convert.ToInt32(DT) > 2000 && Convert.ToInt32(DT) < 2500)
                //{
                //    buddhistYear = Convert.ToInt32(DT) + 543;
                //    DT = buddhistYear.ToString();

                //}

                DT += val_.Substring(5, 2);
                DT += val_.Substring(8, 2);
                //DT += DateTime.Now.ToString(" HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }
            return DT;
        }
        public static string convertdate_YYYY_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                //DT += val_.Substring(0, 4);
                //if (Convert.ToInt32(DT) > 2000 && Convert.ToInt32(DT) < 2500)
                //{
                //    buddhistYear = Convert.ToInt32(DT) + 543;
                //    DT = buddhistYear.ToString();

                //}
                //DT += "-";
                //DT += val_.Substring(5, 2) + "-";
                //DT += val_.Substring(8, 2);
                //DT += DateTime.Now.ToString(" HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }
            return DT;
        }
        public static string convertdate_DD_MM_YYYY_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(8, 2) + "/";
                DT += val_.Substring(5, 2) + "/";


                if (Convert.ToInt32(val_.Substring(0, 4)) > 2000 && Convert.ToInt32(val_.Substring(0, 4)) < 2500)
                {
                    buddhistYear = Convert.ToInt32(val_.Substring(0, 4)) + 543;
                    DT += buddhistYear.ToString();

                }
                else
                {
                    DT += val_.Substring(0, 4);
                }

            }
            return DT;
        }
        public static string convertdate_DD_Mouth_YYYY_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(8, 2) + " ";
                //DT += val_.Substring(5, 2) ;

                switch (val_.Substring(5, 2))
                {
                    case "01":
                        DT += " มกราคม ";
                        break;
                    case "02":
                        DT += " กุมภาพันธ์ ";
                        break;
                    case "03":
                        DT += " มีนาคม ";
                        break;
                    case "04":
                        DT += " เมษายน ";
                        break;
                    case "05":
                        DT += " พฤษภาคม ";
                        break;
                    case "06":
                        DT += " มิถุนายน ";
                        break;
                    case "07":
                        DT += " กรกฎาคม ";
                        break;
                    case "08":
                        DT += " สิงหาคม ";
                        break;
                    case "09":
                        DT += " กันยายน ";
                        break;
                    case "10":
                        DT += " ตุลาคม ";
                        break;
                    case "11":
                        DT += " พฤศจิกายน ";
                        break;
                    case "12":
                        DT += " ธันวาคม ";
                        break;

                }

                if (Convert.ToInt32(val_.Substring(0, 4)) > 2000 && Convert.ToInt32(val_.Substring(0, 4)) < 2500)
                {
                    buddhistYear = Convert.ToInt32(val_.Substring(0, 4)) + 543;
                    DT += "พ.ศ." + buddhistYear.ToString();

                }
                else
                {
                    DT += val_.Substring(0, 4);
                }

            }
            return DT;
        }
        public static string convertdate_DD_MM_YYYY__HH_mm_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(8, 2) + "/";
                DT += val_.Substring(5, 2) + "/";


                if (Convert.ToInt32(val_.Substring(0, 4)) > 2000 && Convert.ToInt32(val_.Substring(0, 4)) < 2500)
                {
                    buddhistYear = Convert.ToInt32(val_.Substring(0, 4)) + 543;
                    DT += buddhistYear.ToString();

                }
                DT += val_.Substring(10);
            }
            return DT;
        }
    }
}
