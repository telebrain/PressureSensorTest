using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductDatabase;

namespace PressureSensorTest
{
    public class ProductDb
    {
        ProductLineDb db;

        public ProductDb(string dbPath)
        {
            db = new ProductLineDb(dbPath);
        }

        public void AddTestInfo(ProductInfo productInfo, string jsonData)
        {
            try
            {
                byte[] testResultFile = string.IsNullOrEmpty(jsonData) ? null : Encoding.Default.GetBytes(jsonData);
                Product product = new Product()
                {
                    SN = productInfo.Device.SerialNumber,
                    Box = productInfo.DeviceBoxNumber,
                    Name = productInfo.Device.Name,
                    Date = productInfo.OpenDateTime,
                    DateCl = productInfo.ClosingDateTime,
                    Error = (int)productInfo.Error,
                    Log = testResultFile
                };
                db.TestComplete(product);
            }
            catch(Exception ex)
            {
                throw new DbException(ex.Message);
            }
        }
    }

    [Serializable]
    public class DbException: Exception
    {
        public DbException(string message): 
            base("Ошибка операции с базой данных линейки \r\n" + message)
        { }
    }
}
