using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwenPressureDevices
{
    public class PressIndicatorData
    {
        PressIndicatorRange range;
        public PressIndicatorRange Range
        {
            get { return range; }
            set
            {
                range = value; UpdPercent(); UpdInd();
            }
        }
        
        float pressure;
        public float Pressure
        {
            get { return pressure; }
            set
            {
                pressure = value;
                UpdPercent();
                UpdInd();
            }
        }

        public float Percent { get; private set; }

        UnitsEnum unit;
        public UnitsEnum Unit
        {
            get { return unit; }
            set { unit = value; UpdInd(); }
        }

        
        public bool Enable { get; private set; }

        public EventHandler<PressIndicatorData> UpdPressIndEvent { get; set; }

        public PressIndicatorData() { }

        public PressIndicatorData(EventHandler<PressIndicatorData> updEvent)
        {
            UpdPressIndEvent = updEvent;
        }

        public void SetDisable()
        {
            Pressure = 0;
            Percent = 0;
            Enable = false;
            Unit = UnitsEnum.Pa;
        }

        public void SetEnable()
        {
            Enable = true;
            UpdInd();
        }

        private void UpdInd()
        {
            UpdPressIndEvent?.Invoke(this, this);
        }

        private void UpdPercent()
        {
            if ((range.Max - range.Min) != 0)
                Percent = 100* (Pressure - range.Min) / (range.Max - range.Min);
            else Percent = 0;
        }
    }

    public struct PressIndicatorRange
    {
        public float Min;
        public float Max;

        public PressIndicatorRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    } 
}
