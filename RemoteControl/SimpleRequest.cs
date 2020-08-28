using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RemoteControl
{
    [Serializable]
    public class SimpleRequest
    {
        public string Title { get; set; }
        public int Status { get; set; }


        public SimpleRequest() { }

        public SimpleRequest(string title, int status)
        {
            Title = title;
            Status = status;
        }

        public virtual string GetJsonFormat()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }

}
