
# 📘 Documentación

## 🧭 Descripción general

La clase `Form_TimerOverlay` es el núcleo del proyecto **TimerOverlay**, encargada de representar **cuadros flotantes** que funcionan como temporizadores, cronómetros o contadores interactivos en pantalla.  
Cada instancia de `Form_TimerOverlay` actúa como una ventana independiente y permite personalizar su comportamiento, aspecto y reacciones ante eventos del teclado global.

Los cuadros pueden ser posicionados donde desees.  
Para activar el menú, tienes que hacer **click derecho** sobre una parte visible del cuadro y se desplegará el menú contextual con las opciones para personalizar:

<p align="center">
  <img src="https://github.com/jechram5588/TimerOverlay/blob/master/TimerOverlay/imgs/paladinConfig.PNG" alt="Perfil de ejemplo" width="400"/>
</p>

<p align="center"><em>Figura 1: Perfil de ejemplo</em></p>

## ⚖️ Aviso de responsabilidad

El código aquí compartido es **de uso libre** para todos los interesados.  
Cada persona que descargue o modifique este proyecto es **totalmente responsable** por el uso que haga del mismo y de cualquier consecuencia derivada.

Este proyecto **no interfiere con las reglas de Tibia**, ya que únicamente muestra temporizadores basados en timers configurados por el propio usuario.  
Es una herramienta de ayuda visual para que cada jugador pueda mejorar su habilidad y gestión del tiempo, pero **la jugabilidad sigue dependiendo enteramente del jugador**.  

⚠️ **No me hago responsable** de daños, errores o mal funcionamiento que puedan surgir al modificar o utilizar el código de forma inapropiada.

Se recomienda revisar y probar cuidadosamente cualquier cambio antes de ponerlo en producción o en entornos sensibles.

 ⚠️ Limitación para motor gráfico OpenGL en pantalla completa

Actualmente, los temporizadores **no se pueden mostrar en el tope** cuando la aplicación se ejecuta usando el motor gráfico **OpenGL en modo pantalla completa (fullscreen)**.

Esto se debe a restricciones propias del motor gráfico que impiden que los cuadros flotantes (timers) se superpongan correctamente en esta configuración.

Para evitar este problema, se recomienda usar el modo ventana (windowed) o un motor gráfico diferente si se necesita visualizar los temporizadores en la parte superior de la pantalla.


## ⚙️ Funcionalidades principales

| Categoría | Descripción |
|------------|-------------|
| **Temporización** | Controla conteo ascendente o descendente mediante `Timer` de Windows Forms. |
| **Modos de contador** | Admite cuatro tipos: temporizador, cronómetro, contador incremental y decremental. |
| **Animaciones visuales** | Parpadeo, mostrar imagen fija o mostrar texto con cuenta. |
| **Reproducción de sonido** | Usa **NAudio** para reproducir archivos `.wav` en eventos de alerta o cuenta. |
| **Teclas globales** | Se integra con `GlobalKeyboardHook` para ejecutar acciones aunque la ventana no tenga foco. |
| **Menú contextual** | Permite cambiar imagen, sonido, tamaño, animación y modo directamente con clic derecho. |
| **Persistencia de perfiles** | Guarda y carga configuraciones mediante `TimerStorage` y perfiles JSON. |

---

## 🧩 Estructura interna de la clase

### 🔹 Campos principales

| Campo | Tipo | Descripción |
|--------|------|-------------|
| `timerRun` | `Timer` | Controla el conteo principal (1 segundo por tick). |
| `timerStop` | `Timer` | Controla las animaciones de finalización. |
| `tipoContador` | `enum TipoContador` | Define el modo de funcionamiento. |
| `tipoAnimacion` | `enum TipoAnimacion` | Controla la animación visual activa. |
| `ctrolImgTxt` | `ImageTextControl` | Control que muestra imagen y texto. |
| `imagenIcono` | `Image` | Imagen actual mostrada. |
| `sonidoAlerta` / `sonidoCD` | `string` | Archivos de sonido reproducidos en alerta o cuenta. |
| `teclaGlobal` | `Keys` | Tecla asignada globalmente para activar el cuadro. |
| `instancias` | `List<Form_TimerOverlay>` | Lista de cuadros activos (para controlar todos simultáneamente). |

---

## 🧠 Enumeraciones internas

### `TipoContador`
Define el comportamiento del cuadro:
- `Temporizador` → Cuenta regresiva desde un tiempo inicial.  
- `Cronometro` → Cuenta progresiva hasta un límite.  
- `ContadorIncremental` → Incrementa el número con una tecla global.  
- `ContadorDecremental` → Decrementa el número hasta 0.

### `TipoAnimacion`
Controla el efecto visual mostrado:
- `Ninguna` → Imagen estática.  
- `Mostrar` → Muestra imagen fija al finalizar.  
- `Parpadear` → Alterna visibilidad (blink).  
- `MostrarConCD` → Muestra texto del conteo con imagen.  
- `ParpadearConCD` → Parpadeo combinado con texto.

