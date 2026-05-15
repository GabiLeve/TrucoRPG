import BootScene from "./escenas/BootScene.js";
import MenuScene from "./escenas/MenuScene.js";
import CharacterScene from "./escenas/CharacterScene.js";
import WeaponScene from "./escenas/WeaponScene.js";
import LobbyScene from "./escenas/LobbyScene.js";
import GameScene from "./escenas/GameScene.js";
import GameScene2 from "./escenas/GameScene2.js";
import BaseScene from "./escenas/BaseScene.js";
import TrucoMultiScene from "./escenas/TrucoMultiScene.js";
import TrucoSoloScene  from "./escenas/TrucoSoloScene.js";

const config = {
    type: Phaser.AUTO,
    scale: {
        mode: Phaser.Scale.FIT,
        autoCenter: Phaser.Scale.CENTER_BOTH,
        width: 1280,
        height: 720,
        fullscreenTarget: 'contenedor-juego'
    },
    backgroundColor: '#1a5c1a',
    physics: {
        default: 'arcade',
        arcade: { debug: false }
    },
    scene: [
        BootScene, MenuScene, CharacterScene, WeaponScene,
        LobbyScene, GameScene, GameScene2, BaseScene, TrucoMultiScene, TrucoSoloScene
    ],
    parent: 'contenedor-juego',
};

new Phaser.Game(config);
