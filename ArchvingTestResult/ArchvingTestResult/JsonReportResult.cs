using System;
using Newtonsoft.Json;
using System.Globalization;

namespace ArchvingTestResult
{
    public class JsonReportResult
    {
        const string dateTimeFormat = "yyyy-MM-ddTHH:mm:sszz";


        public string dataSource = "stand";
        // Тип изделия
        public int deviceType { get; set; }
        // SN прибора
        public string deviceSN { get; set; } = "";
        // SN для Danfos
        public string deviceSNDF { get; set; } = "";
        // Метрологическая группа
        public int metrologGrID { get; set; }

        public JsonSemiFinishedData[] semiFinished { get; set; } = new JsonSemiFinishedData[0];
        // Дата время
        string time;
        public string measureTime
        {
            get
            {
                return time;
            }
            set
            {
                DateTime dt = Convert.ToDateTime(value);
                time = dt.ToString(dateTimeFormat);
            }
        }
        // Id стенда
        public int standId { get; set; }
        // Id линейки
        public string lineId { get; set; }
        // ПО стенда
        public string standFirmwareVer { get; set; }
        // Аппаратная часть стенда
        public string standHardwareVer { get; set; }
        // Вывод 
        public bool isGood { get; set; }
        // Флаг первичной поверки
        public bool primaryVerification { get; set; }
        // Таблица результатов
        public JsonTestPointData[] measuresMetrological { get; set; }
        // Дополнительные параметры
        public JsonTestPointData[] measuresCommon { get; set; } = new JsonTestPointData[0];
       
        

        public string GetJsonByString()
        {
            var sett = new JsonSerializerSettings();
            var cult = CultureInfo.CurrentCulture;
            
            sett.Culture = CultureInfo.CurrentCulture;
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void AddCurrentDateTime()
        {
            var dt = DateTime.Now;
            measureTime = dt.ToString(dateTimeFormat);
        }

        public string GetFileName()
        {
            return deviceSN + "_" + measureTime.Replace(':', '.') + ".json";
        }
    }

    
}
