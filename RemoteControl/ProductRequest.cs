using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControl
{
    [Serializable]
    public class ProductRequest: SimpleRequest
    {
        public string SN { get; set; }
        public string Box { get; set; }
        public string Name { get; set; }
        public string DateOp { get; set; }
	    public string DateCl { get; set; }
	    public int ProductStatus { get; set; }

    }
}
