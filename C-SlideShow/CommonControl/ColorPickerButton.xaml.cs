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


namespace C_SlideShow.CommonControl
{
    /// <summary>
    /// ColorPickerButton.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorPickerButton : UserControl
    {
        public ColorPickerButton()
        {
            InitializeComponent();
        }

        // "Click"イベントをルーティングイベントとして登録する。  
        public static readonly RoutedEvent ColorPickedEvent = EventManager.RegisterRoutedEvent("ColorPicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorPickerButton));  

        public event RoutedEventHandler ColorPicked
        {
            add { AddHandler(ColorPickedEvent, value); }
            remove { RemoveHandler(ColorPickedEvent, value); }
        }

        // 選択された色
        private Color pickedColor;
        public Color PickedColor
        {
            set
            {
                this.ColorPicker_Border.Background = new SolidColorBrush(value);
                pickedColor = value;
            }
            get
            {
                return pickedColor;
            }
        }

        private void ColorPicker_Button_Click(object sender, RoutedEventArgs e)
        {
            // (todo) 親ウインドウを取得して、ダイアログを中央に表示

            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if( cd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                PickedColor = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
                RoutedEventArgs newEventArgs = new RoutedEventArgs(ColorPickerButton.ColorPickedEvent);  
                RaiseEvent(newEventArgs);  
                //RaiseEvent(e);  
                //this.Click?.Invoke(this, e);
            }
        }
    }
}
