export default class BootScene extends Phaser.Scene {

    constructor(){
        super('BootScene');
    }

    preload(){
        //personajitos 
        this.load.image('nena', './assets/personajes/nena.png');
        this.load.image('nene', './assets/personajes/nene.png');

        //sprites de los personajes
        this.load.spritesheet('nena-arco','./assets/sprites/nena-arco.png',{frameWidth:64,frameHeight:64});
        this.load.spritesheet('nena-hacha','./assets/sprites/nena-hacha.png',{frameWidth:64,frameHeight:64});
        this.load.spritesheet('nene-arco','./assets/sprites/nene-arco.png',{frameWidth:64,frameHeight:64});
        this.load.spritesheet('nene-hacha','./assets/sprites/nene-hacha.png',{frameWidth:64,frameHeight:64});
        this.load.spritesheet('troll','./assets/sprites/troll.png',{frameWidth:64,frameHeight:64});

        //armas
        this.load.image('arco','./assets/armas/arco.png');
        this.load.image('hacha','./assets/armas/hacha.png');

        //objetos random que vamos a usar
        this.load.image('casa','./assets/objetos/casa.png');

        this.load.image('pasto','./assets/objetos/pasto.png');
        this.load.image('piso','./assets/objetos/piso.png');
        this.load.image('pasto-flores','./assets/objetos/pasto-flores.png');

        //Plugin (A PROBAR) del Joystick cuando pasemos a mobile o.o
        this.load.plugin('rexvirtualjoystickplugin', 'https://raw.githubusercontent.com/rexrainbow/phaser3-rex-notes/master/dist/rexvirtualjoystickplugin.min.js', true);
    }

    
    async create(){
        await document.fonts.load('16px "Jersey 10"');
        await document.fonts.load('16px "Pixelify Sans"');
        await document.fonts.ready;

        const dummy = this.add.text(0, 0, ' ', {
            fontFamily: 'Jersey 10'
        });
        dummy.setVisible(false);

        this.scene.start('MenuScene');
    }
}