let manoActual = null;
let nivelMentiraEnvido = 0;
let nivelMentiraTruco = 0;

const sliderMentiraEnvido = document.getElementById("sliderMentiraEnvido");
const valorMentiraEnvido = document.getElementById("valorMentiraEnvido");
const sliderMentiraTruco = document.getElementById("sliderMentiraTruco");
const valorMentiraTruco = document.getElementById("valorMentiraTruco");
const btnNuevaMano = document.getElementById("btnNuevaMano");
const btnEnvido = document.getElementById("btnEnvido");
const btnRealEnvido = document.getElementById("btnRealEnvido");
const btnFaltaEnvido = document.getElementById("btnFaltaEnvido");
const btnTruco = document.getElementById("btnTruco");
const btnIrseAlMazo = document.getElementById("btnIrseAlMazo");
const btnQuieroCanto = document.getElementById("btnQuieroCanto");
const btnNoQuieroCanto = document.getElementById("btnNoQuieroCanto");
const btnRetruco = document.getElementById("btnRetruco");
const btnValeCuatro = document.getElementById("btnValeCuatro");
const respuestaUsuarioWrap = document.getElementById("respuestaUsuarioWrap");
const labelRespuesta = document.getElementById("labelRespuesta");
const btnNuevaPartida = document.getElementById("btnNuevaPartida");
const cartelDecisionMaquina = document.getElementById("cartelDecisionMaquina");

sliderMentiraEnvido.addEventListener("input", async () => {
    nivelMentiraEnvido = parseInt(sliderMentiraEnvido.value);
    valorMentiraEnvido.textContent = nivelMentiraEnvido;

    if (!manoActual || !manoActual.id) {
        return;
    }

    const response = await fetch("/api/Truco/configurar-nivel-mentira-envido", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id, nivelMentira: nivelMentiraEnvido })
    });

    if (response.ok) {
        manoActual = await response.json();
        renderizarMano();
    }
});

sliderMentiraTruco.addEventListener("input", async () => {
    nivelMentiraTruco = parseInt(sliderMentiraTruco.value);
    valorMentiraTruco.textContent = nivelMentiraTruco;

    if (!manoActual || !manoActual.id) {
        return;
    }

    const response = await fetch("/api/Truco/configurar-nivel-mentira-truco", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id, nivelMentira: nivelMentiraTruco })
    });

    if (response.ok) {
        manoActual = await response.json();
        renderizarMano();
    }
});

btnNuevaMano.addEventListener("click", async () => {
    const response = await fetch("/api/Truco/nueva-mano", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoAnteriorId: manoActual?.id || null })
    });

    if (!response.ok) {
        alert("Error al crear nueva mano");
        return;
    }

    manoActual = await response.json();
    await configurarNivelesMentira();
    renderizarMano();
});

btnNuevaPartida.addEventListener("click", async () => {
    const response = await fetch("/api/Truco/nueva-partida", {
        method: "POST"
    });

    if (!response.ok) {
        alert("Error al crear nueva partida");
        return;
    }

    manoActual = await response.json();
    await configurarNivelesMentira();
    renderizarMano();
});

btnEnvido.addEventListener("click", async () => {
    await cantarEnvido("Envido");
});

btnRealEnvido.addEventListener("click", async () => {
    await cantarEnvido("Real Envido");
});

btnFaltaEnvido.addEventListener("click", async () => {
    await cantarEnvido("Falta Envido");
});

btnTruco.addEventListener("click", async () => {
    if (!manoActual || !manoActual.id) {
        alert("Primero tenés que crear una mano.");
        return;
    }

    const endpoint = manoActual.trucoCantado
        ? "/api/Truco/escalar-truco"
        : "/api/Truco/cantar-truco";

    const response = await fetch(endpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id })
    });

    if (!response.ok) {
        const errorTexto = await response.text();
        alert("Error: " + errorTexto);
        return;
    }

    manoActual = await response.json();
    renderizarMano();
});

btnIrseAlMazo.addEventListener("click", async () => {
    if (!manoActual || !manoActual.id) return;

    const response = await fetch("/api/Truco/irse-al-mazo", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id })
    });

    if (!response.ok) {
        const errorTexto = await response.text();
        alert("Error: " + errorTexto);
        return;
    }

    manoActual = await response.json();
    renderizarMano();
});

