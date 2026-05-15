class MultiplayerManager {
    constructor() {
        this.connection = null;
        this.conectado = false;
        this.esHost = false;
        this.codigoSala = null;

        // Callbacks — movimiento
        this.onSalaLista = null;
        this.onPosicionActualizada = null;
        this.onJugadorDesconectado = null;

        // Callbacks — Truco
        this.onTrucoEstado = null;
    }

    async conectar() {
        if (this.conectado) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/gamehub")
            .withAutomaticReconnect()
            .build();

        this.connection.on("SalaLista", () => {
            if (this.onSalaLista) this.onSalaLista();
        });
        this.connection.on("PosicionActualizada", (x, y, animacion, sprite, escena) => {
            if (this.onPosicionActualizada) this.onPosicionActualizada(x, y, animacion, sprite, escena);
        });
        this.connection.on("JugadorDesconectado", () => {
            if (this.onJugadorDesconectado) this.onJugadorDesconectado();
        });
        this.connection.on("TrucoEstado", (data) => {
            if (this.onTrucoEstado) this.onTrucoEstado(data);
        });

        await this.connection.start();
        this.conectado = true;
    }

    async crearSala() {
        this.codigoSala = await this.connection.invoke("CrearSala");
        this.esHost = true;
        return this.codigoSala;
    }

    async unirseASala(codigo) {
        const ok = await this.connection.invoke("UnirseASala", codigo);
        if (ok) { this.codigoSala = codigo; this.esHost = false; }
        return ok;
    }

    async enviarPosicion(x, y, animacion, sprite, escena) {
        if (this.conectado && this.connection?.state === signalR.HubConnectionState.Connected) {
            try { await this.connection.invoke("ActualizarPosicion", x, y, animacion, sprite, escena); }
            catch (_) {}
        }
    }

    // ── Truco ──────────────────────────────────────────────────
    async iniciarTruco() {
        await this.connection.invoke("IniciarTruco");
    }
    async jugarCarta(numero, palo) {
        await this.connection.invoke("JugarCarta", numero, palo);
    }
    async solicitarEnvido(tipo) {
        await this.connection.invoke("SolicitarEnvido", tipo);
    }
    async responderEnvido(aceptar) {
        await this.connection.invoke("ResponderEnvido", aceptar);
    }
    async solicitarTruco() {
        await this.connection.invoke("SolicitarTruco");
    }
    async responderTruco(aceptar, escalarA = null) {
        await this.connection.invoke("ResponderTruco", aceptar, escalarA);
    }
    async escalarTruco() {
        await this.connection.invoke("EscalarTruco");
    }
    async irseAlMazo() {
        await this.connection.invoke("IrseAlMazo");
    }
    async nuevaMano() {
        await this.connection.invoke("NuevaMano");
    }
    async nuevaPartida() {
        await this.connection.invoke("NuevaPartida");
    }
    // ──────────────────────────────────────────────────────────

    async desconectar() {
        if (this.connection) await this.connection.stop();
        this.conectado = false;
        this.esHost = false;
        this.codigoSala = null;
    }

    limpiarCallbacks() {
        this.onSalaLista = null;
        this.onPosicionActualizada = null;
        this.onJugadorDesconectado = null;
        this.onTrucoEstado = null;
    }
}

export const multiplayerManager = new MultiplayerManager();
