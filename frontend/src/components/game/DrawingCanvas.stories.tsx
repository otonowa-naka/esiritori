import type { Meta, StoryObj } from '@storybook/react';
import { DrawingCanvas } from './DrawingCanvas';

const meta: Meta<typeof DrawingCanvas> = {
  title: 'Game/DrawingCanvas',
  component: DrawingCanvas,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    width: {
      control: { type: 'range', min: 200, max: 800, step: 50 },
    },
    height: {
      control: { type: 'range', min: 200, max: 600, step: 50 },
    },
    readOnly: {
      control: 'boolean',
    },
  },
};

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    width: 600,
    height: 400,
    onDrawingUpdate: (drawingData: string) => {
      console.log('Drawing updated:', drawingData.length, 'bytes');
    },
  },
};

export const Small: Story = {
  args: {
    width: 300,
    height: 200,
    onDrawingUpdate: (drawingData: string) => {
      console.log('Drawing updated:', drawingData.length, 'bytes');
    },
  },
};

export const Large: Story = {
  args: {
    width: 800,
    height: 600,
    onDrawingUpdate: (drawingData: string) => {
      console.log('Drawing updated:', drawingData.length, 'bytes');
    },
  },
};

export const ReadOnly: Story = {
  args: {
    width: 600,
    height: 400,
    readOnly: true,
    onDrawingUpdate: (drawingData: string) => {
      console.log('Drawing updated:', drawingData.length, 'bytes');
    },
  },
};

export const WithoutCallback: Story = {
  args: {
    width: 600,
    height: 400,
  },
};