async function cantarEnvido(tipo) {
    if (!manoActual || !manoActual.id) {
        alert("Primero tenés que crear una mano.");
        return;
    }

    const response = await fetch("/api/Truco/cantar-envido-tipo", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id, tipo: tipo })
    });

    if (!response.ok) {
        const errorTexto = await response.text();
        alert("Error: " + errorTexto);
        return;
    }

    manoActual = await response.json();
    renderizarMano();
}

btnQuieroCanto.addEventListener("click", async () => {
    if (manoActual?.envidoPendienteRespuestaHumano) {
        await responderEnvido(true);
    } else {
        await responderTruco(true, null);
    }
});

btnNoQuieroCanto.addEventListener("click", async () => {
    if (manoActual?.envidoPendienteRespuestaHumano) {
        await responderEnvido(false);
    } else {
        await responderTruco(false, null);
    }
});

btnRetruco.addEventListener("click", async () => {
    await responderTruco(true, "Retruco");
});

btnValeCuatro.addEventListener("click", async () => {
    await responderTruco(true, "ValeCuatro");
});

async function responderEnvido(aceptar) {
    if (!manoActual || !manoActual.id) {
        alert("No hay mano activa.");
        return;
    }

    const response = await fetch("/api/Truco/responder-envido", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id, aceptar: aceptar })
    });

    if (!response.ok) {
        const errorTexto = await response.text();
        alert("Error: " + errorTexto);
        return;
    }

    manoActual = await response.json();
    renderizarMano();
}

async function responderTruco(aceptar, escalarA) {
    if (!manoActual || !manoActual.id) {
        alert("No hay mano activa.");
        return;
    }

    const body = { manoId: manoActual.id, aceptar: aceptar };
    if (escalarA) body.escalarA = escalarA;

    const response = await fetch("/api/Truco/responder-truco", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });

    if (!response.ok) {
        const errorTexto = await response.text();
        alert("Error: " + errorTexto);
        return;
    }

    manoActual = await response.json();
    renderizarMano();
}

async function jugarCarta(numero, palo) {
    if (!manoActual || !manoActual.id) {
        alert("Primero tenés que crear una mano.");
        return;
    }

    const response = await fetch("/api/Truco/jugar-carta", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id, numero: numero, palo: palo })
    });

    if (!response.ok) {
        const errorTexto = await response.text();
        alert("Error: " + errorTexto);
        return;
    }

    manoActual = await response.json();
    renderizarMano();
}

function renderizarMano() {
    renderizarCartasJugador();
    renderizarCartasMaquina();
    renderizarJugadas();
    renderizarResumenMaquina();
    renderizarTanteador();
    renderizarEstadoEnvido();
    renderizarEstadoTruco();
    renderizarDecisiones();
    renderizarCartelDecisionMaquina();
    actualizarBotonEnvido();
    actualizarBotonTruco();
    actualizarBotonMazo();
    actualizarBotonesRespuestaUsuario();
}

function renderizarCartelDecisionMaquina() {
    if (!manoActual) {
        cartelDecisionMaquina.className = "cartel-decision d-none";
        cartelDecisionMaquina.textContent = "";
        return;
    }

    const textoTruco = (manoActual.estadoTruco || "").toLowerCase();
    const textoEnvido = (manoActual.estadoEnvido || "").toLowerCase();

    if (textoTruco.includes("la máquina quiso") || textoEnvido.includes("la máquina quiso")) {
        cartelDecisionMaquina.className = "cartel-decision decision-quiero";
        cartelDecisionMaquina.textContent = "QUIERO";
        return;
    }

    if (textoTruco.includes("la máquina no quiso") || textoEnvido.includes("la máquina no quiso")) {
        cartelDecisionMaquina.className = "cartel-decision decision-no-quiero";
        cartelDecisionMaquina.textContent = "NO QUIERO";
        return;
    }

    if (textoTruco.includes("retruco") || textoTruco.includes("vale cuatro")) {
        cartelDecisionMaquina.className = "cartel-decision decision-quiero";
        cartelDecisionMaquina.textContent = textoTruco.includes("vale cuatro") ? "VALE CUATRO" : "RETRUCO";
        return;
    }

    cartelDecisionMaquina.className = "cartel-decision d-none";
    cartelDecisionMaquina.textContent = "";
}

