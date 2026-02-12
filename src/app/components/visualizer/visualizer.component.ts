import { Component, OnInit, OnDestroy } from '@angular/core';
import { PulseSignalService } from '../../services/signalr.service';
import { Subscription } from 'rxjs';

import Phaser from 'phaser';
import { PulseScene } from '../../../game/scenes/pulse.scene';
import { EventBus } from '../../../game/EventBus';

@Component({
  selector: 'app-visualizer',
  template: `<div id="game-container"></div>`,
  styles: [`#game-container { width: 100vw; height: 100vh; }`]
})
export class VisualizerComponent implements OnInit, OnDestroy {
  private game!: Phaser.Game;
  private scene!: PulseScene;
  private pulseSubscription!: Subscription;
  private onSceneReady!: (scene: Phaser.Scene) => void;

  constructor(private pulseSignalService: PulseSignalService) {}

  ngOnInit() {
    this.scene = new PulseScene();

    const config = {
      type: Phaser.AUTO,
      parent: 'game-container',
      width: window.innerWidth,
      height: window.innerHeight,
      transparent: true,
      scene: [this.scene]
    };

    this.game = new Phaser.Game(config);

    this.onSceneReady = (scene: Phaser.Scene) => {
      if (scene instanceof PulseScene) {
        this.pulseSubscription?.unsubscribe();
        this.pulseSubscription = this.pulseSignalService.newPulse$.subscribe(pulse => {
          this.scene.createPulseBall(pulse);
        });

        this.pulseSignalService.startConnection();
      }
    };
    EventBus.on('current-scene-ready', this.onSceneReady);
  }

  ngOnDestroy() {
    EventBus.off('current-scene-ready', this.onSceneReady);
    this.pulseSubscription?.unsubscribe();
    this.game.destroy(true);
    this.pulseSignalService.stopConnection();
  }
}
