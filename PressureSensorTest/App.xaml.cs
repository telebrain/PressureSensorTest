using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using log4net;
using log4net.Config;

namespace PressureSensorTest
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        readonly Mutex _mutex = new Mutex(false, "ApplicationOfMyGranny");

        // public static readonly ILog log = LogManager.GetLogger(typeof(App));

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!_mutex.WaitOne(500, false))
            {
                Environment.Exit(0);
            }
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
