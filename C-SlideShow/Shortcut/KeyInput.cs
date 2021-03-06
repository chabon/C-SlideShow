﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Windows.Input;


namespace C_SlideShow.Shortcut
{
    /// <summary>
    /// キー入力情報
    /// </summary>
    [DataContract(Name = "KeyInput")]
    public class KeyInput : IEquatable<KeyInput>
    {
        /// <summary>
        /// 修飾キー
        /// 
        /// </summary>
        [DataMember]
        public ModifierKeys Modifiers;

        /// <summary>
        /// キー
        /// </summary>
        [DataMember]
        public Key Key;

        public KeyInput(ModifierKeys modifiers, Key key)
        {
            this.Modifiers = modifiers;
            this.Key = key;
        }

        public KeyInput(Key key)
        {
            this.Modifiers = ModifierKeys.None;
            this.Key = key;
        }

        public KeyInput Clone()
        {
            return new KeyInput(this.Modifiers, this.Key);
        }

        public override string ToString()
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

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public bool Equals(KeyInput other)
        {
            if (  ( Modifiers.Equals(other.Modifiers) ) && ( Key.Equals(other.Key) )  )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((KeyInput)obj);
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public override int GetHashCode()
        {
            return ( (int)Modifiers | (int)Key);
        }
    }
}
