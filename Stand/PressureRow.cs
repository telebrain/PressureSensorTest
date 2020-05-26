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
            List<int> row = new List<int>();

            int mult = 1;

            int[] pressureRow = rangeType != RangeTypeEnum.DA ? new int[] { 100, 160, 250, 400, 600 }: 
                new int[] { 60000, 100000, 160000, 250000, 400000 };
            

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

                    // Если текцщий диапазон изделия выходит за границы пневмосистемы
                    if (!CheckSupportPsysRange(range))
                    {
                        if (range.RangeType == RangeTypeEnum.DA && range.Max <= pressSystemInfo.RangeHi)
                        {
                            // Для ДА поиск продолжим, если диапазон меньше верхней границы. 
                            // Возможно, текущий диапазон просто ниже нижней границы пневмосистемы
                            continue;
                        }
                        else
                        {
                            searshComplete = true;
                            break;
                        }
                    }
                    // Если найден контроллер, поддерживающий диапазон и тип датчика, добавляем в ряд
                    if (CheckSupportPressControllersRange(range))
                        row.Add(rangeLabel);
                }
                mult *= 10;
            }
            AddAdvancerItem(row);
            if (row.Count == 0)
                row = null;

            return row;
        }

        private bool CheckSupportPsysRange(DeviceRange range)
        {
            bool result = false;
            switch (rangeType)
            {
                case RangeTypeEnum.DV:
                    result = pressSystemInfo.CheckRange(0, range.Max);                  
                    break;

                case RangeTypeEnum.DA:
                    result = pressSystemInfo.CheckRange(range.Max + DeviceRange.VacuumPressure, range.Min + DeviceRange.VacuumPressure);
                    break;

                default:
                    result = pressSystemInfo.CheckRange(range.Max, range.Min);
                    break;
            }         
            return result;
        }

        public bool CheckSupportPressControllersRange(DeviceRange range)
        {
            // Минимальные значения проверяются только для ДИВ
            if (rangeType == RangeTypeEnum.DIV)
            {
                if (SearshController(range.Min, range.Max, range.Min, devicePrecision) < 0)
                    return false;
            }
            // и ДА
            if (rangeType == RangeTypeEnum.DA)
            {
                if (SearshController(range.Min + DeviceRange.VacuumPressure, range.Max + DeviceRange.VacuumPressure, 
                    range.Min + DeviceRange.VacuumPressure, devicePrecision) < 0)
                    return false;
            }

            // Максимальные значения проверяются для всех типов
            if (SearshController(range.Max, range.Max, range.Min, devicePrecision) < 0)
                return false;
            return true;
        }

        // Поиск номера контроллера. Если контроллер не найден, возвращает -1
        public int SearshController(double targetPressure, double targetRangeMax, double targetRangeMin, double precisionClass)
        {
            const double precisionCoef = 3; // Коэффициент запаса точности образцовых приборов

            foreach (PressControllerInfo controller in pressSystemInfo.Controllers)
            {
                if (controller.IsEnabled)
                {
                    // Проверка диапазона контроллера
                    bool checkRange = targetPressure >= 0 ? targetRangeMax <= controller.RangeHi : targetRangeMin >= controller.RangeLo;
                    if (!checkRange)
                        continue; // Контроллер не подходит по диапазону

                    // Проверка погрешности контроллера

                    // Погрешность контроллера давления в Па
                    double controllerAbsPrecision = controller.RangeHi * controller.Precision / 100;
                    // Погрешность преобразователя давления в Па
                    double absPrecision = Math.Abs((targetRangeMax - targetRangeMin) * precisionClass / 100);
                    // Соотношения погрешщностей должно быть не меньше, чем precisionCoef
                    if ((absPrecision / controllerAbsPrecision) >= precisionCoef)
                        return controller.Number; // Погрешнсть контроллера не удовлетворяет классу точности изделия
                }
            }

            return -1; // В пневмосистеме не найден ни один контроллер

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

        private void AddAdvancerItem(List<int> row)
        {
            // Здесь добавляются диапазоны, не входящие в ряд
            if (rangeType == RangeTypeEnum.DIV)
            {
                int label = 100000;
                DeviceRange range = new DeviceRange(label, RangeTypeEnum.DIV);
                if (CheckSupportPsysRange(range) && CheckSupportPressControllersRange(range))
                {
                    row.Add(label);
                    row.Sort();
                }
            }
        }
    }
}
