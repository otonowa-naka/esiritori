'use client'

import React, { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Button, Input, Card, CardHeader, CardTitle, CardContent } from '@/components/ui'

export default function HomePage() {
  const router = useRouter()
  const [playerName, setPlayerName] = useState('')
  const [roomId, setRoomId] = useState('')
  const [errors, setErrors] = useState<{ playerName?: string; roomId?: string }>({})

  const validatePlayerName = (name: string) => {
    if (!name.trim()) {
      return 'プレイヤー名を入力してください'
    }
    if (name.length > 20) {
      return 'プレイヤー名は20文字以内で入力してください'
    }
    return null
  }

  const handleCreateRoom = () => {
    const nameError = validatePlayerName(playerName)
    if (nameError) {
      setErrors({ playerName: nameError })
      return
    }

    setErrors({})
    router.push(`/create?playerName=${encodeURIComponent(playerName)}`)
  }

  const handleJoinRoom = () => {
    const nameError = validatePlayerName(playerName)
    let roomError = null

    if (!roomId.trim()) {
      roomError = 'ルームIDを入力してください'
    }

    if (nameError || roomError) {
      setErrors({ playerName: nameError || undefined, roomId: roomError || undefined })
      return
    }

    setErrors({})
    router.push(`/game/${roomId}?playerName=${encodeURIComponent(playerName)}`)
  }

  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-4">
      <div className="w-full max-w-md space-y-6">
        {/* タイトル */}
        <div className="text-center">
          <h1 className="text-heading font-bold text-primary mb-2">
            お絵描き当てゲーム
          </h1>
          <p className="text-body text-gray-600">
            みんなで楽しく絵を描いて当てよう！
          </p>
        </div>

        {/* プレイヤー名入力 */}
        <Card>
          <CardHeader>
            <CardTitle>プレイヤー情報</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Input
              label="プレイヤー名"
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              placeholder="あなたの名前を入力してください"
              error={errors.playerName}
              maxLength={20}
            />
          </CardContent>
        </Card>

        {/* ルーム作成・参加 */}
        <Card>
          <CardHeader>
            <CardTitle>ゲームを始める</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* ルーム作成 */}
            <div className="space-y-2">
              <Button
                onClick={handleCreateRoom}
                variant="primary"
                size="lg"
                fullWidth
                disabled={!playerName.trim()}
              >
                新しいルームを作成
              </Button>
              <p className="text-small text-gray-500 text-center">
                新しいゲームルームを作成して友達を招待
              </p>
            </div>

            {/* 区切り線 */}
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-300" />
              </div>
              <div className="relative flex justify-center text-small">
                <span className="bg-white px-2 text-gray-500">または</span>
              </div>
            </div>

            {/* ルーム参加 */}
            <div className="space-y-3">
              <Input
                label="ルームID"
                value={roomId}
                onChange={(e) => setRoomId(e.target.value)}
                placeholder="参加するルームIDを入力"
                error={errors.roomId}
              />
              <Button
                onClick={handleJoinRoom}
                variant="secondary"
                size="lg"
                fullWidth
                disabled={!playerName.trim() || !roomId.trim()}
              >
                ルームに参加
              </Button>
              <p className="text-small text-gray-500 text-center">
                友達から共有されたルームIDで参加
              </p>
            </div>
          </CardContent>
        </Card>

        {/* フッター */}
        <div className="text-center text-small text-gray-500">
          <p>最大4人まで同時プレイ可能</p>
        </div>
      </div>
    </div>
  )
}
