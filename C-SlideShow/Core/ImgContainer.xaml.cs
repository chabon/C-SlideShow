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

using System.Threading;
using System.Diagnostics;

namespace C_SlideShow.Core
{
    /// <summary>
    /// TileContainer.xaml の相互作用ロジック
    /// </summary>
    public partial class ImgContainer : UserControl
    {
        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public int DefaultIndex { get; private set; }   // コンテナの並び順(デフォルト)
        public int CurrentIndex { get; set; }           // コンテナの並び順(現在の)
        public List<ImageFileContext> ImageFileContextMapList { get; set; } // 画像ファイル付随情報へのマップ(順序はGridと同期) 
        public ImgContainerAnimation Animation { get; set; } 
        public bool     IsActiveSliding { get { return Animation.IsActiveSliding; } }
        public CancellationTokenSource Cts { get; set; }    // Taskのキャンセルを管理


        public Size         BitmapDecodePixelOfTile { get; set; } = new Size(640, 480); // 1タイル毎のBitmapの上限
        static public int   StandardInnerTileWidth = 1000; // タイル幅の基準(インナーサイズ)

        public int        InnerTileWidth  { get; private set; } // マージンを含まない(アス比に対応)
        public int        InnerTileHeight { get; private set; }

        public Profile  TempProfile { get { return MainWindow.Current.Setting.TempProfile; } }

        public int NumofGrid
        {
            get { return MainGrid.ColumnDefinitions.Count * MainGrid.RowDefinitions.Count; }
        }

        public int NumofRow
        {
            get { return MainGrid.RowDefinitions.Count; }
        }

        public int NumofCol
        {
            get { return MainGrid.ColumnDefinitions.Count; }
        }

        public int NumofImage
        {
            get { return ImageFileContextMapList.Count(c => !c.IsDummy); }
        }

        public Point Pos
        {
            get { return new Point(Margin.Left, Margin.Top); }
        }

        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
        public ImgContainer(int defaultIndex)
        {
            InitializeComponent();
            DefaultIndex = defaultIndex;
            CurrentIndex = defaultIndex;
            ImageFileContextMapList = new List<ImageFileContext>();
            Animation = new ImgContainerAnimation(this);
        }

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        public void InitIndex()
        {
            CurrentIndex = DefaultIndex;
        }

        public void InitSize(int aspectRatioH, int aspectRatioV, int tilePadding, int numofCol, int numofRow)
        {
            // タイルのインナーサイズ(Paddingを抜いたサイズ)
            if( aspectRatioH > StandardInnerTileWidth ) aspectRatioH = StandardInnerTileWidth;
            int mod = StandardInnerTileWidth % aspectRatioH;
            InnerTileWidth = StandardInnerTileWidth - mod; // InnerTileWidthがaspectRatioHの整数倍になるようにする
            int p = InnerTileWidth / aspectRatioH;
            InnerTileHeight = aspectRatioV * p;

            // コンテナサイズ
            Width = InnerTileWidth * numofCol;
            Width += tilePadding * numofCol * 2;
            MainGrid.Width = this.Width;

            Height = InnerTileHeight * numofRow;
            Height += tilePadding * numofRow * 2;
            MainGrid.Height = this.Height;
        }

        public void InitPos(SlideDirection slideDirection)
        {
            double left, top;
            switch( slideDirection )
            {
                default:
                case SlideDirection.Left:
                    left = CurrentIndex * Width;
                    top  = 0;
                    break;
                case SlideDirection.Top:
                    left = 0;
                    top  = CurrentIndex * Height;
                    break;
                case SlideDirection.Right:
                    left = - (CurrentIndex * Width);
                    top  = 0;
                    break;
                case SlideDirection.Bottom:
                    left = 0;
                    top  = - (CurrentIndex * Height);
                    break;
            }
            Margin = new Thickness(left, top, Margin.Right, Margin.Bottom);
        }

