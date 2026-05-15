import BaseScene from "./BaseScene.js";

const PALO  = { Oro: '★', Espada: '†', Copa: '♦', Basto: '♣' };
const API   = '/api/truco';
const FONT  = '"Jersey 10"';

const BG_SIDE  = 0x12100a;
const BG_MESA  = 0x0c4a0c;
const BG_MESA2 = 0x094009;
const DIVIDER  = 0x5a4020;
const CARTA    = 0xf5e6c8;
const REVERSO  = 0x7a1a1a;
const VACIA    = 0x1a6e1a;

// Zonas del tanteador: Malas (0-15) y Buenas (16-30)
// Se ubican a la DERECHA de los labels "Vos:" / "Máq:"
const XZ1 = 1145;   // inicio zona Malas
const XZ2 = 1200;   // inicio zona Buenas

export default class TrucoSoloScene extends BaseScene {
    constructor() { super('TrucoSoloScene'); }

    init(data) {
        this.playerSprite = data.playerSprite || 'nene-hacha';
        this.mano = null;
        this._btnObjs = [];
        // Estado previo para detectar respuesta de la máquina
        this._prevEstadoTruco  = null;
        this._prevEstadoEnvido = null;
        this._prevPendTru  = false;
        this._prevPendEnv  = false;
        this._bubbleTimer  = null;
        this._loading       = false;
        this._gameOverShown = false;
    }

    async create() {
        this.cameras.main.setBackgroundColor('#0a0805');
        this._buildLayout();
        this._buildErrorToast();
        this._buildGameOverOverlay();
        await this._call('nueva-mano', {});
    }

    // ─── LAYOUT ──────────────────────────────────────────────────
    _buildLayout() {
        const W = 1280, H = 720;

        this.add.rectangle(100,  H/2, 200, H, BG_SIDE).setDepth(0);
        this.add.rectangle(1180, H/2, 200, H, BG_SIDE).setDepth(0);
        this.add.rectangle(200,  H/2, 2, H, DIVIDER).setDepth(1);
        this.add.rectangle(1080, H/2, 2, H, DIVIDER).setDepth(1);

        this.add.rectangle(640, H/2, 880, H,   BG_MESA).setDepth(0);
        this.add.rectangle(640, H/2, 820, 660, BG_MESA2).setStrokeStyle(3, 0x062806).setDepth(0);

        this._yBtns = 18;
        this._buildRightPanel();
        this._buildCenter();
    }

    _buildRightPanel() {
        const xL = 1090, xC = 1180;
        const F  = { fontFamily: FONT };

        // ── TANTEADOR ──
        this.add.text(xC, 14, 'TANTEADOR', { ...F, fontSize: '16px', fill: '#ddaa55' }).setOrigin(0.5, 0).setDepth(3);

        // Cabeceras de zona — centradas sobre sus respectivas zonas de palitos
        // Zona 1 (Malas): XZ1=1145, 15 palitos → ~51px → centro ≈ 1170
        // Zona 2 (Buenas): XZ2=1200, 15 palitos → ~51px → centro ≈ 1225
        this.add.text(1170, 33, 'Malas',  { ...F, fontSize: '10px', fill: '#888888' }).setOrigin(0.5, 0).setDepth(3);
        this.add.text(1228, 33, 'Buenas', { ...F, fontSize: '10px', fill: '#888888' }).setOrigin(0.5, 0).setDepth(3);

        // Labels jugadores a la izquierda; palitos arrancan en XZ1 (a la derecha)
        this._txtVos = this.add.text(xL, 50, 'Vos: 0', { ...F, fontSize: '13px', fill: '#aaffaa' }).setDepth(3);
        this._txtMaq = this.add.text(xL, 73, 'Máq: 0', { ...F, fontSize: '13px', fill: '#ffaaaa' }).setDepth(3);

        // Graphics para los palitos (se limpia y redibuja en cada update)
        this._gTally = this.add.graphics().setDepth(4);

        this.add.rectangle(xC, 100, 165, 1, DIVIDER).setDepth(3);

        // ── ENVIDO ──
        this.add.text(xL, 106, 'ENVIDO', { ...F, fontSize: '14px', fill: '#55aadd' }).setDepth(3);
        this._pEnvido = this.add.text(xL, 124, 'Todavía no se cantó envido.',
            { ...F, fontSize: '12px', fill: '#bbbbbb', wordWrap: { width: 178 } }).setDepth(3);

        this.add.rectangle(xC, 320, 165, 1, DIVIDER).setDepth(3);

        // ── TRUCO ──
        this.add.text(xL, 326, 'TRUCO', { ...F, fontSize: '14px', fill: '#ddcc44' }).setDepth(3);
        this._pTruco = this.add.text(xL, 344, 'Todavía no se cantó truco.',
            { ...F, fontSize: '12px', fill: '#bbbbbb', wordWrap: { width: 178 } }).setDepth(3);

        this.add.rectangle(xC, 665, 165, 1, DIVIDER).setDepth(3);
        const bk = this.add.rectangle(xC, 692, 165, 34, 0x221810).setStrokeStyle(1, DIVIDER).setDepth(3).setInteractive();
        this.add.text(xC, 692, 'Volver', { fontFamily: FONT, fontSize: '16px', fill: '#aa6633' }).setOrigin(0.5).setDepth(4);
        bk.on('pointerover', () => bk.setFillStyle(0x332818));
        bk.on('pointerout',  () => bk.setFillStyle(0x221810));
        bk.on('pointerdown', () => this.scene.start('GameScene', { playerSprite: this.playerSprite }));
    }

