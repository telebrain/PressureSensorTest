using System.Collections.Generic;
using PressSystems;

namespace OwenPressureDevices
{
    public interface IDeviceSpecification
    {
        List<string> Classes { get; }

        List<string> OutPortType { get; }

        string ImagePath { get; }

        List<string> Modifications { get; }

        List<string> Modifications2 { get; }

        List<string> ThreadTypes { get; }

        List<string> RangeTypesLabels { get; }

        List<string> Titles { get; }

        // int RangeFromLabel(string rangeLabel);

        List<string> GetPressureRowLabels(int rangeTypeIndex, int classIndex);

        RangeTypeEnum RangeTypeFromLabel(string RangeLabel);

        // ComponentsOfDeviceName ParseName(string name);

        // string ConcateName(ComponentsOfDeviceName nameItems);

        int GetIndexClass(string classLabel);

        int GetIndexPressureRange(string pressRangeLabel, int rangeTypeIndex, int classIndex);

        int GetIndexModification(string modification);

        int GetIndexRangeType(string rangeTypeLabel);

        void CheckRangeSupport(IDevice device);
    }

    public enum PressureUnitsEnum { Pa = 0, kPa = 1, MPa = 2 }
}