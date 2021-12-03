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
        // PressureIndication pressureIndication;
        JsonReportSettings settings;

        public string FileName { get; private set; }

        public JsonAdapter(JsonReportSettings settings)
        {
            this.settings = settings;
        }

        public void AddResults(string serialNumber, int deviceType, int sensorType, bool primaryTest, TestResults results,
            PressureIndication pressureIndication, DateTime dateTime, int metrologicGroup)
        {
            bool isGood = testResults.GetResume() == TestResultEnum.IsGood;
            AddResults(serialNumber, deviceType, sensorType, primaryTest, results, pressureIndication, isGood, dateTime, 
                metrologicGroup);          
        }

        // Для ручной разбраковки - можно поставить любой результат
        public void AddResults(string serialNumber, int deviceType, int sensorType, bool primaryTest, TestResults results,
           PressureIndication pressureIndication, bool isGood, DateTime dateTime, int metrologicGroup)
        {
            testResults = results;
            // this.pressureIndication = pressureIndication;
            JsonReportResult = new JsonReportResult
            {
                deviceSN = serialNumber,
                deviceType = deviceType,
                lineId = settings.LineId,
                metrologGrID = metrologicGroup,
                measureTime = dateTime.ToString(),
                standHardwareVer = settings.StandHardwareVer,
                standFirmwareVer = Application.ProductVersion,
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
            int pressureUnitCode = (int)testResults.PressureUnits;

            JsonTestPointData jsonTestPointData = new JsonTestPointData()
            {
                channelNumber = 1,
                checkPoint = settings.PointsCode[number],
                sensorType = sensotType
            };

            List<JsonParametr> parametrs = new List<JsonParametr>();
            parametrs.Add(new JsonParametr(1 + pressureUnitCode, testPointUpwards.ReferencePressure.PressureByUnits));
            if (testPointUpwards is CurrentCheckPoint)
            {
                parametrs.Add(new JsonParametr(4, (testPointUpwards as CurrentCheckPoint).CurrentFromEtalonPressure));
                parametrs.Add(new JsonParametr(4, (testPointUpwards as CurrentCheckPoint).MeasuredCurrent));
            }
            parametrs.Add(new JsonParametr(6 + pressureUnitCode, testPointUpwards.Pressure.PressureByUnits));
            parametrs.Add(new JsonParametr(9, testPointUpwards.ErrorMeasure));
            parametrs.Add(new JsonParametr(10 + pressureUnitCode, testPointTopdown.ReferencePressure.PressureByUnits));
            if (testPointTopdown is CurrentCheckPoint)
            {
                parametrs.Add(new JsonParametr(13, (testPointTopdown as CurrentCheckPoint).CurrentFromEtalonPressure));
                parametrs.Add(new JsonParametr(13, (testPointTopdown as CurrentCheckPoint).MeasuredCurrent));
            }
            parametrs.Add(new JsonParametr(15 + pressureUnitCode, testPointTopdown.Pressure.PressureByUnits));
            parametrs.Add(new JsonParametr(18, testPointTopdown.ErrorMeasure));


            var variation = testResults.Variations.GetVariationPointByPercent(testPointUpwards.PercentRange);
            if (variation != null)
                parametrs.Add(new JsonParametr(19, variation.Value));

            jsonTestPointData.dataMetrological = parametrs.ToArray();

            return jsonTestPointData;
        }
        
    }
}