    _buildCenter() {
        const F = { fontFamily: FONT };

        // Retrato del rival (troll de frente, frame 312)
        this._sprRetrato = this.add.sprite(640, 52, 'troll').setFrame(312).setDisplaySize(76, 76).setDepth(5);
        this._lblPortrait = this.add.text(640, 100, 'Esperando mano',
            { ...F, fontSize: '14px', fill: '#aaaaaa', backgroundColor: '#00000066', padding: { x: 6, y: 2 } })
            .setOrigin(0.5).setDepth(5);
        this._lblTurno = this.add.text(640, 124, '',
            { ...F, fontSize: '17px', fill: '#ffff88', backgroundColor: '#00000066', padding: { x: 8, y: 3 } })
            .setOrigin(0.5).setDepth(5);

        // ── Burbuja de diálogo del rival ──
        this._bubbleBg   = this.add.rectangle(800, 45, 220, 48, 0xfffff0)
                               .setStrokeStyle(2, 0x333333).setDepth(20).setVisible(false);
        this._bubbleTxt  = this.add.text(800, 45, '',
                               { fontFamily: FONT, fontSize: '15px', fill: '#111111', align: 'center', wordWrap: { width: 205 } })
                               .setOrigin(0.5).setDepth(21).setVisible(false);
        this._bubbleTail = this.add.triangle(692, 52, 0, -8, 14, 8, 0, 8, 0xfffff0)
                               .setStrokeStyle(1, 0x333333).setDepth(19).setVisible(false);

        // Cartas del oponente (reverso)
        this.add.text(640, 155, 'OPONENTE', { ...F, fontSize: '14px', fill: '#ffaaaa' }).setOrigin(0.5).setDepth(5);
        this._cartsOp = [];
        [500, 610, 720].forEach(x => {
            const r = this.add.rectangle(x, 208, 72, 98, REVERSO).setStrokeStyle(2, 0xdddddd).setDepth(5);
            const l = this.add.text(x, 208, '?', { ...F, fontSize: '28px', fill: '#fff' }).setOrigin(0.5).setDepth(6);
            this._cartsOp.push({ r, l });
        });

        // Mesa / bazas
        this.add.line(640, 378, -320, 0, 320, 0, 0x4a9a4a, 1).setDepth(4);
        this.add.text(342, 360, 'Vos:', { ...F, fontSize: '14px', fill: '#aaffaa' }).setDepth(5);
        this.add.text(342, 385, 'Máq:', { ...F, fontSize: '14px', fill: '#ffaaaa' }).setDepth(5);

        this._slots = [];
        [430, 580, 730].forEach(x => {
            const rV = this.add.rectangle(x, 352, 120, 44, VACIA).setStrokeStyle(1, 0x4aaa4a).setDepth(5);
            const tV = this.add.text(x, 352, '', { ...F, fontSize: '13px', fill: '#111', align: 'center' }).setOrigin(0.5).setDepth(6);
            const rM = this.add.rectangle(x, 402, 120, 44, VACIA).setStrokeStyle(1, 0xaa4a4a).setDepth(5);
            const tM = this.add.text(x, 402, '', { ...F, fontSize: '13px', fill: '#111', align: 'center' }).setOrigin(0.5).setDepth(6);
            const tG = this.add.text(x, 377, '', { ...F, fontSize: '13px', fill: '#ffff00', backgroundColor: '#00000099', padding: { x: 4, y: 2 } }).setOrigin(0.5).setDepth(7);
            this._slots.push({ rV, tV, rM, tM, tG });
        });

        // Mis cartas
        this.add.text(640, 463, 'TUS CARTAS', { ...F, fontSize: '14px', fill: '#aaffaa' }).setOrigin(0.5).setDepth(5);
        this._misCarts = [];
        [500, 610, 720].forEach(x => {
            const r = this.add.rectangle(x, 560, 84, 120, CARTA).setStrokeStyle(2, 0x8b6914).setDepth(5).setInteractive();
            const n = this.add.text(x, 530, '', { ...F, fontSize: '28px', fill: '#000' }).setOrigin(0.5).setDepth(6);
            const p = this.add.text(x, 562, '', { ...F, fontSize: '13px', fill: '#333' }).setOrigin(0.5).setDepth(6);
            const v = this.add.text(x, 579, '', { ...F, fontSize: '11px', fill: '#666' }).setOrigin(0.5).setDepth(6);
            const idx = this._misCarts.length;
            r.on('pointerover', () => { if (r.visible) { r.setFillStyle(0xffe8a0); r.y=552; n.y=522; p.y=554; v.y=571; }});
            r.on('pointerout',  () => { if (r.visible) { r.setFillStyle(CARTA);    r.y=560; n.y=530; p.y=562; v.y=579; }});
            r.on('pointerdown', () => this._jugarCarta(idx));
            this._misCarts.push({ r, n, p, v, carta: null });
        });
    }