        public void InitGrid(int numofCol, int numofRow)
        {
            MainGrid.Children.Clear();
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();

            for(int i=0; i< numofCol; i++)
            {
                ColumnDefinition c = new ColumnDefinition();
                MainGrid.ColumnDefinitions.Add(c);
            }

            for(int i=0; i< numofRow; i++)
            {
                RowDefinition r = new RowDefinition();
                MainGrid.RowDefinitions.Add(r);
            }
        }

        public void InitBitmapDecodePixelOfTile(int aspectRatioH, int aspectRatioV)
        {
            // タイルのアス比
            double tileAspectRatio = (double)aspectRatioV / aspectRatioH;

            int w, h;
            int pixelSize = TempProfile.BitmapDecodeTotalPixel.Value;

            if( NumofRow > NumofCol ) // 行が多い(横3x縦4等)
            {
                h = pixelSize / NumofRow;
                w = (int)(h / tileAspectRatio);
            }
            else // 列が多い(書籍の横2x縦1等)
            {
                w = pixelSize / NumofCol;
                h = (int)(w * tileAspectRatio);
            }

            BitmapDecodePixelOfTile = new Size(w, h);
        }

        public void SetImageElementToGrid()
        {
            SlideDirection      slideDirection      = TempProfile.SlideDirection.Value;
            TileOrientation     orientaition        = TempProfile.TileOrientation.Value;
            TileOrigin          origin              = TempProfile.TileOrigin.Value;
            TileImageStretch    tileImageStretch    = TempProfile.TileImageStretch.Value;

            int numofRow = MainGrid.RowDefinitions.Count;
            int numofCol = MainGrid.ColumnDefinitions.Count;

            // スライド方向から自動で配置を決定する場合
            if( TempProfile.UseDefaultTileOrigin.Value )
            {
                switch( slideDirection )
                {
                    default:
                    case SlideDirection.Left:
                        orientaition    = TileOrientation.Vertical;
                        origin          = TileOrigin.TopLeft;
                        break;
                    case SlideDirection.Top:
                        orientaition    = TileOrientation.Horizontal;
                        origin          = TileOrigin.TopLeft;
                        break;
                    case SlideDirection.Right:
                        orientaition    = TileOrientation.Vertical;
                        origin          = TileOrigin.TopRight;
                        break;
                    case SlideDirection.Bottom:
                        orientaition    = TileOrientation.Horizontal;
                        origin          = TileOrigin.BottomRight;
                        break;
                }
            }

            Action<int, int> setToGrid = (i, j) =>
            {
                // Image
                Image image = new Image();

                if( tileImageStretch == TileImageStretch.UniformToFill )
                {
                    image.Stretch = Stretch.UniformToFill;
                    image.HorizontalAlignment = HorizontalAlignment.Center;
                    image.VerticalAlignment = VerticalAlignment.Center;
                }
                else if( tileImageStretch == TileImageStretch.Fill )
                {
                    image.Stretch = Stretch.Fill;
                }

                // Border
                Border border = new Border();
                int tilePadding = TempProfile.TilePadding.Value;
                if(tilePadding != 0) {
                    border.BorderThickness = new Thickness(tilePadding + 2);
                    border.Margin = new Thickness(-2); // これと↑の-2は隣のGridとの境目を完全に消すため
                }
                border.BorderBrush = new SolidColorBrush(TempProfile.GridLineColor.Value);
                border.Child = image;

                // セット
                MainGrid.Children.Add(border);
                Grid.SetRow(border, i);
                Grid.SetColumn(border, j);
            };

            if(orientaition == TileOrientation.Horizontal )
            {
                switch( origin )
                {
                default:
                case TileOrigin.TopLeft:
                    for(int i=0; i < numofRow; i++) for(int j=0; j< numofCol; j++ ) { setToGrid(i, j); }
                    break;
                case TileOrigin.TopRight:
                    for (int i = 0; i < numofRow; i++) for (int j = numofCol -1; j >= 0; j-- ) { setToGrid(i, j); }
                    break;
                case TileOrigin.BottomRight:
                    for (int i = numofRow -1; i >=0; i--) for (int j = numofCol -1; j >=0; j-- ) { setToGrid(i, j); }
                    break;
                case TileOrigin.BottomLeft:
                    for (int i = numofRow -1; i >=0; i--) for (int j = 0; j < numofCol; j++ ) { setToGrid(i, j); }
                    break;
                }
            }
            else
            {
                switch( origin )
                {
                default:
                case TileOrigin.TopLeft:
                    for (int i = 0; i < numofCol; i++) for (int j = 0; j < numofRow; j++ ) { setToGrid(j, i); }
                    break;
                case TileOrigin.TopRight:
                    for (int i = numofCol -1; i >=0; i--) for (int j = 0; j < numofRow; j++ ) { setToGrid(j, i); }
                    break;
                case TileOrigin.BottomRight:
                    for (int i = numofCol -1; i >=0; i--) for (int j = numofRow -1; j >=0; j-- ) { setToGrid(j, i); }
                    break;
                case TileOrigin.BottomLeft:
                    for (int i = 0; i < numofCol; i++) for (int j = numofRow -1; j >=0; j-- ) { setToGrid(j, i); }
                    break;
                }
            }

        }

