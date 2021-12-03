using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PressSystems;
using PressureSensorTestCore;

namespace PressureSensorTest
{
    public class DigitalPdSimulator: IDigitalPort
    {
        IPressSystem pressSystem;
        double rangeMin;
        double rangeMax;
        double precision;
        bool absoluteType;
        readonly Random random;

        public DigitalPdSimulator(IPressSystem pressSystem, double rangeMin, double rangeMax, double precision, bool absoluteType)
        {
            this.pressSystem = pressSystem;
            this.rangeMin = rangeMin;
            this.rangeMax = rangeMax;
            this.precision = precision;
            this.absoluteType = absoluteType;
            random = new Random();
        }

        public void Init()
        {
            
        }

        public double GetPressurePa()
        {
            double shift = absoluteType ? pressSystem.PressSystemVariables.Barometr * (-1) : rangeMin;
            double span = rangeMax - rangeMin;
            double point = pressSystem.ConnectState ? (pressSystem.PressSystemVariables.Pressure - shift) / span : shift / span;
            double error = (2 * random.NextDouble() * precision - precision) / 100;
            double value = (point + error) * span + shift;
            return value;
        }

        public void Dispose()
        {
            
        }
    }
}