    // ─── TANTEADOR: palitos ───────────────────────────────────────
    _redrawTally(ptsVos, ptsMaq) {
        this._gTally.clear();
        const yV = 50, yM = 73, H = 13;

        this._drawPalitos(Math.min(ptsVos, 15),     XZ1, yV, H, 0x88ff88);
        this._drawPalitos(Math.max(0, ptsVos - 15), XZ2, yV, H, 0x44cc44);
        this._drawPalitos(Math.min(ptsMaq, 15),     XZ1, yM, H, 0xff8888);
        this._drawPalitos(Math.max(0, ptsMaq - 15), XZ2, yM, H, 0xcc4444);

        // Separador entre zona Malas y zona Buenas
        const sep = XZ2 - 4;
        this._gTally.lineStyle(1, 0x5a4020, 1);
        this._gTally.beginPath();
        this._gTally.moveTo(sep, yV - 4);
        this._gTally.lineTo(sep, yM + H + 4);
        this._gTally.strokePath();
    }

    _drawPalitos(count, xStart, y, h, color) {
        if (count <= 0) return;
        this._gTally.lineStyle(2, color, 1);
        let cx = xStart, gs = xStart;
        for (let i = 0; i < count; i++) {
            const p = i % 5;
            if (p === 0) gs = cx;
            if (p === 4) {
                // Diagonal del grupo de 5
                this._gTally.beginPath();
                this._gTally.moveTo(gs - 2, y + h + 2);
                this._gTally.lineTo(cx + 1,  y - 2);
                this._gTally.strokePath();
                cx += 7; // espacio entre grupos
            } else {
                this._gTally.beginPath();
                this._gTally.moveTo(cx, y);
                this._gTally.lineTo(cx, y + h);
                this._gTally.strokePath();
                cx += 3;
            }
        }
    }

    // ─── BUBBLE HELPERS ──────────────────────────────────────────
    _showBubble(txt) {
        this._bubbleTxt.setText(txt);
        this._bubbleBg.setVisible(true);
        this._bubbleTxt.setVisible(true);
        this._bubbleTail.setVisible(true);
    }

    _hideBubble() {
        this._bubbleBg.setVisible(false);
        this._bubbleTxt.setVisible(false);
        this._bubbleTail.setVisible(false);
    }

    // Muestra la burbuja y la oculta automáticamente después de `ms` milisegundos
    _showTempBubble(txt, ms) {
        if (this._bubbleTimer) { this._bubbleTimer.remove(); this._bubbleTimer = null; }
        this._showBubble(txt);
        this._bubbleTimer = this.time.delayedCall(ms, () => {
            this._bubbleTimer = null;
            this._hideBubble();
        }, [], this);
    }

