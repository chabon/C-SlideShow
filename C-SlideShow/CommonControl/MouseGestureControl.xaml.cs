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
        public string Stroke { get; set; }
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
        public void SetStroke(string stroke)
        {
            this.Stroke = stroke;
            this.StrokeText.Text = stroke;
            Ready();
        }

        public void Clear()
        {
            this.Stroke = "";
            this.StrokeText.Text = "";
            Ready();
        }

        public void StartAcceptingInput()
        {
            this.MainBorder.Background = new SolidColorBrush(Colors.LightGreen);
            this.StrokeText.Text = "右ドラッグしてジェスチャを登録";

            // 親ウインドウ取得
            if(parentWindow == null )
            {
                parentWindow = Window.GetWindow(this);
                parentWindow.MouseRightButtonDown += ParentWindow_MouseRightButtonDown;
                parentWindow.Closing += (s, e) => {
                    if( mouseGesture != null ) mouseGesture.UnHook();
                };
            }
        }

        public void Ready()
        {
            if( mouseGesture != null ) mouseGesture.End();
            this.MainBorder.Background = new SolidColorBrush(Colors.White);
            this.StrokeText.Text = Stroke;
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        private void ParentWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(mouseGesture == null )
            {
                mouseGesture = new Shortcut.MouseGesture();
                mouseGesture.StrokeChanged += MouseGesture_StrokeChanged;
                mouseGesture.GestureFinished += MouseGesture_GestureFinished;
            }

            if(MainBorder.IsFocused) mouseGesture.Start();
        }

        private void MouseGesture_StrokeChanged(object sender, EventArgs e)
        {
            this.Stroke = mouseGesture.Stroke;
            this.StrokeText.Text = mouseGesture.Stroke;
        }

        private void MouseGesture_GestureFinished(object sender, EventArgs e)
        {
            this.Stroke = mouseGesture.Stroke;
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
