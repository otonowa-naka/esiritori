'use client'

import React, { useState, useRef, useEffect } from 'react'
import { Button, Input } from '@/components/ui'
import { ChatMessage } from '@/lib/types'

interface ChatBoxProps {
  messages: ChatMessage[]
  onSendMessage?: (content: string) => void
  disabled?: boolean
  placeholder?: string
}

export function ChatBox({ 
  messages, 
  onSendMessage, 
  disabled = false,
  placeholder = "メッセージを入力..." 
}: ChatBoxProps) {
  const [inputValue, setInputValue] = useState('')
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!inputValue.trim() || disabled) return

    onSendMessage?.(inputValue.trim())
    setInputValue('')
  }

  const formatTime = (timestamp: string) => {
    return new Date(timestamp).toLocaleTimeString('ja-JP', {
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  const getMessageStyle = (type: ChatMessage['type']) => {
    switch (type) {
      case 'system':
        return 'bg-gray-100 text-gray-600 italic'
      case 'correct':
        return 'bg-green-100 text-green-800 font-medium'
      default:
        return 'bg-white'
    }
  }

  return (
    <div className="flex flex-col h-full">
      {/* メッセージ表示エリア */}
      <div className="flex-1 overflow-y-auto p-4 space-y-2 bg-gray-50 rounded-t-lg">
        {messages.length === 0 ? (
          <div className="text-center text-gray-500 text-small">
            まだメッセージがありません
          </div>
        ) : (
          messages.map((message) => (
            <div
              key={message.id}
              className={`p-3 rounded-lg ${getMessageStyle(message.type)}`}
            >
              <div className="flex items-start justify-between gap-2">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-medium text-small">
                      {message.playerName}
                    </span>
                    <span className="text-gray-500 text-small">
                      {formatTime(message.timestamp)}
                    </span>
                  </div>
                  <div className="text-body">{message.content}</div>
                </div>
              </div>
            </div>
          ))
        )}
        <div ref={messagesEndRef} />
      </div>

      {/* メッセージ入力フォーム */}
      <form onSubmit={handleSubmit} className="p-4 bg-white rounded-b-lg border-t">
        <div className="flex gap-2">
          <input
            type="text"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            placeholder={placeholder}
            disabled={disabled}
            className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary disabled:bg-gray-100 disabled:cursor-not-allowed"
          />
          <Button
            type="submit"
            disabled={disabled || !inputValue.trim()}
            size="md"
          >
            送信
          </Button>
        </div>
      </form>
    </div>
  )
}
