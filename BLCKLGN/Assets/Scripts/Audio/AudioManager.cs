// ============================================================
// AUDIOMANAGER - BLCKLGN
// Tres canales independientes (Music, SFX, Ambience).
// El guion activa los sonidos por nombre: sfx: aplausos, music: tema_menu, amb: lluvia
// ============================================================

using System;                       
using System.Collections;           
using System.Collections.Generic;   
using UnityEngine;                  
using UnityEngine.Audio;            // Necesario para AudioMixer y AudioMixerGroup

public class AudioManager : MonoBehaviour  
{
    public static AudioManager Instance { get; private set; }

    // ---------- CLASE AUXILIAR ----------

    [Serializable]                  // Hace que Unity pueda mostrar y guardar esta clase en el Inspector
    public class NamedClip          // Une un nombre (el que usa el guion) con un clip de audio
    {
        public string name;         // Nombre con el que el guion llama al sonido
        public AudioClip clip;      // El archivo de audio
    }

    // ---------- CAMPOS DEL INSPECTOR ----------

    // El mixer y sus grupos se asignan a mano en el Inspector (arrastrando el grupo, no el mixer completo).
    // Cada canal sale por su propio grupo para poder controlar su volumen por separado.
    [Header("Mixer")]                                   // Título que se ve en el Inspector
    [SerializeField] private AudioMixer mixer;          // El mixer completo (para cambiar volúmenes)
    [SerializeField] private AudioMixerGroup music_group;     // Grupo Music
    [SerializeField] private AudioMixerGroup sfx_group;       // Grupo SFX
    [SerializeField] private AudioMixerGroup ambience_group;  // Grupo Ambience

    // Deben coincidir exactamente (mayúsculas y guion bajo) con los nombres en Exposed Parameters.
    // Si no coinciden, SetFloat falla sin avisar y el volumen no cambia.
    [Header("Parámetros expuestos en el Mixer")]
    [SerializeField] private string music_param = "Music_Vol";
    [SerializeField] private string sfx_param = "SFX_Vol";
    [SerializeField] private string ambience_param = "Ambience_Vol";

    // Las listas son lo que tú llenas a mano en el Inspector (nombre + clip)
    [Header("Biblioteca de sonidos")]
    [SerializeField] private List<NamedClip> music = new List<NamedClip>();     // Sonidos de música
    [SerializeField] private List<NamedClip> sfx = new List<NamedClip>();       // Efectos de sonido
    [SerializeField] private List<NamedClip> ambience = new List<NamedClip>();  // Sonidos ambiente

    // ---------- CAMPOS INTERNOS ----------

    // Los diccionarios son los que usa el código para buscar un clip por su nombre.
    // Empiezan vacíos y se llenan en Awake copiando lo que hay en las listas.
    // readonly = la variable no puede apuntar a otro diccionario, pero sí puedes agregarle elementos.
    private readonly Dictionary<string, AudioClip> music_lib = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioClip> sfx_lib = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioClip> amb_lib = new Dictionary<string, AudioClip>();

    private AudioSource music_source;      // Reproductor del canal de música
    private AudioSource ambience_source;   // Reproductor del canal de ambiente
    private AudioSource sfx_source;        // Reproductor del canal de efectos

    private Coroutine music_fade;          // Guarda el fade de música en curso (para cancelarlo si llega otro)
    private Coroutine ambience_fade;       // Lo mismo para el ambiente

    // ---------- INICIALIZACIÓN ----------

    // Awake lo llama Unity solo, una vez, al crearse el objeto y antes de Start
    private void Awake()
    {
        // Si ya existe otro AudioManager, este sobra: se destruye y se sale del método
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;                  // Este es el AudioManager oficial
        DontDestroyOnLoad(gameObject);    // No se destruye al cambiar de escena (el audio no se corta)

        // Crea un reproductor por canal, conectado a su grupo del mixer (true = hace loop)
        music_source = Create_Source("MusicSource", music_group, true);
        ambience_source = Create_Source("AmbienceSource", ambience_group, true);
        sfx_source = Create_Source("SFXSource", sfx_group, false);

        // Copia cada lista (Inspector) a su diccionario (búsqueda rápida)
        Build_Library(music, music_lib);
        Build_Library(sfx, sfx_lib);
        Build_Library(ambience, amb_lib);
    }

