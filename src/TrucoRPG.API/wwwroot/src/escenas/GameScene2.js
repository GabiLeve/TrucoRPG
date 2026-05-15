import JugadorPrincipal from "../personajes/JugadorPrincipal.js";
import BaseScene from "./BaseScene.js";

export default class GameScene2 extends BaseScene {
    constructor() {
        super('GameScene2');
    }

    init(data) {
        this.playerKey = data.playerSprite || 'player';
    }

    create() {
        this.botonPantallaCompleta();

        this.cameras.main.fadeIn(1000, 0, 0, 0);

        this.fondo = this.add.tileSprite(0, 0, this.scale.width, this.scale.height, 'piso')
            .setOrigin(0, 0);
        
        this.JugadorPrincipal = new JugadorPrincipal(
            this,
            640,
            80,
            this.playerKey
        ).setDepth(1);

        this.JugadorPrincipal.setCollideWorldBounds(true);
        
        this.keys = this.input.keyboard.createCursorKeys();
    }

    update() {
       this.JugadorPrincipal.update(this.keys);
    }
}