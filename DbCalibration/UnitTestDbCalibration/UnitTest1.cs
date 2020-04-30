using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbCalibration;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace UnitTestDbCalibration
{
    [TestClass]
    public class UnitTest1
    {
        const string DbPath = @"c:\Users\s.nikonov\Documents\Proj_C#\Librarys\DbCalibration\UnitTestDbCalibration\Db\DbOfStend.accdb";
        const string LogFilePath = @"c:\Users\s.nikonov\Documents\Proj_C#\Librarys\DbCalibration\UnitTestDbCalibration\Db\32119190346014066.xml";
        // Перед запуском теста сделать копию DbOfStend_1.accdb и переименовать в DbOfStend.accdb
        [TestMethod]
        public void ProductMove()
        {
            DbCal db = new DbCal(DbPath);
            CheckResult(db.AddNewProduct(out InpProduct product));
            if (product.SN != "32120190446014750") throw (new Exception("Из входного буфера была извлечена не та строка"));
            List<InpProduct> products = new List<InpProduct>();

            for (int i = 0; i < 3; i++)
            {
                CheckResult(db.AddNewProduct(out product));
                products.Add(product);
            }

            if (product != null) throw (new Exception("Не очистилась таблица входного буфера"));
            product = products[0];
            CheckResult(db.CloseProduct(DateTime.Now, product.Date, product.SN, product.Device, product.Name, 1));
            Thread.Sleep(1000);
            CheckResult(db.CloseProduct(DateTime.Now, product.Date, product.SN, product.Device, product.Name, 1));
            byte[] logFile = ReadFile(LogFilePath);
            CheckResult(db.CloseProduct(DateTime.Now, product.Date, product.SN, product.Device, product.Name, 1, logFile));
            DateTime dt = DateTime.Now;
            CheckResult(db.CloseProduct(dt, product.Date, product.SN, product.Device, product.Name, 0, logFile));
            CheckResult(db.CloseProduct(dt, product.Date, product.SN, product.Device, product.Name, 0, logFile));
            CheckResult(db.AlarmSystemCloseProduct("32120190446014758"));
            CheckResult(db.AlarmClearSystem());
        }

        private byte[] ReadFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return data;
        }

        private void CheckResult(Result result)
        {
            if (result.Error == ResultEnum.No_Error) return;
            {
                throw (new Exception(result.Message));
            }
        }
    }
}
