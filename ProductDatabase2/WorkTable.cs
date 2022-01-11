using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDatabase2
{
    public class WorkTable : BaseTable
    {
        public WorkTable(string name, DbCore db) : base(name, db)
        {
            dbName = db.BaseName;
        }


        readonly string dbName;
        const string SnHeader = "Номер";
        const string BoxHeader = "Оснастка";

        public void AddProduct(Product product)
        {
            try
            {
                string[] strRow = ReadRow(SnHeader, AddQuotes(product.SN));
                if (strRow != null)
                {
                    DeleteRow(SnHeader, AddQuotes(product.SN));
                }
                else
                {
                    strRow = new string[4];
                }

                strRow[0] = DateTimeToAccessFormat(product.Date);
                strRow[1] = AddQuotes(product.SN);
                strRow[2] = AddQuotes(product.Box);
                strRow[3] = AddQuotes(product.Name);
                WriteRow(strRow);
            }
            catch (Exception ex)
            {
                throw new WriteToWorkTableException(ex, dbName, TableName, product.SN);
            }
        }

        public Product CutProduct(string sn)
        {
            try
            {
                Product product = GetProduct(sn);
                DeleteRow(SnHeader, AddQuotes(product.SN));
                return product;
            }
            catch (Exception ex)
            {
                throw new CutProductFromWorkTableException(ex, dbName, TableName, sn);
            }
        }

        public Product GetProduct(string id)
        {
            try
            {
                string[] row = null;
                if (id.Length == 4)
                {
                    // в метод был передан номер оснастки
                    row = ReadRow(BoxHeader, AddQuotes(id));
                }
                else
                {
                    // В метод был передан заводской номер
                    row = ReadRow(SnHeader, AddQuotes(id));
                }
                if (row == null)
                    return null;
                Product product = new Product
                {
                    Date = base.DateTimeFromAccess(row[0]),
                    SN = row[1],
                    Box = row[2],
                    Name = row[3]
                };
                return product;
            }
            catch(Exception ex)
            {
                throw new ReadProductFromWorkTableException(ex, dbName, TableName, id);
            }
        }

        public void ClearTable() => DeleteAll();
    }

}

