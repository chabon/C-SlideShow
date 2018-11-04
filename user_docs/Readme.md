# C-SlideShow version 4.0


## 概要

画像ビューアー兼スライドショーアプリです。以下のような特徴があります。

* 行列数やアスペクト比を設定し、同時に表示する画像の数や大きさなどを調節できます。
* 調節した設定は、プロファイルとして保存することができます。
* 画像の遷移やスライドショーは、スライドアニメーションでスムーズに行います。
* なるべく省スペースにして、デスクトップの邪魔にならないようなUIをコンセプトにしています。
* UIを簡素にしている代わりに、キーボード入力やマウスジェスチャ等の豊富なショートカット機能が用意されています。


## 使い道

* 電子化した書籍、漫画などのビューアーとして
* 例えばバナー画像など、特殊な形状の画像を一括で大きめサイズで見るためのビューアーとして
* スライドショー機能でデモなどに

などなど。何かに役立ててもらえれば嬉しいです。


## 動作環境

* Windows7以上
* .Net Framework 4.5.2以上


## インストールとアンインストール

インストールに特別な作業はありません。ダウンロードしたzipファイルをお好きな場所に解凍し、C-SlideShow.exeを実行してください。

アンインストールするには、アプリケーションのフォルダごと削除するだけでOKです。レジストリは利用しておりません。


## 基本的な使い方

### 読み込み

起動後、画像ファイルもしくは画像ファイルの入ったフォルダを、ドラッグアンドドロップすることで読み込ませることが出来ます。  
画像ファイルの入り圧縮ファイル、PDFファイルも読み込み可能です。  

ファイル種別    | 対応している拡張子                   
--------------  | ------------------------
画像ファイル    | .jpg .png .bmp .gif .ico
圧縮ファイル    | .zip  .rar  .7z  .tar   
その他          | .pdf .lnk               

### 基本操作

設定から変更可能です。

操作                                    | 処理内容                
--------------------------------------- | ---------------------------------
マウスホイール                          | 進む/戻る               
Shift + マウスホイール                  | 画像一枚分だけ進む/戻る        
右クリック                              | 画像をウインドウ全体に拡大して表示/戻す
マウス右ボタン押下 + マウスホイール　   | ウインドウを拡大縮小          
左クリック長押し                        | コンテキストメニューを表示       
スペースキー                            | スライドショー再生開始/停止      


## Tips

* Shiftキーを押しながらフォルダやファイルをドロップすると、追加として読み込ませることが出来ます。



## 更新履歴

#### 2018/11/04 ver 4.0

* 根本部分を作り直し、マルチスレッドでの読み込みを効率化
* 画像拡大中に、次の画像、前の画像に移動できるように
* PDFファイルの読み込みに対応
* 見開き画像の検出機能(書籍画像用)
* ショートカット用コマンドをいろいろ追加

#### 2018/09/15 ver 3.0

* 履歴機能
* ショートカット機能
* プロファイル機能
* 画像拡大機能
* アスペクト比を非固定にする設定の追加
* 画像の配置方法に関する設定の追加

#### 2018/02/27 ver 2.0

* 非同期読み込みにより、スライド時のカクつきが減った
* ドラッグアンドドロップでの読み込み
* 画像ファイル入りzipファイルに対応
* スライドに関する設定を色々追加
* シークバーの追加
* フルスクリーン機能
* アスペクト比設定

#### 2018/02/10 ver 1.0




## License

このアプリケーションはMITライセンスのもとで公開されています。  
<https://github.com/chabon/C-SlideShow/blob/master/LICENSE.txt>{target="new"}


## Libraries License

#### SharpCompress.dll

* Copyright (c) 2014  Adam Hathcock
* URL: <https://github.com/adamhathcock/sharpcompress>{target="new"}
* Licensed under [MIT License](https://github.com/adamhathcock/sharpcompress/blob/master/LICENSE.txt){target="new"}


#### WpfAnimatedGif.dll

* URL: <https://github.com/XamlAnimatedGif/WpfAnimatedGif>{target="new"}
* Licensed under [Apache License 2.0](https://github.com/XamlAnimatedGif/WpfAnimatedGif/blob/master/LICENSE.txt){target="new"}


#### PdfiumViewer.dll

* Copyright (c) Pieter van Ginkel
* URL: <https://github.com/pvginkel/PdfiumViewer>{target="new"}
* Licensed under [Apache License 2.0](https://github.com/pvginkel/PdfiumViewer/blob/master/LICENSE){target="new"}


#### PDFium.dll

* Copyright 2014 PDFium Authors. All rights reserved
* URL: <https://pdfium.googlesource.com/pdfium/>{target="new"}
* Licensed under [BSD 3-clause](https://pdfium.googlesource.com/pdfium/+/master/LICENSE){target="new"}



## リポジトリ

<https://github.com/chabon/C-SlideShow>{target="new"}


## 連絡先

* Mail:  chaboneko@gmail.com                                
* 配布ブログ: [ツール置き場](http://chaboneko.daiwa-hotcom.com/wordpress/?p=691){target="new"}


