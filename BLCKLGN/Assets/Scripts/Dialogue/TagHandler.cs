using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// =====================================================
// TagHandler — BLCKLGN
// Paso 5.3: lee los tags de cada línea y los manda a quien corresponda.
//
// Cada tag viene como "clave: valor" (ej. "bg: set_tv").
// Este script separa la clave del valor y, según la clave,
// cambia el fondo, el personaje, el nombre, el sonido, etc.
//
// REGLA DE ASSETS: el nombre del archivo = el valor del tag.
//   #bg: set_tv            -> Assets/Art/Backgrounds/.../set_tv
//   #sprite: roses_botella -> Assets/Art/Characters/.../roses_botella
//   #cg: anomalia_cristal  -> Assets/Art/CG/.../anomalia_cristal
//   #sfx: aplausos_publico -> Assets/Audio/SFX/.../aplausos_publico
//   #amb: foro_zumbido     -> Assets/Audio/Ambience/.../foro_zumbido
//   #bgm: tema_final       -> Assets/Audio/Music/.../tema_final
// Los busca a través de la BibliotecaAssets.
// Si falta un archivo, sale un aviso amarillo en la consola (no truena).
// =====================================================
public class TagHandler : MonoBehaviour
{
    [Header("Assets")]
    [Tooltip("Arrastra aquí el archivo BibliotecaAssets")]
    [SerializeField] private BibliotecaAssets biblioteca;

    [Header("Diálogo")]
    [SerializeField] private TMP_Text textoNombre;
    [Tooltip("Opcional: el panel del nombre, para ocultarlo cuando nadie habla")]
    [SerializeField] private GameObject cajaNombre;
    [SerializeField] private TMP_Text textoDialogo;
    [Tooltip("Speakers que NO muestran caja de nombre (narración de Oliver)")]
    [SerializeField] private List<string> idsSinCajaDeNombre = new List<string> { "oliver_vo" };

    [Header("Imágenes")]
    [SerializeField] private Image fondo;
    [SerializeField] private Image imagenCG;
    [Tooltip("Objeto con Horizontal Layout Group donde aparecen los personajes")]
    [SerializeField] private Transform contenedorPersonajes;

    [Header("Títulos")]
    [SerializeField] private TMP_Text tituloEscena;
    [SerializeField] private TMP_Text tituloActo;
    [SerializeField] private float tiempoVisibleTitulo = 2f;
    [SerializeField] private float duracionFade = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource fuenteSFX;
    [SerializeField] private AudioSource fuenteAmbiente;
    [SerializeField] private AudioSource fuenteMusica;

    // Tags que todavía no se implementan aquí. Otros scripts se pueden
    // "suscribir" a estos eventos (cámara, efectos, transiciones, QTE).
    public event Action<string> OnCamara;
    public event Action<string> OnVFX;
    public event Action<string> OnTransicion;
    public event Action<Dictionary<string, string>> OnQTE; // paso 6

    // ID del speaker -> nombre que se ve en pantalla.
    // Los que terminan en _vo se muestran en itálica (voz en off).
    private static readonly Dictionary<string, string> nombres = new Dictionary<string, string>
    {
        { "oliver", "Oliver" },
        { "oliver_vo", "Oliver" },
        { "alex", "Alex" },
        { "bennedict", "Bennedict" },
        { "noah", "Noah" },
        { "roses", "Roses" },
        { "presentador", "Presentador" },
        { "camarografo_vo", "Camarógrafo" },
        { "tecnico_os", "Técnico" },
        { "stiller", "Stiller" },
        { "asistente", "Asistente" },
        { "estudiante_1", "Estudiante 1" },
        { "estudiante_2", "Estudiante 2" },
    };

    private readonly Dictionary<string, Image> personajesEnPantalla = new Dictionary<string, Image>();
    private readonly Dictionary<TMP_Text, Coroutine> fadesActivos = new Dictionary<TMP_Text, Coroutine>();

