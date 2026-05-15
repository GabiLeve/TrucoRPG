import BaseScene from "./BaseScene.js";
import { multiplayerManager } from "../MultiplayerManager.js";

const PALO   = { Oro: '★', Espada: '†', Copa: '♦', Basto: '♣' };
const FONT   = '"Jersey 10"';

// Paleta — idéntica a TrucoSoloScene
const BG_SIDE  = 0x12100a;
const BG_MESA  = 0x0c4a0c;
const BG_MESA2 = 0x094009;
const DIVIDER  = 0x5a4020;
const CARTA    = 0xf5e6c8;
const REVERSO  = 0x7a1a1a;
const VACIA    = 0x1a6e1a;

export default class TrucoMultiScene extends BaseScene {
    constructor() { super('TrucoMultiScene'); }

    init(data) {
        this.miRol = data.miRol || 'J1';
        this._btnObjs = [];
    }

    create() {
        this.cameras.main.setBackgroundColor('#0a0805');
        this._buildLayout();

        multiplayerManager.onTrucoEstado = (data) => this._updateUI(data);
        multiplayerManager.onJugadorDesconectado = () => {
            this._pTruco.setText('El otro jugador se desconectó.');
        };

        if (multiplayerManager.esHost) multiplayerManager.iniciarTruco();

        this.events.on('shutdown', () => { multiplayerManager.onTrucoEstado = null; });
    }

    // ─── LAYOUT ──────────────────────────────────────────────────
    _buildLayout() {
        const W = 1280, H = 720;

        // Sidebars
        this.add.rectangle(100,  H/2, 200, H, BG_SIDE).setDepth(0);
        this.add.rectangle(1180, H/2, 200, H, BG_SIDE).setDepth(0);
        this.add.rectangle(200,  H/2, 2,   H, DIVIDER).setDepth(1);
        this.add.rectangle(1080, H/2, 2,   H, DIVIDER).setDepth(1);

        // Mesa central
        this.add.rectangle(640, H/2, 880, H,   BG_MESA).setDepth(0);
        this.add.rectangle(640, H/2, 820, 660, BG_MESA2).setStrokeStyle(3, 0x062806).setDepth(0);

        this._yBtns = 18;
        this._buildRightPanel();
        this._buildCenter();
    }

    _buildRightPanel() {
        const xL = 1090, xC = 1180;
        const F  = { fontFamily: FONT };

        // ── TANTEADOR con palitos ──
        this.add.text(xC, 14, 'TANTEADOR', { ...F, fontSize: '16px', fill: '#ddaa55' }).setOrigin(0.5, 0).setDepth(3);

        // Etiquetas "Malas" y "Buenas"
        const xZ1 = 1105, xZ2 = 1105 + 64;
        this.add.text((xZ1 + xZ1+56)/2, 33, 'Malas',  { ...F, fontSize: '10px', fill: '#888888' }).setOrigin(0.5, 0).setDepth(3);
        this.add.text((xZ2 + xZ2+56)/2, 33, 'Buenas', { ...F, fontSize: '10px', fill: '#888888' }).setOrigin(0.5, 0).setDepth(3);

        // Labels jugadores
        this._txtJ1 = this.add.text(xL, 50, 'J1: 0', { ...F, fontSize: '13px', fill: '#aaffaa' }).setDepth(3);
        this._txtJ2 = this.add.text(xL, 73, 'J2: 0', { ...F, fontSize: '13px', fill: '#ffaaaa' }).setDepth(3);

        // Tally graphics (se redibuja en cada update)
        this._gTally = this.add.graphics().setDepth(4);

        this.add.rectangle(xC, 100, 165, 1, DIVIDER).setDepth(3);

        // ── ENVIDO ──
        this.add.text(xL, 106, 'ENVIDO', { ...F, fontSize: '14px', fill: '#55aadd' }).setDepth(3);
        this._pEnvido = this.add.text(xL, 124, 'Todavía no se cantó envido.', { ...F, fontSize: '12px', fill: '#bbbbbb', wordWrap: { width: 178 } }).setDepth(3);

        this.add.rectangle(xC, 320, 165, 1, DIVIDER).setDepth(3);

        // ── TRUCO ──
        this.add.text(xL, 326, 'TRUCO', { ...F, fontSize: '14px', fill: '#ddcc44' }).setDepth(3);
        this._pTruco = this.add.text(xL, 344, 'Todavía no se cantó truco.', { ...F, fontSize: '12px', fill: '#bbbbbb', wordWrap: { width: 178 } }).setDepth(3);

        this.add.rectangle(xC, 665, 165, 1, DIVIDER).setDepth(3);
        const bk = this.add.rectangle(xC, 692, 165, 34, 0x221810).setStrokeStyle(1, DIVIDER).setDepth(3).setInteractive();
        this.add.text(xC, 692, 'Volver', { fontFamily: FONT, fontSize: '16px', fill: '#aa6633' }).setOrigin(0.5).setDepth(4);
        bk.on('pointerover', () => bk.setFillStyle(0x332818));
        bk.on('pointerout',  () => bk.setFillStyle(0x221810));
        bk.on('pointerdown', () => {
            multiplayerManager.limpiarCallbacks();
            this.scene.start('GameScene', { multijugador: true });
        });
    }

