using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PressSystems;

namespace OwenPressureDevices
{
    public class PressureRow
    {
        PressSystemInfo pressSystemInfo;
        readonly RangeTypeEnum rangeType;
        readonly float devicePrecision;

        // Давление, равное полному вакууму, относительно атмосферного
        const int VacuumPressure = -100000;


        public PressureRow(RangeTypeEnum rangeType, PressSystemInfo pressSystemInfo, float devicePrecision)
        {
            this.rangeType = rangeType;
            this.devicePrecision = devicePrecision;
            this.pressSystemInfo = pressSystemInfo;
        }

        //Возвращает ряд доступных диапазонов в Па
        public List<int> GetPressureRow()
        {
            return CreatePressureRow();
        }

        //Возвращает ряд доступных диапазонов в МПа в string (обозначение диапазона для ПД100)
        public List<string> GetPressureRowMPa()
        {
            List<int> row = CreatePressureRow();
            if (row == null) return null;

            List<string> rowMPa = new List<string>();
            
            foreach (int range in row)
            {
                string item = Convert.ToString(Convert.ToSingle(range) / 1000000);
                if (item.Length == 1)
                    item += ",0";
                rowMPa.Add(item);
            }
            return rowMPa;
        }

        //Возвращает ряд доступных диапазонов в кПа или Па с обозначением единиц измерения в string (обозначение диапазона для ПД150)
        public List<string> GetPressureRowWithUnit()
        {
            List<int> row = CreatePressureRow();
            if (row == null) return null;

            List<string> rowWithUnit = new List<string>();

            foreach (int range in row)
            {
                string item = "";
                if (range >= 1000)
                {
                    item = Convert.ToString(Convert.ToSingle(range) / 1000);
                    if (item.Length == 1)
                        item += ",0";
                    item += "К";
                }
                else
                {
                    item = Convert.ToString(Convert.ToSingle(range)) + "П";
                }

                rowWithUnit.Add(item);
            }
            return rowWithUnit;
        }

        private List<int> CreatePressureRow()
        {
            int mult = 10;
            int[] pressureRow = new int[] { 4, 6, 10, 16, 25 };
            List<int> row = new List<int>();

            bool searshComplete = false;

            // На основе ряда создаем диапазон изделия и проверяем, поддерживается ли он пневмосистемой
            while(!searshComplete)
            {
                foreach (int item in pressureRow)
                {
                    // Преобразуем значение из ряда в значение диапазона в Па, 
                    // который будет обозначен в названии изделия
                    int rangeLabel = PressRowItemToRangeLabel(item * mult);

                    
                    DeviceRange range = new DeviceRange(rangeLabel, rangeType);

                    // Если текцщий диапазон изделия выходит за границы пневмосистемы, прекращаем поиск
                    if (!CheckSupportPsysRange(range, out searshComplete))
                    {
                        if (searshComplete)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    
                    // Если найден контроллер, поддерживающий диапазон и тип датчика, добавляем в ряд
                    if (CheckSupportPressControllersRange(range))
                    {
                        row.Add(rangeLabel);
                    }
                }
                mult *= 10;
            }
            if (row.Count == 0)
            {
                row = null;
            }
            return row;
        }

        private bool CheckSupportPsysRange(DeviceRange range, out bool searshComplete)
        {
            searshComplete = false;
            switch (rangeType)
            {
                case RangeTypeEnum.DV:
                    if (!pressSystemInfo.CheckRangeMin(range.Max))
                    {
                        searshComplete = true;
                        return false;
                    }
                    break;

                case RangeTypeEnum.DA:
                    if (pressSystemInfo.CheckRangeMax(range.Max, range.Min))
                    {
                        if (!pressSystemInfo.CheckRangeMin(range.Max, range.Min))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        searshComplete = true;
                        return false;
                    }
                    break;

                default:
                    if (!(pressSystemInfo.CheckRangeMax(range.Max, range.Min)
                    && pressSystemInfo.CheckRangeMin(range.Max, range.Min)))
                    {
                        searshComplete = true;
                        return false;
                    }
                    break;
            }         
            return true;
        }

        private bool CheckSupportPressControllersRange(DeviceRange range)
        {
            // Минимальные значения проверяются только для ДИВ и ДА
            if (rangeType == RangeTypeEnum.DIV || rangeType == RangeTypeEnum.DA)
            {
                if (pressSystemInfo.SearshController(range.Min, range.Max - range.Min, devicePrecision) < 0)
                    return false;
            }

            // Максимальные значения проверяются для всех типов
            if (pressSystemInfo.SearshController(range.Max, range.Max - range.Min, devicePrecision) < 0)
                return false;
            return true;
        }

        private int PressRowItemToRangeLabel(int pressRowItem)
        {
            if (rangeType != RangeTypeEnum.DIV)
            {
                return pressRowItem;
            }
            else
            {
                if (pressRowItem > 2*Math.Abs(VacuumPressure))
                {
                    return pressRowItem + VacuumPressure;
                }
                else
                {
                    return pressRowItem / 2;
                }
            }
        }
    }
}
