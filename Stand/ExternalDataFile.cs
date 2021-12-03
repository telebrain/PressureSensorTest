using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleExcelClosedXML;
using System.IO;

namespace OwenPressureDevices
{
    public abstract class ExternalDataFile
    {
        protected  XlFileReader reader;
        readonly string localPath;
        readonly string remotePath;


        public ExternalDataFile(string localPath, string remotePath)
        {
            this.localPath = localPath;
            this.remotePath = remotePath;
            reader = new XlFileReader();
        }


        bool isLoaded = false;
        private void LoadTable()
        {
            try
            {
                ReadDatalFile(localPath);
            }
            catch
            {
                // не удалось прочитать локальную таблицу, читаем удаленную
                ReadDatalFile(remotePath);
                UpdateLocalTable();
            }
            finally
            {
                isLoaded = true;
            }
        }

        private void ReadDatalFile(string path)
        {
            if (!File.Exists(path))
                throw new FileSearshException();
            try
            {
                reader.LoadFile(path);
            }
            catch 
            {
                throw new FileSearshException();
            }

        }

        protected abstract object AbstractSearshData(object content);

        protected object SearshData(object content, bool usedRemoteSource = false)
        {
            if (!isLoaded)
                LoadTable();
            object data = AbstractSearshData(content);
            if (data == null)
            {
                if (!usedRemoteSource)
                {
                    try
                    {
                        ReadDatalFile(remotePath);
                        UpdateLocalTable();
                        data = SearshData(content, true);
                    }
                    catch
                    {
                        throw new FileSearshException();
                    }
                }
                else
                {
                    throw new DataSearshException();
                }
            }
            return data;
        }

        protected string GetCell(int row, int column)
        {
            return reader.GetCellValue(row, column);
        }

        private void UpdateLocalTable()
        {
            try
            {
                File.Copy(remotePath, localPath);
            }
            catch
            {
                throw new UpdateFileException();
            }
        }
    }
    [Serializable]
    public class FileSearshException: Exception { }
    [Serializable]
    public class DataSearshException: Exception { }
    [Serializable]
    public class UpdateFileException: Exception { }
}
