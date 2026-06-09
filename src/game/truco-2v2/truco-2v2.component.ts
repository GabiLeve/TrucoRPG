import {
  Component, OnInit, OnDestroy,
  ChangeDetectorRef, ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { SalaService } from '../../app/services/sala.service';

// ── Tipos del backend (camelCase, como serializa SignalR) ──────────────────
export interface Carta2v2 { numero: number; palo: string; valorTruco: number; }

interface Vuelta2v2 {
  cartasJugadas: Record<string, Carta2v2>;
  ganadorVuelta: string | null;
}

interface EstadoMano2v2 {
  numeroDeMano: number;
  turnoActual: string;
  jugadorMano: string;
  equipoMano: string;
  ganadorMano: string | null;
  manoTerminada: boolean;
  partidaTerminada: boolean;
  ganadorPartida: string | null;
  puntosEquipoA: number;
  puntosEquipoB: number;

  estadoEnvido: string | null;
  estadoTruco: string | null;
  envidoCantado: boolean;
  envidoResuelto: boolean;
  tipoEnvidoCantado: string | null;
  cantorEnvido: string | null;
  ganadorEnvido: string | null;
  puntosEnvido: number;
  faseEnvido: string | null;
  envidoPendienteRespuestaDe: string | null;
  sonBuenasDeclarado: boolean;
  tantosDeclarados: Record<string, number | null>;

  trucoCantado: boolean;
  trucoResuelto: boolean;
  nivelTruco: number;
  puntosTrucoMano: number;
  cantorTruco: string | null;
  equipoCantorTruco: string | null;
  trucoPendienteRespuestaDe: string | null;
  puedeEscalarTruco: string | null;

  vueltas: Vuelta2v2[];
  vueltaActual: Vuelta2v2 | null;
}

interface EstadoRecibido2v2 {
  miRol: string;            // "J1".."J4"
  miEquipo: string;         // "EquipoA" | "EquipoB"
  misCartas: Carta2v2[];
  misJugadas: Carta2v2[];
  cartasCompanero: Carta2v2[];
  estado: EstadoMano2v2;
}

interface BtnAccion { label: string; color: string; enabled: boolean; action: () => void; }
interface MesaJugadas { yo: Carta2v2[]; compa: Carta2v2[]; izq: Carta2v2[]; der: Carta2v2[]; }
interface TallyStick { x1: number; y1: number; x2: number; y2: number; color: string; }

const ORDEN = ['J1', 'J2', 'J3', 'J4'];

@Component({
  selector: 'app-truco-2v2',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './truco-2v2.component.html',
  styleUrls: [
    '../truco-solo/truco-solo.component.css',
    './truco-2v2.component.css',
    '../truco-solo-2v2/truco-solo-2v2.component.css',
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrucoMulti2v2Component implements OnInit, OnDestroy {

  // ── Estado actual ──────────────────────────────────────────────
  st: EstadoRecibido2v2 | null = null;

  // Roles por asiento (relativos a mí): yo abajo, compa arriba, der/izq rivales
  miRol = 'J1';
  rolDer = 'J2';
  rolCompa = 'J3';
  rolIzq = 'J4';

  // ── UI ─────────────────────────────────────────────────────────
  btns: BtnAccion[] = [];
  mesa: MesaJugadas = { yo: [], compa: [], izq: [], der: [] };
  miMano: Carta2v2[] = [];
  tantoInput = 0;
  turnoBadge = 'Esperando a los jugadores...';
  toastMsg = '';
  mostrarConfirmSalir = false;
  gameOver = false;
  gameOverGanamos = false;

  countdown: number | null = null;
  private countdownInterval: ReturnType<typeof setInterval> | null = null;
  private prevGanadorMano: string | null = null;

  puntosNosotros = 0;
  puntosEllos = 0;
  tallySticksNosotros: TallyStick[] = [];
  tallySticksEllos: TallyStick[] = [];

  readonly fanAngles = [-12, 0, 12];
  readonly fanXOff = [-18, 0, 18];

  private toastTimer: ReturnType<typeof setTimeout> | null = null;
  private subs: Subscription[] = [];

  constructor(
    private sala: SalaService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.subs.push(
      this.sala.trucoEstado2v2$.subscribe(data => {
        if (data) { this.aplicarEstado(data as EstadoRecibido2v2); }
      }),
      this.sala.jugadorDesconectado$.subscribe(v => {
        if (v) this.showToast('Un jugador se desconectó de la partida.');
      }),
    );
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.cancelarCountdown();
  }

  // ── Recibir estado ─────────────────────────────────────────────
  private aplicarEstado(data: EstadoRecibido2v2): void {
    if (!data || !data.estado) return;
    this.st = data;
    this.miRol = data.miRol;

    const i = ORDEN.indexOf(data.miRol);
    this.rolDer   = ORDEN[(i + 1) % 4];
    this.rolCompa = ORDEN[(i + 2) % 4];
    this.rolIzq   = ORDEN[(i + 3) % 4];

    this.miMano = data.misCartas ?? [];
    this.mesa = this.buildMesa(data.estado);
    this.tantoInput = this.calcularMiTanto(data);

    const e = data.estado;
    const nosotrosA = data.miEquipo === 'EquipoA';
    this.puntosNosotros = nosotrosA ? e.puntosEquipoA : e.puntosEquipoB;
    this.puntosEllos    = nosotrosA ? e.puntosEquipoB : e.puntosEquipoA;
    this.redrawTally();

    this.turnoBadge = this.calcularBadge(e);
    this.buildBtns(e);

    if (e.partidaTerminada && !this.gameOver) {
      this.gameOver = true;
      this.gameOverGanamos = e.ganadorPartida === data.miEquipo;
    }

    // Nueva mano automática (cuenta regresiva de 3s); el botón sigue disponible.
    if (e.ganadorMano && !e.partidaTerminada) {
      if (e.ganadorMano !== this.prevGanadorMano) {
        this.iniciarCountdown(() => {
          if (this.st?.estado.ganadorMano && !this.st?.estado.partidaTerminada) this.nuevaMano();
        });
      }
    } else {
      this.cancelarCountdown();
    }
    this.prevGanadorMano = e.ganadorMano ?? null;

    this.cdr.markForCheck();
  }

  // ── Mesa ───────────────────────────────────────────────────────
  private buildMesa(e: EstadoMano2v2): MesaJugadas {
    const mesa: MesaJugadas = { yo: [], compa: [], izq: [], der: [] };
    const vueltas: Vuelta2v2[] = [...(e.vueltas ?? [])];
    if (e.vueltaActual) vueltas.push(e.vueltaActual);
    for (const v of vueltas) {
      const cj = v.cartasJugadas ?? {};
      if (cj[this.miRol])   mesa.yo.push(cj[this.miRol]);
      if (cj[this.rolCompa]) mesa.compa.push(cj[this.rolCompa]);
      if (cj[this.rolIzq])  mesa.izq.push(cj[this.rolIzq]);
      if (cj[this.rolDer])  mesa.der.push(cj[this.rolDer]);
    }
    return mesa;
  }

  /** Cartas que le quedan a un jugador (para dibujar reversos). */
  cartasRestantes(rol: string): number {
    const e = this.st?.estado;
    if (!e) return 0;
    let jugadas = 0;
    for (const v of [...(e.vueltas ?? []), ...(e.vueltaActual ? [e.vueltaActual] : [])]) {
      if (v.cartasJugadas && v.cartasJugadas[rol]) jugadas++;
    }
    return Math.max(0, 3 - jugadas);
  }

  reversos(rol: string): number[] {
    return Array.from({ length: this.cartasRestantes(rol) }, (_, k) => k);
  }

  esMano(rol: string): boolean { return this.st?.estado.jugadorMano === rol; }

  // ── Helpers de equipo ──────────────────────────────────────────
  private get miEquipo(): string { return this.st?.miEquipo ?? 'EquipoA'; }
  private esRival(rol: string): boolean {
    const e = this.st?.estado;
    if (!e) return false;
    // EquipoA = J1,J3 ; EquipoB = J2,J4
    const equipoRol = (rol === 'J1' || rol === 'J3') ? 'EquipoA' : 'EquipoB';
    return equipoRol !== this.miEquipo;
  }

  // ── Acciones (invocan el Hub) ──────────────────────────────────
  private hub(metodo: string, ...args: unknown[]): void {
    this.sala.invocarHub(metodo, ...args).catch(() => this.showToast('Error de conexión.'));
  }

  jugarCarta(c: Carta2v2): void {
    const e = this.st?.estado;
    if (!e) return;
    if (e.manoTerminada || e.ganadorMano || e.partidaTerminada) { this.showToast('La mano ha sido terminada.'); return; }
    if (e.turnoActual !== this.miRol) return;
    if (e.trucoPendienteRespuestaDe === this.miRol || e.envidoPendienteRespuestaDe === this.miRol) return;
    this.hub('JugarCarta2v2', c.numero, c.palo);
  }

  cantarEnvido(tipo: string): void { this.hub('SolicitarEnvido2v2', tipo); }
  escalarEnvido(tipo: string): void { this.hub('EscalarEnvido2v2', tipo); }
  responderEnvido(aceptar: boolean): void { this.hub('ResponderEnvido2v2', aceptar); }
  declararTanto(): void { this.hub('DeclararTanto2v2', this.tantoInput); }
  sonBuenas(): void { this.hub('SonBuenas2v2'); }
  cantarTruco(): void { this.hub('SolicitarTruco2v2'); }
  responderTruco(aceptar: boolean, escalarA?: string): void { this.hub('ResponderTruco2v2', aceptar, escalarA ?? null); }
  escalarTruco(): void { this.hub('EscalarTruco2v2'); }
  irseAlMazo(): void { this.hub('IrseAlMazo2v2'); }
  nuevaMano(): void { this.cancelarCountdown(); this.hub('NuevaMano2v2'); }

  // ── Botones ────────────────────────────────────────────────────
  private buildBtns(e: EstadoMano2v2): void {
    const btns: BtnAccion[] = [];
    const esMiTurno = e.turnoActual === this.miRol;
    const manoEnd = e.manoTerminada || !!e.ganadorMano || e.partidaTerminada;

    const envidoDisponible = !e.envidoCantado && !e.envidoResuelto && (e.vueltas?.length ?? 0) === 0
      && (this.st?.misJugadas?.length ?? 0) === 0
      && (!e.trucoCantado || (e.trucoPendienteRespuestaDe === this.miRol && e.nivelTruco === 1
                              && e.equipoCantorTruco !== this.miEquipo));

    // ── Responder envido ─────────────────────────────────────────
    if (e.envidoPendienteRespuestaDe === this.miRol && e.faseEnvido === 'pendiente_respuesta') {
      btns.push({ label: 'QUIERO', color: '#44ff44', enabled: true, action: () => this.responderEnvido(true) });
      const tipo = e.tipoEnvidoCantado ?? 'Envido';
      if (tipo === 'Envido')
        btns.push({ label: 'ENVIDO', color: '#4488ff', enabled: true, action: () => this.escalarEnvido('Envido Envido') });
      if (tipo === 'Envido' || tipo === 'EnvidoEnvido')
        btns.push({ label: 'REAL ENVIDO', color: '#4488ff', enabled: true, action: () => this.escalarEnvido('Real Envido') });
      if (tipo !== 'FaltaEnvido')
        btns.push({ label: 'FALTA ENVIDO', color: '#4488ff', enabled: true, action: () => this.escalarEnvido('Falta Envido') });
      btns.push({ label: 'NO QUIERO', color: '#ff4444', enabled: true, action: () => this.responderEnvido(false) });
    }
    // ── Declarar tanto ───────────────────────────────────────────
    else if (e.envidoPendienteRespuestaDe === this.miRol && e.faseEnvido === 'declarando_tantos') {
      btns.push({ label: `TENGO ${this.tantoInput}`, color: '#44ff44', enabled: true, action: () => this.declararTanto() });
      if (this.algunRivalDeclaro(e))
        btns.push({ label: 'SON BUENAS', color: '#ffaa00', enabled: true, action: () => this.sonBuenas() });
    }
    // ── Esperando resolución del envido ──────────────────────────
    else if ((e.faseEnvido === 'pendiente_respuesta' || e.faseEnvido === 'declarando_tantos')
             && e.envidoPendienteRespuestaDe != null) {
      // sin botones (no es tu turno de responder)
    }
    // ── Responder truco ──────────────────────────────────────────
    else if (e.trucoPendienteRespuestaDe === this.miRol) {
      btns.push({ label: 'QUIERO', color: '#44ff44', enabled: true, action: () => this.responderTruco(true) });
      if ((e.nivelTruco ?? 0) < 3 && e.puedeEscalarTruco === this.miRol) {
        const lbl = e.nivelTruco === 1 ? 'RETRUCO' : 'VALE 4';
        const esc = e.nivelTruco === 1 ? 'retruco' : 'valecuatro';
        btns.push({ label: lbl, color: '#ffaa00', enabled: true, action: () => this.responderTruco(true, esc) });
      }
      btns.push({ label: 'NO QUIERO', color: '#ff4444', enabled: true, action: () => this.responderTruco(false) });
      if (envidoDisponible) {
        btns.push({ label: 'Envido', color: '#4488ff', enabled: true, action: () => this.cantarEnvido('Envido') });
        btns.push({ label: 'Real Envido', color: '#4488ff', enabled: true, action: () => this.cantarEnvido('Real Envido') });
        btns.push({ label: 'Falta Envido', color: '#4488ff', enabled: true, action: () => this.cantarEnvido('Falta Envido') });
      }
    }
    // ── Mano terminada ───────────────────────────────────────────
    else if (e.manoTerminada && !e.partidaTerminada) {
      btns.push({ label: 'NUEVA MANO', color: '#cc8800', enabled: true, action: () => this.nuevaMano() });
    }
    // ── Turno normal ─────────────────────────────────────────────
    else if (!manoEnd) {
      if (envidoDisponible) {
        btns.push({ label: 'Envido', color: '#4488ff', enabled: esMiTurno, action: () => this.cantarEnvido('Envido') });
        btns.push({ label: 'Real Envido', color: '#4488ff', enabled: esMiTurno, action: () => this.cantarEnvido('Real Envido') });
        btns.push({ label: 'Falta Envido', color: '#4488ff', enabled: esMiTurno, action: () => this.cantarEnvido('Falta Envido') });
      }
      if (!e.trucoCantado) {
        btns.push({ label: 'Truco', color: '#dd4422', enabled: esMiTurno, action: () => this.cantarTruco() });
      } else if (e.trucoPendienteRespuestaDe == null && (e.nivelTruco ?? 0) >= 1 && (e.nivelTruco ?? 0) < 3
                 && e.equipoCantorTruco !== this.miEquipo) {
        const lbl = e.nivelTruco === 1 ? 'Retruco' : 'Vale 4';
        btns.push({ label: lbl, color: '#ffaa00', enabled: esMiTurno, action: () => this.escalarTruco() });
      }
      btns.push({ label: 'Ir al mazo', color: '#884422', enabled: esMiTurno, action: () => this.irseAlMazo() });
    }

    this.btns = btns;
  }

  private algunRivalDeclaro(e: EstadoMano2v2): boolean {
    const t = e.tantosDeclarados ?? {};
    return (t[this.rolDer] ?? null) !== null || (t[this.rolIzq] ?? null) !== null;
  }

  private calcularBadge(e: EstadoMano2v2): string {
    if (e.partidaTerminada) return '';
    if (e.manoTerminada) {
      const gan = e.ganadorMano === this.miEquipo ? '¡Ganaron la mano!' : 'Perdieron la mano.';
      return this.countdown != null ? `${gan} Nueva mano en ${this.countdown}...` : gan;
    }
    if (e.trucoPendienteRespuestaDe === this.miRol) return 'Respondé el Truco';
    if (e.envidoPendienteRespuestaDe === this.miRol && e.faseEnvido === 'pendiente_respuesta') return 'Respondé el Envido';
    if (e.envidoPendienteRespuestaDe === this.miRol && e.faseEnvido === 'declarando_tantos') return 'Declarás tus tantos';
    if (e.envidoPendienteRespuestaDe != null) return 'Esperando el envido...';
    if (e.trucoPendienteRespuestaDe != null) return 'Esperando respuesta al truco...';
    if (e.turnoActual === this.miRol) return 'Tu turno — jugá una carta o cantá';
    if (e.turnoActual === this.rolCompa) return 'Turno de tu compañero...';
    return 'Turno de un rival...';
  }

  // ── Cálculo del tanto propio ──────────────────────────────────
  private calcularMiTanto(data: EstadoRecibido2v2): number {
    const cartas = [...(data.misCartas ?? []), ...(data.misJugadas ?? [])];
    return this.calcularTanto(cartas);
  }

  private calcularTanto(cartas: Carta2v2[]): number {
    const grupos: Record<string, number[]> = {};
    for (const c of cartas) {
      const v = c.numero >= 10 ? 0 : c.numero;
      (grupos[c.palo] ??= []).push(v);
    }
    let mejor = 0;
    for (const vals of Object.values(grupos)) {
      const s = [...vals].sort((a, b) => b - a);
      if (s.length >= 2) mejor = Math.max(mejor, s[0] + s[1] + 20);
    }
    if (mejor > 0) return mejor;
    return Math.max(...cartas.map(c => c.numero >= 10 ? 0 : c.numero), 0);
  }

  // ── Cuenta regresiva ──────────────────────────────────────────
  private iniciarCountdown(onComplete: () => void): void {
    this.cancelarCountdown();
    this.countdown = 3;
    this.cdr.markForCheck();
    this.countdownInterval = setInterval(() => {
      this.countdown = (this.countdown ?? 1) - 1;
      this.cdr.markForCheck();
      if ((this.countdown ?? 0) <= 0) { this.cancelarCountdown(); onComplete(); }
    }, 1000);
  }

  private cancelarCountdown(): void {
    if (this.countdownInterval) { clearInterval(this.countdownInterval); this.countdownInterval = null; }
    this.countdown = null;
  }

  // ── Tanteador (dos mitades: 1-15 / 16-30) ─────────────────────
  private redrawTally(): void {
    this.tallySticksNosotros = this.buildTally(this.puntosNosotros, '#c8a030');
    this.tallySticksEllos    = this.buildTally(this.puntosEllos, '#d46010');
  }

  private buildTally(pts: number, color: string): TallyStick[] {
    const out: TallyStick[] = [];
    const capped = Math.min(pts, 30);
    if (capped <= 0) return out;
    this.buildTallyMitad(out, Math.min(capped, 15), color, 6);
    if (capped > 15) this.buildTallyMitad(out, capped - 15, color, 71);
    return out;
  }

  private buildTallyMitad(out: TallyStick[], pts: number, color: string, startX: number): void {
    if (pts <= 0) return;
    const full = Math.floor(pts / 5), rem = pts % 5;
    const BS = 14, BGAP = 3, SL = 9, SGAP = 3, y = 19;
    let bx = startX;
    for (let i = 0; i < Math.min(full, 3); i++) { this.addBox(out, bx, y, BS, color); bx += BS + BGAP; }
    if (rem > 0 && full < 3) {
      let sx = bx;
      for (let i = 0; i < rem; i++) { out.push({ x1: sx, y1: y + SL, x2: sx + SL, y2: y, color }); sx += SL + SGAP; }
    }
  }

  private addBox(out: TallyStick[], x: number, y: number, s: number, color: string): void {
    out.push({ x1: x, y1: y + s, x2: x, y2: y, color });
    out.push({ x1: x, y1: y, x2: x + s, y2: y, color });
    out.push({ x1: x + s, y1: y, x2: x + s, y2: y + s, color });
    out.push({ x1: x + s, y1: y + s, x2: x, y2: y + s, color });
    out.push({ x1: x, y1: y + s, x2: x + s, y2: y, color });
  }

  // ── Imagen de carta ───────────────────────────────────────────
  cardImg(c: Carta2v2): string {
    const nums: Record<number, number> = { 1: 1, 2: 2, 3: 3, 4: 4, 5: 5, 6: 6, 7: 7, 10: 8, 11: 9, 12: 10 };
    const palos: Record<string, number> = { Oro: 0, Copa: 10, Espada: 20, Basto: 30 };
    return `assets/cards/${(palos[c.palo] ?? 0) + (nums[c.numero] ?? 1)}.PNG`;
  }

  get estadoEnvido(): string { return this.st?.estado.estadoEnvido ?? 'No se cantó.'; }
  get estadoTruco(): string { return this.st?.estado.estadoTruco ?? 'No se cantó.'; }

  // ── Navegación ────────────────────────────────────────────────
  salirPartida(): void { this.mostrarConfirmSalir = true; }
  cancelarSalir(): void { this.mostrarConfirmSalir = false; }
  async confirmarSalir(): Promise<void> {
    this.mostrarConfirmSalir = false;
    await this.sala.abandonar();
    this.router.navigate(['/home']);
  }

  private showToast(msg: string): void {
    this.toastMsg = msg;
    this.cdr.markForCheck();
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => { this.toastMsg = ''; this.cdr.markForCheck(); }, 3500);
  }
}
