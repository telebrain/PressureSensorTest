using System;
using System.IO;
using System.Text;

namespace ArchvingTestResult
{
    public class ArchivingToFolder: Archiving
    {
        public ArchivingToFolder(string folder, string localFolder, int maxArchivingIntervalInHours):
            base(maxArchivingIntervalInHours)
        {

            RemotePath = folder;
            LocalFolder = localFolder;
        }

        public override void AddResultToArchive(string fileName, string data)
        {
            CheckExistingFolder();
            try
            {
                string filePath = Path.Combine(RemotePath, fileName);
                //bool newFile = !File.Exists(fileName);
                //string content = FormatContentBySaving(data, newFile);
                //long position = newFile ? 0 : -1;
                WriteFile(filePath, data, 0);
            }
            catch
            {
                throw new SavingToRemoteArchiveException("Не удалось сохранить файл результатов");
            }
        }

        private void CheckExistingFolder()
        {
            if (!Directory.Exists(RemotePath))
            {
                throw new SavingToRemoteArchiveException("Директрория сохранения результатов недоступна или неверно задан путь");
            }
        }
      
    }
}
