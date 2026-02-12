// API и SignalR константы для фронтенда
export const API_BASE_URL = 'http://localhost:5196';
export const SIGNALR_HUB_PATH = '/pulseHub';

// Enum для типов SignalR сообщений
export enum SignalrMessageType {
  ReceivePulse = 'ReceivePulse'
}
