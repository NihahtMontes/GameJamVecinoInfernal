# 🎮 GameJam Vecino Infernal — Documentación Completa

> **Motor:** Unity 6.3 LTS | **Género:** Survival Horror 2D Top-Down | **Perspectiva:** Vista cenital

---

## 📖 Concepto del Juego

Eres un vecino atrapado en una casa infestada por un **fantasma**. Tu única herramienta de defensa es una **linterna**. Tienes que sobrevivir **3 minutos** completos o **eliminar al fantasma** para ganar. Si el fantasma te toca, pierdes.

La clave está en la estrategia: ¿Usas las cargas de tu linterna para aturdir al fantasma y escapar? ¿O esperas al ítem especial para convertirte en el cazador?

---

## 🕹️ Controles

| Acción | Control |
|---|---|
| Moverse | W A S D o flechas del teclado |
| Disparar linterna | Clic izquierdo del mouse (apunta con el cursor) |
| Avanzar diálogo | E o Barra espaciadora |
| Saltar texto de diálogo | E o Barra espaciadora (mientras escribe) |
| Zoom cámara | Rueda del mouse |

---

## 🏆 Condiciones de Victoria (2 formas de ganar)

### Forma 1 — Sobrevivir la noche
Aguanta los **3 minutos** completos sin que el fantasma te toque. Al llegar el tiempo a cero, aparece el panel "¡SOBREVIVISTE LA NOCHE!".

### Forma 2 — Eliminar al fantasma
Recoge el **Ítem de Poder** (círculo amarillo) y usa tu linterna para dar **8 golpes** directos al fantasma. Al llegar a 0 vidas, el fantasma desaparece y aparece el panel de victoria.

---

## 💀 Condición de Derrota

El fantasma te toca físicamente → aparece el panel "GAME OVER" con botón de reinicio.

---

## 🗺️ Escenas del Juego

### Escena Main
- Escena exterior de introducción.
- El jugador puede caminar hacia la entrada de la casa para ir al Interior.
- Aquí vive el AudioManager que persiste en todas las escenas.

### Escena Interior
- Aquí ocurre todo el gameplay.
- El fantasma patrulla, persigue y reacciona a la luz.
- Aparecen los ítems de recarga y poder.

---

## 👻 El Fantasma — Sistema de IA

El fantasma tiene 3 comportamientos que cambian dinámicamente:

### 1. Patrullaje (Patrol)
- Estado por defecto cuando no detecta al jugador.
- Recorre una lista aleatoria de puntos de patrullaje (usando Fisher-Yates Shuffle).
- Camina a patrolSpeed (1.5 u/s por defecto).
- Hace pequeñas pausas aleatorias entre puntos (0.8–2.5 segundos).

### 2. Escuchar al jugador (Hear)
- Si el jugador se mueve dentro de hearingRange (5u), el fantasma lo escucha.
- El sonido atraviesa paredes.
- Interrumpe la patrulla y camina a la última posición donde escuchó al jugador.
- Muestra el indicador "?" sobre su cabeza.
- Alerta en pantalla: "Cuidado... el fantasma te escuchó"
- Sonido: Inicia SonPersecucion.

### 3. Ver al jugador (Chase)
- Si el jugador está dentro de visionRange (4u) y sin paredes de por medio (Raycast).
- El fantasma corre a chaseSpeed (2.8 u/s) directamente hacia el jugador.
- Muestra el indicador "!" sobre su cabeza.
- Alerta en pantalla: "!CORRE! El fantasma te vio"
- Sonido: Inicia SonPersecucion.
- Si pierde al jugador de vista → vuelve a patrullar y detiene SonPersecucion.

### 4. Huir (Modo Cazador activo)
- Cuando el jugador tiene el Ítem de Poder activo.
- El fantasma evalúa todos los puntos de patrullaje y corre al más lejano del jugador.
- Garantizado que no sale del mapa (todos los puntos son interiores).

---

## 🔦 Sistema de Linterna

- Tienes 10 cargas por defecto.
- Cada Clic izquierdo consume 1 carga y activa la linterna por 1 segundo.
- La linterna actúa como un cono de luz hacia donde apunta el cursor.
- Si el fantasma está dentro del alcance (3.5u) y dentro del ángulo del cono (+-45 grados):
  - Sin Ítem de Poder: El fantasma queda aturdido 3 segundos (se vuelve amarillo).
  - Con Ítem de Poder: El fantasma pierde 1 punto de vida (se vuelve rojo).
- Las cargas se recargan recogiendo el FlashlightItem que aparece en el mapa.

TIP: Usa la linterna con cuidado: aturdirlo no lo mata, pero puede salvarte cuando te está alcanzando.

---

## ❤️ Barra de Vida del Fantasma

- Visible en la esquina superior izquierda (Slider de UI).
- 8 puntos de vida por defecto.
- Color cambia según vida restante:
  - Verde (>50%) → Naranja (>25%) → Rojo (<25%)
- Solo se puede reducir con el Ítem de Poder activo.
- Cada golpe de linterna muestra: "Le quitaste X/8 vida al fantasma!"

---

## ⚡ Sistema de Ítems