    private void Awake()
    {
        if (biblioteca == null)
            Debug.LogError("[Tags] Falta asignar la BibliotecaAssets en el TagHandler.");

        if (imagenCG != null) imagenCG.gameObject.SetActive(false);
        if (tituloEscena != null) tituloEscena.gameObject.SetActive(false);
        if (tituloActo != null) tituloActo.gameObject.SetActive(false);
        LimpiarSpeaker();
    }

    // ----------------------------------------------------
    // PUNTO DE ENTRADA: el InkManager llama esto en cada línea.
    // Regresa cuántos segundos esperar (#wait), o 0.
    // ----------------------------------------------------
    public float ProcesarTags(List<string> tags)
    {
        LimpiarSpeaker(); // cada línea trae su propio #speaker

        float espera = 0f;
        Dictionary<string, string> datosQTE = null;

        foreach (string tag in tags)
        {
            Separar(tag, out string clave, out string valor);

            // Todos los tags que empiezan con "qte" se juntan para el QTEManager
            if (clave.StartsWith("qte"))
            {
                if (datosQTE == null) datosQTE = new Dictionary<string, string>();
                datosQTE[clave] = valor;
                continue;
            }

            switch (clave)
            {
                case "speaker":    PonerSpeaker(valor); break;
                case "bg":         CambiarFondo(valor); break;
                case "sprite":     MostrarPersonaje(valor); break;
                case "hide":       OcultarPersonaje(valor); break;
                case "cg":         MostrarCG(valor); break;
                case "title":      MostrarTitulo(tituloEscena, valor); break;
                case "act":        MostrarTitulo(tituloActo, valor); break;
                case "sfx":        TocarSFX(valor); break;
                case "amb":        CambiarLoop(fuenteAmbiente, valor, biblioteca.Ambiente); break;
                case "bgm":        CambiarLoop(fuenteMusica, valor, biblioteca.Musica); break;
                case "wait":       espera = LeerNumero(valor, tag); break;
                case "cam":        Reenviar(OnCamara, "cam", valor); break;
                case "vfx":        Reenviar(OnVFX, "vfx", valor); break;
                case "transition": Reenviar(OnTransicion, "transition", valor); break;
                default:
                    Debug.LogWarning($"[Tags] Tag desconocido: '#{tag}'. ¿Está mal escrito?");
                    break;
            }
        }

        if (datosQTE != null)
        {
            if (OnQTE != null) OnQTE.Invoke(datosQTE);
            else Debug.Log("[Tags] QTE detectado (se implementa en el paso 6): " + string.Join(", ", datosQTE));
        }

        return espera;
    }

    // "bg: set_tv"  ->  clave = "bg", valor = "set_tv"
    // Solo corta en el PRIMER ':' para que "title: 1.1: La entrevista" funcione.
    private void Separar(string tag, out string clave, out string valor)
    {
        int i = tag.IndexOf(':');
        if (i < 0)
        {
            clave = tag.Trim().ToLower();
            valor = "";
            return;
        }
        clave = tag.Substring(0, i).Trim().ToLower();
        valor = tag.Substring(i + 1).Trim();
    }

    // ----------------------------------------------------
    // NOMBRE / SPEAKER
    // ----------------------------------------------------
    private void PonerSpeaker(string id)
    {
        if (!nombres.TryGetValue(id, out string nombre))
        {
            Debug.LogWarning($"[Tags] Speaker '{id}' no está en la tabla de nombres del TagHandler.");
            nombre = id;
        }

        // La voz en off (_vo) siempre va en itálica
        textoDialogo.fontStyle = id.EndsWith("_vo") ? FontStyles.Italic : FontStyles.Normal;

        // Tarjeta "Caja de texto": cuando narra Oliver no aparece caja de nombre
        if (idsSinCajaDeNombre.Contains(id)) return; // se queda oculta (LimpiarSpeaker ya la ocultó)

        textoNombre.text = nombre;
        if (cajaNombre != null) cajaNombre.SetActive(true);
    }

    private void LimpiarSpeaker()
    {
        if (textoNombre != null) textoNombre.text = "";
        if (cajaNombre != null) cajaNombre.SetActive(false);
        if (textoDialogo != null) textoDialogo.fontStyle = FontStyles.Normal;
    }