    // Start corre después de Awake, justo antes del primer frame
    private void Start()
    {
        // Restaura los volúmenes guardados (0.8 por defecto si es la primera vez)
        SetVolume(music_param, PlayerPrefs.GetFloat(music_param, 0.8f));
        SetVolume(sfx_param, PlayerPrefs.GetFloat(sfx_param, 0.8f));
        SetVolume(ambience_param, PlayerPrefs.GetFloat(ambience_param, 0.8f));
    }

    // Crea un GameObject hijo con un AudioSource ya conectado a un grupo del mixer
    private AudioSource Create_Source(string objName, AudioMixerGroup group, bool loop)
    {
        var go = new GameObject(objName);               // Crea un objeto vacío con ese nombre
        go.transform.SetParent(transform);              // Lo pone como hijo del AudioManager
        var src = go.AddComponent<AudioSource>();       // Le añade un AudioSource
        src.outputAudioMixerGroup = group;              // Lo conecta al grupo: así el mixer controla su volumen
        src.loop = loop;                                // Música y ambiente se repiten; SFX no
        src.playOnAwake = false;                        // No suena solo al arrancar
        return src;                                     // Devuelve el AudioSource creado
    }

    // Copia una lista de NamedClip a un diccionario nombre -> clip
    private void Build_Library(List<NamedClip> list, Dictionary<string, AudioClip> dict)
    {
        foreach (var item in list)   // Recorre cada fila de la lista
        {
            // Salta filas incompletas (sin nombre o sin clip)
            if (item == null || string.IsNullOrWhiteSpace(item.name) || item.clip == null) continue;
            // Guarda el clip con el nombre sin espacios y en minúsculas, para que "Aplausos " = "aplausos"
            dict[item.name.Trim().ToLowerInvariant()] = item.clip;
        }
    }

    // Busca un clip por nombre. Devuelve true si lo encuentra (y lo entrega en "clip")
    private bool Try_Get(Dictionary<string, AudioClip> lib, string clip_name, string tipo, out AudioClip clip)
    {
        clip = null;                                            // Valor inicial por si no se encuentra
        if (string.IsNullOrWhiteSpace(clip_name)) return false;  // Nombre vacío: no hay nada que buscar
        // Normaliza el nombre igual que en BuildLibrary y lo busca
        if (lib.TryGetValue(clip_name.Trim().ToLowerInvariant(), out clip)) return true;
        // Si no existe, avisa en la consola con el nombre que falló
        Debug.LogWarning($"[AudioManager] No existe el {tipo} '{clip_name}'. Revisa la biblioteca o la etiqueta del guion.");
        return false;
    }

    // ---------- REPRODUCCIÓN ----------

    public void Play_Music(string clipName, float fade = 1f)
    {
        if (!Try_Get(music_lib, clipName, "bgm", out var clip)) return;     // Si no existe, sale
        if (music_source.clip == clip && music_source.isPlaying) return;      // Si ya suena esa, no la reinicia
        Switch_Loop(ref music_fade, music_source, clip, fade);                 // Cambia con fade
    }

    // Detener la música con fade (clip null = silencio)
    public void Stop_Music(float fade = 1f) => Switch_Loop(ref music_fade, music_source, null, fade);

    public void Play_Ambience(string clipName, float fade = 1.5f)
    {
        if (!Try_Get(amb_lib, clipName, "ambience", out var clip)) return;
        if (ambience_source.clip == clip && ambience_source.isPlaying) return;
        Switch_Loop(ref ambience_fade, ambience_source, clip, fade);
    }

    public void Stop_Ambience(float fade = 1.5f) => Switch_Loop(ref ambience_fade, ambience_source, null, fade);

