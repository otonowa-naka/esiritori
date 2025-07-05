'use client'

import React, { useRef, useEffect, useState } from 'react'
import { Button } from '@/components/ui'

interface DrawingCanvasProps {
  width?: number
  height?: number
  onDrawingUpdate?: (drawingData: string) => void
  readOnly?: boolean
}

export function DrawingCanvas({ 
  width = 600, 
  height = 400, 
  onDrawingUpdate,
  readOnly = false 
}: DrawingCanvasProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const [isDrawing, setIsDrawing] = useState(false)
  const [currentTool, setCurrentTool] = useState<'pen' | 'eraser'>('pen')
  const [brushSize, setBrushSize] = useState(3)
  const [currentColor, setCurrentColor] = useState('#000000')

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    ctx.lineCap = 'round'
    ctx.lineJoin = 'round'
    ctx.fillStyle = 'white'
    ctx.fillRect(0, 0, width, height)
  }, [width, height])

  const startDrawing = (e: React.MouseEvent<HTMLCanvasElement>) => {
    if (readOnly) return
    setIsDrawing(true)
    draw(e)
  }

  const draw = (e: React.MouseEvent<HTMLCanvasElement>) => {
    if (!isDrawing || readOnly) return

    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    const rect = canvas.getBoundingClientRect()
    const x = e.clientX - rect.left
    const y = e.clientY - rect.top

    ctx.lineWidth = brushSize
    ctx.globalCompositeOperation = currentTool === 'eraser' ? 'destination-out' : 'source-over'
    ctx.strokeStyle = currentColor

    ctx.lineTo(x, y)
    ctx.stroke()
    ctx.beginPath()
    ctx.moveTo(x, y)
  }

  const stopDrawing = () => {
    if (!isDrawing) return
    setIsDrawing(false)
    
    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    ctx.beginPath()

    if (onDrawingUpdate) {
      const drawingData = canvas.toDataURL()
      onDrawingUpdate(drawingData)
    }
  }

  const clearCanvas = () => {
    if (readOnly) return
    
    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    ctx.fillStyle = 'white'
    ctx.fillRect(0, 0, width, height)

    if (onDrawingUpdate) {
      const drawingData = canvas.toDataURL()
      onDrawingUpdate(drawingData)
    }
  }

  const colors = ['#000000', '#FF0000', '#00FF00', '#0000FF', '#FFFF00', '#FF00FF', '#00FFFF']

  return (
    <div className="space-y-4">
      {/* ツールバー */}
      {!readOnly && (
        <div className="flex flex-wrap items-center gap-4 p-4 bg-gray-100 rounded-lg">
          {/* ツール選択 */}
          <div className="flex gap-2">
            <Button
              size="sm"
              variant={currentTool === 'pen' ? 'primary' : 'secondary'}
              onClick={() => setCurrentTool('pen')}
            >
              ペン
            </Button>
            <Button
              size="sm"
              variant={currentTool === 'eraser' ? 'primary' : 'secondary'}
              onClick={() => setCurrentTool('eraser')}
            >
              消しゴム
            </Button>
          </div>

          {/* 色選択 */}
          <div className="flex gap-1">
            {colors.map((color) => (
              <button
                key={color}
                className={`w-8 h-8 rounded border-2 ${
                  currentColor === color ? 'border-primary' : 'border-gray-300'
                }`}
                style={{ backgroundColor: color }}
                onClick={() => setCurrentColor(color)}
              />
            ))}
          </div>

          {/* ブラシサイズ */}
          <div className="flex items-center gap-2">
            <span className="text-small">太さ:</span>
            <input
              type="range"
              min="1"
              max="20"
              value={brushSize}
              onChange={(e) => setBrushSize(Number(e.target.value))}
              className="w-20"
            />
            <span className="text-small w-6">{brushSize}</span>
          </div>

          {/* クリアボタン */}
          <Button
            size="sm"
            variant="danger"
            onClick={clearCanvas}
          >
            クリア
          </Button>
        </div>
      )}

      {/* キャンバス */}
      <div className="border-2 border-gray-300 rounded-lg overflow-hidden bg-white">
        <canvas
          ref={canvasRef}
          width={width}
          height={height}
          className={`block ${!readOnly ? 'cursor-crosshair' : 'cursor-default'}`}
          onMouseDown={startDrawing}
          onMouseMove={draw}
          onMouseUp={stopDrawing}
          onMouseLeave={stopDrawing}
        />
      </div>
    </div>
  )
}
