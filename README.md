# Cinemachine Room2D Extension

## 概要

矩形領域内に `Perspective` カメラの移動を制限する為の 2.5D ゲーム用<sup>*</sup> `Cinemachine Extension`  

`画面端まで移動したらスクロールが停止する範囲を Rect で指定する` 為の拡張。

<sup>*</sup> 2D (Orthographic) にも対応しているが、2D だけであれば標準の `CinemachineConfiner` で移動制限可能。そちらは Collider を使っているのでより複雑な領域設定が出来る。(多分)  

![image01](docs/images/image01.png)

## Demo (WebGL)

https://tukanpo.github.io/CinemachineRoom2DExtension/

上にある画像の `緑の枠` が設定済の制限領域。（`Room` と呼ぶ）  
`WASD 又はカーソルキー`で移動したりはみ出したりして確認可能。  
`Space キー` で投影方式の切替え。 

## 環境

- Unity 2022.3.8f1
- Cinemachine 2.9.7

## Class

`Assets/App/Scripts/Cameras` ディレクトリ以下が必須ファイル

#### [Room2D](Assets/App/Scripts/Cameras/Room2D.cs)
矩形領域を表現する為のコンポーネント。  
内部で Rect を保持しているだけの単純なクラス

#### [Room2DEditor](Assets/App/Scripts/Cameras/Editor/Room2DEditor.cs)
SceneView 上での編集をサポートする為のエディタ拡張  

#### [CinemachineRoom2D](Assets/App/Scripts/Cameras/CinemachineRoom2D.cs)
Room2D 内カメラ移動制限 Cinemachine Extension

## 使用方法
- GameObject に Room2D コンポーネントをアタッチしてサイズ調整する。
- CinemachineVirtualCamera の設定を行う  
  - `Body` を `Framing Transposer` にする
  - `Follow` ターゲットを設定する (ランタイムでも可)
  - `Extension` に CinemachineRoom2D を追加し、Rooms に Room2D をアタッチした GameObject を指定する。  

## Demo シーンについて追記

Space キーでの投影方式切替時に `投影方式の見た目を(可能な限り)合わせる為のカメラ設定` が UnityEditor 上でログ出力される。  
（Perspective なら `OrthographicSize`、Orthographic なら `FOV` を出力）  

あくまでカメラから見た z = 0 の位置にある同じ平面上の見た目が揃うだけ。  
Demo では灰色のキューブの前面が揃うよう位置を合わせてある。

詳細は [DemoScene](Assets/App/Scripts/Scenes/DemoScene.cs) コンポーネント内のコードを参照。  

## 参考
カメラからの距離で求める錐台のサイズ  
https://docs.unity3d.com/ja/2022.3/Manual/FrustumSizeAtDistance.html
