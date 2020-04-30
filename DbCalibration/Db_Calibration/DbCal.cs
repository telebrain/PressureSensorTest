using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS_Access_DB;

namespace DbCalibration
{
    public class DbCal
    {
        readonly string _dbPath = "";
        const int AlarmSystemErrNymber = 16;

        public DbCal(string dbPath)
        {
            _dbPath = dbPath;
        }

        // Чтение из таблицы "Входной_буфер" значения самой ранней записи, добавляет в таблицу "В работе"
        public Result AddNewProduct(out InpProduct productOut)
        {
            productOut = null;
            // Подключение
            Result result = ConnectDb(_dbPath, out DbCore db);
            if (result.Error != ResultEnum.No_Error) return result;

            // Чтение из таблицы "Входной_буфер
            InpProduct product = null;
            try
            {
                InpBufferTbl tbl = new InpBufferTbl(db);
                product = tbl.ExtractFirstItem();
                // Добавение датчика из таблицы "В работе". Необязательная операция
                InpToWorkTable(product, db);
            }
            catch (Exception e)
            {
                result = new Result(ResultEnum.Err_Data_Moves, e);
            }
            db.Disconnect();
            productOut = product;
            return result;
        }

        // Вызывается при выходе продукта со стенда калибровки
        public Result CalCloseProduct(DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err, byte[] logFile)
        {
            // Подключение
            Result result = ConnectDb(_dbPath, out DbCore db);
            if (result.Error != ResultEnum.No_Error) return result;
            // Удаление датчика из таблицы "В работе". Необязательная операция
            DeleteRowWorkTbl(db, SN);
            try
            {
                // Запись в таблицу "Закрытые" или "Брак"
                ToClosedTbl(db, dtClosed, dtOpen, SN, dev, name, err, logFile);
                // Добавление в таблицу "Выходной_буфер" если был брак
                if (err != 0)
                    ToOutBuffer(db, dtClosed, SN, err);
            }
            catch (Exception e)
            {
                result = new Result(ResultEnum.Err_Data_Moves, e);
            }
            db.Disconnect();
            return result;
        }

        // Вызывается при выходе продукта со стенда калибровки
        public Result CalCloseProduct(DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err)
        {
            // Подключение
            Result result = ConnectDb(_dbPath, out DbCore db);
            if (result.Error != ResultEnum.No_Error) return result;
            // Удаление датчика из таблицы "В работе". Необязательная операция
            DeleteRowWorkTbl(db, SN);
            try
            {
                // Запись в таблицу "Закрытые" или "Брак"
                ToClosedTbl(db, dtClosed, dtOpen, SN, dev, name, err);
                // Добавление в таблицу "Выходной_буфер" если был брак
                if (err != 0)
                    ToOutBuffer(db, dtClosed, SN, err);
            }
            catch (Exception e)
            {
                result = new Result(ResultEnum.Err_Data_Moves, e);
            }
            
            db.Disconnect();
            return result;
        }

        public Result FineCloseProduct(DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err, byte[] testResultFile)
        {
            Result result = ConnectDb(_dbPath, out DbCore db);
            if (result.Error != ResultEnum.No_Error)
                return result;
            try
            {
                // Запись в таблицу "Поверка"
                ToTestTbl(db, dtClosed, dtOpen, SN, dev, name, err, testResultFile);
                // Добавление в таблицу "Выходной буфер"
                ToOutBuffer(db, dtClosed, SN, err);
            }
            catch (Exception e)
            {
                result = new Result(ResultEnum.Err_Data_Moves, e);
            }

            db.Disconnect();
            return result;

        }

        public Result FineCloseProduct(DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err)
        {
            Result result = ConnectDb(_dbPath, out DbCore db);
            if (result.Error != ResultEnum.No_Error)
                return result;
            try
            {
                // Запись в таблицу "Поверка"
                ToTestTbl(db, dtClosed, dtOpen, SN, dev, name, err);
                // Добавление в таблицу "Выходной буфер"
                ToOutBuffer(db, dtClosed, SN, err);
            }
            catch (Exception e)
            {
                result = new Result(ResultEnum.Err_Data_Moves, e);
            }

            db.Disconnect();
            return result;

        }


