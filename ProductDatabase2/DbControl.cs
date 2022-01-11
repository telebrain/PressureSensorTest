using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDatabase2
{
    public class DbControl
    {
        readonly Buffers buffers;
        readonly ProductDb products; 


        public DbControl(Buffers buffers, ProductDb productDb)
        {
            products = productDb;
            this.buffers = buffers;
        }

        public Product GetProductToCalibration()
        {
            lock (buffers.SyncRoot)
            {
                return buffers.GetCalibrationTask();
            }
        }

        public void MoveToCalibration(string sn)
        {
            Product product = null;
            lock (buffers.SyncRoot)
            {
                product = buffers.CutCalibrationTask(sn);
            }
            lock (products.SyncRoot)
            {
                products.AddToCalibration(product);
            }
        }


        public void AddCalibrationLog(string sn, byte[] content)
        {
            lock (products.SyncRoot)
            {
                products.LogToCalResults(sn, content);
            }
        }

        public void CloseCalibrationTask(Product product, bool sendToTestBuffer = false)
        {
            lock (buffers.SyncRoot)
            { 
                buffers.CloseCalibrationTask(product, sendToTestBuffer && product.Error == 0);
            }
            lock (products.SyncRoot)
            {
                products.AddToCalResults(product);
                products.DeleteProductFromCalibration(product);
                
            }
        }

        public Product GetProductToTest(string box)
        {
            lock (buffers.SyncRoot)
            {
                return buffers.GetTestTaskByBox(box);
            }
        }

        public void MoveToTest(string sn)
        {
            Product product = null;
            lock (buffers.SyncRoot)
            {
                product = buffers.GetTestTaskBySN(sn);
            }
            lock (products.SyncRoot)
            {
                products.AddToTest(product);
            }
        }

        public void CloseTestTask(Product product)
        {
            lock (buffers.SyncRoot)
            {
                buffers.CloseTestTask(product);
            }
            lock (products.SyncRoot)
            {
                products.CutProductFromTest(product);
            }
        }

        public void ReturnToTest(Product product)
        {
            lock (buffers.SyncRoot)
            {
                buffers.AddTestTask(product);
            }
            lock (products.SyncRoot)
            {
                products.CutProductFromTest(product);
            }
        }


    }
}
