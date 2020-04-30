﻿using System;
using System.IO;
using System.Xml.Serialization;


namespace PressureSensorTest
{
    [Serializable]
    public class Settings
    {
        public PressureSystemSettings PsysSettings { get; set; }
        public AmmetrSettins AmmetrSettins { get; set; }

        public string PathToDb { get; set; }
        public bool UsedStandDatabase { get; set; }
        public bool UsedRemoteControl { get; set; }
        public bool UsedAutomaticSortingOut { get; set; }

        public event EventHandler UpdSettingsEvent;
        public JsonReportSettings JsonReportSettings { get; set; }

       
        public Settings()
        {
            PsysSettings = new PressureSystemSettings();
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
            AmmetrSettins = settings.AmmetrSettins;
            JsonReportSettings = settings.JsonReportSettings;
            PathToDb = settings.PathToDb;
            UsedStandDatabase = settings.UsedStandDatabase;
            UsedRemoteControl = settings.UsedRemoteControl;
            UsedAutomaticSortingOut = settings.UsedAutomaticSortingOut;
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
}
