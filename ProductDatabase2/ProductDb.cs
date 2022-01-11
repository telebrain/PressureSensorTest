using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDatabase2
{
    public sealed class ProductDb: IDisposable
    {
        readonly DbCore db = new DbCore();
        readonly WorkTable Calibration;
        readonly WorkTable MetrologicTest;
        readonly ResultTable CalibrationResults;
        readonly ResultTable TestResults;

        public object SyncRoot { get; } = new object();

        bool isConnected = false;

        const string DbName = "База производственной линейки";

        public ProductDb(string path, string password = "")
        {
            db = new DbCore(DbName, path, password);
            Calibration = new WorkTable("Калибровка", db);
            MetrologicTest = new WorkTable("Поверка", db);
            CalibrationResults = new ResultTable("[Результаты калибровки]", db);
            TestResults = new ResultTable("[Результаты поверки]", db);
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
                throw new ConnectToDbException(ex, DbName);
            }
        }

        private void CheckAndConnect()
        {
            if (!isConnected)
                Connect();
        }

        public void AddToCalibration(Product product)
        {
            try
            {
                CheckAndConnect();
                Calibration.AddProduct(product);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void DeleteProductFromCalibration(Product product)
        {
            try
            {
                CheckAndConnect();
                Calibration.CutProduct(product.SN);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void AddToTest(Product product)
        {
            try
            {
                CheckAndConnect();
                MetrologicTest.AddProduct(product);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void CutProductFromTest(Product product)
        {
            try
            {
                CheckAndConnect();
                MetrologicTest.CutProduct(product.SN);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void AddToCalResults(Product product)
        {
            try
            {
                CheckAndConnect();
                CalibrationResults.AddProduct(product);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void LogToCalResults(string SN, byte[] content)
        {
            try
            {
                CheckAndConnect();
                CalibrationResults.AddLogFile(SN, content, ".json");
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void AddToTestResults(Product product)
        {
            try
            {
                CheckAndConnect();
                TestResults.AddProduct(product);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void LogToTestlResults(string SN, byte[] content)
        {
            try
            {
                CheckAndConnect();
                TestResults.AddLogFile(SN, content, ".json");
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
