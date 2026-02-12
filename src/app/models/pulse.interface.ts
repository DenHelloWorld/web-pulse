import { Source } from './source.enum';

export interface Pulse {
  sentiment: number;
  message: string;
  fullText: string;
  color: string;
  source: Source;
  author: string;
  url: string;
  timestamp: Date;
}

export { Source };
