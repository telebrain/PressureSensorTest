using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Policy;
using System.IO;

namespace ArchvingTestResult
{
    public class ArchivingProcess
    {
        Archiving archiving;
        TrackingResultFiles trackingResultFiles;

        public event EventHandler SuccessfulCopyToServerEvent;

        public ArchivingProcess(string path, int maxArchivingInterval, bool usedFtp, string login, string password)
        {
            if (usedFtp)
                archiving = new ArchivingToFtp(path, LocalFolder, maxArchivingInterval, login, password);
            else
                archiving = new ArchivingToFolder(path, LocalFolder, maxArchivingInterval);
            trackingResultFiles = new TrackingResultFiles(archiving, 1);
            trackingResultFiles.SuccessfulSaveEvent += (obj, e) => SuccessfulCopyToServerEvent?.Invoke(this, new EventArgs());
            
        }

        public void Save(string fileName, string content)
        {
            lock (archiving.SyncRoot)
            {
                try
                {
                    archiving.SaveResults(fileName, content);
                }
                catch (SavingToRemoteArchiveException e)
                {
                    archiving.WriteToLocalFolder(fileName, content);
                    throw e;
                }
                catch
                {
                    throw;
                }
            }
        }

        public void CheckLocalFolderState()
        {
            archiving.CheckLocalFolderState();
        }

        // Запускает слежение за локальной папкой сохранения результатов
        // При появлении связи с сервером, пытается туда копировать файлы из локальной директории
        public void StartTracking()
        {
            trackingResultFiles.StartTracking();
        }

        private string LocalFolder
        {
            get
            {
                string appName = AppDomain.CurrentDomain.FriendlyName;
                appName = appName.Split('.')[0];

                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        appName, "Result");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return dir;
            }
        }

        ~ArchivingProcess()
        {
            trackingResultFiles?.StartTracking();
        }
    }

    public class ServerErrorException: Exception
    {
        public ServerErrorException(string message): base(message) { }
    }
}
