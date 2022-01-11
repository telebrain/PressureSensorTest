using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDatabase2
{
    public class Buffers: IDisposable
    {
        readonly DbCore db;
        const string DbName = "Буфферные таблицы 1С";

        readonly InputBufferTable calibrationInput;
        readonly OutBuffer calibrationOut;
        readonly InputBufferTable testInput;
        readonly OutBuffer testOutput;

        const int NumberTestPlace = 3;

        bool isConnected = false;

        public object SyncRoot { get; } = new object();

        public Buffers(string path, string password = "")
        {
            db = new DbCore(DbName, path, password);
            calibrationInput = new InputBufferTable(db, "СТ_Вх");
            calibrationOut = new OutBuffer(db, "СТ_вых");
            testInput = new InputBufferTable(db, "ПИ_Вх");
            testOutput = new OutBuffer(db, "ПИ_Вых");
        }


        public void Connect()
        {
            try
            {
                db.Connect();
                isConnected = true;
            }
            catch (Exception ex)
            {
                isConnected = false;
                throw new ConnectToBuffersException(ex, DbName);
            }
        }

        private void CheckAndConnect()
        {
            if (!isConnected)
                Connect();
        }


        public void AddCalibrationTask(Product product)
        {
            try
            {
                CheckAndConnect();
                calibrationInput.AddProduct(product);
            }
            catch
            {
                Disconnect();
                throw;
            } 
        }

        public Product GetCalibrationTask()
        {
            try
            {
                CheckAndConnect();
                var product = calibrationInput.GetProduct();
                return product;
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public Product CutCalibrationTask(string sn)
        {
            try
            {
                CheckAndConnect();
                var product = calibrationInput.CutProduct(sn);
                return product;
            }
            catch
            {
                Disconnect();
                throw;
            }
        }


        public void CloseCalibrationTask(Product product, bool sendToTestBuffer = false)
        {
            try
            {
                CheckAndConnect();               
                calibrationOut.AddProduct(product);
                if (sendToTestBuffer)
                {
                    testInput.DeleteProductByBox(product.Box);
                    testInput.AddProduct(product, false);
                }
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void AddTestTask(Product product)
        {
            try
            {
                CheckAndConnect();
                testInput.AddProduct(product, false);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public Product GetTestTaskByBox(string box)
        {
            try
            {
                CheckAndConnect();
                var product = testInput.GetProductByBox(box);
                return product;
            }
            catch
            {
                Disconnect();
                throw;
            }

        }

        public Product GetTestTaskBySN(string SN)
        {
            try
            {
                CheckAndConnect();
                var product = testInput.GetProductBySn(SN);
                return product;
            }
            catch
            {
                Disconnect();
                throw;
            }

        }

        public Product CutTestTask(string sn)
        {
            try
            {
                CheckAndConnect();
                var product = testInput.CutProduct(sn);
                return product;
            }
            catch
            {
                Disconnect();
                throw;
            }
        }


        public void CloseTestTask(Product product)
        {
            try
            {
                CheckAndConnect();
                testInput.CutProduct(product.SN);
                testOutput.AddProduct(product);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                db.Disconnect();
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