function renderizarJugadas() {
    const contenedor = document.getElementById("cartasJugadas");

    const tieneBazas = manoActual && manoActual.bazas && manoActual.bazas.length > 0;
    const tieneCartaEnMesa = manoActual && manoActual.cartaMaquinaEnMesa;

    if (!tieneBazas && !tieneCartaEnMesa) {
        contenedor.innerHTML = "Todavía no se jugó ninguna mano.";
        return;
    }

    let html = "";

    if (tieneCartaEnMesa) {
        html += renderizarCartaMaquinaEnMesa(manoActual.cartaMaquinaEnMesa);
    }

    if (tieneBazas) {
        const jugadasHtml = manoActual.bazas
            .map((baza, index) => renderizarJugadaEnMesa(baza, index))
            .join("");
        html += `
            <div class="panel-titulo">Manos</div>
            <div class="jugadas-en-mesa">${jugadasHtml}</div>
        `;
    }

    contenedor.innerHTML = html;
}

function renderizarCartaMaquinaEnMesa(carta) {
    return `
        <div class="carta-maquina-en-mesa">
            <div class="panel-titulo">La máquina jugó — tu turno</div>
            <div class="carta carta-jugada carta-maquina-mesa-card">
                <div>
                    <div class="carta-numero">${carta.numero}</div>
                    <div class="carta-palo">${carta.palo}</div>
                </div>
                <div class="carta-centro">Máquina</div>
                <div class="carta-valor">Valor truco: ${carta.valorTruco}</div>
            </div>
        </div>
    `;
}

function renderizarJugadaEnMesa(baza, index) {
    if (!baza || !baza.cartaJugador || !baza.cartaMaquina) {
        return "";
    }

    const cartaJugador = baza.cartaJugador;
    const cartaMaquina = baza.cartaMaquina;
    const jugadorGana = cartaJugador.valorTruco > cartaMaquina.valorTruco;

    const cartaFondo = jugadorGana ? cartaMaquina : cartaJugador;
    const cartaFrente = jugadorGana ? cartaJugador : cartaMaquina;
    const etiquetaFondo = jugadorGana ? "Máquina" : "Usuario";
    const etiquetaFrente = jugadorGana ? "Usuario" : "Máquina";

    const claseEmpate = cartaJugador.valorTruco === cartaMaquina.valorTruco ? "carta-jugada-empate" : "";

    const ganadorLabel = baza.ganador === "Parda"
        ? "Parda (empate)"
        : baza.ganador === "Humano" ? "Gana Usuario" : "Gana Máquina";

    return `
        <div class="jugada-item">
            <div class="jugada-titulo">Mano ${index + 1} — ${ganadorLabel}</div>
            <div class="mesa-superpuesta ${claseEmpate}">
                ${renderizarCartaMesa(cartaFondo, etiquetaFondo, "carta-fondo")}
                ${renderizarCartaMesa(cartaFrente, etiquetaFrente, "carta-frente")}
            </div>
        </div>
    `;
}

function renderizarCartaMesa(carta, propietario, claseExtra) {
    return `
        <div class="carta carta-jugada ${claseExtra}">
            <div>
                <div class="carta-numero">${carta.numero}</div>
                <div class="carta-palo">${carta.palo}</div>
            </div>
            <div class="carta-centro">${propietario}</div>
            <div class="carta-valor">Valor truco: ${carta.valorTruco}</div>
        </div>
    `;
}

function actualizarBotonEnvido() {
    if (!manoActual) {
        btnEnvido.disabled = true;
        btnRealEnvido.disabled = true;
        btnFaltaEnvido.disabled = true;
        return;
    }

    const yaSeJugoBaza = manoActual.bazas && manoActual.bazas.length > 0;
    const yaSeCanto = manoActual.envidoCantado || manoActual.envidoResuelto;
    const terminoMano = !!manoActual.ganadorMano;

    const esperandoRespuestaEnvido = manoActual.envidoPendienteRespuestaHumano;

    const disabled = manoActual.partidaTerminada || yaSeJugoBaza || yaSeCanto || terminoMano || esperandoRespuestaEnvido;
    btnEnvido.disabled = disabled;
    btnRealEnvido.disabled = disabled;
    btnFaltaEnvido.disabled = disabled;
}

