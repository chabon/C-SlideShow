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

		// マウスジェスチャーMap (ジェスチャー -> コマンドIDを取得)
        //[DataMember]
        //public Dictionary<string, Commands> GestureMap = new Dictionary<string, Commands>();
	
        // タッチ入力Map (タッチ入力 - コマンドID)


        public ShortcutSetting()
        {
            initKeyMap();
        }

        public void initKeyMap()
        {
            KeyMap.Clear();

            // 全般
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.Control,                      Key.F),        CommandID.OpenFolder                 )  );
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.Control | ModifierKeys.Shift, Key.F),        CommandID.OpenAdditionalFolder       )  );

            // 通常時
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.F),        CommandID.SlideToForward             )  );
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.B),        CommandID.SlideToBackward            )  );
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.Shift,                        Key.F),        CommandID.SlideToForwardByOneImage   )  );
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.Shift,                        Key.B),        CommandID.SlideToBackwardByOneImage  )  );
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Left),     CommandID.SlideToLeft                )  );
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Up),       CommandID.SlideToTop                 )  );
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Right),    CommandID.SlideToRight               )  );
            KeyMap.Add(  new KeyMap( new KeyInput(ModifierKeys.None,                         Key.Down),     CommandID.SlideToBottom              )  );

            // 画像拡大時
        }

    }
}
