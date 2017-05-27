# Yoshinani
Unity（C#）で NCMB REST API を叩くやつです。（WebGL向けに）

## できること

* ユーザ自動認証  
* ユーザのニックネーム変更  
* スコア送信  
* トップランカー取得  
* ユーザ自身の順位取得  

### Prefab

梱包されている**RankingBoard**というプレファブを使えば、  
ノンコーディングでランキング表示できます  
（※**uGUI**で構築されています）

## セットアップ

1. まずはNCMBアカウントつくってください！（すでにあるならSkip！）  
2. そして新規にアプリを作ってください（すでにあるならSkip！）  
3. つくったアプリのデータストアで**Scores**っていうクラスを作っておいてください（さしあたっては作るだけでいいです）  
4. ここまででNCMBでの作業はおわり。つぎに、[ここ](https://github.com/anzfactory/Yoshinani/releases/latest)から**Yoshinani.unitypackage.zip**をダウンロードします  
5. 展開してプロジェクトにImportします  
6. 適当にGameObjectを作って、`NCMBRanking`をアタッチ  
7. Inspectorから**ApplicationKey**と**ClientKey**の両方を設定（これらはNCMBのアプリ設定に記載されています！）  
8. 以上です！あとは、アタッチした`NCMBRanking`をつかってあれこれできます  


## 使い方

以下に簡単なサンプルコードを。

    // こういう感じにアタッチさせておいて
    [SerializeField] private NCMBRanking ncmbRanking;

    // ユーザ登録（これは何度呼んでも大丈夫。最初の場合は登録で以降はログインという処理になっている）
    this.ncmbRanking.RegisterUser((bool isError, NCMBRanking.User registerdUser) => {
        if (!isError) {
            Debug.Log(string.Format("ようこそ {0}", registerdUser.nickname));
        } else {
            Debug.LogError("何らかの理由でユーザ認証失敗！");
        }
    });

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

    // トップ50取得
    this.ncmbRanking.Top50((scoreList) => {
        foreach (var score in scoreList) {
            Debug.Log(string.Format("{0}: {1}", score.nickname, score.score.ToString()));
        }
    });

### RankingBoard

RankingBoardというプレファブの使い方

1. Canvasを設置（すでにあるならスキップ）  
2. `Yoshinani>Refabs>RankingBoard`をCanvas内にドラッグ＆ドロップ  
3. 大きさ等を適宜調整  
4. Inspector上でNCMBRankingコンポーネントのApplicationKey/ClientKeyを設定
5. 後はよしななタイミングで`RankingBoard.Show()`を呼び出すだけ！

デザインなんかは各々調整してくださいまし...

## CREDIT

[darktable/MiniJSON.cs](https://gist.github.com/darktable/1411710)

## LICENSE

MIT
