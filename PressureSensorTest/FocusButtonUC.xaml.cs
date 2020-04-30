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
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PressureSensorTest
{
    /// <summary>
    /// Interaction logic for FocusButtonUC.xaml
    /// </summary>
    public partial class FocusButtonUC : UserControl
    {
        DoubleAnimation animation;

        public FocusButtonUC()
        {
            InitializeComponent();
            animation = new DoubleAnimation(14, 18, TimeSpan.FromMilliseconds(300))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
        }
               
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FocusButtonUC), new PropertyMetadata(""));



        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(FocusButtonUC));

        public bool Signal
        {
            get { return (bool)GetValue(SignalProperty); }
            set { SetValue(SignalProperty, value); }
        }

        public static readonly DependencyProperty SignalProperty =
            DependencyProperty.Register("Signal", typeof(bool), typeof(FocusButtonUC), 
                new PropertyMetadata(false, SignalPropertyChanged));

        private static void SignalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uc = (FocusButtonUC)d;
            if ((bool)e.NewValue)
            {
                uc.MyButton.FontWeight = FontWeights.Bold;
                uc.MyButton.Foreground = uc.SignalTextColor;
                uc.MyButton.BeginAnimation(Button.FontSizeProperty, uc.animation);
                uc.MyButton.Focus();
            }
            else
            {
                uc.MyButton.FontWeight = FontWeights.Normal;
                uc.MyButton.Foreground = uc.IsEnabled ? Brushes.Black : Brushes.Gray;
                uc.MyButton.BeginAnimation(Button.FontSizeProperty, null);
            }

        }

        public Brush SignalTextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register("TextColor", typeof(Brush), typeof(FocusButtonUC), new PropertyMetadata(Brushes.Black));


    }
}