---

## 🧱 Ciclo de vida del formulario

1. **Creación:**  
   - El constructor recibe un objeto `TimerData` con la configuración del cuadro (tiempo, posición, imagen, sonidos, etc.).  
   - Se inicializan los temporizadores, el control visual y el menú contextual.

2. **Ejecución:**  
   - `timerRun_Tick()` se ejecuta cada segundo actualizando el conteo.  
   - Dependiendo del modo, incrementa, decrementa o detiene el conteo.  
   - Al llegar al límite, activa la animación y reproduce el sonido configurado.

3. **Finalización:**  
   - `timerStop_Tick()` ejecuta animaciones como parpadeo.  
   - Se reinicia o se espera nueva interacción según configuración.

---

## 🎛️ Eventos clave

| Evento | Descripción |
|---------|--------------|
| `timerRun_Tick()` | Actualiza el valor de tiempo o contador en cada tick. |
| `timerStop_Tick()` | Ejecuta animación visual al finalizar. |
| `GlobalKeyboardHook_OnKeyPressed()` | Detecta teclas globales y aplica acciones (reiniciar, contar, etc.). |
| `Form_MouseDown/Move/Up()` | Permite mover el cuadro arrastrándolo. |
| `Control_MouseClick()` | Muestra el menú contextual. |
| `OnLoad()` / `OnFormClosed()` | Agrega o remueve la instancia de la lista global. |

---

## 🔊 Reproducción de sonido

La función `ReproduceSonido(string archivo)`:
- Usa la librería **NAudio** para leer y reproducir el archivo.  
- Implementa un `Timer` auxiliar para detener el sonido automáticamente tras un breve lapso.  
- Libera recursos al finalizar la reproducción.

---
# 📁 Carpeta `assets/`

Esta carpeta está destinada a almacenar archivos de audio que se utilizarán en los ⏱️ **temporizadores (timers)** de la aplicación.



## 🎵 Formatos permitidos

- `.wav`
- `.mp3`

## 🛠️ Instrucciones

1. 📥 Coloca aquí los archivos de sonido que desees usar como alertas para los temporizadores.
2. 📝 Asegúrate de que los archivos tengan nombres descriptivos (por ejemplo: `alarma.mp3`, `notificacion.wav`, etc.).


## ✅ Recomendaciones

- ⏳ Usa sonidos cortos (menos de 5 segundos) la app solo reproduce 1.5 segundos para evitar interrupciones innecesarias.


## 🧩 Menú contextual principal

Cada instancia de `Form_TimerOverlay` incluye un menú contextual accesible con clic derecho.  
Principales opciones:

| Opción | Función |
|--------|----------|
| **Agregar** | Crea un nuevo cuadro de temporizador. |
| **Cerrar / Cerrar todo** | Cierra una o todas las instancias activas. |
| **Modificar tiempo / contador / tamaño** | Cambia parámetros del cuadro. |
| **Asignar sonido / imagen / tecla / animación** | Personaliza la apariencia y el comportamiento. |
| **Cambiar modo** | Alterna entre los cuatro modos disponibles. |
| **Reiniciar / Pausar / Reanudar** | Controla el flujo de conteo. |
| **Exportar / Cargar perfil** | Guarda o restaura configuraciones. |
| **Salir** | Cierra toda la aplicación. |

---

## 🪄 Métodos auxiliares relevantes

| Método | Propósito |
|--------|------------|
| `Reiniciar()` | Reinicia el contador o temporizador al valor inicial. |
| `Pausar()` / `Reanudar()` | Detiene o continúa el conteo. |
| `ReiniciarTodos()` / `PausarTodos()` | Aplica acciones globales a todas las instancias. |
| `ExportarTodos()` | Guarda el estado actual en un perfil JSON. |
| `CargarPerfil()` | Carga configuraciones desde un perfil almacenado. |
| `FormatearTiempo()` | Convierte los segundos a formato mm:ss. |
| `FormateaContador()` | Actualiza el texto del contador según su valor actual. |

---

## 🧩 Integración con `GlobalKeyboardHook`

La clase `Form_TimerOverlay` se conecta con `GlobalKeyboardHook` para detectar **atajos de teclado globales** incluso cuando la ventana no está activa.

- `GlobalKeyboardHook.Start()` inicia la captura de teclado.  
- Cada vez que se presiona una tecla, se lanza el evento `OnKeyPressed(Keys key)`.  
- `Form_TimerOverlay` escucha este evento y compara la tecla con la asignada (`teclaGlobal`).  
- Según el modo activo, reinicia el contador, incrementa o reproduce un sonido.

---

## 🧱 Perfiles

