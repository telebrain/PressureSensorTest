namespace PressureSensorTestCore
{
    public interface ICheckPoint
    {
        double ErrorMeasure { get; }
        int PercentRange { get; }
        Pressure Pressure { get; }
        Pressure ReferencePressure { get; }
        bool Resume { get; }
        PressureUnitsEnum PressureUnits { get; }
    }

}