'use client'

import React, { useState, useEffect, Suspense } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Button, Input, Card, CardHeader, CardTitle, CardContent } from '@/components/ui'
import { apiClient } from '@/lib/api'

function CreateRoomContent() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const [playerName, setPlayerName] = useState('')
  const [settings, setSettings] = useState({
    timeLimit: 60,
    roundCount: 3,
    playerCount: 4
  })
  const [isCreating, setIsCreating] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [createdRoom, setCreatedRoom] = useState<{
    id: string
    url: string
  } | null>(null)

  useEffect(() => {
    const nameFromQuery = searchParams.get('playerName')
    if (nameFromQuery) {
      setPlayerName(decodeURIComponent(nameFromQuery))
    }
  }, [searchParams])

  const handleCreateRoom = async () => {
    setIsCreating(true)
    setError(null)
    
    try {
      // C# APIを呼び出してゲームを作成
      const response = await apiClient.createGame(playerName, {
        timeLimit: settings.timeLimit,
        roundCount: settings.roundCount,
        playerCount: settings.playerCount
      })
      
      const roomId = response.game.id
      const roomUrl = `${window.location.origin}/game/${roomId}`
      
      setCreatedRoom({
        id: roomId,
        url: roomUrl
      })
    } catch (error) {
      console.error('ルーム作成エラー:', error)
      setError(error instanceof Error ? error.message : 'ルームの作成に失敗しました')
    } finally {
      setIsCreating(false)
    }
  }

  const handleStartGame = () => {
    if (createdRoom) {
      router.push(`/game/${createdRoom.id}?playerName=${encodeURIComponent(playerName)}`)
    }
  }

  const copyRoomUrl = async () => {
    if (createdRoom) {
      try {
        await navigator.clipboard.writeText(createdRoom.url)
      } catch (error) {
        console.error('コピーエラー:', error)
      }
    }
  }

  if (createdRoom) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center p-4">
        <div className="w-full max-w-md space-y-6">
          <div className="text-center">
            <h1 className="text-heading font-bold text-primary mb-2">
              ルーム作成完了！
            </h1>
            <p className="text-body text-gray-600">
              友達を招待してゲームを始めよう
            </p>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>ルーム情報</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <label className="block text-body font-medium text-text mb-2">
                  ルームID
                </label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    value={createdRoom.id}
                    readOnly
                    className="flex-1 px-3 py-2 border border-gray-300 rounded-md bg-gray-50 text-body"
                  />
                  <Button
                    onClick={() => navigator.clipboard.writeText(createdRoom.id)}
                    variant="secondary"
                    size="sm"
                  >
                    コピー
                  </Button>
                </div>
              </div>

              <div>
                <label className="block text-body font-medium text-text mb-2">
                  招待URL
                </label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    value={createdRoom.url}
                    readOnly
                    className="flex-1 px-3 py-2 border border-gray-300 rounded-md bg-gray-50 text-body text-small"
                  />
                  <Button
                    onClick={copyRoomUrl}
                    variant="secondary"
                    size="sm"
                  >
                    コピー
                  </Button>
                </div>
              </div>

              <div className="pt-4 space-y-3">
                <Button
                  onClick={handleStartGame}
                  variant="primary"
                  size="lg"
                  fullWidth
                >
                  ゲームを開始
                </Button>
                <Button
                  onClick={() => router.push('/')}
                  variant="secondary"
                  size="md"
                  fullWidth
                >
                  トップページに戻る
                </Button>
              </div>
            </CardContent>
          </Card>

          <div className="text-center text-small text-gray-500">
            <p>参加者が揃ったらゲームを開始してください</p>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-4">
      <div className="w-full max-w-md space-y-6">
        <div className="text-center">
          <h1 className="text-heading font-bold text-primary mb-2">
            ルーム作成
          </h1>
          <p className="text-body text-gray-600">
            ゲームの設定を選択してください
          </p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>プレイヤー情報</CardTitle>
          </CardHeader>
          <CardContent>
            <Input
              label="プレイヤー名"
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              placeholder="あなたの名前"
              maxLength={20}
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>ゲーム設定</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="block text-body font-medium text-text mb-2">
                制限時間（秒）
              </label>
              <select
                value={settings.timeLimit}
                onChange={(e) => setSettings(prev => ({ ...prev, timeLimit: Number(e.target.value) }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <option value={30}>30秒</option>
                <option value={60}>60秒</option>
                <option value={90}>90秒</option>
                <option value={120}>120秒</option>
                <option value={0}>無制限</option>
              </select>
            </div>

            <div>
              <label className="block text-body font-medium text-text mb-2">
                ラウンド数
              </label>
              <select
                value={settings.roundCount}
                onChange={(e) => setSettings(prev => ({ ...prev, roundCount: Number(e.target.value) }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <option value={1}>1ラウンド</option>
                <option value={3}>3ラウンド</option>
                <option value={5}>5ラウンド</option>
                <option value={10}>10ラウンド</option>
              </select>
            </div>

            <div>
              <label className="block text-body font-medium text-text mb-2">
                最大プレイヤー数
              </label>
              <select
                value={settings.playerCount}
                onChange={(e) => setSettings(prev => ({ ...prev, playerCount: Number(e.target.value) }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <option value={2}>2人</option>
                <option value={3}>3人</option>
                <option value={4}>4人</option>
              </select>
            </div>
          </CardContent>
        </Card>

        <div className="space-y-3">
          {error && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-md">
              <p className="text-red-600 text-small">{error}</p>
            </div>
          )}
          <Button
            onClick={handleCreateRoom}
            variant="primary"
            size="lg"
            fullWidth
            disabled={!playerName.trim() || isCreating}
          >
            {isCreating ? 'ルーム作成中...' : 'ルームを作成'}
          </Button>
          <Button
            onClick={() => router.push('/')}
            variant="secondary"
            size="md"
            fullWidth
          >
            戻る
          </Button>
        </div>
      </div>
    </div>
  )
}

export default function CreateRoomPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-body">読み込み中...</p>
        </div>
      </div>
    }>
      <CreateRoomContent />
    </Suspense>
  )
}