function actualizarBotonTruco() {
    if (!manoActual) {
        btnTruco.textContent = "Truco";
        btnTruco.disabled = true;
        return;
    }

    const terminoMano = !!manoActual.ganadorMano;
    const esperandoRespuesta = manoActual.envidoPendienteRespuestaHumano || manoActual.trucoPendienteRespuestaHumano;
    const bloqueado = manoActual.partidaTerminada || terminoMano || esperandoRespuesta;

    if (!manoActual.trucoCantado) {

        btnTruco.textContent = "Truco";
        btnTruco.disabled = bloqueado;
    } else if (
        !manoActual.trucoResuelto &&
        manoActual.nivelTruco < 3 &&
        manoActual.cantorTruco === "Maquina" &&
        !bloqueado
    ) {
        btnTruco.textContent = manoActual.nivelTruco === 1 ? "Retruco" : "Vale Cuatro";
        btnTruco.disabled = false;
    } else {
        btnTruco.textContent = "Truco";
        btnTruco.disabled = true;
    }
}

function actualizarBotonMazo() {
    if (!manoActual) {
        btnIrseAlMazo.disabled = true;
        return;
    }

    const puedeMazo = !manoActual.partidaTerminada
        && !manoActual.ganadorMano
        && !manoActual.envidoPendienteRespuestaHumano
        && !manoActual.trucoPendienteRespuestaHumano;

    btnIrseAlMazo.disabled = !puedeMazo;
}

function actualizarBotonesRespuestaUsuario() {
    const hayRespuestaPendiente = manoActual &&
        (manoActual.envidoPendienteRespuestaHumano || manoActual.trucoPendienteRespuestaHumano);

    respuestaUsuarioWrap.classList.toggle("d-none", !hayRespuestaPendiente);

    if (!hayRespuestaPendiente) {
        btnRetruco.classList.add("d-none");
        btnValeCuatro.classList.add("d-none");
        return;
    }

    if (manoActual.envidoPendienteRespuestaHumano) {
        labelRespuesta.textContent = "La máquina cantó Envido. ¿Querés?";
        btnRetruco.classList.add("d-none");
        btnValeCuatro.classList.add("d-none");
    } else if (manoActual.trucoPendienteRespuestaHumano) {
        const nivel = manoActual.nivelTruco || 1;
        const nombreCanto = nivel === 3 ? "Vale Cuatro" : nivel === 2 ? "Retruco" : "Truco";
        labelRespuesta.textContent = `La máquina cantó ${nombreCanto}. ¿Querés?`;

        const puedeRetruco = nivel === 1;
        const puedeValeC = nivel === 2;

        btnRetruco.classList.toggle("d-none", !puedeRetruco);
        btnValeCuatro.classList.toggle("d-none", !puedeValeC);
    }
}

function renderizarCartasJugador() {
    const contenedor = document.getElementById("cartasJugador");
    contenedor.innerHTML = "";

    if (!manoActual || !manoActual.humano || !manoActual.humano.mano || manoActual.humano.mano.length === 0) {
        contenedor.innerHTML = "<div class='panel-info'>No hay cartas disponibles.</div>";
        return;
    }

    manoActual.humano.mano.forEach(carta => {
        const boton = document.createElement("button");
        boton.className = "carta carta-jugable";
        boton.type = "button";
        boton.dataset.palo = carta.palo;

        const simboloPalo = { Espada: "🗡", Basto: "🪵", Oro: "🪙", Copa: "🏆" }[carta.palo] || "🂠";

        boton.innerHTML = `
            <div class="carta-numero">${carta.numero}</div>
            <div class="carta-palo">${carta.palo}</div>
            <div class="carta-centro">${simboloPalo}</div>
            <div class="carta-valor">Truco: ${carta.valorTruco}</div>
        `;

        boton.onclick = () => jugarCarta(carta.numero, carta.palo);

        if (manoActual.ganadorMano || manoActual.envidoPendienteRespuestaHumano ||
            manoActual.trucoPendienteRespuestaHumano || manoActual.partidaTerminada) {
            boton.disabled = true;
        }

        contenedor.appendChild(boton);
    });
}