- Los cuadros pueden **exportarse** y **restaurarse** con todos sus parámetros visuales y funcionales.  
- Usa `TimerStorage.Guardar()` y `TimerStorage.CargarPerfil()` para manejar archivos `.json`.  
- Guarda: posición, imagen, tamaño, fuente, modo, animación, sonidos y tecla asignada.

📁 Carpeta `profiles/`

Dentro de la carpeta `profiles/` se almacenan todos los perfiles de temporizadores creados o guardados por el usuario.  
Cada perfil se guarda como un archivo `.json` que contiene la configuración completa de uno o varios cuadros temporizadores.

---

### 🧾 ¿Qué contiene cada archivo?

Cada archivo `.json` incluye información como:

- 📍 Posición en pantalla (`X`, `Y`)
- ⏱️ Tiempo configurado
- 🖼️ Imagen codificada en **Base64**
- 🔊 Nombre del sonido de alerta asociado
- 🧩 Función del temporizador (Temporizador, Cronómetro, Contador, etc.)
- ⌨️ Tecla global asignada
- 🔢 Contador inicial
- 📐 Dimensiones (`Ancho`, `Alto`)
- 🔤 Tamaño de la fuente
- 🎞️ Tipo de animación
- 🎵 Sonido para el cooldown (si aplica)

---

### 💡 Ejemplo de estructura JSON

```json
{
  "X": 881,
  "Y": 172,
  "Tiempo": 595,
  "ImagenBase64": "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAB...",
  "SonidoAlerta": "bling.wav",
  "Funcion": "Temporizador",
  "Tecla": 123,
  "Contador": 10,
  "Alto": 150,
  "Ancho": 150,
  "TamañoFuente": 20,
  "Animacion": "MostrarConCD",
  "SonidoCD": "Ninguno"
}
```
### 📤 Compartir perfiles

Los archivos `.json` en la carpeta `profiles/` están diseñados para ser **compartidos fácilmente** entre usuarios.

🔐 Las imágenes se codifican dentro del archivo en formato **Base64**, por lo que no necesitas enviar archivos separados para que otro usuario vea la misma imagen al cargar el perfil.

Para compartir un perfil:

1. 📁 Ubica el archivo `.json` dentro de tu carpeta `profiles/`.
2. 📤 Envíalo a otro jugador (por correo, Discord, etc.).
3. 📥 El otro jugador debe colocar ese archivo en su propia carpeta `profiles/`.

✅ Al cargar el perfil, se restaurarán la posición, imagen, función y configuración exacta del temporizador.


## 🔧 Dependencias

- **NAudio.Wave** — para reproducción de audio.  
- **System.Windows.Forms** — para controles gráficos.  
- **TimerStorage** — para persistencia de perfiles.  
- **GlobalKeyboardHook** — para atajos globales.  

---

## 🧭 Resumen de flujo general

```text
┌────────────────────────────────────────┐
│    Se crea Form_TimerOverlay           │
├────────────────────────────────────────┤
│ Carga parámetros de TimerData          │
│ Inicializa controles y menú            │
│ Inicia temporizadores                  │
├────────────────────────────────────────┤
│ Cada tick: actualiza tiempo            │
│ Si llega al límite: sonido + animación │
├────────────────────────────────────────┤
│ Detecta teclas globales                │
│ → Reinicia, incrementa o decrementa    │
├────────────────────────────────────────┤
│ Permite exportar/cargar perfiles       │
└────────────────────────────────────────┘
```

---

## 🧩 Clase relacionada: `GlobalKeyboardHook`

- Implementa un **hook global de teclado de Windows (WH_KEYBOARD_LL)**.  
- Captura la pulsación de teclas (`WM_KEYUP`) incluso fuera del foco de la aplicación.  
- Expone el evento público `OnKeyPressed(Keys key)` para notificar a `Form_TimerOverlay`.  
- Se utiliza para sincronizar acciones entre varios cuadros del overlay.

## 🔗 Descarga del compilado

Para la version compilada Google Drive
[TimerOverlay 1.0.0.0](https://drive.google.com/drive/folders/1UDc2Sgln58FfGDIaIzJRRJ1dIxqwkd5w?usp=sharing)

# 🙌 Apoya Esta Herramienta

Esta pequeña app la hice por mi cuenta, programando la mayor parte del código yo mismo, aunque mucho del código fue generado con ayuda de IA.  
Invertí mi tiempo libre con el objetivo de mejorar un poco la experiencia de juego para todos. No es un gran proyecto, simplemente una herramienta pensada para que todo lo que ofrece esté disponible in-game lo más pronto posible.

🎁 **Si te gustó y crees que valió la pena**, puedes apoyarme regalándome Tibia Coins directamente a mi personaje:

`Arti Shooter`

Usaré las coins para mejorar el set de mi char y seguir disfrutando del juego. 💛

¡Gracias por probarla y que tengas buena cacería siempre! 🙏
