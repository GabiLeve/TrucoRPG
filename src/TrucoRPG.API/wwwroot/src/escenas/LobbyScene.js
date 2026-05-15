import BaseScene from "./BaseScene.js";
import { multiplayerManager } from "../MultiplayerManager.js";

export default class LobbyScene extends BaseScene {
    constructor() {
        super('LobbyScene');
    }

    init(data) {
        this.playerSprite = data.playerSprite || 'nene-hacha';
    }

    async create() {
        this.botonPantallaCompleta();
        multiplayerManager.limpiarCallbacks();

        this.add.text(640, 130, 'MULTIJUGADOR', {
            fontFamily: '"Jersey 10"',
            fontSize: '55px',
            fill: '#44aaff'
        }).setOrigin(0.5);

        // Código de sala: letra negra con fondo celeste para máxima visibilidad
        this.codigoText = this.add.text(640, 490, '', {
            fontFamily: '"Jersey 10"',
            fontSize: '36px',
            fill: '#000000',
            backgroundColor: '#44ccff',
            padding: { x: 20, y: 8 }
        }).setOrigin(0.5);

        // Status: negro sobre fondo claro para que siempre se lea
        this.statusText = this.add.text(640, 560, 'Conectando...', {
            fontFamily: '"Jersey 10"',
            fontSize: '22px',
            fill: '#222222',
            backgroundColor: '#ffffffcc',
            padding: { x: 10, y: 4 }
        }).setOrigin(0.5);

        multiplayerManager.onSalaLista = () => {
            this.statusText.setStyle({ fill: '#006600', backgroundColor: '#aaffaa' });
            this.statusText.setText('Sala lista! Iniciando...');
            this.time.delayedCall(1000, () => {
                this.scene.start('GameScene', {
                    playerSprite: this.playerSprite,
                    multijugador: true
                });
            });
        };

        try {
            await multiplayerManager.conectar();
            this.statusText.setText('Conectado. Elegí una opcion:');
            this.crearBotones();
        } catch (e) {
            this.statusText.setStyle({ fill: '#ff0000', backgroundColor: '#ffcccc' });
            this.statusText.setText('Error de conexion. Reintentá más tarde.');
            console.error('[Lobby] Error de conexión:', e);
        }
    }

    crearBotones() {
        const btnCrear = this.add.text(640, 270, '[ CREAR SALA ]', {
            fontFamily: '"Jersey 10"',
            fontSize: '40px',
            fill: '#36ff36'
        }).setOrigin(0.5).setInteractive();

        const btnUnirse = this.add.text(640, 360, '[ UNIRSE A SALA ]', {
            fontFamily: '"Jersey 10"',
            fontSize: '40px',
            fill: '#333333'
        }).setOrigin(0.5).setInteractive();

        const btnVolver = this.add.text(640, 450, 'VOLVER', {
            fontFamily: '"Jersey 10"',
            fontSize: '28px',
            fill: '#ff4444'
        }).setOrigin(0.5).setInteractive();

        [btnCrear, btnUnirse, btnVolver].forEach(btn => {
            btn.on('pointerover', () => btn.setAlpha(0.6));
            btn.on('pointerout',  () => btn.setAlpha(1));
        });

        btnCrear.on('pointerdown', async () => {
            btnCrear.disableInteractive();
            btnUnirse.disableInteractive();
            const codigo = await multiplayerManager.crearSala();
            console.log('[Lobby] Sala creada, código:', JSON.stringify(codigo));
            this.codigoText.setText(`Codigo: ${codigo}`);
            this.statusText.setText('Esperando al otro jugador...');
        });

        btnUnirse.on('pointerdown', async () => {
            const codigo = await this.mostrarInputCodigo();
            if (!codigo) return;

            console.log('[Lobby] Intentando unirse con código:', JSON.stringify(codigo));
            this.statusText.setText(`Conectando a sala: ${codigo}`);

            const ok = await multiplayerManager.unirseASala(codigo);
            console.log('[Lobby] Resultado unirse:', ok);
            if (!ok) {
                this.statusText.setStyle({ fill: '#ff0000', backgroundColor: '#ffcccc' });
                this.statusText.setText('Codigo invalido o sala llena. Intentá de nuevo.');
            } else {
                btnCrear.disableInteractive();
                btnUnirse.disableInteractive();
                this.statusText.setStyle({ fill: '#222222', backgroundColor: '#ffffffcc' });
                this.statusText.setText('Uniéndose...');
            }
        });

        btnVolver.on('pointerdown', async () => {
            await multiplayerManager.desconectar();
            this.scene.start('MenuScene');
        });
    }

    // Panel DOM para ingresar código — más confiable que prompt()
    mostrarInputCodigo() {
        return new Promise((resolve) => {
            const overlay = document.createElement('div');
            overlay.style.cssText = [
                'position:fixed', 'top:0', 'left:0', 'width:100%', 'height:100%',
                'background:rgba(0,0,0,0.85)', 'display:flex', 'align-items:center',
                'justify-content:center', 'z-index:9999', 'flex-direction:column', 'gap:18px'
            ].join(';');

            const titulo = document.createElement('div');
            titulo.textContent = 'Ingresá el código de sala:';
            titulo.style.cssText = 'color:#44ccff;font-size:26px;font-family:monospace;letter-spacing:2px;';

            const input = document.createElement('input');
            input.type = 'text';
            input.maxLength = 6;
            input.placeholder = 'XXXXXX';
            input.autocomplete = 'off';
            input.style.cssText = [
                'font-size:40px', 'padding:10px 20px', 'text-align:center',
                'letter-spacing:10px', 'text-transform:uppercase',
                'border:3px solid #44ccff', 'background:#111', 'color:#fff',
                'font-family:monospace', 'width:240px', 'outline:none'
            ].join(';');

            const btnRow = document.createElement('div');
            btnRow.style.cssText = 'display:flex;gap:16px;';

            const btnOk = document.createElement('button');
            btnOk.textContent = 'UNIRSE';
            btnOk.style.cssText = 'font-size:22px;padding:10px 36px;background:#44aaff;border:none;cursor:pointer;font-family:monospace;color:#000;font-weight:bold;';

            const btnCancelar = document.createElement('button');
            btnCancelar.textContent = 'CANCELAR';
            btnCancelar.style.cssText = 'font-size:18px;padding:10px 28px;background:#ff4444;border:none;cursor:pointer;font-family:monospace;color:#fff;';

            btnRow.appendChild(btnOk);
            btnRow.appendChild(btnCancelar);
            overlay.appendChild(titulo);
            overlay.appendChild(input);
            overlay.appendChild(btnRow);
            document.body.appendChild(overlay);

            // Forzar mayúsculas mientras escribe
            input.addEventListener('input', () => {
                const pos = input.selectionStart;
                input.value = input.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
                input.setSelectionRange(pos, pos);
            });

            const confirmar = () => {
                const codigo = input.value.trim().toUpperCase();
                document.body.removeChild(overlay);
                resolve(codigo || null);
            };
            const cancelar = () => {
                document.body.removeChild(overlay);
                resolve(null);
            };

            btnOk.addEventListener('click', confirmar);
            btnCancelar.addEventListener('click', cancelar);
            input.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') confirmar();
                if (e.key === 'Escape') cancelar();
            });

            setTimeout(() => input.focus(), 50);
        });
    }
}