function renderizarCartasMaquina() {
    const contenedor = document.getElementById("cartasMaquina");
    contenedor.innerHTML = "";

    if (!manoActual || !manoActual.maquina || !manoActual.maquina.mano) {
        return;
    }

    const cantidad = manoActual.maquina.mano.length;

    if (cantidad === 0) {
        contenedor.innerHTML = "<div class='panel-info'>La máquina ya no tiene cartas.</div>";
        return;
    }

    for (let i = 0; i < cantidad; i++) {
        const carta = document.createElement("div");
        carta.className = "carta-boca-abajo";
        contenedor.appendChild(carta);
    }
}

function renderizarResumenMaquina() {
    const badge = document.getElementById("maquinaResumen");

    if (!manoActual || !manoActual.maquina) {
        badge.textContent = "Esperando mano";
        return;
    }

    const restantes = manoActual.maquina.mano ? manoActual.maquina.mano.length : 0;
    const jugadas = manoActual.maquina.jugadas ? manoActual.maquina.jugadas.length : 0;
    const inicia = manoActual.manoIniciadaPor === "Humano" ? "Usuario" : "Máquina";

    badge.textContent = `Restantes: ${restantes} | Jugadas: ${jugadas} | Inicia: ${inicia}`;
}

function renderizarEstadoEnvido() {
    const contenedor = document.getElementById("estadoEnvido");

    if (!manoActual) {
        contenedor.innerHTML = "Todavía no se cantó envido.";
        return;
    }

    if (!manoActual.envidoCantado && !manoActual.envidoResuelto) {
        contenedor.innerHTML = "Todavía no se cantó envido.";
        return;
    }

    let html = "";

    if (manoActual.estadoEnvido) {
        html += `<div class="envido-ok">${manoActual.estadoEnvido}</div>`;
    }

    if (manoActual.tantoHumano !== null && manoActual.tantoHumano !== undefined) {
        html += `<div><strong>Tu tanto real:</strong> ${manoActual.tantoHumano}</div>`;
    }

    if (manoActual.tantoCantadoMaquina !== null && manoActual.tantoCantadoMaquina !== undefined) {
        html += `<div><strong>La máquina cantó ${manoActual.tantoCantadoMaquina} de envido.</strong></div>`;
    }

    if (manoActual.tantoMaquina !== null && manoActual.tantoMaquina !== undefined) {
        html += `<div><strong>Tanto real de la máquina:</strong> ${manoActual.tantoMaquina}</div>`;
    }

    if (manoActual.tipoCantoEnvidoMaquina) {
        let texto = "";
        switch (manoActual.tipoCantoEnvidoMaquina) {
            case "mintio":    texto = "La máquina mintió al cantar envido."; break;
            case "se_jugo":   texto = "La máquina se la jugó en el canto."; break;
            case "tenia":     texto = "La máquina tenía buen envido."; break;
        }
        html += `<div class="envido-mentira"><strong>${texto}</strong></div>`;
    }

    if (manoActual.ganadorEnvido) {
        const ganadorNombre = manoActual.ganadorEnvido === "Humano" ? "Usuario" : "Máquina";
        html += `<div><strong>Ganador del envido:</strong> ${ganadorNombre}</div>`;
    }

    if (manoActual.puntosEnvido) {
        html += `<div><strong>Puntos del envido:</strong> ${manoActual.puntosEnvido}</div>`;
    }

    contenedor.innerHTML = html;
}

function renderizarDecisiones() {
    const contenedor = document.getElementById("decisionesPartida");

    if (!manoActual) {
        contenedor.innerHTML = "Sin decisiones todavía.";
        return;
    }

    const decisiones = [];

    if (manoActual.cantorEnvido) {
        const cantor = manoActual.cantorEnvido === "Humano" ? "Usuario" : "Máquina";
        const tipo = manoActual.tipoEnvidoCantado || "Envido";
        decisiones.push(`<div>🎤 Envido cantado por: <strong>${cantor}</strong> (${tipo}).</div>`);
    }

    if (manoActual.cantorTruco) {
        const cantor = manoActual.cantorTruco === "Humano" ? "Usuario" : "Máquina";
        const nivel = manoActual.nivelTruco || 1;
        const nombreNivel = nivel === 3 ? "Vale Cuatro" : nivel === 2 ? "Retruco" : "Truco";
        decisiones.push(`<div>🎤 Último canto de truco: <strong>${nombreNivel}</strong> por <strong>${cantor}</strong>.</div>`);
    }

    if (manoActual.numeroDeMano) {
        const inicio = manoActual.manoIniciadaPor === "Humano" ? "Usuario" : "Máquina";
        decisiones.push(`<div>🃏 Mano #${manoActual.numeroDeMano}: inicia <strong>${inicio}</strong>.</div>`);
    }

    const turnoNombre = manoActual.turnoActual === "Humano" ? "Usuario" : "Máquina";
    decisiones.push(`<div>⏩ Turno actual: <strong>${turnoNombre}</strong>.</div>`);

    contenedor.innerHTML = decisiones.length > 0 ? decisiones.join("") : "Sin decisiones todavía.";
}

