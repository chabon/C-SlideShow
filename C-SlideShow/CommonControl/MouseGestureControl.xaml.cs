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
        private Window parentWindow;
        private Shortcut.MouseGesture mouseGesture;

        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public string Stroke { get; set; } = "";
        public MouseButton StartingButton { get; set; }
        public bool ShowStartingButtonText { get; set; } = true;
        public string OperationDesc { get; } = "任意のボタンを押下しながら\nドラッグ or クリック or ホイール";
        public bool AllowLButtonStart { get; set; } = true;
        public bool AllowLButtonDrag { get; set; } = true;
        public bool AllowHoldClick { get; set; } = true; // ジェスチャ中のクリックをジェスチャとみなすかどうか
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

        public event EventHandler MainBorderLostFocus;
        public event EventHandler GestureAssigned;


        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
        public MouseGestureControl()
        {
            InitializeComponent();

            this.MouseDown += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("MouseGestureControl_MouseDown");
            };

            this.MouseUp += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("MouseGestureControl_MouseUp");
            };
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
            //this.StrokeText.Text = "ボタン押下後、ドラッグして入力";
            this.StrokeText.Text = OperationDesc;

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
            if( Stroke.Length > 0 )
                UpdateStrokeText();
            else
                this.StrokeText.Text = "";
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
            // 左クリックのみしか押して無い場合は更新せず、操作説明の表示
            if( StartingButton == MouseButton.Left && Stroke == "" )
            {
                this.StrokeText.Text = OperationDesc;
                return;
            }

            // 更新
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
            if( MainBorder.IsFocused )
            {
                if(mouseGesture == null )
                {
                    mouseGesture = new Shortcut.MouseGesture();
                    mouseGesture.StrokeChanged += MouseGesture_StrokeChanged;
                    mouseGesture.GestureFinished += MouseGesture_GestureFinished;
                }

                if( !AllowLButtonStart && e.ChangedButton == MouseButton.Left ) return;
                if( mouseGesture.IsActive ) return; // 既にジェスチャ入力中

                // ジェスチャー準備
                StartingButton = e.ChangedButton;
                Stroke = "";
                UpdateStrokeText();

                // ジェスチャー開始
                if( !AllowLButtonDrag && e.ChangedButton == MouseButton.Left ) mouseGesture.EnableDragGesture = false;
                else mouseGesture.EnableDragGesture = true;
                mouseGesture.Start(e.ChangedButton);
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

        private void MainBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MainBorder_MouseDown");
        }

        private void MainBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MainBorder_PreviewMouseDown");
            Keyboard.Focus(sender as Border);
        }

        private void MainBorder_LostFocus(object sender, RoutedEventArgs e)
        {
            Ready();
            MainBorderLostFocus?.Invoke(this, new EventArgs());
        }

    }
}
