import { Game, ChatMessage } from './types';

export interface WebSocketMessage {
  type: 'model_updated';
  modelType: 'Game' | 'ChatMessage' | 'Player';
  modelId: string;
  event?: string;
  updatedAt?: string;
}

export class WebSocketClient {
  private ws: WebSocket | null = null;
  private gameId: string | null = null;
  private listeners: Map<string, Set<(data: any) => void>> = new Map();

  connect(gameId: string): Promise<void> {
    return new Promise((resolve, reject) => {
      const wsUrl = process.env.NEXT_PUBLIC_WS_URL || `ws://localhost:3001/ws/${gameId}`;
      
      this.ws = new WebSocket(wsUrl);
      this.gameId = gameId;

      this.ws.onopen = () => {
        console.log('WebSocket connected');
        resolve();
      };

      this.ws.onmessage = (event) => {
        try {
          const message: WebSocketMessage = JSON.parse(event.data);
          this.handleMessage(message);
        } catch (error) {
          console.error('Failed to parse WebSocket message:', error);
        }
      };

      this.ws.onclose = () => {
        console.log('WebSocket disconnected');
        this.ws = null;
      };

      this.ws.onerror = (error) => {
        console.error('WebSocket error:', error);
        reject(error);
      };
    });
  }

  disconnect(): void {
    if (this.ws) {
      this.ws.close();
      this.ws = null;
      this.gameId = null;
    }
  }

  private handleMessage(message: WebSocketMessage): void {
    const eventType = `${message.modelType}_updated`;
    const listeners = this.listeners.get(eventType);
    
    if (listeners) {
      listeners.forEach(listener => listener(message));
    }

    const allListeners = this.listeners.get('*');
    if (allListeners) {
      allListeners.forEach(listener => listener(message));
    }
  }

  on(eventType: string, listener: (data: any) => void): void {
    if (!this.listeners.has(eventType)) {
      this.listeners.set(eventType, new Set());
    }
    this.listeners.get(eventType)!.add(listener);
  }

  off(eventType: string, listener: (data: any) => void): void {
    const listeners = this.listeners.get(eventType);
    if (listeners) {
      listeners.delete(listener);
    }
  }

  onGameUpdate(listener: (message: WebSocketMessage) => void): void {
    this.on('Game_updated', listener);
  }

  onChatUpdate(listener: (message: WebSocketMessage) => void): void {
    this.on('ChatMessage_updated', listener);
  }

  onPlayerUpdate(listener: (message: WebSocketMessage) => void): void {
    this.on('Player_updated', listener);
  }
}

export const wsClient = new WebSocketClient();