    // Devuelve el texto corto para la burbuja cuando la máquina canta
    _cantoBubbleText(m, pendTru, pendEnv) {
        if (pendTru) {
            if (m.nivelTruco === 2) return '¡Retruco!';
            if (m.nivelTruco === 3) return '¡Vale Cuatro!';
            return '¡Truco!';
        }
        if (pendEnv) {
            const s = (m.estadoEnvido || '').toLowerCase();
            if (s.includes('falta')) return '¡Falta Envido!';
            if (s.includes('real'))  return '¡Real Envido!';
            return '¡Envido!';
        }
        return null;
    }

    // ─── ERROR TOAST ─────────────────────────────────────────────
    _buildErrorToast() {
        this._toastBg  = this.add.rectangle(640, 670, 700, 40, 0x880000).setStrokeStyle(1, 0xff4444).setDepth(50).setVisible(false);
        this._toastTxt = this.add.text(640, 670, '', { fontFamily: FONT, fontSize: '14px', fill: '#ffcccc', align: 'center' }).setOrigin(0.5).setDepth(51).setVisible(false);
        this._toastTimer = null;
    }

    _showToast(msg) {
        if (this._toastTimer) { this._toastTimer.remove(); this._toastTimer = null; }
        this._toastTxt.setText(msg);
        this._toastBg.setVisible(true);
        this._toastTxt.setVisible(true);
        this._toastTimer = this.time.delayedCall(4000, () => {
            this._toastBg.setVisible(false);
            this._toastTxt.setVisible(false);
            this._toastTimer = null;
        }, [], this);
    }

