using System;
using System.IO;
using System.Xml.Serialization;


namespace PressureSensorTest
{
    [Serializable]
    public class Settings
    {
        public PressSystemEnum PressSystemItem { get; set; }

        public PressureSystemSettings PsysSettings { get; set; }
        public PaceSettings PaceSettings { get; set; }
        public AmmetrSettins AmmetrSettins { get; set; }

        public string PathToDb { get; set; }
        public string DbPassword { get; set; }
        public bool UsedStandDatabase { get; set; }
        public bool UsedRemoteControl { get; set; }
        public bool UsedAutomaticSortingOut { get; set; }
        public string RemoteControlIp { get; set; }
        public string RemoteControlVer { get; set; }
        public string Password { get; set; } = "owen";

        public bool ShowVariation { get; set; }

        // Пауза при тестировании на 100% диапазона в сек
        public int TestPause100 { get; set; } = 60;

        public event EventHandler UpdSettingsEvent;
        public JsonReportSettings JsonReportSettings { get; set; }

       
        public Settings()
        {
            PsysSettings = new PressureSystemSettings();
            PaceSettings = new PaceSettings();
            AmmetrSettins = new AmmetrSettins();
            JsonReportSettings = new JsonReportSettings();
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(GetSettingsFolderPath()))
                    Directory.CreateDirectory(GetSettingsFolderPath());
                using (FileStream fs = new FileStream(GetSettingFilePath(), FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(Settings));
                    serializer.Serialize(fs, this);
                }

                UpdSettingsEvent?.Invoke(this, new EventArgs());
            }
            catch { System.Diagnostics.Debug.WriteLine("Не удалось сохранить файл настроек"); }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(GetSettingFilePath()))
                {
                    using (FileStream fs = new FileStream(GetSettingFilePath(), FileMode.Open))
                    {
                        var serializer = new XmlSerializer(typeof(Settings));
                        UpdSettings((Settings)serializer.Deserialize(fs));
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Не удалось загрузить файл настроек");
            }
        }

        private void UpdSettings(Settings settings)
        {
            PsysSettings = settings.PsysSettings;
            PaceSettings = settings.PaceSettings;
            AmmetrSettins = settings.AmmetrSettins;
            JsonReportSettings = settings.JsonReportSettings;
            PathToDb = settings.PathToDb;
            DbPassword = settings.DbPassword;
            UsedStandDatabase = settings.UsedStandDatabase;
            UsedRemoteControl = settings.UsedRemoteControl;
            UsedAutomaticSortingOut = settings.UsedAutomaticSortingOut;
            RemoteControlIp = settings.RemoteControlIp;
            if (!string.IsNullOrEmpty(settings.RemoteControlVer))
                RemoteControlVer = settings.RemoteControlVer;
            else
                RemoteControlVer = "v1.0";
            PressSystemItem = settings.PressSystemItem;
            ShowVariation = settings.ShowVariation;
            TestPause100 = settings.TestPause100;
            Password = settings.Password;
        }

        private string GetSettingsFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PressureSensorTest");
        }

        private string GetSettingFilePath()
        {
            return Path.Combine(GetSettingsFolderPath(), "settings.xml");
        }
    }

    public enum PressSystemEnum { PressureRack, Pace5000 }

}