### Ítem de Recarga (Linterna)
- Aparece cada 30 segundos en un punto aleatorio del mapa.
- Solo hay uno activo a la vez (el anterior desaparece al aparecer el nuevo).
- Al recogerlo → recarga las cargas de la linterna.
- Prefab: FlashlightItem (carpeta 02_Prefabs).

### Ítem de Poder (Cazador)
- Aparece cada 15 segundos en un punto aleatorio del mapa.
- Solo hay uno activo a la vez.
- Al recogerlo → activa el Modo Cazador por 10 segundos.
- El ítem desaparece solo después de 25 segundos si no es recogido.
- Prefab: PowerItem (carpeta 02_Prefabs).

---

## 🎯 Modo Cazador

Activo por 10 segundos tras recoger el Ítem de Poder.

Cambios mientras dura:
- Overlay pantalla: Amarillo pulsante (en lugar de rojo)
- Texto modo: ">> MODO CAZADOR <<"
- Linterna sobre fantasma: Le hace daño (en lugar de aturdirlo)
- Comportamiento fantasma: Huye del jugador (en lugar de perseguirlo)

Al expirar, todo vuelve a la normalidad automáticamente.

---

## 🧭 Flechas Indicadoras

Dos pequeños sprites que orbitan al jugador a corta distancia:

- FlashlightArrow (blanca): Apunta al ítem de recarga
- PowerArrow (amarilla): Apunta al ítem de poder

- Invisibles cuando no hay ítems en el mapa.
- Se actualizan cada frame siguiendo la dirección exacta al ítem.
- Permiten orientarse sin ver todo el mapa.

---

## 🎨 Sistema de UI

### Elementos en pantalla
- TimerText (esquina superior derecha): Tiempo restante en MM:SS
- FlashlightText (esquina superior izquierda): Cargas actuales / Max
- GhostHealthBar (esquina superior izquierda): Vida del fantasma - Slider
- DangerOverlay (cubre toda la pantalla): Rojo = peligro, Amarillo = cazador
- ModeText (centro superior): Estado actual del juego
- AlertText (centro pantalla): Mensajes temporales con fade

### Alertas dinámicas (se desvanecen solos)
- "!CORRE! El fantasma te vio" → cuando empieza persecución visual
- "Cuidado... el fantasma te escucho" → cuando el fantasma oye al jugador
- "Aturdiste al fantasma! 3 segundos..." → al aturdir con linterna
- "Le quitaste X/8 vida al fantasma!" → al hacer daño en modo cazador
- "Ahora puedes daniar al fantasma!" → al activar modo cazador
- "!VICTORIA! Derrotaste al fantasma!" → al llegar a 0 vidas

---

## 🔊 Sistema de Audio

Todos los sonidos están en: Assets/05_Sonidos/

| Sonido | Cuando suena | Comportamiento |
|---|---|---|
| SonSuspenso | Siempre, en todas las escenas | Loop permanente, baja al 20% durante persecucion |
| SonPersecucion | Cuando el fantasma ve/escucha al jugador | Loop, se detiene cuando pierde al jugador |
| Pasos | Mientras el jugador se mueve | Loop, se detiene al frenar |
| SonGameOver | Al perder | One-shot, detiene todo lo demas |
| SonGanar | Al ganar (por cualquier condicion) | One-shot, detiene todo lo demas |
| SonTeclado | Mientras el dialogo escribe letra a letra | Loop, se detiene al terminar o saltar |

El AudioManager usa DontDestroyOnLoad y persiste entre escenas.

---

## 📁 Estructura de Scripts

| Script | Responsabilidad |
|---|---|
| GameManager.cs | Singleton. Timer, spawn de items, UI global, estados Win/GameOver, modo cazador |
| EnemyIA.cs | IA del fantasma: patrulla, oye, ve, huye, recibe daño |
| Player.cs | Movimiento, linterna, ruido, flechas indicadoras |
| AudioManager.cs | Singleton. Todos los AudioSources del juego, persiste entre escenas |
| CameraFollow.cs | Sigue al jugador con suavizado, zoom con scroll limitado |
| Dialogue.cs | Sistema de dialogos con typewriter effect y bloqueo de movimiento |
| FlashlightItem.cs | Item de recarga de linterna al tocarlo |
| PowerItem.cs | Item de poder: activa Modo Cazador 10 segundos |
| ItemIndicator.cs | Controla las flechas orbitales que apuntan a los items |
| LanternHand.cs | Sprite de mano/linterna que rota hacia el cursor al disparar |

---

## 🐛 Bugs Conocidos y Soluciones

| Problema | Causa | Solucion |
|---|---|---|
| Fantasma sale del mapa al huir | Logica de huida usaba direccion libre | Ahora corre al punto de patrulla mas lejano |
| Textos encima del panel Win/GameOver | Los textos de UI no se limpiaban | Se limpian en TriggerWin() y TriggerGameOver() |
| Cuadraditos en lugar de emojis | LiberationSans no soporta Unicode | Reemplazados por texto ASCII puro |
| Fantasma no detecta al jugador | Tag del Player no era "Player" | Asegurarse de que el Player tenga el Tag correcto |
| Panel de reinicio no funciona | GameManager no asignado al boton | Arrastrar GameManager al evento OnClick() del boton |

---

Documentacion generada el 15 de abril de 2026 — GameJam Luken
