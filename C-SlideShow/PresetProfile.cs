using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace C_SlideShow
{
    static public class PresetProfile
    {
        static public Profile Default;               // デフォルト
        static public Profile BookBoundOnLeftSide;   // 書籍用(左綴じ)
        static public Profile BookBoundOnRightSide;  // 書籍用(右綴じ)

        static public List<Profile> Items = new List<Profile>();

        static PresetProfile()
        {
            // デフォルト
            Default = new Profile();
            Default.Name = "デフォルト";
            PropertyInfo[] infoArray = Default.GetType().GetProperties();
            foreach( PropertyInfo info in infoArray )
            {
                ProfileMember.IProfileMember member = info.GetValue(Default, null) as ProfileMember.IProfileMember;
                if(member != null) member.IsEnabled = true;
            }
            Default.Path.IsEnabled = false;
            Default.WindowPos.IsEnabled = false;

            // 書籍用(左綴じ)
            BookBoundOnLeftSide = new Profile();
            BookBoundOnLeftSide.NumofMatrix.IsEnabled = true;
            BookBoundOnLeftSide.AspectRatio.IsEnabled = true;
            BookBoundOnLeftSide.NonFixAspectRatio.IsEnabled = true;
            BookBoundOnLeftSide.SlideDirection.IsEnabled = true;
            BookBoundOnLeftSide.UseDefaultTileOrigin.IsEnabled = true;

            BookBoundOnLeftSide.Name = "書籍用(左綴じ)";
            BookBoundOnLeftSide.NumofMatrix.Value = new int[] { 2, 1 };
            BookBoundOnLeftSide.AspectRatio.Value = new int[] { 2, 3 };
            BookBoundOnLeftSide.NonFixAspectRatio.Value = true;
            BookBoundOnLeftSide.SlideDirection.Value = SlideDirection.Left;
            BookBoundOnLeftSide.UseDefaultTileOrigin.Value = true;


            // 書籍用(右綴じ)
            BookBoundOnRightSide = new Profile();
            BookBoundOnRightSide.NumofMatrix.IsEnabled = true;
            BookBoundOnRightSide.AspectRatio.IsEnabled = true;
            BookBoundOnRightSide.NonFixAspectRatio.IsEnabled = true;
            BookBoundOnRightSide.SlideDirection.IsEnabled = true;
            BookBoundOnRightSide.UseDefaultTileOrigin.IsEnabled = true;

            BookBoundOnRightSide.Name = "書籍用(右綴じ)";
            BookBoundOnRightSide.NumofMatrix.Value = new int[] { 2, 1 };
            BookBoundOnRightSide.AspectRatio.Value = new int[] { 2, 3 };
            BookBoundOnRightSide.NonFixAspectRatio.Value = true;
            BookBoundOnRightSide.SlideDirection.Value = SlideDirection.Right;
            BookBoundOnRightSide.UseDefaultTileOrigin.Value = true;

            // リスト
            Items.Add(Default);
            Items.Add(BookBoundOnLeftSide);
            Items.Add(BookBoundOnRightSide);
        }
    }
}
