import Portal from "../objetos/Portal.js";
import JugadorPrincipal from "../personajes/JugadorPrincipal.js";
import JugadorRemoto from "../personajes/JugadorRemoto.js";
import Npc from "../personajes/Npc.js";
import BaseScene from "./BaseScene.js";
import { multiplayerManager } from "../MultiplayerManager.js";

export default class GameScene extends BaseScene {
    constructor() { super('GameScene'); }

    init(data) {
        this.playerKey    = data.playerSprite || 'player';
        this.startX       = data.x || 100;
        this.startY       = data.y || 100;
        this.esMultijugador = data.multijugador || false;
    }

    create() {
        this.botonPantallaCompleta();

        this.fondo = this.add.tileSprite(0, 0, this.scale.width, this.scale.height, 'pasto')
            .setOrigin(0, 0);

        this.JugadorPrincipal = new JugadorPrincipal(
            this, this.startX, this.startY, this.playerKey
        ).setDepth(1);
        this.JugadorPrincipal.setCollideWorldBounds(true);

        this.npc = new Npc(this, 490, 180, 'troll').setDepth(0);
        this.keys   = this.input.keyboard.createCursorKeys();
        this.teclaE = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.E);
        this.portal = new Portal(this, 600, 150, 'GameScene2', 'casa');
        this.physics.add.collider(this.JugadorPrincipal, this.portal.zone);

        this.zonaInteraccion = this.add.zone(this.npc.x, this.npc.y, this.npc.width, this.npc.height);
        this.physics.add.existing(this.zonaInteraccion);
        this.zonaInteraccion.body.setAllowGravity(false);
        this.zonaInteraccion.body.moves = false;

        const mensajeNpcTexto = this.esMultijugador
            ? '¡Presioná E para jugar al TRUCO!'
            : '¡Presioná E para jugar al TRUCO!';
        this.mensajeNpc = this.add.text(
            this.npc.x, this.npc.y - this.npc.height / 2 - 20,
            mensajeNpcTexto,
            { fontFamily: '"Jersey 10"', fontSize: '18px', color: '#000', backgroundColor: '#fff', padding: { x: 10, y: 5 } }
        ).setOrigin(0.5).setVisible(false).setDepth(10);
        this.estaEnZonaNpc = false;
        this.physics.add.overlap(this.JugadorPrincipal, this.zonaInteraccion, () => {
            this.estaEnZonaNpc = true;
        }, null, this);

        // ── MODO MULTIJUGADOR ────────────────────────────────────
        if (this.esMultijugador) {
            this.jugadorRemoto = null;
            this.timerEnvio = 0;
            multiplayerManager.limpiarCallbacks();

            // Posición del otro jugador
            multiplayerManager.onPosicionActualizada = (x, y, animacion, sprite, escena) => {
                if (escena !== 'GameScene') {
                    if (this.jugadorRemoto) { this.jugadorRemoto.destruir(); this.jugadorRemoto = null; }
                    return;
                }
                if (!this.jugadorRemoto)
                    this.jugadorRemoto = new JugadorRemoto(this, x, y, sprite);
                this.jugadorRemoto.actualizar(x, y, animacion);
            };

            multiplayerManager.onJugadorDesconectado = () => {
                if (this.jugadorRemoto) { this.jugadorRemoto.destruir(); this.jugadorRemoto = null; }
                this.add.text(640, 50, 'El otro jugador se desconectó', {
                    fontFamily: '"Jersey 10"', fontSize: '22px',
                    fill: '#ff4444', backgroundColor: '#000000bb', padding: { x: 12, y: 6 }
                }).setOrigin(0.5).setDepth(20);
            };

            this.events.on('shutdown', () => {
                multiplayerManager.limpiarCallbacks();
                if (this.jugadorRemoto) { this.jugadorRemoto.destruir(); this.jugadorRemoto = null; }
            });
        }
    }

    update(time, delta) {
        this.JugadorPrincipal.update(this.keys);
        this.portal.update(this.JugadorPrincipal, this.teclaE);
        this.mensajeNpc.setVisible(this.estaEnZonaNpc);

        // Interacción con el orco: E para jugar Truco
        if (this.estaEnZonaNpc && Phaser.Input.Keyboard.JustDown(this.teclaE)) {
            if (this.esMultijugador) {
                this.scene.start('TrucoMultiScene', {
                    miRol: multiplayerManager.esHost ? 'J1' : 'J2'
                });
            } else {
                this.scene.start('TrucoSoloScene', {
                    playerSprite: this.playerKey
                });
            }
        }

        this.estaEnZonaNpc = false;

        if (this.esMultijugador) {
            this.timerEnvio = (this.timerEnvio || 0) + delta;
            if (this.timerEnvio >= 100) {
                this.timerEnvio = 0;
                const animKey = this.JugadorPrincipal.anims.currentAnim?.key ?? `${this.playerKey}-quieto`;
                multiplayerManager.enviarPosicion(
                    this.JugadorPrincipal.x, this.JugadorPrincipal.y,
                    animKey, this.playerKey, 'GameScene'
                );
            }
        }
    }
}
