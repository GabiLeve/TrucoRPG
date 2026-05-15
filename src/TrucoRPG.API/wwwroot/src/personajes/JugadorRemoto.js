/**
 * JugadorRemoto — sprite del otro jugador en modo multijugador.
 * Su posición se actualiza con datos recibidos por WebSocket,
 * no con teclado como JugadorPrincipal.
 */
export default class JugadorRemoto extends Phaser.GameObjects.Sprite {

    constructor(escena, x, y, nombre) {
        super(escena, x, y, nombre);
        this.nombre = nombre;
        escena.add.existing(this);
        this.setScale(1.1).setDepth(2);

        // Crear animaciones si aún no existen (puede que JugadorPrincipal ya las creó)
        const animaciones = {
            'quieto':              { start: 312, end: 313, frameRate: 5  },
            'caminando-arriba':    { start: 105, end: 113, frameRate: 10 },
            'caminando-abajo':     { start: 131, end: 139, frameRate: 10 },
            'caminando-izquierda': { start: 117, end: 125, frameRate: 10 },
            'caminando-derecha':   { start: 143, end: 151, frameRate: 10 },
        };

        for (const [sufijo, cfg] of Object.entries(animaciones)) {
            const key = `${nombre}-${sufijo}`;
            if (!escena.anims.exists(key)) {
                escena.anims.create({
                    key,
                    frames: escena.anims.generateFrameNumbers(nombre, { start: cfg.start, end: cfg.end }),
                    frameRate: cfg.frameRate,
                    repeat: -1
                });
            }
        }

        // Etiqueta "Jugador 2" sobre el sprite
        this.label = escena.add.text(x, y - 50, 'Jugador 2', {
            fontFamily: '"Jersey 10"',
            fontSize: '16px',
            fill: '#ff6666',
            backgroundColor: '#00000099',
            padding: { x: 6, y: 3 }
        }).setOrigin(0.5).setDepth(10);

        this.anims.play(`${nombre}-quieto`, true);
    }

    /** Llamado cada vez que llega un update de red */
    actualizar(x, y, animacion) {
        this.setPosition(x, y);
        this.label.setPosition(x, y - 50);

        if (animacion && this.anims.exists(animacion)) {
            this.anims.play(animacion, true);
        }
    }

    destruir() {
        if (this.label) this.label.destroy();
        this.destroy();
    }
}