        public async Task LoadImage(CancellationToken token)
        {
            for(int i=0; i < MainGrid.Children.Count; i++ )
            {
                if( token.IsCancellationRequested ) break; // キャンセルがリクエストされた

                var child = MainGrid.Children[i];
                Border  border  = child as Border;
                Image   image   = border.Child as Image;
                ImageFileContext mapedImageFileContext = ImageFileContextMapList[i];

                if( border == null || image == null || mapedImageFileContext == null ) continue;

                if( i < ImageFileContextMapList.Count)
                {
                    if(mapedImageFileContext.BitmapImage == null )
                    {
                        // 新たにファイルから読み込み
                        Debug.WriteLine("BitmapImage = null : " + mapedImageFileContext.FilePath);
                        var source = await ImageFileContextMapList[i].LoadBitmap(BitmapDecodePixelOfTile);
                        mapedImageFileContext.BitmapImage = (BitmapImage)source;

                        if( !token.IsCancellationRequested ) {
                            image.Source = source;
                        }
                        else {
                            Debug.WriteLine("BitmapImgae loaded and canceled by Task cancellation request");
                        }
                    }
                    else if(image.Source != mapedImageFileContext.BitmapImage)
                    {
                        // すでにあるBitmapImageを利用
                        System.Diagnostics.Debug.WriteLine("BitmapImage = exist! : " + mapedImageFileContext.FilePath);
                        image.Source = mapedImageFileContext.BitmapImage;
                    }
                }
            }
        }


        /// <summary>
        /// BitmapDecodePixelOfTileにサイズが足りていないBitmapImageを開放(サイズオーバーは許可する)
        /// </summary>
        public void ReleaseIncorrectSizeBitmap()
        {
            foreach( ImageFileContext ifc in ImageFileContextMapList )
            {
                if( ifc.IsDummy ) continue;
                BitmapImage bmp = ifc.BitmapImage;
                if( bmp == null ) continue;

                if( ifc.Info.PixelSize.Height > ifc.Info.PixelSize.Width ) // 画像が縦長のとき
                {
                    if( bmp.PixelHeight < ifc.Info.PixelSize.Height && bmp.PixelHeight < BitmapDecodePixelOfTile.Height ) ifc.BitmapImage = null;
                }
                else // 画像が横長のとき
                {
                    if( bmp.PixelWidth < ifc.Info.PixelSize.Width && bmp.PixelWidth < BitmapDecodePixelOfTile.Width ) ifc.BitmapImage = null;
                }
            }
        }


        /// <summary>
        /// 見開き表示用に全グリッドを結合して1つのグリッドにする
        /// </summary>
        public void CombineAllGrid()
        {
            InitGrid(1, 1);
            SetImageElementToGrid();
            BitmapDecodePixelOfTile = new Size(TempProfile.BitmapDecodeTotalPixel.Value, TempProfile.BitmapDecodeTotalPixel.Value);
        }


        public void ApplyGridLineColor()
        {
            foreach(var child in MainGrid.Children )
            {
                Border  border  = child as Border;
                if(border != null) border.BorderBrush = new SolidColorBrush(TempProfile.GridLineColor.Value);
            }
        }

    }
}
