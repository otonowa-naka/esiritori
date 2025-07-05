const express = require('express')
const fs = require('fs')
const path = require('path')
const yaml = require('js-yaml')

function generateMockServer() {
  const openApiPath = path.join(__dirname, '../../design/openapi.yaml')
  const openApiContent = fs.readFileSync(openApiPath, 'utf8')
  const apiSpec = yaml.load(openApiContent)
  
  const app = express()
  
  app.use(express.json())
  app.use(require('cors')())
  
  // メモリ内データストア
  const dataStore = {
    games: new Map(),
    players: new Map(),
    chatMessages: new Map(),
    connections: new Map()
  }
  
  // ヘルパー関数
  function generateId(prefix = '') {
    return prefix + Math.random().toString(36).substring(2, 8).toUpperCase()
  }
  
  function generatePlayerId() {
    return Math.random().toString(36).substring(2, 15)
  }
  
  // OpenAPI仕様に基づいてルートを生成
  Object.entries(apiSpec.paths).forEach(([path, pathSpec]) => {
    Object.entries(pathSpec).forEach(([method, methodSpec]) => {
      const expressPath = path.replace(/{([^}]+)}/g, ':$1')
      
      app[method](expressPath, (req, res) => {
        // 基本的なレスポンス生成
        const response = generateResponse(path, method, methodSpec, req, dataStore)
        res.json(response)
      })
    })
  })
  
  return app
}

function generateResponse(path, method, spec, req, dataStore) {
  // パスとメソッドに基づいてレスポンスを生成
  const { games, players, chatMessages } = dataStore
  
  switch (true) {
    case path === '/games' && method === 'post':
      return handleCreateGame(req, dataStore)
    
    case path === '/games/{gameId}/join' && method === 'post':
      return handleJoinGame(req, dataStore)
    
    case path === '/games/{gameId}' && method === 'get':
      return handleGetGame(req, dataStore)
    
    case path === '/games/{gameId}/chat' && method === 'post':
      return handleChatMessage(req, dataStore)
    
    case path === '/games/{gameId}/drawing' && method === 'post':
      return handleDrawing(req, dataStore)
    
    default:
      return { message: 'Mock response generated from OpenAPI spec' }
  }
}

function handleCreateGame(req, dataStore) {
  const { playerName, settings } = req.body
  const gameId = generateId('GAME')
  const playerId = generatePlayerId()
  
  const creator = {
    id: playerId,
    name: playerName,
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
  
  dataStore.games.set(gameId, game)
  dataStore.players.set(playerId, creator)
  dataStore.chatMessages.set(gameId, [])
  
  return { game, player: creator }
}

function handleJoinGame(req, dataStore) {
  const { gameId } = req.params
  const { playerName } = req.body
  
  const game = dataStore.games.get(gameId)
  if (!game) {
    throw new Error('Game not found')
  }
  
  const playerId = generatePlayerId()
  const player = {
    id: playerId,
    name: playerName,
    isReady: false,
    isDrawer: false
  }
  
  game.players.push(player)
  dataStore.players.set(playerId, player)
  dataStore.games.set(gameId, game)
  
  return { game, player }
}

function handleGetGame(req, dataStore) {
  const { gameId } = req.params
  const game = dataStore.games.get(gameId)
  
  if (!game) {
    throw new Error('Game not found')
  }
  
  return game
}

function handleChatMessage(req, dataStore) {
  const { gameId } = req.params
  const { playerId, content } = req.body
  
  const game = dataStore.games.get(gameId)
  const player = dataStore.players.get(playerId)
  
  if (!game || !player) {
    throw new Error('Game or player not found')
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
  
  const messages = dataStore.chatMessages.get(gameId) || []
  messages.push(message)
  dataStore.chatMessages.set(gameId, messages)
  
  return message
}

function handleDrawing(req, dataStore) {
  const { gameId } = req.params
  const { drawingData } = req.body
  
  const game = dataStore.games.get(gameId)
  if (!game) {
    throw new Error('Game not found')
  }
  
  return { success: true }
}

module.exports = { generateMockServer }