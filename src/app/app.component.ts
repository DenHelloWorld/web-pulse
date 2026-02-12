import { Component, OnInit, OnDestroy, viewChild } from '@angular/core';
import { PhaserGame } from './phaser-game.component';
import { CommonModule } from '@angular/common';
import { PulseSignalService } from './services/signalr.service';
import { PulseScene } from '../game/scenes/pulse.scene';
import { SceneKey } from '../game/constants/scene.constants';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, PhaserGame],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit, OnDestroy {

    phaserRef = viewChild.required(PhaserGame);
  private pulseSubscription!: Subscription;

    constructor(private pulseSignalService: PulseSignalService)
    {
    }

    ngOnInit() {
        this.pulseSubscription = this.pulseSignalService.newPulse$.subscribe(pulse => {
            console.log('Received pulse:', pulse);
            const scene = this.phaserRef().scene;
            if (scene instanceof PulseScene) {
                scene.createPulseBall(pulse);
            } else {
                console.log('Current scene key:', scene?.scene?.key, 'expected:', SceneKey.PulseScene);
            }
        });
    }

    ngOnDestroy() {
        this.pulseSubscription?.unsubscribe();
        this.pulseSignalService.stopConnection();
    }

    public launchPulseScene()
    {
        this.phaserRef().game.scene.start(SceneKey.PulseScene);
        this.pulseSignalService.startConnection();
    }

    public pausePulseScene()
    {
        this.pulseSignalService.stopConnection();
    }

}
