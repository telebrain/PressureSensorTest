using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbCalibration;

namespace TestDb
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            DbCal db = new DbCal(@"D:\DbOfStend.accdb");

            byte[] testResultFile = Encoding.Default.GetBytes("asbc");
            Result result = db.FineCloseProduct(DateTime.Now, DateTime.Now, "0000000001",
                "0001", "ПД100-ДИ0,1-311-0,5", 0, testResultFile);
            if (result.Error != ResultEnum.No_Error)
                throw new Exception(result.Message);
        }
    }
}
