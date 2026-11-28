using System.Collections.Generic;
using UnityEngine;

// =====================================================
// SetCapas — BLCKLGN
// Tarjeta "Prototipo de set 2.5D".
//
// Arma un fondo por capas en 3D: cada capa es un sprite a cierta
// distancia (en metros) de una cámara en perspectiva. Como las capas
// lejanas se mueven menos en pantalla que las cercanas, al mover la
// cámara se genera profundidad (paralaje).
//
// CONVENCIONES (las mismas para todos los sets):
//   - 1 unidad de Unity = 1 metro.
//   - La cámara descansa en la posición de este objeto, mirando hacia +Z.
//   - Cada capa se coloca en Z = distancia. No rotes este objeto.
//   - Los sprites deben tener el pivote al centro (Pivot: Center).
//
// MODOS DE CAPA:
//   - Llenar Pantalla: la capa se escala sola para cubrir todo lo que la
//     cámara puede ver a esa distancia, contando lo que la cámara se puede
//     mover (paneo y alejarse). Así nunca se ven huecos.
//   - Tamaño Real: la capa mide lo que diga "Altura Metros" (personajes y
//     objetos). Una persona mide ~1.75 m.
//
// Si una capa no tiene sprite, se dibuja un placeholder cuadriculado de
// color para poder probar el paralaje sin arte.
//
// Herramientas (clic en los ⋮ del componente en el Inspector):
//   - "Imprimir medidas para documentar": escribe en la consola la
//     distancia, escala y tamaño de cada capa.
// Con el objeto seleccionado, la vista Scene dibuja en amarillo lo que ve
// la cámara a la distancia de cada capa y en azul lo que cubre la capa.
// =====================================================
[ExecuteAlways]
public class SetCapas : MonoBehaviour
{
    public enum Modo { LlenarPantalla, TamanoReal }
    public enum EstiloPlaceholder { Completo, FranjaInferior, MarcosLaterales }

    [System.Serializable]
    public class Capa
    {
        public string nombre = "Capa";
        [Tooltip("Vacío = se usa un placeholder de color")]
        public Sprite sprite;
        [Tooltip("Metros desde la cámara")]
        [Min(0.5f)] public float distancia = 10f;
        public Modo modo = Modo.LlenarPantalla;
        [Tooltip("Solo para Tamaño Real: altura en metros (una persona ≈ 1.75)")]
        [Min(0.01f)] public float alturaMetros = 1.75f;
        [Tooltip("Desplazamiento en metros (x = lados, y = arriba/abajo)")]
        public Vector2 posicion;

        [Header("Placeholder (solo si no hay sprite)")]
        public Color colorPlaceholder = Color.gray;
        public EstiloPlaceholder estiloPlaceholder = EstiloPlaceholder.Completo;
        [Tooltip("Tamaño Real: ancho entre alto del placeholder")]
        [Min(0.05f)] public float proporcionPlaceholder = 0.4f;
    }

    [Header("Cámara")]
    public Camera camara;
    [Tooltip("Campo de visión vertical en grados")]
    [Range(15f, 90f)] public float campoDeVision = 35f;
    [Tooltip("Las capas se calculan para al menos esta proporción de pantalla (16:9 = 1.78)")]
    public float aspectoMinimo = 16f / 9f;
    [Tooltip("En modo edición, pone la cámara en el punto de descanso del set")]
    public bool alinearCamaraEnEditor = true;

    [Header("Cuánto se puede mover la cámara (en metros)")]
    [Min(0f)] public float rangoPaneoHorizontal = 1.5f;
    [Min(0f)] public float rangoPaneoVertical = 0.5f;
    [Min(0f)] public float rangoAcercarse = 1f;
    [Min(0f)] public float rangoAlejarse = 1f;
    [Tooltip("Margen de seguridad extra. 0.05 = 5%")]
    [Range(0f, 0.5f)] public float margenExtra = 0.05f;

    [Header("Capas (de la más lejana a la más cercana)")]
    public List<Capa> capas = CapasPorDefecto();

    private const string NombreContenedor = "Capas (generado, no editar)";
    private readonly Dictionary<string, Sprite> placeholders = new Dictionary<string, Sprite>();