function renderizarEstadoTruco() {
    const contenedor = document.getElementById("estadoTruco");

    if (!manoActual || !manoActual.trucoCantado) {
        contenedor.innerHTML = "Todavía no se cantó truco.";
        return;
    }

    let html = "";

    if (manoActual.estadoTruco) {
        html += `<div class="truco-ok">${manoActual.estadoTruco}</div>`;
    }

    const nivel = manoActual.nivelTruco || 1;
    const nombreNivel = nivel === 3 ? "Vale Cuatro" : nivel === 2 ? "Retruco" : "Truco";
    html += `<div><strong>Nivel:</strong> ${nombreNivel} | <strong>Valor:</strong> ${manoActual.puntosTrucoMano || 1} punto(s)</div>`;

    contenedor.innerHTML = html;
}

function renderizarTanteador() {
    const contenedor = document.getElementById("tanteadorPuntos");

    if (!manoActual) {
        contenedor.innerHTML = "";
        return;
    }

    const humano = manoActual.puntosHumano || 0;
    const maquina = manoActual.puntosMaquina || 0;

    contenedor.innerHTML = `
        <div class="tanteador-columnas">
            ${renderizarColumnaPalitos("Usuario", humano)}
            ${renderizarColumnaPalitos("Máquina", maquina)}
        </div>
        ${renderizarEstadoPartida()}
    `;
}

function renderizarEstadoPartida() {
    if (!manoActual || !manoActual.partidaTerminada) {
        return "";
    }

    const ganador = manoActual.ganadorPartida === "Humano" ? "Usuario" : "Máquina";
    return `<div class="estado-partida-final">🏆 ${ganador} llegó a 30 y ganó la partida.</div>`;
}

function renderizarColumnaPalitos(nombre, puntos) {
    const total = 30;
    const puntosSeguros = Math.max(0, Math.min(total, puntos));

    let grupos = "";
    for (let i = 0; i < total; i += 5) {
        const activosGrupo = Math.max(0, Math.min(5, puntosSeguros - i));
        grupos += renderizarGrupoPalitos(activosGrupo);
    }

    return `
        <div class="jugador-tanteador">
            <div class="jugador-nombre">${nombre}</div>
            <div class="palitos-grid">${grupos}</div>
            <div class="jugador-puntos">${puntosSeguros}/30</div>
        </div>
    `;
}

function renderizarGrupoPalitos(activosGrupo) {
    const clases = (indice) => activosGrupo >= indice ? "activo" : "";

    return `
        <div class="grupo-palitos">
            <span class="palito p1 ${clases(1)}"></span>
            <span class="palito p2 ${clases(2)}"></span>
            <span class="palito p3 ${clases(3)}"></span>
            <span class="palito p4 ${clases(4)}"></span>
            <span class="palito p5 ${clases(5)}"></span>
        </div>
    `;
}

async function configurarNivelesMentira() {
    if (!manoActual || !manoActual.id) {
        return;
    }

    const envidoResponse = await fetch("/api/Truco/configurar-nivel-mentira-envido", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id, nivelMentira: nivelMentiraEnvido })
    });

    if (!envidoResponse.ok) return;
    manoActual = await envidoResponse.json();

    const trucoResponse = await fetch("/api/Truco/configurar-nivel-mentira-truco", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ manoId: manoActual.id, nivelMentira: nivelMentiraTruco })
    });

    if (trucoResponse.ok) {
        manoActual = await trucoResponse.json();
    }
}

valorMentiraEnvido.textContent = sliderMentiraEnvido.value;
valorMentiraTruco.textContent = sliderMentiraTruco.value;
