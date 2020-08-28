using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControl
{
    public class SingleValueRequest: SimpleRequest
    {
        public SingleValueRequest(): base() { }

        public SingleValueRequest(string title, int status, string value):
            base(title, status)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}