    // ----------------------------------------------------
    // Valores iniciales: prototipo del set de TV (escena 1.1)
    // ----------------------------------------------------
    private static List<Capa> CapasPorDefecto()
    {
        return new List<Capa>
        {
            new Capa { nombre = "Fondo - pared y cortina", distancia = 15f, modo = Modo.LlenarPantalla,
                       colorPlaceholder = new Color(0.35f, 0.25f, 0.45f), estiloPlaceholder = EstiloPlaceholder.Completo },
            new Capa { nombre = "Plano medio - escritorio y sillones", distancia = 7f, modo = Modo.LlenarPantalla,
                       colorPlaceholder = new Color(0.65f, 0.45f, 0.25f), estiloPlaceholder = EstiloPlaceholder.FranjaInferior },
            new Capa { nombre = "Personaje", distancia = 3.5f, modo = Modo.TamanoReal, alturaMetros = 1.75f,
                       posicion = new Vector2(0.6f, 0f), colorPlaceholder = new Color(0.9f, 0.3f, 0.35f),
                       proporcionPlaceholder = 0.35f },
            new Capa { nombre = "Primer plano - cámaras y luces", distancia = 2.2f, modo = Modo.LlenarPantalla,
                       colorPlaceholder = new Color(0.2f, 0.2f, 0.2f), estiloPlaceholder = EstiloPlaceholder.MarcosLaterales },
        };
    }

    private void Reset()
    {
        camara = Camera.main;
        capas = CapasPorDefecto();
    }

    // ----------------------------------------------------
    // SINCRONIZAR: crea/actualiza un hijo con SpriteRenderer por capa
    // ----------------------------------------------------
    private void OnEnable() => Sincronizar();
    private void Update() => Sincronizar();

#if UNITY_EDITOR
    private void OnValidate()
    {
        // No se pueden crear objetos dentro de OnValidate; lo hacemos justo después
        UnityEditor.EditorApplication.delayCall += () => { if (this != null) Sincronizar(); };
    }
#endif

    private void OnDisable()
    {
        foreach (Sprite s in placeholders.Values)
        {
            if (s == null) continue;
            DestruirObjeto(s.texture);
            DestruirObjeto(s);
        }
        placeholders.Clear();
    }

    public void Sincronizar()
    {
        if (camara == null) camara = Camera.main;
        if (camara == null) return;

        camara.orthographic = false;
        if (!Application.isPlaying)
        {
            camara.fieldOfView = campoDeVision;
            if (alinearCamaraEnEditor)
                camara.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        }

        Transform contenedor = ObtenerContenedor();

        // Ajustar la cantidad de hijos a la cantidad de capas
        while (contenedor.childCount < capas.Count)
        {
            GameObject go = new GameObject("capa", typeof(SpriteRenderer));
            go.transform.SetParent(contenedor, false);
        }
        while (contenedor.childCount > capas.Count)
        {
            Transform sobra = contenedor.GetChild(contenedor.childCount - 1);
            sobra.SetParent(null);
            DestruirObjeto(sobra.gameObject);
        }

        for (int i = 0; i < capas.Count; i++)
            ActualizarCapa(i, capas[i], contenedor.GetChild(i));
    }

    private void ActualizarCapa(int indice, Capa capa, Transform hijo)
    {
        hijo.name = $"{indice:00} {capa.nombre} ({capa.distancia} m)";

        SpriteRenderer sr = hijo.GetComponent<SpriteRenderer>();
        if (sr == null) sr = hijo.gameObject.AddComponent<SpriteRenderer>();

        Sprite sprite = capa.sprite != null ? capa.sprite : Placeholder(capa);
        sr.sprite = sprite;
        // Más lejos = se dibuja antes (atrás)
        sr.sortingOrder = -Mathf.RoundToInt(capa.distancia * 100f);

        hijo.localScale = Vector3.one * CalcularEscala(capa, sprite);
        hijo.SetPositionAndRotation(
            transform.position + new Vector3(capa.posicion.x, capa.posicion.y, capa.distancia),
            Quaternion.identity);
    }

    // ----------------------------------------------------
    // MATEMÁTICAS DE LA ESCALA
    // ----------------------------------------------------
    // Lo que la cámara ve a una distancia d (sin moverse):
    //   alto  = 2 · d · tan(campoDeVision / 2)
    //   ancho = alto · aspecto
    public Vector2 AreaVisible(float distancia)
    {
        float alto = 2f * distancia * Mathf.Tan(campoDeVision * 0.5f * Mathf.Deg2Rad);
        float aspecto = Mathf.Max(camara != null ? camara.aspect : aspectoMinimo, aspectoMinimo);
        return new Vector2(alto * aspecto, alto);
    }

    // Lo que una capa debe cubrir para que no se vean huecos:
    // lo visible desde el punto más alejado + lo que la cámara se puede desplazar.
    public Vector2 AreaNecesaria(float distancia)
    {
        Vector2 visible = AreaVisible(distancia + rangoAlejarse);
        return new Vector2(
            visible.x + 2f * rangoPaneoHorizontal,
            visible.y + 2f * rangoPaneoVertical) * (1f + margenExtra);
    }

