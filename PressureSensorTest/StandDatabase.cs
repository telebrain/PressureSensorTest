using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StandDb;

namespace PressureSensorTest
{
    public class StandDatabase
    {
        Database db;

        public StandDatabase(string dbPath)
        {
            db = new Database(dbPath);
        }

        public void AddTestInfo(ProductInfo productInfo, string jsonData)
        {
            byte[] testResultFile = Encoding.Default.GetBytes(jsonData);
            Result result;
            if (!string.IsNullOrEmpty(jsonData))
            {
                result = db.FineCloseProduct(productInfo.ClosingDateTime, productInfo.OpenDateTime, productInfo.Device.SerialNumber,
                    productInfo.DeviceBoxNumber, productInfo.Device.Name, (int)productInfo.Error, testResultFile);            
            }
            else
            {
                result = db.FineCloseProduct(productInfo.ClosingDateTime, productInfo.OpenDateTime, productInfo.Device.SerialNumber,
                productInfo.DeviceBoxNumber, productInfo.Device.Name, (int)productInfo.Error);
            }
            if (result.Error != ResultEnum.No_Error)
                throw new StandDbException(result);

        }
    }

    [Serializable]
    public class StandDbException: Exception
    {
        public StandDbException(Result resultOperation): 
            base("Ошибка операции с базой данных стенда \r\n" + resultOperation.Message)
        { }
    }
}
