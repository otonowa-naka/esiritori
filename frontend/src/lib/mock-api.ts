import { Game, Player, ChatMessage } from './types'

export class MockApiClient {
  private games: Map<string, Game> = new Map()
  private players: Map<string, Player> = new Map()
  private chatMessages: Map<string, ChatMessage[]> = new Map()

  async createGame(settings: Game['settings'], creatorName: string): Promise<Game> {
    const gameId = this.generateGameId()
    const playerId = this.generatePlayerId()
    
    const creator: Player = {
      id: playerId,
      name: creatorName,
      isReady: false,
      isDrawer: false
    }

    const game: Game = {
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

    this.games.set(gameId, game)
    this.players.set(playerId, creator)
    this.chatMessages.set(gameId, [])

    return game
  }

  async getGame(gameId: string): Promise<Game | null> {
    return this.games.get(gameId) || null
  }

  async joinGame(gameId: string, playerName: string): Promise<{ game: Game; player: Player } | null> {
    const game = this.games.get(gameId)
    if (!game || game.players.length >= game.settings.playerCount) {
      return null
    }

    const playerId = this.generatePlayerId()
    const player: Player = {
      id: playerId,
      name: playerName,
      isReady: false,
      isDrawer: false
    }

    game.players.push(player)
    this.players.set(playerId, player)
    this.games.set(gameId, game)

    return { game, player }
  }

  async updatePlayerReady(gameId: string, playerId: string, isReady: boolean): Promise<boolean> {
    const game = this.games.get(gameId)
    const player = this.players.get(playerId)
    
    if (!game || !player) return false

    player.isReady = isReady
    const playerIndex = game.players.findIndex(p => p.id === playerId)
    if (playerIndex >= 0) {
      game.players[playerIndex] = player
    }

    this.players.set(playerId, player)
    this.games.set(gameId, game)
    
    return true
  }

  async sendChatMessage(gameId: string, playerId: string, content: string): Promise<ChatMessage | null> {
    const game = this.games.get(gameId)
    const player = this.players.get(playerId)
    
    if (!game || !player) return null

    const message: ChatMessage = {
      id: Date.now().toString(),
      gameId,
      playerId,
      playerName: player.name,
      content,
      type: 'normal',
      timestamp: new Date().toISOString()
    }

    const messages = this.chatMessages.get(gameId) || []
    messages.push(message)
    this.chatMessages.set(gameId, messages)

    return message
  }

  async getChatMessages(gameId: string): Promise<ChatMessage[]> {
    return this.chatMessages.get(gameId) || []
  }

  async updateDrawing(gameId: string, drawingData: string): Promise<boolean> {
    const game = this.games.get(gameId)
    if (!game) return false

    console.log(`描画データ更新: ゲーム ${gameId}, データサイズ: ${drawingData.length}`)
    return true
  }

  private generateGameId(): string {
    return Math.random().toString(36).substring(2, 8).toUpperCase()
  }

  private generatePlayerId(): string {
    return Math.random().toString(36).substring(2, 15)
  }
}

export const mockApi = new MockApiClient()
