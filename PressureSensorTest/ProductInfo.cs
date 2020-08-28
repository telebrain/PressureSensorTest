using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwenPressureDevices;

namespace PressureSensorTest
{
    public class ProductInfo
    {
        public IDevice Device { get; private set; }

        public string DeviceBoxNumber { get; private set; }

        public DateTime OpenDateTime { get; private set; }

        public DateTime ClosingDateTime { get; set; }

        public TestErrorEnum Error { get; set; }

        public bool PrimaryTest { get; set; } = true;

        public ProductInfo(IDevice device, DateTime openDateTime)
        {
            Device = device;
            OpenDateTime = openDateTime;
            DeviceBoxNumber = "";
            Error = TestErrorEnum.InDefined;
        }

        public ProductInfo(IDevice device, string deviceBox, DateTime openDateTime)
        {
            Device = device;
            DeviceBoxNumber = deviceBox;
            OpenDateTime = openDateTime;
            Error = TestErrorEnum.InDefined;
        }
    }

    public enum TestErrorEnum
    {
        InDefined = -100,
        NoError = 0,
        BadPrecision = 14, // Не соответсвует метрологическим характеристикам
        Leakage = 15, // Утечка
        SystemError = 16, // Брак из-за неисправности оборудования
        RangeNotSupportByPsys = 17, // Диапазон изделия не поддерживается системой
        OperatorSolution = 18 // В брак по решению оператора
    }
}
