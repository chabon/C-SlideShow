﻿using System;
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
        [DataMember]
        public List<MouseInputMap> MouseInputMap = new List<MouseInputMap>();

        // マウスジェスチャーMap (ジェスチャー - コマンドIDを取得)
        [DataMember]
        public List<MouseGestureMap> MouseGestureMap = new List<MouseGestureMap>();
	
        // タッチ入力Map (タッチ入力 - コマンドID)


        public ShortcutSetting()
        {
            initKeyMap();
            initMouseInputMap();
            initMouseGestureMap();
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

        private void initMouseGestureMap()
        {
            MouseGestureMap.Clear();
            MouseGestureMap = CreateDefaultMouseGestureMap();
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

            // 通常時
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputButton.R_Click, ModifierKeys.None),         CommandID.ZoomImageUnderCursor      )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputButton.WheelUp, ModifierKeys.None),         CommandID.SlideToBackward           )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputButton.WheelDown, ModifierKeys.None),       CommandID.SlideToForward            )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputButton.WheelUp, ModifierKeys.Shift),        CommandID.SlideToBackwardByOneImage )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputButton.WheelDown, ModifierKeys.Shift),      CommandID.SlideToForwardByOneImage  )  );

            // 画像拡大時
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputButton.WheelUp, ModifierKeys.None),         CommandID.ZoomInImage               )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputButton.WheelDown, ModifierKeys.None),       CommandID.ZoomOutImage              )  );
            defaultMouseInputMap.Add(  new MouseInputMap( new MouseInput(MouseInputButton.R_Click, ModifierKeys.None),         CommandID.ExitZoom                  )  );

            return defaultMouseInputMap;
        }

        public static List<MouseGestureMap> CreateDefaultMouseGestureMap()
        {
            List<MouseGestureMap> defaultMouseGestureMap = new List<MouseGestureMap>();

            // 全般
            defaultMouseGestureMap.Add(new MouseGestureMap(new MouseGestureInput(MouseButton.Right, "[WU]"), CommandID.WindowSizeUp));
            defaultMouseGestureMap.Add(new MouseGestureMap(new MouseGestureInput(MouseButton.Right, "[WD]"), CommandID.WindowSizeDown));

            // 通常時

            // 画像拡大時

            return defaultMouseGestureMap;

        }
    }
}