using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

// =====================================================
// BibliotecaAssets — BLCKLGN
// Un "catálogo" de todos los fondos, personajes, CG y audios del juego,
// para que el TagHandler los encuentre por su NOMBRE DE ARCHIVO.
//
//   #bg: set_tv            -> busca un archivo llamado set_tv  en Assets/Art/Backgrounds
//   #sprite: roses_botella -> busca roses_botella en Assets/Art/Characters
//   #cg: anomalia_cristal  -> busca en Assets/Art/CG
//   #sfx / #amb / #bgm     -> Assets/Audio/SFX, /Ambience, /Music
//
// Busca también dentro de subcarpetas (ej. Characters/Roses/roses_botella.png).
// No importan mayúsculas/minúsculas: "Set_TV" y "set_tv" son lo mismo.
//
// SE LLENA SOLA: cada vez que alguien agrega, borra o mueve un archivo
// dentro de Assets/Art o Assets/Audio (ver BibliotecaAssetsAutoRefresh).
// También se puede llenar a mano: clic en los ⋮ del Inspector → "Llenar desde carpetas".
// =====================================================
[CreateAssetMenu(fileName = "BibliotecaAssets", menuName = "BLCKLGN/Biblioteca de Assets")]
public class BibliotecaAssets : ScriptableObject
{
    [Header("Carpetas (respetan la estructura del proyecto)")]
    public string carpetaFondos = "Assets/Art/Backgrounds";
    public string carpetaPersonajes = "Assets/Art/Characters";
    public string carpetaCG = "Assets/Art/CG";
    public string carpetaSFX = "Assets/Audio/SFX";
    public string carpetaAmbiente = "Assets/Audio/Ambience";
    public string carpetaMusica = "Assets/Audio/Music";

    [Header("Se llenan solas — no editar a mano")]
    public List<Sprite> fondos = new List<Sprite>();
    public List<Sprite> personajes = new List<Sprite>();
    public List<Sprite> cgs = new List<Sprite>();
    public List<AudioClip> sfx = new List<AudioClip>();
    public List<AudioClip> ambiente = new List<AudioClip>();
    public List<AudioClip> musica = new List<AudioClip>();

    // Índices nombre -> asset (se arman la primera vez que se buscan)
    [NonSerialized] private Dictionary<string, Sprite> iFondos, iPersonajes, iCG;
    [NonSerialized] private Dictionary<string, AudioClip> iSFX, iAmbiente, iMusica;

    private void OnEnable() => BorrarIndices();

    // ----------------------------------------------------
    // BÚSQUEDAS (las usa el TagHandler)
    // ----------------------------------------------------
    public Sprite Fondo(string id)     => Buscar(ref iFondos, fondos, id, carpetaFondos);
    public Sprite Personaje(string id) => Buscar(ref iPersonajes, personajes, id, carpetaPersonajes);
    public Sprite CG(string id)        => Buscar(ref iCG, cgs, id, carpetaCG);
    public AudioClip SFX(string id)      => Buscar(ref iSFX, sfx, id, carpetaSFX);
    public AudioClip Ambiente(string id) => Buscar(ref iAmbiente, ambiente, id, carpetaAmbiente);
    public AudioClip Musica(string id)   => Buscar(ref iMusica, musica, id, carpetaMusica);

    private T Buscar<T>(ref Dictionary<string, T> indice, List<T> lista, string id, string carpeta) where T : Object
    {
        if (indice == null) indice = Indexar(lista, carpeta);
        if (indice.TryGetValue(id, out T asset)) return asset;

        Debug.LogWarning($"[Assets] No encontré '{id}' en {carpeta}. ¿El archivo se llama exactamente así?");
        return null;
    }

    private static Dictionary<string, T> Indexar<T>(List<T> lista, string carpeta) where T : Object
    {
        var indice = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (T asset in lista)
        {
            if (asset == null) continue;
            if (indice.ContainsKey(asset.name))
                Debug.LogWarning($"[Assets] Hay dos archivos llamados '{asset.name}' en {carpeta}. Se usará el primero.");
            else
                indice[asset.name] = asset;
        }
        return indice;
    }

    private void BorrarIndices()
    {
        iFondos = iPersonajes = iCG = null;
        iSFX = iAmbiente = iMusica = null;
    }

#if UNITY_EDITOR
    // ----------------------------------------------------
    // LLENADO AUTOMÁTICO (solo en el editor)
    // ----------------------------------------------------
    [ContextMenu("Llenar desde carpetas")]
    public void Llenar()
    {
        fondos     = BuscarEn<Sprite>(carpetaFondos, "t:Sprite");
        personajes = BuscarEn<Sprite>(carpetaPersonajes, "t:Sprite");
        cgs        = BuscarEn<Sprite>(carpetaCG, "t:Sprite");
        sfx        = BuscarEn<AudioClip>(carpetaSFX, "t:AudioClip");
        ambiente   = BuscarEn<AudioClip>(carpetaAmbiente, "t:AudioClip");
        musica     = BuscarEn<AudioClip>(carpetaMusica, "t:AudioClip");

        BorrarIndices();
        UnityEditor.EditorUtility.SetDirty(this);

        Debug.Log($"[Assets] Biblioteca actualizada: {fondos.Count} fondos, {personajes.Count} personajes, " +
                  $"{cgs.Count} CG, {sfx.Count} SFX, {ambiente.Count} ambientes, {musica.Count} músicas.");
    }

    private static List<T> BuscarEn<T>(string carpeta, string filtro) where T : Object
    {
        var lista = new List<T>();
        if (!UnityEditor.AssetDatabase.IsValidFolder(carpeta))
        {
            Debug.LogWarning($"[Assets] La carpeta '{carpeta}' no existe. Revisa las rutas en BibliotecaAssets.");
            return lista;
        }

        foreach (string guid in UnityEditor.AssetDatabase.FindAssets(filtro, new[] { carpeta }))
        {
            string ruta = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (asset != null && !lista.Contains(asset)) lista.Add(asset);
        }
        return lista;
    }
#endif
}
