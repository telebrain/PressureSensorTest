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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OwenPressureDevices;

namespace PressureSensorTest
{
    /// <summary>
    /// Логика взаимодействия для RelayTable.xaml
    /// </summary>
    public partial class RelayTable : UserControl
    {
        public RelayTable()
        {
            InitializeComponent();
        }
    }

    public class RelayTableData
    {
        public const int Rows = 3;
        public const int Columns = 8;

        public ElementTableResult[,] Data { get; set; }
        public string Units { get; set; }

        public RelayTableData()
        {
            Data = new ElementTableResult[Rows, Columns];
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    Data[row, column] = new ElementTableResult("", ColorItem.Neutral);
                }
            }
        }

        
    }

}
