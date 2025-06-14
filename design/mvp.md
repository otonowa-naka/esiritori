# お絵描き当てゲーム 設計書 (MVP)

## 🌟 概要

1人が「お題」を見て絵を描き、他のプレイヤーがそれを見て当てる、Webブラウザで動作するリアルタイム型お絵描きゲーム。

##　🎯 ゲームの概要
① 絵しりとり
ルームには複数人のプレイヤーが存在する
プレイヤーは、絵を書く出題者が一人、残りは絵を当てる回答者になる。
最初の絵を書く出題者は、しりとりのお題1文字を渡される。
出題者は絵を描く前に、題材のひらがなで入力する
出題者は、題材の絵を描く（例：「さる」→サルの絵）

書かれた絵は、リアルタイムで回答者の画面に表示される。
回答者は、絵の題材がわかったら、答えを入力する
回答者が入力した題材が合っていれば、残り時間がポイントとして、出題者と回答者に入る。
正解がでた時点で、このターンは終了。次の人が出題者になる。
正解がでない場合は、ペナルティとして絵を描いた時間の半分を出題者のポイントから引く
最終的に何ラウンドかして、ポイントが多い人が勝ち
順番に出題者になり、全てのメンバーが出題者になったところで、一回りとするこれを1巡と言う


## 🔹 動作の流れ
1．管理者がルームを作成する
ルールを設定する
ルールの内容
    - 制限時間 ：絵を描く制限時間　秒単位で設定する。無制限も選択可能
    - ラウンド数　：　しりとりをラウンド巡したら1ゲーム終了するかの設定
ルームのURL、又はルーム番号をメンバーに共有して、参加者を募る。
参加者は、URL又はルーム番号を入力して、プレイヤーはニックネームを入力し、ルームに参加
2. 1人が出題者として選ばれる
3. 出題者は「お題」を入力
4. 出題者はキャンバスで絵を描く
5. 他のプレイヤーは絵を見ながら回答を入力で当てる
6. 正解者が出る or 制限時間終了
7. スコア更新 → 次ターンへ
8. 設定されたラウンドが達成した場合は、結果発表

## 📊 スタック構成

| 項目     | 技術                                       |
| ------ | ---------------------------------------- |
| フロント   | Next.js (App Router) + Tailwind CSS      |
| ホスティング | AWS Amplify Hosting (または S3+CloudFront)  |
| リアルタイム | API Gateway (WebSocket API) + AWS Lambda |
| ストレージ  | DynamoDB (Room/プレイヤー/チャット状態)             |
| 認証     | ニックネームのみ (認証サービス不要)                      |
| デプロイ   | AWSの無償プラン内                               |

## 🔹 DynamoDB スキーマ構成 (MVP)

### テーブル: Rooms

```ts
PK: "ROOM#<roomId>"
SK: "META"
{
  roomId: string
  players: string[] // playerId list
  round: number
  drawerId: string
  answer: string
  startedAt: timestamp
}
```

### テーブル: Players

```ts
PK: "ROOM#<roomId>"
SK: "PLAYER#<playerId>"
{
  playerId: string
  name: string
  score: number
}
```

### テーブル: Chats

```ts
PK: "ROOM#<roomId>"
SK: "CHAT#<timestamp>"
{
  playerId: string
  name: string
  message: string
  isCorrect: boolean
  timestamp: number
}
```

## 🚀 AWS CDK (サービス構成)

* API Gateway (WebSocket API)
* Lambda (connect, disconnect, message)
* DynamoDB (3 テーブルとGSIは1本で合併可)
* Amplify Hosting or S3+CloudFront

→ CDKによる実装は別ファイルで提供

## ⌛ 制限

* 1ルームのみ (MVP)
* 画像はbase64でWebSocket上で送信、保存はしない
* 認証なしで自由参加

## ▶ 次ステップ

* ソケット通信のロジック実装
* 描画キャンバスのUI
* 答えの判定ロジック
* スコア加算 & ランド換り

```

}

```
