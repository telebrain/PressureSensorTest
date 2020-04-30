using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace ArchvingTestResult
{
    public abstract class Archiving
    {
        public string LocalFolder { get; protected set; }

        public string RemotePath { get; protected set; }

        public object SyncRoot { get; } = new object();

        public abstract void AddResultToArchive(string fileName, string data);

        protected int MaxArchivingInteval { get; set; } = 1;

        public Archiving(int maxArchivingIntervalInHours)
        {
            MaxArchivingInteval = maxArchivingIntervalInHours;
        }

        public void SaveResults(string fileName, string data)
        {
            try
            {
                // Попытка записи в удаленную директорию
                AddResultToArchive(fileName, data);
            }
            catch
            {
                // Запись в локальную директорию
                WriteToLocalFolder(fileName, data);
                throw;
            }
        }

        public void WriteToLocalFolder(string fileName, string data)
        {
            try
            {
                string path = GetLocalPath(fileName);
                WriteFile(path, data, 0);
            }
            catch(Exception ex)
            {
                throw new SavingToLocalFolderException(ex.Message);
            }
        }

        public string GetLocalPath(string fileName)
        {
            return Path.Combine(LocalFolder, fileName);
        }

        protected string FormatContentBySaving(string content, bool newFile)
        {
            string result = "";
            if (newFile)
            {
                result = string.Format("[\r\n{0}\r\n]", content);
            }
            else
            {
                result = string.Format(",\r\n{0}\r\n]", content);
            }
            return result;
        }

        protected void WriteFile(string fileName, string data, long position)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                fs.Seek(position, SeekOrigin.End);
                byte[] bytes = Encoding.Default.GetBytes(data);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public string[] GetFilesFromLocalFolder()
        {
            return Directory.GetFiles(LocalFolder, "*.json");
        }

        public void AddFromLocalFolder(string name, string localFileName)
        {
            string content = ReadFile(localFileName);
            AddResultToArchive(name, content);
            File.Delete(localFileName);
        }

        public void CheckLocalFolderState()
        {
            string[] files = GetFilesFromLocalFolder();
            if (files == null || files.Length == 0)
                return;


            List<LocalFilePropertys> localFiles = new List<LocalFilePropertys>();
            foreach (string file in files)
            {
                localFiles.Add(new LocalFilePropertys(file));
            }

            localFiles.Sort();

            TimeSpan timeSpan = DateTime.Now - localFiles[0].CreationTime;

            if (timeSpan > new TimeSpan(MaxArchivingInteval, 0, 0))
                throw new SavingToRemoteArchiveException(string.Format("\r\nПопытки сохранения результатов тестирования в предыдущие {0} и более часов заканчивались неудачно. " +
                        "Для продолжения работы программы устраните причину ошибок - измените настройки сохранения и(или) обеспечьте надежную связь с сервером", MaxArchivingInteval));
        }

        

        private bool DtFromLocalFile(string fileName, out DateTime dateTime)
        {
            const string ext = ".json";
            string resultStr = fileName.Substring(0, fileName.IndexOf(ext));
            resultStr = resultStr.Replace('_', ' ');
            resultStr = resultStr.Replace('-', ':');
            return DateTime.TryParse(resultStr, out dateTime);
        }

        private string ReadFile(string fileName)
        {
            string content = "";
            using (var fs = new StreamReader(fileName))
            {
                content = fs.ReadToEnd();
            }
            return content;
        }
    }
}
