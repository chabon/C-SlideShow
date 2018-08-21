using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Windows.Input;

using C_SlideShow.Shortcut;


namespace C_SlideShow
{
    /// <summary>
    /// ショートカットのマッピング設定クラス
    /// </summary>
    [DataContract(Name = "ShortcutSetting")]
    public class ShortcutSetting
    {
        // キー入力Map (キー入力 - コマンドID)
        [DataMember]
        public List<KeyMap> KeyMap = new List<KeyMap>();

        // マウス入力Map (マウス入力 - コマンドID)
        public List<MouseInputMap> MouseInputMap = new List<MouseInputMap>();

		// マウスジェスチャーMap (ジェスチャー -> コマンドIDを取得)
        //[DataMember]
        //public Dictionary<string, Commands> GestureMap = new Dictionary<string, Commands>();
	
        // タッチ入力Map (タッチ入力 - コマンドID)


        public ShortcutSetting()
        {
            initKeyMap();
            initMouseInputMap();
        }

        private void initKeyMap()
        {
            KeyMap.Clear();
            KeyMap = CreateDefaultKeyMap();
        }

        private void initMouseInputMap()
        {
            MouseInputMap.Clear();
            MouseInputMap = CreateDefaultMouseInputMap();
        } 

        public static List<KeyMap> CreateDefaultKeyMap()
        {
            List<KeyMap> defaultKeyMap = new List<KeyMap>();

            // 全般
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.Control,                      Key.F),        CommandID.OpenFolder                 )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.Control | ModifierKeys.Shift, Key.F),        CommandID.OpenAdditionalFolder       )  );

            // 通常時
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.F),        CommandID.SlideToForward             )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.B),        CommandID.SlideToBackward            )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.Shift,                        Key.F),        CommandID.SlideToForwardByOneImage   )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.Shift,                        Key.B),        CommandID.SlideToBackwardByOneImage  )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Left),     CommandID.SlideToLeft                )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Up),       CommandID.SlideToTop                 )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Right),    CommandID.SlideToRight               )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Down),     CommandID.SlideToBottom              )  );

            // 画像拡大時
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Up),       CommandID.ZoomInImage                )  );
            defaultKeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Down),     CommandID.ZoomOutImage               )  );



            return defaultKeyMap;
        }

        public static List<MouseInputMap> CreateDefaultMouseInputMap()
        {
            List<MouseInputMap> defaultMouseInputMap = new List<MouseInputMap>();

            // 全般
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputHold.R_Button, MouseInputButton.WheelUp),      CommandID.WindowSizeUp             )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputHold.R_Button, MouseInputButton.WheelDown),    CommandID.WindowSizeDown           )  );

            // 通常時
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputHold.None, MouseInputButton.R_Click),          CommandID.ZoomImageUnderCursor     )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputHold.None, MouseInputButton.WheelUp),          CommandID.SlideToBackward          )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputHold.None, MouseInputButton.WheelDown),        CommandID.SlideToForward           )  );

            // 画像拡大時
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputHold.None, MouseInputButton.WheelUp),          CommandID.ZoomInImage             )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputHold.None, MouseInputButton.WheelDown),        CommandID.ZoomOutImage            )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputHold.None, MouseInputButton.R_Click),          CommandID.ExitZoom                )  );

            // test
            //defaultMouseInputMap.Add(new MouseInputMap(new MouseInput(MouseInputHold.Shift, MouseInputButton.L_Click), CommandID.OpenFolder));
            //defaultMouseInputMap.Add(new MouseInputMap(new MouseInput(MouseInputHold.L_Button, MouseInputButton.M_Click), CommandID.OpenFile));
            //defaultMouseInputMap.Add(new MouseInputMap(new MouseInput(MouseInputHold.None, MouseInputButton.L_DoubleClick), CommandID.OpenFolder));


            return defaultMouseInputMap;

        }
    }
}
