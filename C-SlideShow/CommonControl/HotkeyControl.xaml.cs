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
    /// HotkeyControl.xaml の相互作用ロジック
    /// </summary>
    public partial class HotkeyControl : UserControl
    {
        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public ModifierKeys Modifiers { get; set; }

        public Key Key { get; set; }
        private bool isEnabled;
        public new bool IsEnabled
        {
            set
            {
                this.isEnabled = value;
                if( value )
                {
                    this.KeyText.IsEnabled = true;
                    this.MainBorder.IsEnabled = true;
                }
                else
                {
                    this.KeyText.IsEnabled = false;
                    this.MainBorder.IsEnabled = false;
                }
            }
            get { return this.isEnabled; }
        }

        public event EventHandler KeyAssigned;


        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
        public HotkeyControl()
        {
            InitializeComponent();
        }

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        public void SetKey(ModifierKeys modifers, Key key)
        {
            this.Modifiers = modifers;
            this.Key = key;
            this.KeyText.Text = GetKeyString();
        }

        public void Clear()
        {
            this.Modifiers = ModifierKeys.None;
            this.Key = Key.None;
            this.KeyText.Text = GetKeyString();
            Ready();
        }

        public bool IsValidKey()
        {
            // 修飾キー単体
            if( Key.LeftShift <= this.Key && this.Key <= Key.RightAlt ) return false;
            if( this.Key == Key.System ) return false;

            // キーなし
            if( Key == Key.None ) return false;

            return true;
        }

        public void StartAcceptingInput()
        {
            this.MainBorder.Background = new SolidColorBrush(Colors.LightGreen);
            this.KeyText.Text = "設定キーを入力してください";
        }

        public void Ready()
        {
            this.MainBorder.Background = new SolidColorBrush(Colors.White);
            this.KeyText.Text = GetKeyString();
        }

        private string GetKeyString()
        {
            string modStr = "";

            // 修飾キー
            if(  ( (int)Modifiers & (int)ModifierKeys.Control ) != 0  )
            {
                modStr += "Ctrl + ";
            }
            if(  ( (int)Modifiers & (int)ModifierKeys.Shift ) != 0  )
            {
                modStr += "Shift + ";
            }
            if(  ( (int)Modifiers & (int)ModifierKeys.Alt ) != 0  )
            {
                modStr += "Alt + ";
            }

            // キー
            string keyStr = Key.ToString();
            if(Key.D0 <= this.Key && this.Key <= Key.D9 )
            {
                keyStr = keyStr.Replace("D", "");
            }

            else if(Key.LeftShift <= this.Key && this.Key <= Key.RightAlt )
            {
                keyStr = "";
            }

            else if(this.Key == Key.None )
            {
                keyStr = "";
            }

            else if(this.Key == Key.System )
            {
                keyStr = "";
            }

            return modStr + keyStr;
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        private void MainBorder_GotFocus(object sender, RoutedEventArgs e)
        {
            StartAcceptingInput();
        }

        private void MainBorder_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // キー情報取得
            if (e.ImeProcessedKey != Key.None) // Ime有効な場合の対処
            {
                this.Key = e.ImeProcessedKey;
            }
            else if(e.Key == Key.System ) // システムキーが押された場合
            {
                this.Key = e.SystemKey;
            }
            else
            { 
                this.Key = e.Key;
            }

            this.Modifiers = Keyboard.Modifiers;
            this.KeyText.Text = GetKeyString();
            e.Handled = true;

            // キー割り当て完了通知
            if( IsValidKey() ) KeyAssigned?.Invoke( this, new EventArgs() );
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
