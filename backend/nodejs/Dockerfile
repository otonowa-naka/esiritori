FROM node:20-alpine

WORKDIR /app

# package.json をコピー
COPY package.json ./

# 依存関係をインストール
RUN npm install

# ソースコードをコピー
COPY . .

# ポート3001を公開
EXPOSE 3001

# サーバーを起動
CMD ["npm", "start"]
