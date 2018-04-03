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

using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

using Forms = System.Windows.Forms;



namespace C_SlideShow
{
    public partial class MainWindow
    {


        private void InitEvent()
        {
            /* ---------------------------------------------------- */
            //     init events
            /* ---------------------------------------------------- */

            // ウインドウ全体でドラッグ可能に
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();
            

            MenuItem_Matrix.SubmenuOpened += (s, e) =>
            {
                matrixSelecter.SetMatrix(Setting.TempProfile.NumofRow, Setting.TempProfile.NumofCol);
            };

            MenuItem_SlideSetting.SubmenuOpened += (s, e) =>
            {
                slideSettingDialog.ApplySettingToDlg();
            };

            MenuItem_Setting.SubmenuOpened += (s, e) =>
            {
                settingDialog.ApplySettingToDlg();
            };


            this.SourceInitialized += (s, e) =>
            {
                // ウインドウ位置復元が正常に行われたかチェック
                Win32.SetWindowPosProperly(new WindowInteropHelper(this).Handle, 
                    (int)this.Left, (int)this.Top, true, 0, true);
            };

            this.SizeChanged += (s, e) =>
            {
                if (ignoreResizeEvent) return;
                FitMainContentToWindow();
            };

            this.Closing += (s, e) =>
            {
                Setting.TempProfile.LastPageIndex = bitmapPresenter.CurrentIndex;
                SaveWindowRect();
                Setting.saveToXmlFile();
            };

            this.PreviewDragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                    e.Effects = DragDropEffects.All;
                else
                    e.Effects = DragDropEffects.None;
                e.Handled = true;
            };

            this.Drop += (s, e) =>
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                this.ReadFiles(files);
                InitMainContent(0);
            };

            this.MouseWheel += (s, e) =>
            {
                // 右クリック押しながらで、拡大縮小
                if (!Setting.TempProfile.IsFullScreenMode)
                {
                    short stateR = Win32.GetAsyncKeyState(Win32.VK_RBUTTON);
                    if ( (stateR & 0x8000) != 0)
                    {
                        if(e.Delta > 0)
                        {
                            this.Width = Width * 1.1;
                            this.Height = Height * 1.1;
                            UpdateWindowSize();
                        }
                        else
                        {
                            this.Width = Width * 0.9;
                            this.Height = Height * 0.9;
                            UpdateWindowSize();
                        }
                        return;
                    }
                }

                bool isPlayback; // 巻き戻しかどうか
                if (e.Delta > 0) isPlayback = true;  // wheel up
                else isPlayback = false;

                bool slideByOneImage = false; // 画像１枚毎のスライド
                if (IsCtrlOrShiftKeyPressed) slideByOneImage = true;

                StartOperationSlide(isPlayback, slideByOneImage, 300);
            };

            this.Seekbar.ValueChanged += (s, e) =>
            {
                if (this.ignoreSliderValueChangeEvent) return;

                int value = (int)Seekbar.Value;
                //if (isHSeekbarDragStarted && BitmapPresenter.FileInfo.Count > 99)
                if (isSeekbarDragStarted)
                {
                    PageInfoText.Text = String.Format("{0} / {1}",
                        value, bitmapPresenter.NumofImageFile);
                    return;
                }
                Debug.WriteLine("seek bar value: " + Seekbar.Value);
                OnSeekbarValueChanged(value);
            };

            //this.Toolbar_Test.Click += (s, e) =>
            //{
            //    this.tileContainers.ForEach( tc => tc.StopSlideAnimation() );
            //};


            // キーイベント（暫定的に）
            this.PreviewKeyDown += (s, e) =>
            {
                // alt + enter でフルスクリーン切り替え
                //if (e.SystemKey == Key.LeftAlt && e.Key == Key.Enter)
                //{
                //    ToggleFullScreen();
                //    return;
                //}

                // フルスクリーン解除
                if(e.Key == Key.Escape && Setting.TempProfile.IsFullScreenMode)
                {
                    ToggleFullScreen();
                }
            };



            //this.PreviewMouseDoubleClick += (s, e) => ToggleFullScreen();

        }



        private void intervalSlideTimer_Tick(object sender, EventArgs e)
        {
            intervalSlideTimerCount += 1;

            if(Setting.TempProfile.SlidePlayMethod == SlidePlayMethod.Interval)
            {
                if(intervalSlideTimerCount >= Setting.TempProfile.SlideInterval)
                {
                    Profile pf = Setting.TempProfile;
                    StartIntervalSlide(pf.SlideByOneImage, pf.SlideTimeInIntevalMethod);

                    int slideTime = (int)( pf.SlideTimeInIntevalMethod / 1000 );
                    intervalSlideTimerCount = 0 - slideTime;
                }
            }
            else
            {
                intervalSlideTimer.Stop();
            }
        }


        private void Seekbar_DragStarted(object sender, RoutedEventArgs e)
        {
            isSeekbarDragStarted = true;
        }

        private void Seekbar_DragCompleted(object sender, RoutedEventArgs e)
        {
            isSeekbarDragStarted = false;
            OnSeekbarValueChanged((int)this.Seekbar.Value);
        }

        private void OnSeekbarValueChanged(int value)
        {
            int index = value - 1;
            ChangeCurrentImageIndex(index);
        }

        private void MatrixSelecter_MatrixSelected(object sender, EventArgs e)
        {
            MatrixSelecter ms = sender as MatrixSelecter;
            if(ms != null)
            {
                this.MenuItem_Matrix.IsSubmenuOpen = false;
                ChangeGridDifinition(ms.RowValue, ms.ColValue);
                this.Focus();
            }

        }


        // フォルダ読み込み
        private void Toolbar_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Forms.FolderBrowserDialog();
            dlg.Description = "画像フォルダーを選択してください。";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                bitmapPresenter.LoadFileInfoFromDir(dlg.SelectedPath);
                Setting.TempProfile.FolderPath = dlg.SelectedPath;

                string[] path = { dlg.SelectedPath };
                ReadFiles(path);
                InitMainContent(0);
            }
        }


        // グリッドのアスペクト比
        private void Toolbar_AspectRate_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if(item != null)
            {
                string[] str = item.Tag.ToString().Split('_');
                int w = int.Parse(str[0]);
                int h = int.Parse(str[1]);

                ChangeTileSize(w, h);
            }
        }


        // 再生
        private void Toolbar_Play_Click(object sender, RoutedEventArgs e)
        {
            // 再生中だったら停止
            if(tileContainers[0].IsContinuousSliding || intervalSlideTimer.IsEnabled)
            {
                StopSlideShow();
                return;
            }
            else
            {
                // 再生
                StartSlideShow();
            }
        }


        // 再読込
        private void Toolbar_Reload_Click(object sender, RoutedEventArgs e)
        {
            SaveWindowRect();
            this.Setting.TempProfile.LastPageIndex = 0;
            LoadProfile(this.Setting.TempProfile);
        }


        // システムボタン
        private void SystemButton_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SystemButton_Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleFullScreen();
        }

        private void SystemButton_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }




        protected override void OnSourceInitialized(EventArgs e)
        {
            HwndSource hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            hwndSource.AddHook(uiHelper.HwndSourceHook);
         
            base.OnSourceInitialized(e);
        }



    }
}