        public Result AlarmSystemCloseProduct(string SN)
        {
            // Подключение
            Result result = ConnectDb(_dbPath, out DbCore db);
            if (result.Error != ResultEnum.No_Error) return result;
            try
            {
                DeleteRowWorkTbl(db, SN);
                ToOutBuffer(db, DateTime.Now, SN, AlarmSystemErrNymber);
            }
            catch (Exception e)
            {
                result = new Result(ResultEnum.Err_Data_Moves, e);
            }
            return result;
        }

        public Result AlarmClearSystem()
        {

            // Подключение
            Result result = ConnectDb(_dbPath, out DbCore db);
            if (result.Error != ResultEnum.No_Error) return result;
            try
            {
                InpBufferTbl inpTbl = new InpBufferTbl(db);
                OutBuffer outTbl = new OutBuffer(db);
                while (true)
                {
                    InpProduct product = inpTbl.ExtractFirstItem();
                    if (product == null) break;
                    outTbl.AddProduct(DateTime.Now, product.SN, AlarmSystemErrNymber);
                }
                // Очистка таблицы "В работе", необязательная операция
                ClearWorkTable(db);
            }
            catch(Exception e)
            {
                result = new Result(ResultEnum.Err_Data_Moves, e);
            }
            

            db.Disconnect();
            return result;
        }

        private Result ConnectDb(string path, out DbCore db)
        {
            db = null;
            try
            {
                db = new DbCore(_dbPath);
                db.Connect();
                return new Result(ResultEnum.No_Error, "");
            }
            catch (Exception e)
            {
                return new Result(ResultEnum.Err_connect, e);
            }
        }

        private void InpToWorkTable(InpProduct product, DbCore db)
        {
            try
            {
                WorkTable workTabl = new WorkTable(db);
                workTabl.WriteRow(product);
            }
            catch (Exception e)
            {
                string message = e.Message;
                //Необязательная операция, добавить в лог программы, если ошибка
            }
        }

        private void ClearWorkTable(DbCore db)
        {
            try
            {
                WorkTable workTabl = new WorkTable(db);
                workTabl.ClearTable();
            }
            catch
            {
                //Необязательная операция, добавить в лог программы, если ошибка
            }
        }

        private void ToClosedTbl(DbCore db, DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err, byte[] logFile)
        {
            ClosedTable closedTable;
            if (err == 0)
            {
                closedTable = new ClosedTable("Закрытые", db);
            }
            else
            {
                closedTable = new ClosedTable("Брак", db);
            }
            closedTable.AddProduct(dtClosed, dtOpen, SN, dev, name, err, logFile);
            
        }

        private void ToTestTbl(DbCore db, DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err, byte[] content)
        {
            ClosedTable table = new ClosedTable("Поверка", db);
            table.AddProduct(dtClosed, dtOpen, SN, dev, name, err, content, ".json");
        }

        private void ToTestTbl(DbCore db, DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err)
        {
            ClosedTable table = new ClosedTable("Поверка", db);
            table.AddProduct(dtClosed, dtOpen, SN, dev, name, err);
        }

        private void ToClosedTbl(DbCore db, DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err)
        {
            ClosedTable closedTable;
            if (err == 0)
            {
                closedTable = new ClosedTable("Закрытые", db);
            }
            else
            {
                closedTable = new ClosedTable("Брак", db);
            }
            closedTable.AddProduct(dtClosed, dtOpen, SN, dev, name, err);
        }

        private void DeleteRowWorkTbl(DbCore db, string SN)
        {
            try
            {
                WorkTable workTable = new WorkTable(db);
                workTable.ExtractRow(SN);
            }
            catch
            {
                //Необязательная операция, добавить в лог программы, если ошибка
            }
        }

        private void ToOutBuffer(DbCore db, DateTime dtClosed, string SN, int error)
        {
            OutBuffer outBuffer = new OutBuffer(db);
            outBuffer.AddProduct(dtClosed, SN, error);
        }
    }
}
