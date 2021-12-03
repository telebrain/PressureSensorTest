using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductDatabase2;

namespace PressureSensorTest
{
    public class ProductDb
    {
        ProductDatabase2.ProductDb db;

        public ProductDb(string dbPath, string dbPassword)
        {
            db = new ProductDatabase2.ProductDb(dbPath, dbPassword);
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
                    Name = productInfo.Device.Name.Name,
                    Date = productInfo.OpenDateTime,
                    DateCl = productInfo.ClosingDateTime,
                    Error = (int)productInfo.Error,
                    Log = testResultFile
                };

                db.AddToTestResults(product);
                // db.LogToCalResults(product.SN, product.Log);
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
