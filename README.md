# 注意事項 2017.07.21

このリポは**非推奨**です。  
よりオフィシャルに近いところで開発が進んでいるのでそちらをお使い下さい。  

[LeaderboardForUnityWebGL](https://github.com/NCMBMania/LeaderboardForUnityWebGL)

WebGLに正式に対応するというアナウンスもあったので、楽しみですね！

# Yoshinani
Unity（C#）で NCMB REST API を叩くやつです。（WebGL向けに）

## できること

### `NCMBRanking`スクリプト

* スコアユーザのニックネーム変更（一度もスコア送信していない場合はエラーになる）  
* スコア送信  
* トップランカー取得  
* ユーザ自身の順位取得  

### `Capture`スクリプト

* スクリーンショットを撮ってNCMBファイルストアへアップロード  
* スクリーンショットを撮って新しいWindowで開く

NCMBのファイルストアへアップロードしても、公開されるURLがわからないので使いみちはないかも...  
（無料枠だと容量も潤沢にあるわけではないですし...）  
新しいWindowで開くのはありとは思います。Webだと画像保存してツイートはそこまで手間じゃないとおもうので

### Prefab

梱包されている **RankingBoard** というプレファブを使えば、  
`NDMBRanking`を中でよしなにあれこれしているので、  
ノンコーディングでランキング表示できます  
（※ **uGUI** で構築されています）  
スコアの送信自体は適宜行って下さい..  

### サンプルあれこれ

なんとなくイメージをつかればとおもいサンプルもちらほらあります。  
これは **Yoshinani.unitypackage.zip** には入っていないので  
みてみたいという場合は **Source Code** の方をダウンロードしてください。

## セットアップ

1. まずはNCMBアカウントつくってください！（すでにあるならSkip！）  
2. そして新規にアプリを作ってください（すでにあるならSkip！）  
3. つくったアプリのデータストアで **Scores** っていうクラスを作っておいてください（さしあたっては作るだけでいいです）  
4. ここまででNCMBでの作業はおわり。つぎに、[ここ](https://github.com/anzfactory/Yoshinani/releases/latest)から **Yoshinani.unitypackage.zip** をダウンロードします  
5. 展開してプロジェクトにImportします  
6. 適当にGameObjectを作って、`NCMBRanking`をアタッチ  
7. Inspectorから **ApplicationKey** と **ClientKey** の両方を設定（これらはNCMBのアプリ設定に記載されています！）  
8. 以上です！あとは、アタッチした`NCMBRanking`をつかってあれこれできます  


## 使い方

以下に簡単なサンプルコードを。

```csharp
// こういう感じにアタッチさせておいて
[SerializeField] private NCMBRanking ncmbRanking;

// スコア送信
// 第2引数のやつは更新を強制するかどうか
// false: ハイスコア更新時のみにスコアを送信する
// true : ハイスコア更新していなくてもスコアを送信する
this.ncmbRanking.SendScore(120f, false, (isError) => {
    if (!isError) {
        Debug.Log("スコア送信したよ！");
    } else {
        Debug.LogError("何らかの理由でスコア送信失敗！");
    }
});
// スコア送信その２（スコアとニックネームを同時に送るタイプ）
// 第3引数のやつは更新を強制するかどうか
// false: ハイスコア更新時のみにスコアを送信する
// true : ハイスコア更新していなくてもスコアを送信する
this.ncmbRanking.SendScore(120f, "nickname", false, (isError) => {
    // ...something...
});

// トップ50取得
this.ncmbRanking.Top50((scoreList) => {
    foreach (var score in scoreList) {
        Debug.Log(string.Format("{0}: {1}", score.nickname, score.score.ToString()));
    }
});
```

### RankingBoard

RankingBoardというプレファブの使い方

1. Canvasを設置（すでにあるならスキップ）  
2. `Yoshinani>Refabs>RankingBoard`をCanvas内にドラッグ＆ドロップ  
3. 大きさ等を適宜調整  
4. Inspector上でNCMBRankingコンポーネントのApplicationKey/ClientKeyを設定
5. 後はよしななタイミングで`RankingBoard.Show()`を呼び出すだけ！

デザインなんかは各々調整してくださいまし...

## 補足

### サンプルゲーム

ミニゲームをサンプルとしていれてみました。  
`RankingBoard`とか`NCMBRanking`とかの使うイメージをなんとなくつかめればなーと  
もちろん、これはサンプルであって使い方はどうぞご自由に！  
**Yoshinani.unitypackage.zip** にはサンプルは入っていないので  
見たい場合は **Source Code** を落として下さい

### REST APIを自分でたたきたい

`NCMBRanking`で提供している機能以外を使用する場合は`Xyz.Anzfactory.NCMBUtil.Yoshinani`を直接利用することで、  
NCMB REST APIを利用することができます  
例えば...  
Stagesっていうクラスを作ってデータを登録してあって、それを全取得したい場合は

```csharp
// 必ず先んじてセットアップ
Yoshinani.Instance.Setup("YourApplicationKey", "YourClientKey");

// Stagesからデータ取得
Yoshinani.Instance.Call(Yoshinani.RequestType.Get, "classes/Stages", null, (isError, json) => {
    // jsonが結果なのであとは好きにデシリアライズしてください
});
```

こういう感じになります。  
条件などの指定は[NCMB RESTドキュメント](http://mb.cloud.nifty.com/doc/current/rest/common/query.html)当たりを参照してください。  
（`NCMBRanking`の`SelfRank()`あたりも参考になるかも）



## CREDIT

[Nifty Cloud Mobile Backend](http://mb.cloud.nifty.com/)  
[darktable/MiniJSON.cs](https://gist.github.com/darktable/1411710)

## LICENSE

MIT
