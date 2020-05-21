using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS_Access_DB;

namespace StandDb
{
    public class ClosedTable: BaseTable
    {
        const string DateClosedHeader = "[Дата закрытия]";
        const string DateOpenedHeader = "[Дата открытия]";
        const string SnHeader = "[Серийный номер]";
        const string DevHeader = "[Номер оснастки]";
        const string NameHeader = "[Название датчика]";        
        const string ErrorHeader = "Ошибка";
        const string FilesHeader = "Файлы";

        public ClosedTable(string name, DbCore db): base(name, db) { }

        public void AddProduct(DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err, byte[] logFile, string extension = "")
        {
            string accessFormatDtCloosed = DateTimeToAccessFormat(dtClosed);
            string[] row = ReadRow(DateClosedHeader, accessFormatDtCloosed);
            if (row == null) NewRow(DateClosedHeader, accessFormatDtCloosed);
            UpdField(DateOpenedHeader, DateClosedHeader, accessFormatDtCloosed, dtOpen);
            UpdField(SnHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(SN));
            UpdField(DevHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(dev));
            UpdField(NameHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(name));
            UpdField(ErrorHeader, DateClosedHeader, accessFormatDtCloosed, Convert.ToString(err));
            AttachmentToTable(accessFormatDtCloosed, SN, logFile, extension);
        }

        public void AddProduct(DateTime dtClosed, DateTime dtOpen, string SN, string dev, string name, int err)
        {
            string accessFormatDtCloosed = DateTimeToAccessFormat(dtClosed);
            string[] row = ReadRow(DateClosedHeader, accessFormatDtCloosed);
            if (row == null) NewRow(DateClosedHeader, accessFormatDtCloosed);
            UpdField(DateOpenedHeader, DateClosedHeader, accessFormatDtCloosed, dtOpen);
            UpdField(SnHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(SN));
            UpdField(DevHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(dev));
            UpdField(NameHeader, DateClosedHeader, accessFormatDtCloosed, AddQuotes(name));
            UpdField(ErrorHeader, DateClosedHeader, accessFormatDtCloosed, Convert.ToString(err));
        }

        private void AttachmentToTable(string accessFormatDtCloosed, string SN, byte[] content, string extension = "")
        {
            List<Attachment> existingAttach = ReadAttachments(FilesHeader, DateClosedHeader, accessFormatDtCloosed);
            string name = GetNameLogFile(SN, existingAttach, extension);
            Attachment attachment = new Attachment(name, TypeContentEnum.txt, content);
            AddAttachment(FilesHeader, DateClosedHeader, accessFormatDtCloosed, attachment);
        }

        private string GetNameLogFile(string SN, List<Attachment> existingAttach, string extension = "")
        {
            string name = SN;
            if (extension == "")
                name += ".xml";
            else
                name += extension;

            if (existingAttach == null) return name;
           
            for (int i = 2; i < 100; i++)
            {
                name = SN + "_" + Convert.ToString(i) + ".xml";
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
    }
}
