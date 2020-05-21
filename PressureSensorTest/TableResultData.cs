using System;
using PressureSensorTestCore;


namespace PressureSensorTest
{
    public class TableResultData
    {
        public static int Rows { get; } = 5;
        public static int Columns { get; } = 11;

        public ElementTableResult[,] Data { get; set; }
        public string Units { get; set; }

        PressureIndication pressureIndication;

        public TableResultData()
        {
            Data = new ElementTableResult[Rows, Columns];
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    Data[row, column] = new ElementTableResult("", ColorItem.Neutral);
                }
            }
        }

        public TableResultData(TestResults testResults): this()
        {
            if (testResults != null)
            {
                pressureIndication = new PressureIndication(testResults.RangeMax);
                Units = pressureIndication.UnitLable;
                AddResultUpwards(testResults.MeasureResultsUpwards);
                AddResultTopDown(testResults.MeasureResultsTopdown);
                AddVariations(testResults.Variations);
            }
        }

        private void AddResultUpwards(MeasureResults results)
        {
            AddMeasureResults(results, 0, false);
        }

        private void AddResultTopDown(MeasureResults results)
        {
            AddMeasureResults(results, 5, true);
        }

        private void AddMeasureResults(MeasureResults results, int shiftColumn, bool reverse)
        {
            const string CurrentFormat = "0.000";
            const string ErrorFormat = "0.000";

            if (results != null)
            {
                for (int pos = 0; pos < results.Count; pos++)
                {
                    int row = pos;
                    if (reverse)
                        row = Rows - pos - 1; 
                    var point = results[pos];
                    Data[row, shiftColumn].Content = pressureIndication.GetPressure((double)point.EtalonPressure);
                    Data[row, 1 + shiftColumn].Content = (Convert.ToSingle(point.CurrentFromEtalonPressure)).ToString(CurrentFormat);
                    Data[row, 2 + shiftColumn].Content = (Convert.ToSingle(point.MeasuredCurrent)).ToString(CurrentFormat);
                    Data[row, 3 + shiftColumn].Content = pressureIndication.GetPressure((double)point.Pressure);
                    Data[row, 4 + shiftColumn].Content = (Convert.ToSingle(point.ErrorMeasure)).ToString(ErrorFormat);
                    Data[row, 4 + shiftColumn].Color = ResumeToColor(results[pos].Resume);                   
                }
            }
        }

        private void AddVariations(Variations result)
        {
            const string VariationFormat = "0.000";
            if (result != null)
            {
                for (int row = 0; row < result.Count; row++)
                {
                    Data[row, 10].Content = result[row].Value.ToString(VariationFormat);
                    Data[row, 10].Color = ResumeToColor(result[row].Resume);
                }
            }
        }

        private ColorItem ResumeToColor(bool? resume)
        {
            if (resume == true)
                return ColorItem.Ok;
            else if (resume == false)
                return ColorItem.Error;
            else
                return ColorItem.Neutral;
        }

        //private string SetUnitsAndGetPressureFormat(double rangeMax)
        //{        
        //    if (rangeMax >= 10000)
        //    {
        //        Units = kPa;
        //        multipler = 0.001F;              
        //    }
        //    else
        //    {
        //        Units = Pa;
        //        multipler = 1;
        //    }

        //    double range = rangeMax * multipler;
        //    if (range < 10)
        //        return "0.0000";
        //    if (range < 100)
        //        return "0.000";
        //    return "0.00";
        //}

        // public event PropertyChangedEventHandler PropertyChanged;

    }

    public struct ElementTableResult
    {
        public string Content { get; set; }
        public ColorItem Color { get; set; }

        public ElementTableResult(string content, ColorItem color)
        {
            Content = content;
            Color = color;
        }
        public ElementTableResult(string content)
        {
            Content = content;
            Color = ColorItem.Neutral;
        }
    }

    public enum ColorItem { Neutral = 0, Ok, Error}
}


