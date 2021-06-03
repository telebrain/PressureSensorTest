using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PressureSensorTest
{
    /// <summary>
    /// Логика взаимодействия для Password.xaml
    /// </summary>
    public partial class Password : Window
    {
        readonly string password = "";

        public bool? PwChecked { get; private set; }

        public Password(string password)
        {
            InitializeComponent();
            this.password = password;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PwChecked = password == PasswordFeld.Password;
            Close();
        }
    }
}
