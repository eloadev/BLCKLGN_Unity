using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Ink.Runtime;
using Ink.UnityIntegration;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// =====================================================
// InkManager — BLCKLGN
// Paso 5.2: carga la historia de Ink y pide la siguiente línea.
// Paso 5.3: le pasa los tags de cada línea al TagHandler.
// Paso 5.4 (parte de ejecución): manda a la consola los errores
// que ocurran mientras se juega.
// Tarjeta "Caja de texto": avanzar con clic y con barra espaciadora.
// Tarjeta "Efecto máquina de escribir": el texto sale letra por letra;
// un clic a media línea la completa y un clic con la línea completa avanza.
//
// Qué hace:
//   1. Al iniciar, carga main.ink y muestra la primera línea.
//   2. Cada vez que se llama Avanzar() (clic en BotonAvanzar o Espacio):
//      - si la línea se está escribiendo, la completa;
//      - si ya está completa, pide la siguiente línea a Ink.
//   3. Le manda los tags al TagHandler (fondo, nombre, sonido...).
//   4. Si la línea empieza con @ (línea de acción), no la muestra
//      y avanza sola, esperando lo que diga su #wait.
//   5. Si hay opciones, crea un botón por cada una cuando la línea
//      termina de escribirse.
// =====================================================
public class InkManager : MonoBehaviour
{
    [Header("Historia")]
    [Tooltip("Arrastra aquí main.ink (el Master File)")]
    [SerializeField] private InkFile archivoInk;

    [Header("Tags")]
    [SerializeField] private TagHandler tagHandler;

    [Header("UI de diálogo")]
    [SerializeField] private TMP_Text textoDialogo;
    [Tooltip("El componente EfectoMaquinaEscribir que está en TextoDialogo")]
    [SerializeField] private EfectoMaquinaEscribir maquinaEscribir;
    [SerializeField] private Button botonAvanzar;

    [Header("UI de opciones")]
    [Tooltip("Objeto vacío con Vertical Layout Group donde aparecen los botones")]
    [SerializeField] private Transform contenedorOpciones;
    [Tooltip("Prefab de botón con un TMP_Text como hijo")]
    [SerializeField] private Button prefabBotonOpcion;

    private Story story;
    private bool esperandoEleccion = false;
    private bool bloqueado = false; // true mientras corre un #wait

    // ----------------------------------------------------
    // 1. CARGAR LA HISTORIA
    // ----------------------------------------------------
    private void Start()
    {
        if (archivoInk == null)
        {
            Debug.LogError("[Ink] No hay archivo asignado. Arrastra main.ink al campo 'Archivo Ink' del InkManager.");
            return;
        }
        if (tagHandler == null)
        {
            Debug.LogError("[Ink] Falta asignar el TagHandler en el InkManager.");
            return;
        }
        if (maquinaEscribir == null)
        {
            Debug.LogError("[Ink] Falta asignar Maquina Escribir en el InkManager (el componente que está en TextoDialogo).");
            return;
        }

        try
        {
            story = new Story(archivoInk.storyJson);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Ink] No se pudo cargar la historia. ¿main.ink tiene errores? Revisa su Inspector.\n" + e.Message);
            return;
        }

        // 5.4 — errores mientras se juega (ej. un -> a un knot que no existe)
        story.onError += (mensaje, tipo) =>
        {
            if (tipo == Ink.ErrorType.Warning)
                Debug.LogWarning("[Ink] Advertencia: " + mensaje);
            else
                Debug.LogError("[Ink] Error en el guion: " + mensaje);
        };

        botonAvanzar.onClick.AddListener(ClicAvanzar);
        LimpiarOpciones();
        Avanzar(); // muestra la primera línea
    }

    // ----------------------------------------------------
    // ENTRADA: clic y barra espaciadora
    // ----------------------------------------------------
    private void Update()
    {
        if (EspacioPresionado()) Avanzar();
    }

    private bool EspacioPresionado()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private void ClicAvanzar()
    {
        // Si el botón se queda "seleccionado", la barra espaciadora también lo
        // activaría y avanzaría dos veces. Por eso lo deseleccionamos.
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        Avanzar();
    }

    // ----------------------------------------------------
    // 2. AVANZAR / PEDIR LA SIGUIENTE LÍNEA
    // ----------------------------------------------------
    public void Avanzar()
    {
        if (story == null || esperandoEleccion || bloqueado) return;

        // Clic a media línea: la completamos en vez de saltar
        if (maquinaEscribir.EstaEscribiendo)
        {
            maquinaEscribir.Completar();
            return;
        }

        if (!story.canContinue)
        {
            if (story.currentChoices.Count > 0) MostrarOpciones();
            else FinDeHistoria();
            return;
        }

        string linea = story.Continue().Trim();

        // 3. Repartir los tags (fondo, nombre, sonido...)
        float espera = tagHandler.ProcesarTags(story.currentTags);

        // 4. Línea de acción (@): no se muestra, avanza sola
        if (linea.StartsWith("@"))
        {
            maquinaEscribir.Limpiar();
            if (espera > 0f) StartCoroutine(AvanzarDespuesDe(espera));
            else Avanzar();
            return;
        }

        // 5. Escribir la línea. Si después vienen opciones, aparecen
        //    cuando la línea termina de escribirse.
        bool vienenOpciones = !story.canContinue && story.currentChoices.Count > 0;
        if (vienenOpciones) maquinaEscribir.Mostrar(linea, MostrarOpciones);
        else maquinaEscribir.Mostrar(linea);
    }

    private IEnumerator AvanzarDespuesDe(float segundos)
    {
        bloqueado = true;
        yield return new WaitForSeconds(segundos);
        bloqueado = false;
        Avanzar();
    }

    // ----------------------------------------------------
    // OPCIONES (decisiones ilusorias)
    // ----------------------------------------------------
    private void MostrarOpciones()
    {
        esperandoEleccion = true;
        LimpiarOpciones();

        foreach (Choice opcion in story.currentChoices)
        {
            Button boton = Instantiate(prefabBotonOpcion, contenedorOpciones);
            boton.GetComponentInChildren<TMP_Text>().text = opcion.text;

            int indice = opcion.index; // copia local para el clic
            boton.onClick.AddListener(() => Elegir(indice));
        }
    }

    private void Elegir(int indice)
    {
        story.ChooseChoiceIndex(indice);
        esperandoEleccion = false;
        LimpiarOpciones();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        Avanzar();
    }

    private void LimpiarOpciones()
    {
        foreach (Transform hijo in contenedorOpciones)
            Destroy(hijo.gameObject);
    }

    // ----------------------------------------------------
    // FIN
    // ----------------------------------------------------
    private void FinDeHistoria()
    {
        Debug.Log("[Ink] Fin de la historia.");
        botonAvanzar.interactable = false;
        enabled = false; // deja de escuchar la barra espaciadora
    }
}
