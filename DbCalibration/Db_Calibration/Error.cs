using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCalibration
{
    public enum ResultEnum
    {
        No_Error,
        Err_connect,
        Err_Data_Moves
    }

    public class Result
    {
        public ResultEnum Error { get; private set; }
        public string Message { get; private set; }

        public Result (ResultEnum result, string message)
        {
            Error = result;
            Message = message;
        }

        public Result(ResultEnum result, Exception e)
        {
            Error = result;
            Message = e.Message;
        }

    }
}
