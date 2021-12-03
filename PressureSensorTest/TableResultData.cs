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

        // PressureIndication pressureIndication;

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
                // pressureIndication = new PressureIndication(testResults.RangeMax);
                Units = GetUnit(testResults.PressureUnits);
                AddResultUpwards(testResults.MeasureResultsUpwards);
                AddResultTopDown(testResults.MeasureResultsTopdown);
                AddVariations(testResults.MeasureResultsUpwards, testResults.Variations);
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

        const string PressValFormat = "0.0000";
        const string CurrValFormat = "0.0000";
        const string ErrValFormat = "0.0000";

        private void AddMeasureResults(MeasureResults results, int shiftColumn, bool reverse)
        {
            if (results != null)
            {
                for (int pos = 0; pos < results.Count; pos++)
                {
                    int row = pos;
                    if (reverse)
                        row = Rows - pos - 1; 
                    var point = results[pos];
                    Data[row, shiftColumn].Content = point.ReferencePressure.PressureByUnits.ToString(PressValFormat);
                    if (point is CurrentCheckPoint)
                    {
                        Data[row, 1 + shiftColumn].Content = (point as CurrentCheckPoint).CurrentFromEtalonPressure.ToString(CurrValFormat);
                        Data[row, 2 + shiftColumn].Content = (point as CurrentCheckPoint).MeasuredCurrent.ToString(CurrValFormat);
                    }
                    else
                    {
                        Data[row, 1 + shiftColumn].Content = "-";
                        Data[row, 2 + shiftColumn].Content = "-";
                    }
                    Data[row, 3 + shiftColumn].Content = point.Pressure.PressureByUnits.ToString(PressValFormat);
                    Data[row, 4 + shiftColumn].Content = point.ErrorMeasure.ToString(ErrValFormat);
                    Data[row, 4 + shiftColumn].Color = ResumeToColor(results[pos].Resume);                   
                }
            }
        }

        private void AddVariations(MeasureResults measureResults, Variations variationResults)
        {
            const string VariationFormat = "0.0000";
            if (variationResults != null)
            {
                for (int row = 0; row < measureResults.Count; row++)
                {
                    var variation = variationResults.GetVariationPointByPercent(measureResults[row].PercentRange);
                    if (variation != null)
                    {
                        Data[row, 10].Content = variation.Value.ToString(VariationFormat);
                        Data[row, 10].Color = ResumeToColor(variation.Resume);
                    }
                    else
                    {
                        Data[row, 10].Content = "-----";
                        Data[row, 10].Color = ColorItem.Neutral;
                    }
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

        private string GetUnit(PressureSensorTestCore.PressureUnitsEnum pressureUnits)
        {
            string[] units = new string[] { "Па", "КПа", "МПа" };
            return units[(int)pressureUnits];
        }

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


