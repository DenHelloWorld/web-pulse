import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { Pulse } from '../models/pulse.interface';
import { API_BASE_URL, SIGNALR_HUB_PATH, SignalrMessageType } from '../constants/api.constants';

@Injectable({
  providedIn: 'root'
})
export class PulseSignalService {
  private hubConnection = signal<signalR.HubConnection | undefined>(undefined);
  private stoppingPromise = signal<Promise<void> | null>(null);

  pulses = signal<Pulse[]>([]);

  newPulse$ = new Subject<Pulse>();

  constructor() {
    this.initializeConnection();
  }

  private initializeConnection() {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(API_BASE_URL + SIGNALR_HUB_PATH)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.set(connection);

    connection.on(SignalrMessageType.ReceivePulse, (pulse: Pulse) => {
      this.pulses.update(currentPulses => {
        const updated = [...currentPulses, pulse];
        return updated.length > 100 ? updated.slice(-100) : updated;
      });

      this.newPulse$.next(pulse);
    });

    connection.onclose(() => {
      console.log('SignalR connection closed');
    });

    connection.onreconnected(() => {
      console.log('SignalR reconnected');
    });
  }

  async startConnection() {
    const promise = this.stoppingPromise();
    if (promise) {
      await promise;
      this.stoppingPromise.set(null);
    }

    const connection = this.hubConnection();
    if (connection && connection.state === signalR.HubConnectionState.Disconnected) {
      try {
        await connection.start();
        console.log('SignalR connected');
      } catch (err) {
        console.error('SignalR connection error', err);
      }
    }
  }

  stopConnection(): Promise<void> | undefined {
    const connection = this.hubConnection();
    if (connection) {
      const promise = connection.stop();
      this.stoppingPromise.set(promise);
      return promise;
    }
    return undefined;
  }
}
