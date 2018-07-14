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

using System.Reflection;


namespace C_SlideShow.CommonControl
{
    /// <summary>
    /// NumericUpDown.xaml の相互作用ロジック
    /// https://stackoverflow.com/questions/841293/where-is-the-wpf-numeric-updown-control
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        int minvalue = 0,
            maxvalue = 10000,
            startvalue = 10,
            variation = 1;
            

        public NumericUpDown()
        {
            InitializeComponent();
            NUDTextBox.Text = startvalue.ToString();
        }

        public int Value
        {
            get
            {
                int number = 0;
                if( NUDTextBox.Text != "" ) int.TryParse(NUDTextBox.Text, out number);
                return number;
            }
            set { NUDTextBox.Text = value.ToString(); }
        }

        public int MinValue
        {
            get { return minvalue; }
            set { minvalue = value; }
        }

        public int MaxValue
        {
            get { return maxvalue; }
            set { maxvalue = value; }
        }

        public int StartValue
        {
            get { return startvalue; }
            set { startvalue = value; }
        }

        public int Variation
        {
            get { return variation; }
            set { variation = value; }
        }

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number < maxvalue)
                NUDTextBox.Text = Convert.ToString(number + variation); 
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number > minvalue)
                NUDTextBox.Text = Convert.ToString(number - variation); 
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
            {
                NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { true }); 
            }


            if (e.Key == Key.Down)
            {
                NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { true }); 
            }
        }

        private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { false });

            if (e.Key == Key.Down)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { false });
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox.Text!="")
                if (!int.TryParse(NUDTextBox.Text, out number)) NUDTextBox.Text = startvalue.ToString();
            if (number > maxvalue)  NUDTextBox.Text = maxvalue.ToString();
            if (number < minvalue) NUDTextBox.Text = minvalue.ToString();
            NUDTextBox.SelectionStart = NUDTextBox.Text.Length;

        }

    }
}
