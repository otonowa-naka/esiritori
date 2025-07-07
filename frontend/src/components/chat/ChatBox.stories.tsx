import type { Meta, StoryObj } from '@storybook/react';
import { ChatBox } from './ChatBox';
import { ChatMessage } from '@/lib/types';

const meta: Meta<typeof ChatBox> = {
  title: 'Chat/ChatBox',
  component: ChatBox,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    disabled: {
      control: 'boolean',
    },
    placeholder: {
      control: 'text',
    },
  },
};

export default meta;
type Story = StoryObj<typeof meta>;

const sampleMessages: ChatMessage[] = [
  {
    id: '1',
    gameId: 'game-1',
    playerId: 'player-1',
    playerName: 'Alice',
    content: 'こんにちは！',
    type: 'normal',
    timestamp: '2023-12-01T10:00:00Z',
  },
  {
    id: '2',
    gameId: 'game-1',
    playerId: 'player-2',
    playerName: 'Bob',
    content: 'よろしくお願いします！',
    type: 'normal',
    timestamp: '2023-12-01T10:01:00Z',
  },
  {
    id: '3',
    gameId: 'game-1',
    playerId: 'system',
    playerName: 'System',
    content: 'ゲームが開始されました',
    type: 'system',
    timestamp: '2023-12-01T10:02:00Z',
  },
  {
    id: '4',
    gameId: 'game-1',
    playerId: 'player-1',
    playerName: 'Alice',
    content: 'りんご',
    type: 'correct',
    timestamp: '2023-12-01T10:03:00Z',
  },
];

export const Default: Story = {
  args: {
    messages: sampleMessages,
    onSendMessage: (content) => {
      console.log('Message sent:', content);
    },
  },
};

export const EmptyChat: Story = {
  args: {
    messages: [],
    onSendMessage: (content) => {
      console.log('Message sent:', content);
    },
  },
};

export const DisabledChat: Story = {
  args: {
    messages: sampleMessages,
    disabled: true,
    onSendMessage: (content) => {
      console.log('Message sent:', content);
    },
  },
};

export const WithCustomPlaceholder: Story = {
  args: {
    messages: sampleMessages,
    placeholder: 'ここに答えを入力してください...',
    onSendMessage: (content) => {
      console.log('Message sent:', content);
    },
  },
};

export const SystemMessagesOnly: Story = {
  args: {
    messages: [
      {
        id: '1',
        gameId: 'game-1',
        playerId: 'system',
        playerName: 'System',
        content: 'ゲームが開始されました',
        type: 'system',
        timestamp: '2023-12-01T10:00:00Z',
      },
      {
        id: '2',
        gameId: 'game-1',
        playerId: 'system',
        playerName: 'System',
        content: 'ターン1が開始されました',
        type: 'system',
        timestamp: '2023-12-01T10:01:00Z',
      },
    ],
    onSendMessage: (content) => {
      console.log('Message sent:', content);
    },
  },
};