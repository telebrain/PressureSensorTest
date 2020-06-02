using System;
using System.Collections.Generic;
using ArchvingTestResult;
using PressureSensorTestCore;
using System.Windows.Forms;

namespace PressureSensorTest
{
    public class JsonAdapter
    {
        public JsonReportResult JsonReportResult { get; private set; }

        TestResults testResults;
        PressureIndication pressureIndication;
        JsonReportSettings settings;

        public string FileName { get; private set; }

        public JsonAdapter(JsonReportSettings settings)
        {
            this.settings = settings;
        }

        public void AddResults(string serialNumber, int deviceType, int sensorType, bool primaryTest, TestResults results,
            PressureIndication pressureIndication, DateTime dateTime)
        {
            bool isGood = testResults.GetResume() == true;
            AddResults(serialNumber, deviceType, sensorType, primaryTest, results, pressureIndication, isGood, dateTime);          
        }

        // Для ручной разбраковки - можно поставить любой результат
        public void AddResults(string serialNumber, int deviceType, int sensorType, bool primaryTest, TestResults results,
           PressureIndication pressureIndication, bool isGood, DateTime dateTime)
        {
            testResults = results;
            this.pressureIndication = pressureIndication;
            JsonReportResult = new JsonReportResult
            {
                deviceSN = serialNumber,
                deviceType = deviceType,
                lineId = settings.LineId,
                measureTime = dateTime.ToString(),
                hardwareStandVer = settings.StandHardwareVer,
                firmwareStandVer = Application.ProductVersion,
                standId = settings.StandId,
                primaryVerification = primaryTest,
                isGood = isGood
            };
            List<JsonTestPointData> pointsData = new List<JsonTestPointData>();
            for (int i = 0; i < settings.PointsCode.Length; i++)
            {
                pointsData.Add(GetJsonTestPoint(i, sensorType));
            }
            JsonReportResult.measuresMetrological = pointsData.ToArray();
        }

        const string CurrFormat = "0.000";
        const string ErrFormat = "0.000";
        

        private JsonTestPointData GetJsonTestPoint(int number, int sensotType)
        {
            var testPointUpwards = testResults.MeasureResultsUpwards[number];
            var testPointTopdown = testResults.MeasureResultsTopdown[testResults.MeasureResultsTopdown.Count - number - 1];
            JsonTestPointData jsonTestPointData = new JsonTestPointData()
            {
                channelNumber = 1,
                checkPoint = settings.PointsCode[number],
                sensorType = sensotType,
                dataMetrological = new JsonParametr[]
                {
                    new JsonParametr(1, Convert.ToSingle(pressureIndication.GetPressure(testPointUpwards.EtalonPressure))),
                    new JsonParametr(2, Convert.ToSingle(testPointUpwards.CurrentFromEtalonPressure.ToString(CurrFormat))),
                    new JsonParametr(3, Convert.ToSingle(testPointUpwards.MeasuredCurrent.ToString(CurrFormat))),
                    new JsonParametr(4, Convert.ToSingle(pressureIndication.GetPressure(testPointUpwards.Pressure))),
                    new JsonParametr(5, Convert.ToSingle(testPointUpwards.ErrorMeasure.Value.ToString(ErrFormat))),
                    new JsonParametr(6, Convert.ToSingle(pressureIndication.GetPressure(testPointTopdown.EtalonPressure))),
                    new JsonParametr(7, Convert.ToSingle(testPointTopdown.CurrentFromEtalonPressure.ToString(CurrFormat))),
                    new JsonParametr(8, Convert.ToSingle(testPointTopdown.MeasuredCurrent.ToString(CurrFormat))),
                    new JsonParametr(9, Convert.ToSingle(pressureIndication.GetPressure(testPointTopdown.Pressure))),
                    new JsonParametr(10, Convert.ToSingle(testPointTopdown.ErrorMeasure.Value.ToString(ErrFormat))),
                    new JsonParametr(11, Convert.ToSingle(testResults.Variations[number].Value.ToString(ErrFormat)))
                }
            };
            return jsonTestPointData;
        }
        
    }
}
