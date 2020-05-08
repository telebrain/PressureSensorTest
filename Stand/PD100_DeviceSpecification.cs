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

        public List<string> ThreadTypes { get => ParserNamePD100.ThreadType; }

        public List<string> Modifications { get => ParserNamePD100.Modifications; }

        public List<string> Classes { get => ParserNamePD100.Classes; }

        public List<string> RangeTypesLabels { get => ParserNamePD100.RangeTypesLabels; }

        public List<string> Titles { get => ParserNamePD100.Titles; }

        public PD100_DeviceSpecification() { }

        public PD100_DeviceSpecification(PressSystemInfo pressSystemInfo)
        {
            this.pressSystemInfo = pressSystemInfo;
        }

        public string ConcateName(ComponentsOfDeviceName nameItems)
        {
            return ParserNamePD100.ConcateName(nameItems);
        }

        public ComponentsOfDeviceName ParseName(string name)
        {
            return ParserNamePD100.ParseName(name);
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
                    row.Add(ParserNamePD100.GetPressLabel(item));
                }
            }
            else
            {
                row.Add("-----");
            }
            return row;
        }

        public int RangeFromLabel(string rangeLabel)
        {
            return ParserNamePD100.GetPressureRange(rangeLabel);
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
                bool result = pressSystemInfo.CheckRange(device.Range.Max, device.Range.Min);
                if (!result)
                    throw new DeviceNotSupportByPsysException("Диапазон изделия превышает диапазон пневмосистемы");
            }
            else
            {
                if (!pressSystemInfo.CheckRange(0, device.Range.Max))
                    throw new DeviceNotSupportByPsysException("Диапазон изделия превышает диапазон пневмосистемы");
            }
            var pressureRow = new PressureRow(device.Range.RangeType, pressSystemInfo, device.ClassPrecision);
            if (pressureRow.SearshController(device.Range.Max, 0, device.Range.Max, device.ClassPrecision) < 0)
                throw new DeviceNotSupportByPsysException("Не обеспечивается точность установки давления");
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

    public class DeviceNotSupportByPsysException : Exception
    {
        public DeviceNotSupportByPsysException(string message) :
            base("Пневмосистема не поддерживает поверку данного изделия. " + message)
        { }
    }
}
