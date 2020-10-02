using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressSystems
{
    public class SetPressureTimeoutException : Exception
    {
        public SetPressureTimeoutException() :
            base("Время установки давления превысило допустимое")
        { }
    }

    public class PressSystemException : Exception
    {
        public PressSystemException(string message) :
            base("Произошла ошибка при работе со стойкой давления: " + message)
        { }
    }

    public class PsysSupportException : Exception
    {
        public PsysSupportException() :
            base("Стенд не поддерживает поверку данного типа преобразователя давления")
        { }
    }
}
