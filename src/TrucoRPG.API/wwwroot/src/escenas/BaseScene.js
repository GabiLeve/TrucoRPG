export default class BaseScene extends Phaser.Scene{
    constructor(key) {
        super(key);
    }


    botonPantallaCompleta(){
        let teclaF = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.F);
        teclaF.on('down',() => {
            if(this.scale.isFullscreen){
                this.scale.stopFullscreen();
            }else{
                this.scale.startFullscreen();
            }
        });

        this.add.text(10,10,'[Pantalla completa]',{fill:'0f0'})
        .setInteractive()
        .on('pointerdown',() => {
            if(!this.scale.isFullscreen){
                this.scale.startFullscreen();
            }
        }).setDepth(5);
    }

    
}