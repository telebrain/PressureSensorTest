using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwenPressureDevices
{
    public class ArticulsTable: ExternalDataFile
    {
        public ArticulsTable(string localPath, string remotePath):
            base(localPath, remotePath)
        { }

        public string GetNameByArticul(string articul)
        {
            try
            {
                return SearshData(articul) as string;
            }
            catch (FileSearshException)
            {
                throw new ArticulslDataFileException(1);
            }
            catch (DataSearshException)
            {
                throw new ArticulslDataFileException(2);
            }
            catch(UpdateFileException)
            {
                throw new ArticulslDataFileException(3);
            }
        }

        protected override object AbstractSearshData(object content)
        {
            const int keyColumn = 1;
            const int valColumn = 2;
            int row = reader.SearshRowByContent(content as string, keyColumn);
            if (row == -1)
                return null;
            string name = base.GetCell(row, valColumn);
            return name;
        }


    }

    public class ArticulslDataFileException : Exception
    {
        public override string Message { get; }

        readonly string[] errors =
        {
            "",
            "Таблица артикулов недоступна. Определить название изделия не удалось",
            "Артикул изделия не найден",
            "Не удалось обновить локальную таблицу артикулов"
        };

        public ArticulslDataFileException(int messageNumber) : base()
        {
            Message = errors[messageNumber];
        }
    }
}