    public void Play_SFX(string clipName)
    {
        if (!Try_Get(sfx_lib, clipName, "sfx", out var clip)) return;
        sfx_source.PlayOneShot(clip);    // Suena una vez y permite que varios SFX se solapen
    }

    // Cancela el fade anterior (si lo hay) e inicia uno nuevo
    private void Switch_Loop(ref Coroutine handle, AudioSource src, AudioClip next, float fade)
    {
        if (handle != null) StopCoroutine(handle);          // Evita dos fades peleando por el mismo canal
        handle = StartCoroutine(FadeSwitch(src, next, fade));
    }

    // Corrutina: baja el volumen, cambia el clip y lo sube de nuevo
    // (el volume del AudioSource es solo para el fade; el volumen del jugador vive en el Mixer)
    private IEnumerator FadeSwitch(AudioSource src, AudioClip next, float fade)
    {
        float start = src.volume;   // Volumen desde el que empieza a bajar

        // Fade out: baja poco a poco mientras suene algo
        for (float t = 0; t < fade && src.isPlaying; t += Time.unscaledDeltaTime)
        {
            src.volume = Mathf.Lerp(start, 0f, t / fade);   // Interpola de "start" a 0
            yield return null;                              // Espera al siguiente frame
        }
        src.Stop();   // Silencio total

        // Si no hay clip nuevo (era un Stop), restaura el volumen y termina
        if (next == null) { src.volume = 1f; yield break; }

        src.clip = next;     // Pone el clip nuevo
        src.volume = 0f;     // Empieza en silencio
        src.Play();          // Lo reproduce

        // Fade in: sube poco a poco hasta el volumen completo
        for (float t = 0; t < fade; t += Time.unscaledDeltaTime)
        {
            src.volume = Mathf.Lerp(0f, 1f, t / fade);
            yield return null;
        }
        src.volume = 1f;     // Asegura el valor final exacto
    }

    // ---------- VOLUMEN POR CANAL (slider 0..1) ----------

    // Estas tres son las que conectas al On Value Changed de cada Slider
    public void SetMusicVolume(float v) => SetVolume(music_param, v);
    public void SetSFXVolume(float v) => SetVolume(sfx_param, v);
    public void SetAmbienceVolume(float v) => SetVolume(ambience_param, v);

    private void SetVolume(string param, float linear)
    {
        // El Mixer trabaja en decibeles: convierte 0..1 lineal a -80..0 dB
        // (Clamp evita 0, porque el logaritmo de 0 no existe)
        float db = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        mixer.SetFloat(param, db);              // Cambia el parámetro expuesto del mixer
        PlayerPrefs.SetFloat(param, linear);    // Guarda el valor para la próxima vez
    }

    // ---------- CONEXIÓN CON EL GUION ----------

    // Recibe una etiqueta del guion. Acepta "sfx:aplausos", "sfx aplausos" o "sfx: aplausos".
    // Usa "music: stop" / "amb: stop" para detener un canal.
    public void Handle_Tag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;     // Etiqueta vacía: no hace nada

        tag = tag.Trim().TrimStart('#');                // Quita espacios y el # inicial si lo trae
        int split = tag.IndexOfAny(new[] { ':', ' ' }); // Posición del primer ":" o espacio (separa tipo y nombre)
        if (split <= 0) { Debug.LogWarning($"[AudioManager] Etiqueta mal formada: '{tag}'"); return; }

        string kind = tag.Substring(0, split).Trim().ToLowerInvariant();   // Lo de antes del separador: sfx, music o amb
        string value = tag.Substring(split + 1).Trim();                    // Lo de después: el nombre del sonido
        bool stop = value.Equals("stop", StringComparison.OrdinalIgnoreCase);  // ¿Pide detener el canal?

        switch (kind)   // Decide qué función llamar según el tipo
        {
            case "sfx": Play_SFX(value); break;
            case "bgm": if (stop) Stop_Music(); else Play_Music(value); break;
            case "amb": if (stop) Stop_Ambience(); else Play_Ambience(value); break;
                // Otras etiquetas (bg, name...) no son de audio: se ignoran aquí
        }
    }
}