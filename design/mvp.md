# お絵描き当てゲーム 設計書 (MVP)

## 🌟 概要

1人が「お題」を見て絵を描き、他のプレイヤーがそれを見て当てる、Webブラウザで動作するリアルタイム型お絵描きゲーム。

## 🎯 ゲームの概要

### ゲームの流れ
1. ルームには複数人のプレイヤーが存在する
2. プレイヤーは、絵を書く出題者が一人、残りは絵を当てる回答者になる
3. 最初の絵を書く出題者は、しりとりのお題1文字を渡される
4. 出題者は絵を描く前に、題材のひらがなで入力する
5. 出題者は、題材の絵を描く（例：「さる」→サルの絵）

### 回答とポイント
- 書かれた絵は、リアルタイムで回答者の画面に表示される
- 回答者は、絵の題材がわかったら、答えを入力する
- 回答者が入力した題材が合っていれば、残り時間がポイントとして、出題者と回答者に入る
- 正解がでた時点で、このターンは終了。次の人が出題者になる
- 正解がでない場合は、ペナルティとして絵を描いた時間の半分を出題者のポイントから引く
- 最終的に何ラウンドかして、ポイントが多い人が勝ち

### ゲームの進行
- 順番に出題者になり、全てのメンバーが出題者になったところで、一回りとする
- これを1巡と言う

## 🔹 動作の流れ

1. ルーム作成
   - 管理者がルームを作成する
   - ルールを設定する
     - 制限時間：絵を描く制限時間（30秒～300秒）
     - ラウンド数：しりとりを何ラウンド巡したら1ゲーム終了するかの設定（1～10ラウンド）
   - ルームのURL、又はルーム番号をメンバーに共有して、参加者を募る
   - 参加者は、URL又はルーム番号を入力して、プレイヤーはニックネームを入力し、ルームに参加

2. ゲーム進行
   - 1人が出題者として選ばれる
   - 出題者は「お題」を入力
   - 出題者はキャンバスで絵を描く
   - 他のプレイヤーは絵を見ながら回答を入力で当てる
   - 正解者が出る or 制限時間終了
   - スコア更新 → 次ターンへ
   - 設定されたラウンドが達成した場合は、結果発表

## 📊 技術スタック

| 項目 | 技術 |
|------|------|
| フロント | Next.js (App Router) + Tailwind CSS |
| ホスティング | AWS Amplify Hosting (または S3+CloudFront) |
| リアルタイム | API Gateway (WebSocket API) + AWS Lambda |
| ストレージ | DynamoDB (Room/プレイヤー/チャット状態) |
| 認証 | ニックネームのみ (認証サービス不要) |
| デプロイ | AWSの無償プラン内 |

## 🚀 AWS インフラ構成

- API Gateway (WebSocket API)
- Lambda (connect, disconnect, message)
- DynamoDB (3テーブルとGSIは1本で合併可)
- Amplify Hosting or S3+CloudFront

→ CDKによる実装は別ファイルで提供

## ⌛ 制限事項

- 画像はbase64でWebSocket上で送信、保存はしない
- 認証なしで自由参加


## 🛰 WebSocketメッセージ設計

- WebSocketでは「どのモデルが更新されたか」だけを通知し、クライアントはREST APIで最新データを取得して画面を更新する方式とする。
- CQRS/イベント駆動アーキテクチャに適したシンプルな設計。

### メッセージ構造
```json
{
  "type": "model_updated",         // 固定値
  "modelType": "Game",             // 例: "Game", "ChatMessage", "Player"
  "modelId": "abc123",             // 例: ゲームID、チャットID、プレイヤーID
  "event": "GameStarted",          // 省略可: 何のイベントで更新されたか
  "updatedAt": "2024-05-01T12:34:56Z" // 省略可: サーバ時刻
}
```

#### 例
- ゲーム開始: `{"type":"model_updated","modelType":"Game","modelId":"game-xyz","event":"GameStarted"}`
- チャット追加: `{"type":"model_updated","modelType":"ChatMessage","modelId":"chat-123","event":"ChatMessageSent"}`

### クライアントの流れ
1. WebSocketで通知を受信
2. `modelType`と`modelId`をもとにREST APIで最新データを取得
3. 取得したデータで画面を再描画

## 🧪 テストポリシ

### テストレベル
- **ユニットテスト（UT）**  
  ドメイン層・サービス層のメソッド単位でテスト。  
  モック/スタブを活用し、外部依存を排除して高速に検証。

- **統合テスト（IT）**  
  SpringBootのAPIエンドポイントやサービス単位で、DynamoDBやWebSocketなど主要な外部サービスとの連携を含めてテスト。  
  LocalStackを利用し、AWSリソースのモック環境で実施。

- **E2Eテスト（E2E）**  
  フロントエンドとバックエンドを通したシナリオテスト。  
  Playwrightを利用し、Docker ComposeでLocalStack＋SpringBootアプリ＋フロントエンド＋Playwrightを一括起動し、実際のユーザー操作を自動化。

### テスト環境
- **LocalStack**  
  DynamoDB, API Gateway, Lambda, S3 などAWSサービスをローカルで再現し、CI/CDやローカル開発で本番に近い動作検証を可能にする。

- **Docker/Docker Compose**  
  SpringBootアプリケーション、LocalStack、フロントエンド、Playwrightテストランナーをコンテナで一括起動。  
  開発者全員が同一環境でテスト・開発可能。

### 実施方針
- すべてのPRでUT/ITを自動実行（CI/CD連携）
- E2Eは主要なシナリオをPlaywrightで自動実行
- テストデータはDocker起動時に自動投入
- テスト失敗時は原因を明確にログ出力

### 利用技術
- バックエンド: **Java (SpringBoot)**
- UT/IT: JUnit, Mockito など
- E2E: **Playwright**
- インフラ: LocalStack, Docker, Docker Compose

---

## 🚢 コンテナ・Lambda対応方針

- **アプリケーション層（Service/Controller/Domain）は、コンテナ版もLambda版も同じコードを利用する。**
- SpringBootアプリは、
  - **コンテナ（Docker）用の実行ファイル**
  - **AWS Lambda（Javaランタイム）用の実行ファイル**
  の両方をビルド可能とする。
- 将来的な運用形態の切り替え（ECS/Fargate, Lambda, EKS等）にも柔軟に対応できる構成とする。

---
