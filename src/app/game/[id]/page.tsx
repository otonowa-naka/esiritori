'use client'

import React, { useState, useEffect, Suspense } from 'react'
import { useParams, useSearchParams, useRouter } from 'next/navigation'
import { Button, Card, CardHeader, CardTitle, CardContent } from '@/components/ui'
import { DrawingCanvas } from '@/components/game/DrawingCanvas'
import { ChatBox } from '@/components/chat/ChatBox'
import { Game, Player, ChatMessage } from '@/lib/types'

function GameContent() {
  const params = useParams()
  const searchParams = useSearchParams()
  const router = useRouter()
  const gameId = params.id as string
  const playerName = searchParams.get('playerName') || ''

  const [game, setGame] = useState<Game | null>(null)
  const [currentPlayer, setCurrentPlayer] = useState<Player | null>(null)
  const [chatMessages, setChatMessages] = useState<ChatMessage[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadGame = async () => {
      try {
        setIsLoading(true)
        
        const mockGame: Game = {
          id: gameId,
          settings: {
            timeLimit: 60,
            roundCount: 3,
            playerCount: 4
          },
          status: 'playing',
          currentRound: {
            roundNumber: 1,
            currentTurn: {
              turnNumber: 1,
              status: 'not_started',
              drawerId: '',
              answer: ''
            }
          },
          players: [
            {
              id: '1',
              name: playerName,
              isReady: false,
              isDrawer: true
            }
          ],
          scoreRecords: []
        }

        setGame(mockGame)
        setCurrentPlayer(mockGame.players[0])
      } catch (err) {
        setError('ゲームの読み込みに失敗しました')
        console.error('ゲーム読み込みエラー:', err)
      } finally {
        setIsLoading(false)
      }
    }

    if (gameId && playerName) {
      loadGame()
    } else {
      setError('ゲームIDまたはプレイヤー名が不正です')
      setIsLoading(false)
    }
  }, [gameId, playerName])

  const handleReady = async () => {
    try {
      if (currentPlayer) {
        setCurrentPlayer({ ...currentPlayer, isReady: true })
      }
    } catch (error) {
      console.error('準備完了エラー:', error)
    }
  }

  const handleDrawingUpdate = async (drawingData: string) => {
    try {
      console.log('描画データ更新:', drawingData.substring(0, 50) + '...')
    } catch (error) {
      console.error('描画更新エラー:', error)
    }
  }

  const handleSendMessage = async (content: string) => {
    try {
      const newMessage: ChatMessage = {
        id: Date.now().toString(),
        gameId,
        playerId: currentPlayer?.id || '',
        playerName: currentPlayer?.name || '',
        content,
        type: 'normal',
        timestamp: new Date().toISOString()
      }
      
      setChatMessages(prev => [...prev, newMessage])
    } catch (error) {
      console.error('メッセージ送信エラー:', error)
    }
  }

  const handleLeaveGame = () => {
    router.push('/')
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-body">ゲームを読み込み中...</p>
        </div>
      </div>
    )
  }

  if (error || !game) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center p-4">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-red-600">エラー</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-body">{error || 'ゲームが見つかりません'}</p>
            <Button onClick={handleLeaveGame} variant="primary" fullWidth>
              トップページに戻る
            </Button>
          </CardContent>
        </Card>
      </div>
    )
  }

  if (game.status === 'waiting') {
    return (
      <div className="min-h-screen bg-background p-4">
        <div className="max-w-4xl mx-auto space-y-6">
          <div className="flex justify-between items-center">
            <h1 className="text-heading font-bold text-primary">
              ルーム: {gameId}
            </h1>
            <Button onClick={handleLeaveGame} variant="secondary" size="sm">
              退出
            </Button>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>プレイヤー ({game.players.length}/{game.settings.playerCount})</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {game.players.map((player) => (
                  <div
                    key={player.id}
                    className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                  >
                    <span className="text-body font-medium">{player.name}</span>
                    <span
                      className={`text-small px-2 py-1 rounded ${
                        player.isReady
                          ? 'bg-green-100 text-green-800'
                          : 'bg-yellow-100 text-yellow-800'
                      }`}
                    >
                      {player.isReady ? '準備完了' : '待機中'}
                    </span>
                  </div>
                ))}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>ゲーム設定</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-body">制限時間:</span>
                  <span className="text-body font-medium">
                    {game.settings.timeLimit === 0 ? '無制限' : `${game.settings.timeLimit}秒`}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-body">ラウンド数:</span>
                  <span className="text-body font-medium">{game.settings.roundCount}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-body">最大プレイヤー数:</span>
                  <span className="text-body font-medium">{game.settings.playerCount}</span>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="text-center">
            {currentPlayer && !currentPlayer.isReady ? (
              <Button onClick={handleReady} variant="primary" size="lg">
                準備完了
              </Button>
            ) : (
              <div className="space-y-2">
                <p className="text-body text-green-600">準備完了しました</p>
                <p className="text-small text-gray-500">
                  他のプレイヤーの準備完了をお待ちください
                </p>
              </div>
            )}
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-background p-4">
      <div className="max-w-6xl mx-auto space-y-4">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-heading font-bold text-primary">
              ラウンド {game.currentRound.roundNumber} / ターン {game.currentRound.currentTurn.turnNumber}
            </h1>
            <p className="text-body text-gray-600">
              {game.currentRound.currentTurn.status === 'drawing' ? '描画中' : '待機中'}
            </p>
          </div>
          <Button onClick={handleLeaveGame} variant="secondary" size="sm">
            退出
          </Button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
          <div className="lg:col-span-3">
            <Card>
              <CardHeader>
                <CardTitle>描画エリア</CardTitle>
              </CardHeader>
              <CardContent>
                <DrawingCanvas
                  width={800}
                  height={500}
                  onDrawingUpdate={handleDrawingUpdate}
                  readOnly={!currentPlayer?.isDrawer}
                />
              </CardContent>
            </Card>
          </div>

          <div className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>プレイヤー</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                {game.players.map((player) => (
                  <div
                    key={player.id}
                    className={`p-2 rounded text-small ${
                      player.isDrawer
                        ? 'bg-primary text-white'
                        : 'bg-gray-100 text-gray-700'
                    }`}
                  >
                    {player.name}
                    {player.isDrawer && ' (描画中)'}
                  </div>
                ))}
              </CardContent>
            </Card>

            <Card className="h-96">
              <CardHeader>
                <CardTitle>チャット</CardTitle>
              </CardHeader>
              <CardContent className="p-0 h-full">
                <ChatBox
                  messages={chatMessages}
                  onSendMessage={handleSendMessage}
                  disabled={currentPlayer?.isDrawer || false}
                  placeholder={
                    currentPlayer?.isDrawer
                      ? '描画中はチャットできません'
                      : '答えを入力してください'
                  }
                />
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  )
}

export default function GamePage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-body">読み込み中...</p>
        </div>
      </div>
    }>
      <GameContent />
    </Suspense>
  )
}