    _buildCenter() {
        const F = { fontFamily: FONT };

        // Retrato oponente (troll de frente, frame 312)
        this._sprRetrato = this.add.sprite(640, 52, 'troll').setFrame(312).setDisplaySize(76, 76).setDepth(5);
        this._lblPortrait = this.add.text(640, 100, 'Esperando mano', { ...F, fontSize: '14px', fill: '#aaaaaa', backgroundColor: '#00000066', padding: { x: 6, y: 2 } }).setOrigin(0.5).setDepth(5);
        this._lblTurno    = this.add.text(640, 124, '', { ...F, fontSize: '17px', fill: '#ffff88', backgroundColor: '#00000066', padding: { x: 8, y: 3 } }).setOrigin(0.5).setDepth(5);

        // ── Burbuja de diálogo del rival ──
        this._bubbleBg  = this.add.rectangle(800, 45, 220, 48, 0xfffff0).setStrokeStyle(2, 0x333333).setDepth(20).setVisible(false);
        this._bubbleTxt = this.add.text(800, 45, '', { fontFamily: FONT, fontSize: '15px', fill: '#111111', align: 'center', wordWrap: { width: 205 } }).setOrigin(0.5).setDepth(21).setVisible(false);
        this._bubbleTail = this.add.triangle(692, 52, 0, -8, 14, 8, 0, 8, 0xfffff0).setStrokeStyle(1, 0x333333).setDepth(19).setVisible(false);

        // Cartas oponente
        this.add.text(640, 155, 'OPONENTE', { ...F, fontSize: '14px', fill: '#ffaaaa' }).setOrigin(0.5).setDepth(5);
        this._cartsOp = [];
        [500, 610, 720].forEach(x => {
            const r = this.add.rectangle(x, 208, 72, 98, REVERSO).setStrokeStyle(2, 0xdddddd).setDepth(5);
            const l = this.add.text(x, 208, '?', { ...F, fontSize: '28px', fill: '#fff' }).setOrigin(0.5).setDepth(6);
            this._cartsOp.push({ r, l });
        });

        // Mesa
        this.add.line(640, 378, -320, 0, 320, 0, 0x4a9a4a, 1).setDepth(4);
        this.add.text(342, 360, 'J1:', { ...F, fontSize: '14px', fill: '#aaffaa' }).setDepth(5);
        this.add.text(342, 385, 'J2:', { ...F, fontSize: '14px', fill: '#ffaaaa' }).setDepth(5);

        this._slots = [];
        [430, 580, 730].forEach(x => {
            const rJ1 = this.add.rectangle(x, 352, 120, 44, VACIA).setStrokeStyle(1, 0x4aaa4a).setDepth(5);
            const tJ1 = this.add.text(x, 352, '', { ...F, fontSize: '13px', fill: '#111', align: 'center' }).setOrigin(0.5).setDepth(6);
            const rJ2 = this.add.rectangle(x, 402, 120, 44, VACIA).setStrokeStyle(1, 0xaa4a4a).setDepth(5);
            const tJ2 = this.add.text(x, 402, '', { ...F, fontSize: '13px', fill: '#111', align: 'center' }).setOrigin(0.5).setDepth(6);
            const tG  = this.add.text(x, 377, '', { ...F, fontSize: '13px', fill: '#ffff00', backgroundColor: '#00000099', padding: { x: 4, y: 2 } }).setOrigin(0.5).setDepth(7);
            this._slots.push({ rJ1, tJ1, rJ2, tJ2, tG });
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
    _redrawTally(ptsJ1, ptsJ2) {
        this._gTally.clear();
        const xZ1 = 1105, xZ2 = 1105 + 64;
        const yV = 50, yM = 73, H = 13;

        this._drawPalitos(Math.min(ptsJ1, 15),        xZ1, yV, H, 0x88ff88);
        this._drawPalitos(Math.max(0, ptsJ1 - 15),    xZ2, yV, H, 0x44cc44);
        this._drawPalitos(Math.min(ptsJ2, 15),        xZ1, yM, H, 0xff8888);
        this._drawPalitos(Math.max(0, ptsJ2 - 15),    xZ2, yM, H, 0xcc4444);

        // Separador de zonas
        const sep = xZ2 - 5;
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
                // Palito diagonal (5to del grupo)
                this._gTally.beginPath();
                this._gTally.moveTo(gs - 2, y + h + 2);
                this._gTally.lineTo(cx + 1, y - 2);
                this._gTally.strokePath();
                cx += 7; // espacio tras grupo
            } else {
                // Palito vertical
                this._gTally.beginPath();
                this._gTally.moveTo(cx, y);
                this._gTally.lineTo(cx, y + h);
                this._gTally.strokePath();
                cx += 3;
            }
        }
    }

    // ─── SignalR ──────────────────────────────────────────────────
    _jugarCarta(i) {
        const sl = this._misCarts[i];
        if (!sl.carta) return;
        multiplayerManager.jugarCarta(sl.carta.numero, sl.carta.palo);
    }

    // ─── UI ───────────────────────────────────────────────────────
    _updateUI(data) {
        const { miRol, misManos, cantidadCartasOponente, estado: e } = data;
        this.miRol = miRol;
        const esJ1 = miRol === 'J1';

        // Scores + tally
        this._txtJ1.setText(`J1: ${e.puntosHumano}`);
        this._txtJ2.setText(`J2: ${e.puntosMaquina}`);
        this._redrawTally(e.puntosHumano, e.puntosMaquina);

        // Right panels
        this._pEnvido.setText(e.estadoEnvido || 'Todavía no se cantó envido.');
        this._pTruco.setText(e.estadoTruco   || 'Todavía no se cantó truco.');

        const miTurnoRol = esJ1 ? 'Humano' : 'Maquina';
        const pendEnv = esJ1 ? e.envidoPendienteJ1 : e.envidoPendienteJ2;
        const pendTru = esJ1 ? e.trucoPendienteJ1  : e.trucoPendienteJ2;

        // Fix: si el rival jugó primero en la baza (cartaPendiente del rival),
        // el jugador puede responder aunque turnoActual apunte al rival
        const rivalJugoEnMesa = esJ1 ? !!e.cartaPendienteJ2 : !!e.cartaPendienteJ1;
        const esMiTurno = (e.turnoActual === miTurnoRol || rivalJugoEnMesa)
            && !e.ganadorMano && !e.ganadorPartida && !pendEnv && !pendTru;

        // Portrait label
        if (e.ganadorPartida) {
            const perdiste = (esJ1 && e.ganadorPartida === 'Maquina') || (!esJ1 && e.ganadorPartida === 'Humano');
            this._lblPortrait.setText(perdiste ? '¡GANÉ! 😈' : '¡PERDÍ! 😤');
        } else if (e.ganadorMano) {
            const rivalGano = (esJ1 && e.ganadorMano === 'Maquina') || (!esJ1 && e.ganadorMano === 'Humano');
            this._lblPortrait.setText(rivalGano ? '¡Gané la mano!' : '¡Perdí la mano!');
        } else {
            this._lblPortrait.setText(esMiTurno ? '¿Qué jugás?' : 'Esperando...');
        }

        this._lblTurno.setText(
            esMiTurno  ? '⭐ Tu turno — jugá una carta o cantá'
            : pendEnv || pendTru ? '⚡ Respondé el canto del rival'
            : ''
        );

        // ── Burbuja de diálogo del rival ──
        let bubbleTxt = null;
        if (pendTru && e.estadoTruco)   bubbleTxt = e.estadoTruco;
        else if (pendEnv && e.estadoEnvido) bubbleTxt = e.estadoEnvido;

        const showBubble = !!bubbleTxt;
        this._bubbleBg.setVisible(showBubble);
        this._bubbleTxt.setVisible(showBubble);
        this._bubbleTail.setVisible(showBubble);
        if (showBubble) this._bubbleTxt.setText(bubbleTxt);

        // Oponente cards
        this._cartsOp.forEach((c, i) => { const v = i < cantidadCartasOponente; c.r.setVisible(v); c.l.setVisible(v); });

        // Bazas
        this._slots.forEach(s => {
            s.rJ1.setFillStyle(VACIA); s.tJ1.setText('');
            s.rJ2.setFillStyle(VACIA); s.tJ2.setText('');
            s.tG.setText('');
        });
        (e.bazas || []).forEach((b, i) => {
            if (i >= 3) return;
            const s = this._slots[i];
            if (b.cartaJugador) { s.tJ1.setText(`${b.cartaJugador.numero} ${PALO[b.cartaJugador.palo]||''} ${b.cartaJugador.palo}`); s.rJ1.setFillStyle(0xd4c4a0); }
            if (b.cartaMaquina)  { s.tJ2.setText(`${b.cartaMaquina.numero} ${PALO[b.cartaMaquina.palo]||''} ${b.cartaMaquina.palo}`);  s.rJ2.setFillStyle(0xd4c4a0); }
            const gan = b.ganador === 'Humano' ? '→J1' : b.ganador === 'Maquina' ? '→J2' : 'Parda';
            const col = b.ganador === 'Humano' ? '#aaffaa' : b.ganador === 'Maquina' ? '#ffaaaa' : '#ffff88';
            s.tG.setText(gan).setStyle({ fill: col });
        });
        const bazaIdx = (e.bazas || []).length;
        if (bazaIdx < 3) {
            const s = this._slots[bazaIdx];
            if (e.cartaPendienteJ1) { s.tJ1.setText(`${e.cartaPendienteJ1.numero} ${PALO[e.cartaPendienteJ1.palo]||''} ${e.cartaPendienteJ1.palo}`); s.rJ1.setFillStyle(0xffe880); }
            if (e.cartaPendienteJ2) { s.tJ2.setText(`${e.cartaPendienteJ2.numero} ${PALO[e.cartaPendienteJ2.palo]||''} ${e.cartaPendienteJ2.palo}`); s.rJ2.setFillStyle(0xffe880); }
        }

        // Mis cartas
        this._misCarts.forEach((sl, i) => {
            const carta = misManos[i];
            sl.carta = carta || null;
            const vis = !!carta && !e.ganadorMano;
            [sl.r, sl.n, sl.p, sl.v].forEach(o => o.setVisible(vis));
            if (carta) {
                sl.n.setText(String(carta.numero));
                sl.p.setText(`${PALO[carta.palo]||''} ${carta.palo}`);
                sl.v.setText(`Truco: ${carta.valorTruco}`);
            }
        });

        this._buildBtns(e, miRol, esMiTurno, pendEnv, pendTru);
    }

    _buildBtns(e, miRol, esMiTurno, pendEnv, pendTru) {
        this._btnObjs.forEach(o => o.destroy());
        this._btnObjs = [];

        const btns = [];

        if (e.partidaTerminada) {
            btns.push(['Nueva partida', '#226622', () => multiplayerManager.nuevaPartida()]);
        } else if (e.ganadorMano) {
            btns.push(['Nueva mano', '#cc8800', () => multiplayerManager.nuevaMano()]);
        } else if (pendEnv) {
            btns.push(['QUIERO',    '#44ff44', () => multiplayerManager.responderEnvido(true)]);
            btns.push(['NO QUIERO', '#ff4444', () => multiplayerManager.responderEnvido(false)]);
        } else if (pendTru) {
            btns.push(['QUIERO', '#44ff44', () => multiplayerManager.responderTruco(true)]);
            if (e.nivelTruco < 3) {
                const lbl = e.nivelTruco === 1 ? 'RETRUCO' : 'VALE 4';
                const esc = e.nivelTruco === 1 ? 'retruco' : 'valecuatro';
                btns.push([lbl, '#ffaa00', () => multiplayerManager.responderTruco(true, esc)]);
            }
            btns.push(['NO QUIERO', '#ff4444', () => multiplayerManager.responderTruco(false)]);
        } else {
            if (!e.envidoCantado && (e.bazas?.length ?? 0) === 0 && esMiTurno) {
                btns.push(['Envido',       '#4488ff', () => multiplayerManager.solicitarEnvido('Envido')]);
                btns.push(['Real Envido',  '#4488ff', () => multiplayerManager.solicitarEnvido('Real Envido')]);
                btns.push(['Falta Envido', '#4488ff', () => multiplayerManager.solicitarEnvido('Falta Envido')]);
            }
            if (!e.trucoCantado && esMiTurno) {
                btns.push(['Truco', '#cc4444', () => multiplayerManager.solicitarTruco()]);
            } else if (e.trucoCantado && !e.trucoResuelto && e.nivelTruco < 3 && esMiTurno) {
                const lbl = e.nivelTruco === 1 ? 'Retruco' : 'Vale Cuatro';
                btns.push([lbl, '#cc4444', () => multiplayerManager.escalarTruco()]);
            }
            if (esMiTurno)
                btns.push(['Ir al mazo', '#556677', () => multiplayerManager.irseAlMazo()]);
        }

        let y = this._yBtns;
        btns.forEach(([label, color, cb]) => {
            const bg  = this.add.rectangle(100, y+17, 172, 34, 0x222222).setStrokeStyle(1, 0x444444).setDepth(3).setInteractive();
            const txt = this.add.text(100, y+17, label, { fontFamily: FONT, fontSize: '15px', fill: color }).setOrigin(0.5).setDepth(4);
            bg.on('pointerover', () => bg.setFillStyle(0x333333));
            bg.on('pointerout',  () => bg.setFillStyle(0x222222));
            bg.on('pointerdown', cb);
            this._btnObjs.push(bg, txt);
            y += 40;
        });
    }
}