    // ─── API ──────────────────────────────────────────────────────
    async _call(endpoint, body) {
        if (this._loading) return;
        this._loading = true;
        try {
            const res = await fetch(`${API}/${endpoint}`, {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!res.ok) {
                const errMsg = await res.text();
                console.warn('[TrucoSolo]', endpoint, errMsg);
                this._showToast(`Error en ${endpoint}: ${errMsg}`);
                return;
            }
            this.mano = await res.json();
            this._updateUI(this.mano);
        } finally {
            this._loading = false;
        }
    }

    async _jugarCarta(i) {
        const s = this._misCarts[i];
        if (!s.carta || !this.mano) return;
        await this._call('jugar-carta', { manoId: this.mano.id, numero: s.carta.numero, palo: s.carta.palo });
    }

    // ─── GAME OVER OVERLAY ────────────────────────────────────────
    _buildGameOverOverlay() {
        const cx = 640, cy = 360, W = 960, H = 540, d = 80;

        // Panel de fondo semitransparente
        this._goBg = this.add.rectangle(cx, cy, W, H, 0x060403)
            .setAlpha(0.92).setStrokeStyle(4, 0x5a3010).setDepth(d).setVisible(false).setInteractive();

        // Título grande
        this._goTitle = this.add.text(cx, cy - 130, '',
            { fontFamily: FONT, fontSize: '96px', fill: '#ffdd44' })
            .setOrigin(0.5).setDepth(d + 1).setVisible(false);

        // Subtítulo
        this._goSubtitle = this.add.text(cx, cy - 20, '',
            { fontFamily: FONT, fontSize: '26px', fill: '#cccccc' })
            .setOrigin(0.5).setDepth(d + 1).setVisible(false);

        // Decoración — línea horizontal
        this._goLine = this.add.rectangle(cx, cy + 40, 500, 2, 0x5a3010)
            .setDepth(d + 1).setVisible(false);

        // Botón 1: Volver a jugar
        this._goBtn1Bg = this.add.rectangle(cx - 140, cy + 120, 240, 54, 0x1a4a1a)
            .setStrokeStyle(2, 0x44bb44).setDepth(d + 1).setVisible(false).setInteractive();
        this._goBtn1Txt = this.add.text(cx - 140, cy + 120, 'Volver a jugar',
            { fontFamily: FONT, fontSize: '22px', fill: '#66ee66' })
            .setOrigin(0.5).setDepth(d + 2).setVisible(false);
        this._goBtn1Bg.on('pointerover', () => this._goBtn1Bg.setFillStyle(0x286628));
        this._goBtn1Bg.on('pointerout',  () => this._goBtn1Bg.setFillStyle(0x1a4a1a));
        this._goBtn1Bg.on('pointerdown', () => {
            this._hideGameOver();
            this._call('nueva-partida', {});
        });

        // Botón 2: Salir
        this._goBtn2Bg = this.add.rectangle(cx + 140, cy + 120, 240, 54, 0x4a1a1a)
            .setStrokeStyle(2, 0xbb4444).setDepth(d + 1).setVisible(false).setInteractive();
        this._goBtn2Txt = this.add.text(cx + 140, cy + 120, 'Salir al mapa',
            { fontFamily: FONT, fontSize: '22px', fill: '#ee6666' })
            .setOrigin(0.5).setDepth(d + 2).setVisible(false);
        this._goBtn2Bg.on('pointerover', () => this._goBtn2Bg.setFillStyle(0x662828));
        this._goBtn2Bg.on('pointerout',  () => this._goBtn2Bg.setFillStyle(0x4a1a1a));
        this._goBtn2Bg.on('pointerdown', () => this.scene.start('GameScene', { playerSprite: this.playerSprite }));
    }

    _showGameOver(humanWon) {
        if (this._gameOverShown) return;
        this._gameOverShown = true;
        this._goTitle.setText(humanWon ? '¡Ganaste!' : '¡Perdiste!');
        this._goTitle.setStyle({ fill: humanWon ? '#ffdd44' : '#ff6633', fontSize: '96px', fontFamily: FONT });
        this._goSubtitle.setText(humanWon ? '¡Llegaste a 30 puntos! El troll cayó derrotado.' : 'El troll llegó a 30 puntos. ¡La próxima será!');
        const objs = [this._goBg, this._goTitle, this._goSubtitle, this._goLine,
                      this._goBtn1Bg, this._goBtn1Txt, this._goBtn2Bg, this._goBtn2Txt];
        objs.forEach(o => o.setVisible(true));
    }

    _hideGameOver() {
        if (!this._gameOverShown) return;
        this._gameOverShown = false;
        const objs = [this._goBg, this._goTitle, this._goSubtitle, this._goLine,
                      this._goBtn1Bg, this._goBtn1Txt, this._goBtn2Bg, this._goBtn2Txt];
        objs.forEach(o => o.setVisible(false));
    }

    // ─── UI ───────────────────────────────────────────────────────
    _updateUI(m) {
        // Tanteador
        this._txtVos.setText(`Vos: ${m.puntosHumano}`);
        this._txtMaq.setText(`Máq: ${m.puntosMaquina}`);
        this._redrawTally(m.puntosHumano, m.puntosMaquina);

        // ── Game Over overlay ─────────────────────────────────────
        if (m.ganadorPartida) {
            this._showGameOver(m.ganadorPartida === 'Humano');
            return; // no actualizar nada más mientras se muestra el cartel
        } else {
            this._hideGameOver();
        }

        // Paneles de estado
        this._pEnvido.setText(m.estadoEnvido || 'Todavía no se cantó envido.');
        this._pTruco.setText(m.estadoTruco   || 'Todavía no se cantó truco.');

        // Retrato label
        if (m.ganadorPartida)
            this._lblPortrait.setText(m.ganadorPartida === 'Humano' ? '¡PERDÍ! 😤' : '¡GANÉ! 😈');
        else if (m.ganadorMano)
            this._lblPortrait.setText(m.ganadorMano === 'Humano' ? '¡Perdí la mano!' : '¡Gané la mano!');
        else
            this._lblPortrait.setText(m.turnoActual === 'Maquina' ? 'Pensando...' : '...');

        // Flags de turno
        const pendEnv  = !!m.envidoPendienteRespuestaHumano;
        const pendTru  = !!m.trucoPendienteRespuestaHumano;
        const esMiTurno = (m.turnoActual === 'Humano' || !!m.cartaMaquinaEnMesa)
            && !m.ganadorMano && !m.ganadorPartida && !pendEnv && !pendTru;

        this._lblTurno.setText(
            esMiTurno        ? '⭐ Tu turno — jugá una carta o cantá'
            : pendEnv || pendTru ? '⚡ Respondé el canto de la máquina'
            : ''
        );

        // ── Burbuja de diálogo ──────────────────────────────────
        if (pendTru || pendEnv) {
            // La máquina cantó → burbuja persistente hasta que el humano responda
            if (this._bubbleTimer) { this._bubbleTimer.remove(); this._bubbleTimer = null; }
            const txt = this._cantoBubbleText(m, pendTru, pendEnv);
            if (txt) this._showBubble(txt);
        } else {
            // Detectar si la máquina acaba de RESPONDER a un canto del humano
            const trucoChanged  = m.estadoTruco  !== this._prevEstadoTruco;
            const envidoChanged = m.estadoEnvido !== this._prevEstadoEnvido;

            let responseText = null;

            if (trucoChanged && m.trucoCantado && !this._prevPendTru) {
                // Estado del truco cambió y no venía de un pendiente nuestro → máquina respondió
                const t = (m.estadoTruco || '').toLowerCase();
                if      (t.includes('no quiso') || t.includes('no quiere')) responseText = '¡No quiero!';
                else if (t.includes('quiso')    || t.includes('quiere') || t.includes('acepto')) responseText = '¡Quiero!';
            } else if (envidoChanged && m.envidoCantado && !this._prevPendEnv) {
                const e = (m.estadoEnvido || '').toLowerCase();
                if      (e.includes('no quiso') || e.includes('no quiere')) responseText = '¡No quiero!';
                else if (e.includes('quiso')    || e.includes('quiere') || e.includes('acepto')) responseText = '¡Quiero!';
            }

            if (responseText) {
                this._showTempBubble(responseText, 2500);
            } else if (!this._bubbleTimer) {
                // Sin burbuja activa ni timer → ocultar
                this._hideBubble();
            }
        }

        // Guardar estado previo para el próximo ciclo
        this._prevEstadoTruco  = m.estadoTruco;
        this._prevEstadoEnvido = m.estadoEnvido;
        this._prevPendTru = pendTru;
        this._prevPendEnv = pendEnv;

        // Cartas oponente (reverso)
        const cantOp = m.maquina?.mano?.length ?? 0;
        this._cartsOp.forEach((c, i) => { const v = i < cantOp; c.r.setVisible(v); c.l.setVisible(v); });

        // Bazas
        this._slots.forEach(s => {
            s.rV.setFillStyle(VACIA); s.tV.setText('');
            s.rM.setFillStyle(VACIA); s.tM.setText('');
            s.tG.setText('');
        });
        (m.bazas || []).forEach((b, i) => {
            if (i >= 3) return;
            const s = this._slots[i];
            if (b.cartaJugador) { s.tV.setText(`${b.cartaJugador.numero} ${PALO[b.cartaJugador.palo]||''} ${b.cartaJugador.palo}`); s.rV.setFillStyle(0xd4c4a0); }
            if (b.cartaMaquina)  { s.tM.setText(`${b.cartaMaquina.numero} ${PALO[b.cartaMaquina.palo]||''} ${b.cartaMaquina.palo}`);  s.rM.setFillStyle(0xd4c4a0); }
            const gan = b.ganador === 'Humano' ? '→Vos' : b.ganador === 'Maquina' ? '→Máq' : 'Parda';
            const col = b.ganador === 'Humano' ? '#aaffaa' : b.ganador === 'Maquina' ? '#ffaaaa' : '#ffff88';
            s.tG.setText(gan).setStyle({ fill: col });
        });
        if (m.cartaMaquinaEnMesa) {
            const idx = (m.bazas || []).length;
            if (idx < 3) {
                const c = m.cartaMaquinaEnMesa;
                this._slots[idx].tM.setText(`${c.numero} ${PALO[c.palo]||''} ${c.palo}`);
                this._slots[idx].rM.setFillStyle(0xffe880);
            }
        }

        // Mis cartas
        this._misCarts.forEach((sl, i) => {
            const carta = m.humano?.mano?.[i];
            sl.carta = carta || null;
            const vis = !!carta && !m.ganadorMano;
            [sl.r, sl.n, sl.p, sl.v].forEach(o => o.setVisible(vis));
            if (carta) {
                sl.n.setText(String(carta.numero));
                sl.p.setText(`${PALO[carta.palo]||''} ${carta.palo}`);
                sl.v.setText(`Truco: ${carta.valorTruco}`);
            }
        });

        this._buildBtns(m, esMiTurno, pendEnv, pendTru);
    }

    _buildBtns(m, esMiTurno, pendEnv, pendTru) {
        this._btnObjs.forEach(o => o.destroy());
        this._btnObjs = [];

        const manoEnd       = !!m.ganadorMano || !!m.ganadorPartida;
        const trucoCantado  = !!m.trucoCantado;
        const trucoResuelto = !!m.trucoResuelto;
        const btns = []; // [label, activeColor, callback | null]

        if (m.partidaTerminada) {
            btns.push(['Nueva partida', '#226622', () => this._call('nueva-partida', {})]);

        } else if (m.ganadorMano) {
            btns.push(['Nueva mano', '#cc8800', () => this._call('nueva-mano', { manoAnteriorId: m.id })]);

        } else if (pendEnv) {
            btns.push(['QUIERO',    '#44ff44', () => this._call('responder-envido', { manoId: m.id, aceptar: true  })]);
            btns.push(['NO QUIERO', '#ff4444', () => this._call('responder-envido', { manoId: m.id, aceptar: false })]);

        } else if (pendTru) {
            btns.push(['QUIERO',    '#44ff44', () => this._call('responder-truco', { manoId: m.id, aceptar: true })]);
            if (m.nivelTruco < 3) {
                const lbl = m.nivelTruco === 1 ? 'RETRUCO' : 'VALE 4';
                const esc = m.nivelTruco === 1 ? 'retruco' : 'valecuatro';
                btns.push([lbl, '#ffaa00', () => this._call('responder-truco', { manoId: m.id, aceptar: true, escalarA: esc })]);
            }
            btns.push(['NO QUIERO', '#ff4444', () => this._call('responder-truco', { manoId: m.id, aceptar: false })]);

        } else {
            // ── Estado normal: mostrar botones de canto habilitados/deshabilitados ──

            // ENVIDO — solo válido antes de la primera baza
            const envOk = !m.envidoCantado && (m.bazas?.length ?? 0) === 0 && esMiTurno && !manoEnd;
            btns.push(['Envido',       '#4488ff', envOk ? () => this._call('cantar-envido-tipo', { manoId: m.id, tipo: 'Envido'       }) : null]);
            btns.push(['Real Envido',  '#4488ff', envOk ? () => this._call('cantar-envido-tipo', { manoId: m.id, tipo: 'Real Envido'  }) : null]);
            btns.push(['Falta Envido', '#4488ff', envOk ? () => this._call('cantar-envido-tipo', { manoId: m.id, tipo: 'Falta Envido' }) : null]);

            // TRUCO / RETRUCO / VALE CUATRO
            if (!trucoCantado) {
                // Truco no cantado aún → mostrar botón Truco (inhabilitado si no es mi turno)
                const ok = esMiTurno && !manoEnd;
                btns.push(['Truco', '#cc4444', ok ? () => this._call('cantar-truco', { manoId: m.id }) : null]);
            } else if (!trucoResuelto && m.nivelTruco < 3) {
                // Truco aceptado, se puede escalar → mostrar escalación (inhabilitada si no es mi turno)
                const lbl = m.nivelTruco === 1 ? 'Retruco' : 'Vale Cuatro';
                const ok  = esMiTurno && !manoEnd;
                btns.push([lbl, '#cc4444', ok ? () => this._call('escalar-truco', { manoId: m.id }) : null]);
            }
            // Si trucoCantado && trucoResuelto: la mano debería haber terminado (no mostrar nada de truco)

            // IR AL MAZO — solo visible cuando es mi turno
            if (esMiTurno && !manoEnd) {
                btns.push(['Ir al mazo', '#556677', () => this._call('irse-al-mazo', { manoId: m.id })]);
            }
        }

        // ── Renderizar botones (con soporte para estado inhabilitado) ──
        let y = this._yBtns;
        btns.forEach(([label, color, cb]) => {
            const enabled = !!cb;
            const bg = this.add.rectangle(100, y + 17, 172, 34,
                enabled ? 0x222222 : 0x161616)
                .setStrokeStyle(1, enabled ? 0x444444 : 0x252525)
                .setDepth(3);
            const txt = this.add.text(100, y + 17, label,
                { fontFamily: FONT, fontSize: '15px', fill: enabled ? color : '#444444' })
                .setOrigin(0.5).setDepth(4);
            if (enabled) {
                bg.setInteractive();
                bg.on('pointerover', () => bg.setFillStyle(0x333333));
                bg.on('pointerout',  () => bg.setFillStyle(0x222222));
                bg.on('pointerdown', cb);
            }
            this._btnObjs.push(bg, txt);
            y += 40;
        });
    }
}
