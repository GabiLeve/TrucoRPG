import BaseScene from "./BaseScene.js";

export default class CharacterScene extends BaseScene {
    constructor() {
        super('CharacterScene');
    }

    init(data) {
        this.modo = data.modo || 'maquina';
    }

    create() {
        this.botonPantallaCompleta();

        this.add.text(640, 210, 'ELEGI TU PERSONAJE', {
            fontFamily: '"Jersey 10"',
            fontSize: '35px',
            fill: '#000000'
        }).setOrigin(0.5);

        const nena = this.add.image(540, 330, 'nena').setOrigin(0.5).setInteractive();
        const nene = this.add.image(740, 330, 'nene').setOrigin(0.5).setInteractive();

        nena.on('pointerdown', () => {
            this.scene.start('WeaponScene', { player: 'nena', modo: this.modo });
        });

        nene.on('pointerdown', () => {
            this.scene.start('WeaponScene', { player: 'nene', modo: this.modo });
        });
    }
}