    // ----------------------------------------------------
    // IMÁGENES
    // ----------------------------------------------------
    private void CambiarFondo(string id)
    {
        Sprite s = biblioteca.Fondo(id);
        if (s != null) fondo.sprite = s;
    }

    private void MostrarCG(string id)
    {
        if (id == "ocultar")
        {
            imagenCG.gameObject.SetActive(false);
            return;
        }
        Sprite s = biblioteca.CG(id);
        if (s == null) return;
        imagenCG.sprite = s;
        imagenCG.gameObject.SetActive(true);
    }

    // "roses_botella" -> personaje "roses", expresión "botella".
    // Si Roses ya está en pantalla, solo cambia su imagen.
    private void MostrarPersonaje(string id)
    {
        string personaje = id.Split('_')[0];
        Sprite s = biblioteca.Personaje(id);
        if (s == null) return;

        if (!personajesEnPantalla.TryGetValue(personaje, out Image img))
        {
            GameObject go = new GameObject(personaje, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(contenedorPersonajes, false);
            img = go.GetComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false; // para que no bloquee el clic de avanzar
            personajesEnPantalla[personaje] = img;
        }
        img.sprite = s;
    }

    // "#hide: alex" quita a Alex.  "#hide: todos" limpia la pantalla.
    private void OcultarPersonaje(string personaje)
    {
        if (personaje == "todos")
        {
            foreach (Image img in personajesEnPantalla.Values) Destroy(img.gameObject);
            personajesEnPantalla.Clear();
            return;
        }
        if (personajesEnPantalla.TryGetValue(personaje, out Image imagen))
        {
            Destroy(imagen.gameObject);
            personajesEnPantalla.Remove(personaje);
        }
    }

    // ----------------------------------------------------
    // TÍTULOS (aparece, se queda un rato y se desvanece)
    // ----------------------------------------------------
    private void MostrarTitulo(TMP_Text texto, string contenido)
    {
        if (texto == null) return;
        if (fadesActivos.TryGetValue(texto, out Coroutine anterior) && anterior != null)
            StopCoroutine(anterior);
        fadesActivos[texto] = StartCoroutine(TituloConFade(texto, contenido));
    }

    private IEnumerator TituloConFade(TMP_Text texto, string contenido)
    {
        texto.text = contenido;
        texto.alpha = 1f;
        texto.gameObject.SetActive(true);

        yield return new WaitForSeconds(tiempoVisibleTitulo);

        float t = 0f;
        while (t < duracionFade)
        {
            t += Time.deltaTime;
            texto.alpha = 1f - (t / duracionFade);
            yield return null;
        }
        texto.gameObject.SetActive(false);
    }

    // ----------------------------------------------------
    // AUDIO
    // ----------------------------------------------------
    private void TocarSFX(string id)
    {
        AudioClip clip = biblioteca.SFX(id);
        if (clip != null) fuenteSFX.PlayOneShot(clip);
    }

    // Ambiente y música se repiten en loop. "#amb: stop" los detiene.
    private void CambiarLoop(AudioSource fuente, string id, Func<string, AudioClip> buscar)
    {
        if (id == "stop")
        {
            fuente.Stop();
            return;
        }
        AudioClip clip = buscar(id);
        if (clip == null) return;
        fuente.clip = clip;
        fuente.loop = true;
        fuente.Play();
    }

    // ----------------------------------------------------
    // UTILIDADES
    // ----------------------------------------------------
    // InvariantCulture: "3.5" se lee igual en cualquier computadora
    private float LeerNumero(string valor, string tag)
    {
        if (float.TryParse(valor, NumberStyles.Float, CultureInfo.InvariantCulture, out float n))
            return n;
        Debug.LogWarning($"[Tags] '#{tag}' no tiene un número válido.");
        return 0f;
    }

    private void Reenviar(Action<string> evento, string clave, string valor)
    {
        if (evento != null) evento.Invoke(valor);
        else Debug.Log($"[Tags] #{clave}: {valor} (pendiente: nadie lo escucha todavía)");
    }
}
