using System;
using System.Collections.Generic;
using PressSystems;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwenPressureDevices
{
    public class PD100_DeviceSpecification : IDeviceSpecification
    {
        readonly List<RangeTypeEnum> rangeTypes = new List<RangeTypeEnum> { RangeTypeEnum.DI, RangeTypeEnum.DV, RangeTypeEnum.DIV,
                                    RangeTypeEnum.DG, RangeTypeEnum.DA };

        readonly PressSystemInfo pressSystemInfo = null;

        public string ImagePath { get; } = "";

        public List<string> Titles { get; } = new List<string>() { "ПД100", "ПД100И" };

        public List<string> RangeTypesLabels { get; } = new List<string>() { "ДИ", "ДВ", "ДИВ", "ДГ", "ДА" };

        public List<string> Modifications { get; } = new List<string>() { "1", "3", "8" };

        public List<string> ThreadTypes { get; } = new List<string>() { "1", "2", "4", "6", "7", "8" };



        public List<string> Modifications2 { get; } = new List<string>() { "1", "3", "5", "7" };


        public List<string> Classes { get; } = new List<string>() { "0,25", "0,5", "1,0", "1,5", "2,5" };

        public List<string> OutPortType { get; } = new List<string>() { "", "-" + DeviceName.RS485label };

                          
        public PD100_DeviceSpecification() { }

        public PD100_DeviceSpecification(PressSystemInfo pressSystemInfo)
        {
            this.pressSystemInfo = pressSystemInfo;
        }


        public int GetIndexClass(string classLabel)
        {
            return GetIndex(Classes, classLabel);
        }

        public int GetIndexModification(string modification)
        {
            return GetIndex(Modifications, modification);
        }

        public int GetIndexPressureRange(string pressRangeLabel, int rangeTypeIndex, int classIndex)
        {
            List<string> row = GetPressureRowLabels(rangeTypeIndex, classIndex);
            return GetIndex(row, pressRangeLabel);
        }

        public int GetIndexRangeType(string rangeTypeLabel)
        {
            return GetIndex(RangeTypesLabels, rangeTypeLabel);
        }


        public List<string> GetPressureRowLabels(int rangeTypeIndex, int classIndex)
        {
            List<string> row = new List<string>();
            if (pressSystemInfo == null)
                return row;
            RangeTypeEnum rangeType = RangeTypeFromLabel(RangeTypesLabels[rangeTypeIndex]);
            float precision = Convert.ToSingle(Classes[classIndex]);
            PressureRow pressureRow = new PressureRow(rangeType, pressSystemInfo, precision);
            List<int> pressRow = pressureRow.GetPressureRow();
            if (pressRow != null)
            {
                foreach (int item in pressRow)
                {
                    row.Add(GetPressLabel(item));
                }
            }
            else
            {
                row.Add("-----");
            }
            return row;
        }

        private string GetPressLabel(int PressValue_Pa)
        {
            double range = Convert.ToDouble(PressValue_Pa) / 1000000;
            string lbl = range.ToString();
            if (lbl.Length == 1)
                lbl = range.ToString("0.0");
            return lbl;
        }

        public RangeTypeEnum RangeTypeFromLabel(string RangeLabel)
        {
            int index = RangeTypesLabels.IndexOf(RangeLabel);
            if (index < 0) return RangeTypeEnum.NotDefined;
            return rangeTypes[index];
        }

        public void CheckRangeSupport(IDevice device)
        {
            if (device.Range.RangeType != RangeTypeEnum.DV)
            {
                bool result = pressSystemInfo.CheckRange(device.Range.Max_Pa, device.Range.Min_Pa);
                if (!result)
                    throw new DeviceNotSupportByPsysException("Диапазон изделия превышает диапазон пневмосистемы");
            }
            else
            {
                if (!pressSystemInfo.CheckRange(0, device.Range.Max_Pa))
                    throw new DeviceNotSupportByPsysException("Диапазон изделия превышает диапазон пневмосистемы");
            }
            var pressureRow = new PressureRow(device.Range.RangeType, pressSystemInfo, device.Precision);
            if (pressureRow.SearshController(device.Range.Max_Pa, device.Range.Min_Pa, device.Range.Max_Pa, device.Precision) < 0)
                throw new DeviceNotSupportByPsysException("Система не может обеспечить точность установки давления");
        }

        

        private int GetIndex(List<string> row, string label)
        {
            if (row == null) return 0;
            int index = row.IndexOf(label);
            if (index < 0)
            {
                index = 0;
            }
            return index;
        }
    }
    [Serializable]
    public class DeviceNotSupportByPsysException : Exception
    {
        public DeviceNotSupportByPsysException(string message) :
            base("Пневмосистема не поддерживает поверку данного изделия. " + message)
        { }
    }
}
