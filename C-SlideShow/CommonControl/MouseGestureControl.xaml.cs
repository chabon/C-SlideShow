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
    /// MouseGestureControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseGestureControl : UserControl
    {
        private bool isEnabled;
        public Window parentWindow;
        private Shortcut.MouseGesture mouseGesture;

        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public string Stroke { get; set; } = "";
        public MouseButton StartingButton { get; set; }
        public bool ShowStartingButtonText { get; set; } = true;
        public bool AllowLButtonStart { get; set; } = false;
        public new bool IsEnabled
        {
            set
            {
                this.isEnabled = value;
                if( value )
                {
                    this.StrokeText.IsEnabled = true;
                    this.MainBorder.IsEnabled = true;
                }
                else
                {
                    this.StrokeText.IsEnabled = false;
                    this.MainBorder.IsEnabled = false;
                }
            }
            get { return this.isEnabled; }
        }
        public int Range
        {
            set
            {
                if(mouseGesture != null) mouseGesture.Range = value;
            }
        }

        public event EventHandler GestureAssigned;


        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
        public MouseGestureControl()
        {
            InitializeComponent();
        }

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        public void SetValue(string stroke, MouseButton startingButton)
        {
            this.Stroke = stroke;
            this.StartingButton = startingButton;
            UpdateStrokeText();
        }

        public void Clear()
        {
            this.Stroke = "";
            this.StrokeText.Text = "";
            if( mouseGesture != null ) mouseGesture.End();
            this.MainBorder.Background = new SolidColorBrush(Colors.White);
        }

        public void StartAcceptingInput()
        {
            this.MainBorder.Background = new SolidColorBrush(Colors.LightGreen);
            this.StrokeText.Text = "ボタン押下後、ドラッグして入力";

            // 親ウインドウ取得
            if(parentWindow == null )
            {
                parentWindow = Window.GetWindow(this);
                parentWindow.MouseDown += ParentWindow_MouseDown;
                parentWindow.Closing += (s, e) => {
                    if( mouseGesture != null ) mouseGesture.UnHook();
                };
            }
        }

        public void Ready()
        {
            if( mouseGesture != null ) mouseGesture.End();
            this.MainBorder.Background = new SolidColorBrush(Colors.White);
            UpdateStrokeText();
        }

        public string GetStartingButtonText()
        {
            switch(StartingButton)
            {
                case MouseButton.Left:
                    return "L";
                case MouseButton.Right:
                    return "R";
                case MouseButton.Middle:
                    return "M";
                case MouseButton.XButton1:
                    return "X1";
                case MouseButton.XButton2:
                    return "X2";
            }
            return "";
        }

        private void UpdateStrokeText()
        {
            if( ShowStartingButtonText)
            {
                this.StrokeText.Text = "(" + GetStartingButtonText() + ") " + Stroke;
            }
            else
            {
                this.StrokeText.Text = Stroke;
            }
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        private void ParentWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(mouseGesture == null )
            {
                mouseGesture = new Shortcut.MouseGesture();
                mouseGesture.StrokeChanged += MouseGesture_StrokeChanged;
                mouseGesture.GestureFinished += MouseGesture_GestureFinished;
            }

            if( MainBorder.IsFocused )
            {
                if( !AllowLButtonStart && e.ChangedButton == MouseButton.Left ) return;

                if( AllowLButtonStart ) mouseGesture.AllowLButtonStart = true;
                mouseGesture.Start(e.ChangedButton);
                this.StartingButton = e.ChangedButton;
                this.Stroke = "";
                UpdateStrokeText();
            }
        }

        private void MouseGesture_StrokeChanged(object sender, EventArgs e)
        {
            this.Stroke = mouseGesture.Stroke;
            UpdateStrokeText();
        }

        private void MouseGesture_GestureFinished(object sender, EventArgs e)
        {
            this.Stroke = mouseGesture.Stroke;
            UpdateStrokeText();
            this.GestureAssigned?.Invoke( this, new EventArgs() );
        }

        private void MainBorder_GotFocus(object sender, RoutedEventArgs e)
        {
            StartAcceptingInput();
        }

        private void MainBorder_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void MainBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(sender as Border);
        }

        private void MainBorder_LostFocus(object sender, RoutedEventArgs e)
        {
            Ready();
        }

    }
}
