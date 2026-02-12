import Phaser from 'phaser';
import { Pulse } from '../../app/models/pulse.interface';
import { EventBus } from '../EventBus';
import { SceneKey } from '../constants/scene.constants';

export class PulseScene extends Phaser.Scene {
  constructor() {
    super(SceneKey.PulseScene);
  }

  preload() {
    const dot = this.make.graphics({ x: 0, y: 0 });
    dot.fillStyle(0xffffff);
    dot.fillCircle(10, 10, 10);
    dot.generateTexture('white-dot', 20, 20);
    dot.destroy();
  }

  create() {
    EventBus.emit('current-scene-ready', this);
  }

  createPulseBall(pulse: Pulse) {
    const x = Phaser.Math.Between(100, Number(this.scale.width) - 100);
    const startY = Number(this.scale.height) + 50;

    const color = Phaser.Display.Color.HexStringToColor(pulse.color).color;

    const emitter = this.add.particles(0, 0, 'white-dot', {
      speed: 20,
      scale: { start: 0.3, end: 0 },
      alpha: { start: 0.5, end: 0 },
      tint: color,
      lifespan: 600,
      blendMode: 'ADD'
    });

    const ball = this.add.image(x, startY, 'white-dot')
      .setTint(color)
      .setScale(0.5 + Math.abs(pulse.sentiment));

    emitter.startFollow(ball);

    this.tweens.add({
      targets: ball,
      y: -100,
      duration: 5000 + Math.random() * 2000,
      onComplete: () => {
        ball.destroy();
        emitter.destroy();
      }
    });
  }
}