    private float CalcularEscala(Capa capa, Sprite sprite)
    {
        Vector2 tam = sprite.bounds.size;
        if (tam.x <= 0f || tam.y <= 0f) return 1f;

        if (capa.modo == Modo.TamanoReal)
            return capa.alturaMetros / tam.y;

        Vector2 necesaria = AreaNecesaria(capa.distancia);
        return Mathf.Max(necesaria.x / tam.x, necesaria.y / tam.y);
    }

    // ----------------------------------------------------
    // PLACEHOLDERS (cuadrícula de color, generados en memoria)
    // ----------------------------------------------------
    private Sprite Placeholder(Capa capa)
    {
        bool llena = capa.modo == Modo.LlenarPantalla;
        string clave = $"{capa.colorPlaceholder}|{llena}|{capa.estiloPlaceholder}|{capa.proporcionPlaceholder}";
        if (placeholders.TryGetValue(clave, out Sprite existente) && existente != null) return existente;

        int alto = 64;
        int ancho = llena ? 114 : Mathf.Max(4, Mathf.RoundToInt(alto * capa.proporcionPlaceholder));

        Texture2D tex = new Texture2D(ancho, alto, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color baseColor = capa.colorPlaceholder; baseColor.a = 1f;
        Color linea = baseColor * 0.6f; linea.a = 1f;
        Color transparente = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                bool dibujar = true;
                if (llena && capa.estiloPlaceholder == EstiloPlaceholder.FranjaInferior)
                    dibujar = y < alto * 0.4f;
                else if (llena && capa.estiloPlaceholder == EstiloPlaceholder.MarcosLaterales)
                    dibujar = x < ancho * 0.12f || x >= ancho * 0.88f;

                bool esLinea = llena
                    ? (x % 8 == 0 || y % 8 == 0)
                    : (x < 2 || y < 2 || x >= ancho - 2 || y >= alto - 2);

                tex.SetPixel(x, y, dibujar ? (esLinea ? linea : baseColor) : transparente);
            }
        }
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, ancho, alto), new Vector2(0.5f, 0.5f), alto);
        sprite.name = "placeholder " + capa.nombre;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        placeholders[clave] = sprite;
        return sprite;
    }

    // ----------------------------------------------------
    // UTILIDADES
    // ----------------------------------------------------
    private Transform ObtenerContenedor()
    {
        Transform contenedor = transform.Find(NombreContenedor);
        if (contenedor == null)
        {
            contenedor = new GameObject(NombreContenedor).transform;
            contenedor.SetParent(transform, false);
        }
        return contenedor;
    }

    private static void DestruirObjeto(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    [ContextMenu("Imprimir medidas para documentar")]
    private void ImprimirMedidas()
    {
        if (camara == null) camara = Camera.main;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[Sets] {name} — FOV {campoDeVision}°, aspecto mín. {aspectoMinimo:0.00}, " +
                      $"paneo ±{rangoPaneoHorizontal} m (h) ±{rangoPaneoVertical} m (v), " +
                      $"acercarse {rangoAcercarse} m, alejarse {rangoAlejarse} m, margen {margenExtra:P0}");
        Transform contenedor = transform.Find(NombreContenedor);
        for (int i = 0; i < capas.Count; i++)
        {
            Capa c = capas[i];
            Vector2 visible = AreaVisible(c.distancia);
            string escala = "?";
            string tamano = "?";
            if (contenedor != null && i < contenedor.childCount)
            {
                Transform hijo = contenedor.GetChild(i);
                SpriteRenderer sr = hijo.GetComponent<SpriteRenderer>();
                escala = hijo.localScale.x.ToString("0.###");
                if (sr != null && sr.sprite != null)
                {
                    Vector2 t = sr.sprite.bounds.size * hijo.localScale.x;
                    tamano = $"{t.x:0.00} x {t.y:0.00} m";
                }
            }
            sb.AppendLine($"  {c.nombre} | {c.distancia} m | {c.modo} | escala {escala} | " +
                          $"mide {tamano} | la cámara ve {visible.x:0.00} x {visible.y:0.00} m a esa distancia");
        }
        Debug.Log(sb.ToString());
    }

    private void OnDrawGizmosSelected()
    {
        if (camara == null) return;
        foreach (Capa c in capas)
        {
            Vector3 centro = transform.position + new Vector3(0f, 0f, c.distancia);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(centro, AreaVisible(c.distancia));
            if (c.modo == Modo.LlenarPantalla)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(centro, AreaNecesaria(c.distancia));
            }
        }
    }
}
