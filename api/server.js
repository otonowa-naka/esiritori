const express = require('express')
const cors = require('cors')
const WebSocket = require('ws')
const http = require('http')

const app = express()
const server = http.createServer(app)
const wss = new WebSocket.Server({ server })

app.use(cors())
app.use(express.json())

const games = new Map()
const players = new Map()
const chatMessages = new Map()

const connections = new Map()

function generateGameId() {
  return Math.random().toString(36).substring(2, 8).toUpperCase()
}

function generatePlayerId() {
  return Math.random().toString(36).substring(2, 15)
}

function broadcastToGame(gameId, message) {
  const gameConnections = Array.from(connections.values()).filter(conn => conn.gameId === gameId)
  gameConnections.forEach(conn => {
    if (conn.ws.readyState === WebSocket.OPEN) {
      conn.ws.send(JSON.stringify(message))
    }
  })
}


app.post('/api/games', (req, res) => {
  const { settings, creatorName } = req.body
  const gameId = generateGameId()
  const playerId = generatePlayerId()
  
  const creator = {
    id: playerId,
    name: creatorName,
    isReady: false,
    isDrawer: false
  }

  const game = {
    id: gameId,
    settings,
    status: 'waiting',
    currentRound: {
      roundNumber: 1,
      currentTurn: {
        turnNumber: 1,
        status: 'not_started',
        drawerId: '',
        answer: ''
      }
    },
    players: [creator],
    scoreRecords: []
  }

  games.set(gameId, game)
  players.set(playerId, creator)
  chatMessages.set(gameId, [])

  res.json({ game, player: creator })
})

app.get('/api/games/:gameId', (req, res) => {
  const { gameId } = req.params
  const game = games.get(gameId)
  
  if (!game) {
    return res.status(404).json({ error: 'ゲームが見つかりません' })
  }
  
  res.json(game)
})

app.post('/api/games/:gameId/join', (req, res) => {
  const { gameId } = req.params
  const { playerName } = req.body
  
  const game = games.get(gameId)
  if (!game) {
    return res.status(404).json({ error: 'ゲームが見つかりません' })
  }
  
  if (game.players.length >= game.settings.playerCount) {
    return res.status(400).json({ error: 'ゲームが満員です' })
  }
  
  const playerId = generatePlayerId()
  const player = {
    id: playerId,
    name: playerName,
    isReady: false,
    isDrawer: false
  }
  
  game.players.push(player)
  players.set(playerId, player)
  games.set(gameId, game)
  
  broadcastToGame(gameId, {
    type: 'player_joined',
    game,
    player
  })
  
  res.json({ game, player })
})

app.put('/api/games/:gameId/players/:playerId/ready', (req, res) => {
  const { gameId, playerId } = req.params
  const { isReady } = req.body
  
  const game = games.get(gameId)
  const player = players.get(playerId)
  
  if (!game || !player) {
    return res.status(404).json({ error: 'ゲームまたはプレイヤーが見つかりません' })
  }
  
  player.isReady = isReady
  const playerIndex = game.players.findIndex(p => p.id === playerId)
  if (playerIndex >= 0) {
    game.players[playerIndex] = player
  }
  
  players.set(playerId, player)
  games.set(gameId, game)
  
  broadcastToGame(gameId, {
    type: 'player_ready_updated',
    game,
    playerId,
    isReady
  })
  
  res.json({ success: true })
})

app.post('/api/games/:gameId/chat', (req, res) => {
  const { gameId } = req.params
  const { playerId, content } = req.body
  
  const game = games.get(gameId)
  const player = players.get(playerId)
  
  if (!game || !player) {
    return res.status(404).json({ error: 'ゲームまたはプレイヤーが見つかりません' })
  }
  
  const message = {
    id: Date.now().toString(),
    gameId,
    playerId,
    playerName: player.name,
    content,
    type: 'normal',
    timestamp: new Date().toISOString()
  }
  
  const messages = chatMessages.get(gameId) || []
  messages.push(message)
  chatMessages.set(gameId, messages)
  
  broadcastToGame(gameId, {
    type: 'chat_message',
    message
  })
  
  res.json(message)
})

app.post('/api/games/:gameId/drawing', (req, res) => {
  const { gameId } = req.params
  const { drawingData } = req.body
  
  const game = games.get(gameId)
  if (!game) {
    return res.status(404).json({ error: 'ゲームが見つかりません' })
  }
  
  broadcastToGame(gameId, {
    type: 'drawing_update',
    drawingData
  })
  
  res.json({ success: true })
})

wss.on('connection', (ws, req) => {
  const connectionId = Math.random().toString(36).substring(2, 15)
  
  connections.set(connectionId, {
    ws,
    gameId: null,
    playerId: null
  })
  
  ws.on('message', (data) => {
    try {
      const message = JSON.parse(data)
      const connection = connections.get(connectionId)
      
      switch (message.type) {
        case 'join_game':
          connection.gameId = message.gameId
          connection.playerId = message.playerId
          connections.set(connectionId, connection)
          
          ws.send(JSON.stringify({
            type: 'joined',
            gameId: message.gameId
          }))
          break
          
        case 'drawing_data':
          if (connection.gameId) {
            broadcastToGame(connection.gameId, {
              type: 'drawing_update',
              drawingData: message.drawingData
            })
          }
          break
      }
    } catch (error) {
      console.error('WebSocketメッセージ処理エラー:', error)
    }
  })
  
  ws.on('close', () => {
    connections.delete(connectionId)
  })
})

const PORT = process.env.PORT || 3001

server.listen(PORT, () => {
  console.log(`APIモックサーバーがポート${PORT}で起動しました`)
})
