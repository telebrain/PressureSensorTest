using System;

namespace RemoteControl
{
    public class ProductInfoEventArgs: EventArgs
    {
        ProductInfoEventArgs(ProductRequest productRequest)
        {
            SerialNumber = productRequest.SN;
            BoxNumber = productRequest.Box;
            Name = productRequest.Name;
            Date = DateTime.Parse(productRequest.DateOp);
        }

        public string SerialNumber{ get; set; }
        public string BoxNumber { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public static ProductInfoEventArgs Parse (object request)
        {
            try
            {
                var productRequest = (ProductRequest)request;
                return new ProductInfoEventArgs(productRequest);
            }
            catch
            {
                return null;
            }
        }
        
        public bool CheckProductNumber()
        {
            return SerialNumber.Length >= 17 && SerialNumber.Length <= 19;
        }

        public bool CheckBoxNumber()
        {
            return BoxNumber.Length == 4 && int.TryParse(BoxNumber, out int box);
        }
    }
}
