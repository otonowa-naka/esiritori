export interface Game {
  id: string;
  settings: {
    timeLimit: number;
    roundCount: number;
    playerCount: number;
  };
  status: 'waiting' | 'playing' | 'finished';
  currentRound: {
    roundNumber: number;
    currentTurn: {
      turnNumber: number;
      status: 'not_started' | 'setting_answer' | 'drawing' | 'finished';
      drawerId: string;
      answer: string;
    };
  };
  players: Player[];
  scoreRecords: ScoreRecord[];
}

export interface Player {
  id: string;
  name: string;
  isReady: boolean;
  isDrawer: boolean;
}

export interface ScoreRecord {
  playerId: string;
  roundNumber: number;
  turnNumber: number;
  points: number;
  reason: 'correct_answer' | 'drawer_penalty';
  timestamp: string;
}

export interface ChatMessage {
  id: string;
  gameId: string;
  playerId: string;
  playerName: string;
  content: string;
  type: 'normal' | 'system' | 'correct';
  timestamp: string;
}
