using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDatabase2
{
    public class ResultTable : BaseTable
    {
        const string DateClosedHeader = "[Дата закрытия]";
        const string DateOpenedHeader = "[Дата открытия]";
        const string SnHeader = "[Серийный номер]";
        const string DevHeader = "[Номер оснастки]";
        const string NameHeader = "[Название датчика]";
        const string ErrorHeader = "Ошибка";
        const string FilesHeader = "Файлы";

        readonly string dbName;

        public ResultTable(string name, DbCore db) : base(name, db)
        {
            dbName = db.BaseName;
        }

        public void AddProduct(Product product)
        {
            try
            {
                string accessFormatDtCloosed = DateTimeToAccessFormat(product.DateCl);
                string[] row = ReadRow(DateClosedHeader, accessFormatDtCloosed);
                if (row == null)
                    NewRow(DateClosedHeader, accessFormatDtCloosed);
                UpdField(DateOpenedHeader, DateClosedHeader, accessFormatDtCloosed, product.Date);
                UpdField(SnHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(product.SN));
                UpdField(DevHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(product.Box));
                UpdField(NameHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(product.Name));
                UpdField(ErrorHeader, DateClosedHeader, accessFormatDtCloosed, Convert.ToString(product.Error));
                if (product.Log != null)
                    AttachmentToTable(accessFormatDtCloosed, product.SN, product.Log, product.LogFileExtension);
            }
            catch(Exception ex)
            {
                throw new WriteProductToResultTableException(ex, dbName, TableName, product.SN);
            }
        }

        public void AddLogFile(DateTime dateCl, string SN, byte[] log, string extension)
        {
            try
            {
                string accessFormatDtCloosed = DateTimeToAccessFormat(dateCl);
                AttachmentToTable(accessFormatDtCloosed, SN, log, extension);
            }
            catch(Exception ex)
            {
                throw new WriteLogException(ex, dbName, TableName, SN);
            }
        }

        public void AddLogFile(string SN, byte[] log, string extension)
        {
            // Если ключевой параметр (дата закрытия) неизвестен, выполняется поиск последней записи по дате
            var rows = ReadRows(SnHeader, AddQuotes(SN));
            var dates = new DateTime[rows.Count];
            for (int i = 0; i < dates.Length; i++)
            {
                dates[i] = DateTimeFromAccess((rows[i])[0]);
            }
            Array.Sort(dates);
            AddLogFile(dates[dates.Length - 1], SN, log, extension);
        }

        private void AttachmentToTable(string accessFormatDtCloosed, string SN, byte[] logFile, string ext)
        {
            List<Attachment> existingAttach = ReadAttachments(FilesHeader, DateClosedHeader, accessFormatDtCloosed);
            string name = GetNameLogFile(SN, existingAttach);
            Attachment attachment = new Attachment(name, TypeContentEnum.text, logFile);
            AddAttachment(FilesHeader, DateClosedHeader, accessFormatDtCloosed, attachment);
        }

        private string GetNameLogFile(string SN, List<Attachment> existingAttach, string extension = ".json")
        {
            string name = SN + extension;
            if (existingAttach == null) return name;

            for (int i = 2; i < 100; i++)
            {
                name = SN + "_" + Convert.ToString(i) + extension;
                bool searshOK = true;
                foreach (Attachment item in existingAttach)
                {
                    if (item.Name == name)
                    {
                        searshOK = false; break;
                    }
                }
                if (searshOK) break;
            }
            return name;
        }

        public void ClearTable() => DeleteAll();
    }
}
