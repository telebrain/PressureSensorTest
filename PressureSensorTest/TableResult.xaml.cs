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


namespace PressureSensorTest
{
    /// <summary>
    /// Логика взаимодействия для TableResult.xaml
    /// </summary>
    public partial class TableResult : UserControl
    {
        TextBlock[,] dataTableInd = new TextBlock[TableResultData.Rows, TableResultData.Columns];

        string[,] TableData;
        

        public TableResult()
        {
            InitializeComponent();

            int Columns = TableResultData.Columns;
            int Rows = TableResultData.Rows;

            const int StartRow = 3;
            Rectangle[,] rectangles = new Rectangle[Rows, Columns];

            TableData = new string[Rows, Columns];
            for (int row = 0; row < Rows; row++)               
            {
                for (int column = 0; column < Columns; column++)
                {
                    // Создание прямоугольника
                    rectangles[row, column] = new Rectangle();
                    Grid.SetColumn(rectangles[row, column], column);
                    Grid.SetRow(rectangles[row, column], row + StartRow);
                    Tbl.Children.Add(rectangles[row, column]);
                    

                    // Создание TextBlock для индикации
                    dataTableInd[row, column] = new TextBlock();
                    Grid.SetColumn(dataTableInd[row, column], column);
                    Grid.SetRow(dataTableInd[row, column], row + StartRow);
                    Tbl.Children.Add(dataTableInd[row, column]);

                }
            }
        }

        public string Units
        {
            get { return (string)GetValue(unitsProperty); }
            set { SetValue(unitsProperty, value); }
        }

        public static DependencyProperty unitsProperty = DependencyProperty.Register("Units", typeof(string), typeof(TableResult),
            new PropertyMetadata("кПа"));

        public TableResultData TableDataSrc
        {
            get { return (TableResultData)GetValue(tableResultDataProperty); }
            set { SetValue(tableResultDataProperty, value); }
        }

        public static DependencyProperty tableResultDataProperty = DependencyProperty.Register("TableDataSrc", typeof(TableResultData),
            typeof(TableResult), new UIPropertyMetadata(new TableResultData(), TableResultChange));

        private static void TableResultChange(object sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = (TableResult)sender;
            var data = (TableResultData)e.NewValue;

            for (int row = 0; row < TableResultData.Rows; row++)
            {
                for (int column = 0; column < TableResultData.Columns; column++)
                {
                    obj.dataTableInd[row, column].Text = data.Data[row, column].Content;
                    switch (data.Data[row, column].Color)
                    {
                        case ColorItem.Neutral:
                            obj.dataTableInd[row, column].Foreground = Brushes.Black;  
                            break;
                        case ColorItem.Ok:
                            obj.dataTableInd[row, column].Foreground = Brushes.Green;
                            obj.dataTableInd[row, column].FontWeight = FontWeights.Bold;
                            break;
                        case ColorItem.Error:
                            obj.dataTableInd[row, column].Foreground = Brushes.Red;
                            obj.dataTableInd[row, column].FontWeight = FontWeights.Bold;
                            break;
                    }
                }
            }
            // Костыль, но уже времени нет разбираться, почему привязка не работает
            obj.UnitInd_1.Text = data.Units;
            obj.UnitInd_2.Text = data.Units;
            obj.UnitInd_3.Text = data.Units;
            obj.UnitInd_4.Text = data.Units;
        }


        //public string Units
        //{
        //    get { return (string)GetValue(unitsDependencyProperty); }
        //    set { SetValue(unitsDependencyProperty, value); }
        //}

        //private static DependencyProperty unitsDependencyProperty = DependencyProperty.Register("Units", typeof(string), typeof(TableResult));

    }
}
