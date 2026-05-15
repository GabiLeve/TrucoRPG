export default class Npc extends Phaser.Physics.Arcade.Sprite {

    constructor(escena, x, y, nombre){
        super(escena, x, y, nombre);

        this.nombre = nombre;

        escena.add.existing(this);
        escena.physics.add.existing(this);

        this.setScale(1.4);

        this.body.setAllowGravity(false);

        escena.anims.create({
            key: `${this.nombre}-quieto`,
            frames: escena.anims.generateFrameNumbers(this.nombre, { start: 312, end: 313 }),
            frameRate: 5,
            repeat: -1
        });

        this.anims.play(`${this.nombre}-quieto`, true);
    }

    update(keys){
    }
}