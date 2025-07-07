import { Game, Player, ChatMessage } from './types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || '/api/v1';

export class ApiClient {
  private async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
      ...options,
    });

    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`);
    }

    return response.json();
  }

  async createGame(creatorName: string, settings: { timeLimit: number; roundCount: number; playerCount: number }): Promise<{ game: Game; player: Player }> {
    return this.request<{ game: Game; player: Player }>('/games', {
      method: 'POST',
      body: JSON.stringify({ 
        creatorName, 
        settings: {
          timeLimit: settings.timeLimit,
          roundCount: settings.roundCount,
          playerCount: settings.playerCount
        }
      }),
    });
  }

  async joinGame(gameId: string, playerName: string): Promise<Player> {
    return this.request<Player>(`/games/${gameId}/join`, {
      method: 'POST',
      body: JSON.stringify({ playerName }),
    });
  }

  async getGame(gameId: string): Promise<Game> {
    return this.request<Game>(`/games/${gameId}`);
  }

  async setReady(gameId: string): Promise<void> {
    await this.request(`/games/${gameId}/ready`, {
      method: 'POST',
    });
  }

  async setAnswer(gameId: string, answer: string): Promise<void> {
    await this.request(`/games/${gameId}/answer`, {
      method: 'POST',
      body: JSON.stringify({ answer }),
    });
  }

  async updateDrawing(gameId: string, drawingData: string): Promise<void> {
    await this.request(`/games/${gameId}/drawing`, {
      method: 'POST',
      body: JSON.stringify({ drawingData }),
    });
  }

  async submitAnswer(gameId: string, answer: string): Promise<{ isCorrect: boolean }> {
    return this.request<{ isCorrect: boolean }>(`/games/${gameId}/submit`, {
      method: 'POST',
      body: JSON.stringify({ answer }),
    });
  }

  async sendChatMessage(gameId: string, content: string): Promise<ChatMessage> {
    return this.request<ChatMessage>(`/games/${gameId}/chat`, {
      method: 'POST',
      body: JSON.stringify({ content }),
    });
  }

  async getChatHistory(gameId: string): Promise<ChatMessage[]> {
    return this.request<ChatMessage[]>(`/games/${gameId}/chat`);
  }

  async startGame(gameId: string): Promise<void> {
    await this.request(`/games/${gameId}/start`, {
      method: 'POST',
    });
  }
}

export const apiClient = new ApiClient();
