using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPserver;
using Newtonsoft.Json;


namespace PressureSensorTest
{
    public class ReqHandler : IRequestHandler
    {
        public int ReceiveBuffer { get; } = 512;
        public event EventHandler ClosingConnection;

        readonly RemoteControl remoteControl;

        public ReqHandler(RemoteControl remoteControl)
        {
            this.remoteControl = remoteControl;
        }

        public string Handle(string received)
        {
            System.Diagnostics.Debug.WriteLine("Received: " + received + "\n");
            string toSend = "";
            try
            {
                var request = (SimpleRequest)JsonConvert.DeserializeObject(received, typeof(SimpleRequest));
                if (request.Title == "TestStart")
                {
                    ProductRequest productRequest = (ProductRequest)JsonConvert.DeserializeObject(received, typeof(ProductRequest));
                    toSend = TestStartReqHandle(productRequest);
                }
                else if (request.Title == "ReqStateProcess")
                {
                    toSend = StateProcessReqHandle(request);
                }
                else if (request.Title == "ReqProcessResult")
                {
                    toSend = ReqResultHandle(request);
                }
                else if (request.Title == "ReqSystemStatus")
                {
                    toSend = SystemStatusReqHandle(request);
                }
                else if (request.Title == "Cancel")
                {
                    CancelReqHandle();
                }
                else
                {
                    toSend = (new SimpleRequest("Error", 2)).GetJsonFormat();
                }
                System.Diagnostics.Debug.WriteLine("ToSend: " + toSend + "\n");
                return toSend;
            }
            catch
            {
                return "";
            }
        }

        private string ReqResultHandle(SimpleRequest request)
        {
            
            return (new SingleValueRequest(request.Title, 0, remoteControl.ProductStatus.ToString())).GetJsonFormat();
        }

        public void LostCustomerConnection()
        {
            remoteControl.Cancel();
        }

        private string TestStartReqHandle(ProductRequest request)
        {
            string result = "";
            int statusMessage = 0;
            if (remoteControl.StateProcess != StateProcessEnum.Started)
            {
                remoteControl.StartProcess(request);
            }
            result = (new SimpleRequest(request.Title, statusMessage)).GetJsonFormat();
            return result;
        }

        private string StateProcessReqHandle(SimpleRequest request)
        {
            return (new SingleValueRequest(request.Title, 0, ((int)remoteControl.StateProcess).ToString())).GetJsonFormat();
        }

        private string SystemStatusReqHandle(SimpleRequest request)
        {
            int status = remoteControl.SystemError ? 1 : 0;
            return (new SingleValueRequest(request.Title, 0, status.ToString())).GetJsonFormat();
        }

        private void CancelReqHandle()
        {
            ClosingConnection?.Invoke(this, new EventArgs());
            remoteControl.Cancel();
        }
    }

    

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

    [Serializable]
    public class SingleValueRequest : SimpleRequest
    {
        public SingleValueRequest() : base() { }

        public SingleValueRequest(string title, int status, string value) :
            base(title, status)
        {
            Value = value;
        }

        public string Value { get; set; }
    }

    [Serializable]
    public class ProductRequest : SimpleRequest
    {
        public string SN { get; set; }
        public string Box { get; set; }
        public string Name { get; set; }
        public string DateOp { get; set; }
        public string DateCl { get; set; }
        public int ProductStatus { get; set; }
        public bool PrimaryVerification { get; set; }
    }
}
