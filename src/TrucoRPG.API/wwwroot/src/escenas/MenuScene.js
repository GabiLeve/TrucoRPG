import BaseScene from "./BaseScene.js";

export default class MenuScene extends BaseScene {
    constructor() {
        super('MenuScene');
    }

    create() {
        this.botonPantallaCompleta();

        this.add.text(640, 200, 'TRUCO RPG', {
            fontFamily: '"Jersey 10"',
            fontSize: '70px',
            fill: '#36ff36'
        }).setOrigin(0.5);

        const vsMaquina = this.add.text(640, 330, 'VS MAQUINA', {
            fontFamily: '"Jersey 10"',
            fontSize: '46px',
            fill: '#cccccc'
        }).setOrigin(0.5).setInteractive();

        const multi = this.add.text(640, 410, 'MULTIJUGADOR', {
            fontFamily: '"Jersey 10"',
            fontSize: '46px',
            fill: '#44aaff'
        }).setOrigin(0.5).setInteractive();

        const salir = this.add.text(640, 490, 'SALIR', {
            fontFamily: '"Jersey 10"',
            fontSize: '36px',
            fill: '#ff4444'
        }).setOrigin(0.5).setInteractive();

        // Efectos hover
        [vsMaquina, multi, salir].forEach(btn => {
            btn.on('pointerover', () => btn.setAlpha(0.7));
            btn.on('pointerout',  () => btn.setAlpha(1));
        });

        vsMaquina.on('pointerdown', () => {
            this.scene.start('CharacterScene', { modo: 'maquina' });
        });

        multi.on('pointerdown', () => {
            this.scene.start('CharacterScene', { modo: 'multijugador' });
        });

        salir.on('pointerdown', () => {
            console.log("Saliste");
        });
    }
}
