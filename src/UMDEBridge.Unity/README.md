<h1 align="center">UMDEBridge</h1>

MasterMemory を利用して作るマスターデータを Unity と Excel で MySql を介してデータの編集を行えるようにします。

MessagePack とポリモーフィズムで作ったマスターデータのクラス、UnityEditor 上では便利に作業出来るものの、id の振り方など Excel のマクロ等で編集したい部分もあります。  
「UntiyEditor と Excel、両方ともいい感じに使ってマスターデータの設定を行いたい、規模的には個人で作れる範囲で」の気持ちを元に出来ています。

MemoryTableAttribute の付いたクラスのフィールドを MySQL のカラムとし、その変数の内容を json で挿入します。  
つまり int 等のプリミティブな型はそのままの値が入り、MessagePackObject なクラス型のフィールドは json が入ります。

CEDEC2019 の FF14 の NEX の発想を参考にしました。  
しかし、同期したり差分を取ったり串刺し Lookup したり出来るものではなく、あくまで上記の事がそこそこ楽に出来るようになればいいなぁ程度の目的ですので悪しからず。

<!-- [簡単な紹介動画はこちら](https://youtu.be/2JuSoOIIYX0) -->

## 必要な知識

- MessagePack-CSharp と MasterMemory の最低限な使い方
- Unity の Editor 拡張について
- MySql を個人で運用するための知識（MAMP 等でも可。localhost で DB サーバー立ち上げて接続していじれれば OK）

## Setup

このプロジェクトを動かすためには以下のライブラリが必要です。

== Unity ==

Unity2022.3.12f1 以降。

1. [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp) (v2)をインストールする
2. [MasterMemory](https://github.com/Cysharp/MasterMemory) (v2)をインストールする
3. Window > Package Manager から Package Manager を開く
4. 「+」ボタン > Add package from git URL
5. 以下を入力してインストール
   - https://github.com/sakabadev/UMDEBridge.git?path=/Assets/UMDEBridge
6. Asset/Packages/UMDEBridge/unitypackages/UMDEBridge.dll.unitypackage をインストールする
7. 必要に応じて Asset/Packages/UMDEBridge/unitypackages/UMDEBridge.Demo.unitypackage をインストールする

下記は DLL 関連で不整合が出た場合の対処法です。

Unity の Plugins に入れる DLL がわからない場合。  
VisualStudio 等で空のクラスライブラリ等のプロジェクトを作成し、参照に NuGet でダウンロードします。  
そうするとプロジェクトの packages フォルダに色々入ってきているので、各ライブラリの lib の中の良さげなバージョンの DLL を持ってきましょう。

UnityEditor のバージョンと依存 DLL のインポートタイミングによっては、プロジェクト内にある DLL と依存したい DLL のバージョンが違うエラーが出る事があります。  
エラーに従って正しいバージョンの DLL をインポートしてもいいですが、Project Settings > Player > Assembly Version Validation のチェックを外して難を逃れる手段もあります。

== Excel Addin ==

VSTO アドインで作っております。よって Windows 限定。

1. ExcelAddin\MD2DBFromExcel\bin\Release 　を丸ごとダウンロードする。
2. .vsto ファイルをダブルクリックでインストールする。

## 使い方

以下、/は Git リポジトリのルートを指します。

簡単な使用の流れですが、

### [Unity]

この作業での成果物は、

- MySQL へ class_name テーブル、class_name_config テーブルを作成
- /Excels/に xlsx ファイルを作成
- /Excels/に excel から MySQL への接続を行うための情報が書かれた設定 json ファイルを作成

① Project ウィンドウで右クリックし、Create > UMDEBridge > Create Settings で UMDEBridgeSettingsAsset ファイルを作る。  
　どこでもいいので Editor > Resourses > UMDEBridgeSettingsAsset のフォルダ構成で配置する。  
　 UMDEBridgeSettingsAsset に MySQL への接続情報を書く。

② MemoryTable 属性のある Entity を作り、コードジェネレートする。既存の Entity の属性を参考に属性を書くことと、ついでに値編集用の EditorWindow も作る。（ここから下は既に作ってある Item クラスを元に説明します。）

③ UnityEditor 上部の UMDEBridge/Open ExcelFileCreator を開き、"Load MemoryTable list"を押す。

④ プルダウンでテーブルを作りたいクラスを選び、"create xlsx"と"create table to MySql"を押す。

⑤ /Excels/に xlsx （と存在しなければ設定用 json ファイル）が生成され、DB に対象のクラス用テーブル（class_name テーブル、class_name_config テーブル）が作れた事を確認する。

### [Excel]

この作業での成果物は、

- xlsx ファイルのシートに class_name テーブルに入る形式のテーブルが作成される。
- シートのテーブルに入力したデータが MySQL のテーブルに入る。
- Excel から MySQL に保存・読み込みが出来るようになる。

⑥ /ExcelAddin/MD2DBFromExcel/MD2DBFromExcel.sln をビルドし、Excel の VSTO アドインをインストールする。  
　 bin > Debug か Release のビルドしたターゲットのフォルダ > .vsto ファイル　がインストーラー。  
　アンインストールは「プログラムと機能」から行える。

⑦ xlsx を開き、値を編集したいシートを選択してリボンの UMDEBridge > "選択中のシートを読み込む"をクリックする。

⑧ 少し待つと上部には列の設定データが入力され、その下にテーブルが作られるので、id を定義していく。ついでに入力したいフィールドがあれば入力する。

⑨ リボンから”選択中のシートを保存”をクリックする。成功すると、MySql にテーブルへ入力した情報が保存される。

### [Unity]

この作業での成果物は、

- UnityEditor 上で、Excel で入力したデータが確認できる
- MD.bytes として、MasterMemory の MemoryDatabase が永続化される。
- Unity から MySQL に保存・読み込みが出来るようになる。

⑩ UnityEditor に戻り、UMDEBridge/Master Editor/Item をクリック、マスターデータ Editor を開く。

⑪ 左上の Import を押すと、MySql のデータが MemoryDatabase に保存される。具体的に言うと Assets/Demo/Resources/MD.bytes に保存される。

⑫ いい感じに編集し、EditorWindow 左下の Save をクリックすると、Assets/Demo/Resources/MD.bytes に保存される。

⑬ Assets/Demo/Resources/MD.bytes に保存した状態で Export ボタンを押すと、MySql に保存される。

大まかな流れは以上です。

## class_name_config テーブルとは

コンフィグの値を見て、Excel シートに色々と行っています。

- prefer_excel の値を 1 にすると、Excel へ Import した時に対象の列は上書きしません。null や 0 なら書き込みます。  
  これにより、Excel の表の中でマクロを使えるようにしています。
- column_width は Import 時にセルの幅をいじります。
- column_name は書き込むフィールドカラムを指定するためのものです。
- sort_label は表の見出しになります。

## 既に DB に追加した MemoryTable 属性のクラスを変更する時

マイグレーションの仕組みはありません。Entity の構造、特にプロパティの数を変更したら新しく create table to MySql をして DropTable からの新規テーブル作成を行います。手動で DB をいじってももちろん構いません。

大きく構造を変化させるときは、Excel にデータを残しておくとそこからデータの復旧出来る可能性が高いです。若干シート構造が変わってしまっても、シート名をテーブル名に、key の行をカラム名に一致させると、DB に保存する事が可能です。

もしくは SQL としてバックアップを出力しておいて、新しい Entity に合わせて書き換えた物をリストアする方が簡単かもしれません。

## 注意点

- Book 名に制限はありません。MySql のテーブルの検索はシート名で行っているためです。Namespace 違いの同名のクラスは作らないようにしてください。
- MessagePack でシリアライズ出来る型のみ対応。
- フィールド名に MySql の予約語を使わないようご注意下さい。（group, trigger, release など）
- Excel でシート上部の列オプションをいじっても反映されません。  
  リボンでシートデータを読み込む際に MySql に保存されている値を見て処理が走ります。
- Excel ファイルの 1 列目の"key","value"という値は、Addin で読み取っているのでずらしたり消したりすると正常に処理ができなくなります。
- クラスのプロパティの命名規則は SampleTextField（パスカルケース） です。クエリ作成に影響するため現在はこれで統一です。
- MySql から Unity へのインポートに失敗する場合、特にシリアライズで失敗する時は MySql のカラムの型とプロパティの型が合っておらず、例えば int 型のフィールドに string の値の json を読み込もうとして失敗する事があります。 {"hp":100} としなければいけない所を {"hp":"100"} になってる等。（MySql で TEXT 型で保存されてしまっている等の理由により。）  
  その場合、一度 Excel に MySql に保存されているデータを逃がした後、UMDEBridgeSettingsAsset の ColumnTypeMapping が意図通りに設定されているか確認し、create table して型が正しくセットされた事を確認し、Excel に逃がしたデータを保存してください。

## License

THE BEERWARE LICENSE.
