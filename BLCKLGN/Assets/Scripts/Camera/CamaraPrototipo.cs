using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// =====================================================
// CamaraPrototipo — BLCKLGN
// Tarjeta "Prototipo de set 2.5D": mover la cámara para probar el paralaje.
// Va en la Main Camera. Respeta los rangos que define SetCapas, así que
// nunca se sale de la zona donde las capas cubren toda la pantalla.
//
// Controles (en Play):
//   A / D            paneo izquierda / derecha
//   Flecha arriba/abajo  paneo vertical
//   W / S            dolly: acercarse / alejarse (cambia la profundidad)
//   Q / E            zoom (campo de visión) para acercar / regresar
//   R                regresar a la posición inicial
// Con "Demo Automatica" la cámara se mueve sola, para ver el efecto sin tocar nada.
//
// Nota: el dolly (W/S) SÍ cambia el paralaje porque la cámara se mueve.
// El zoom (Q/E) solo amplía la imagen; no cambia la profundidad.
// =====================================================
[RequireComponent(typeof(Camera))]
public class CamaraPrototipo : MonoBehaviour
{
    [SerializeField] private SetCapas set;

    [Header("Velocidades")]
    [SerializeField, Min(0.1f)] private float velocidadPaneo = 1.5f;   // metros por segundo
    [SerializeField, Min(0.1f)] private float velocidadDolly = 1.5f;   // metros por segundo
    [SerializeField, Min(1f)] private float velocidadZoom = 15f;       // grados por segundo
    [Tooltip("Cuánto se puede cerrar el campo de visión. 0.6 = hasta 60% del original")]
    [SerializeField, Range(0.3f, 1f)] private float zoomMaximo = 0.6f;

    [Header("Prueba")]
    [SerializeField] private bool demoAutomatica = false;

    private Camera cam;
    private Vector3 desplazamiento; // respecto al punto de descanso del set

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (set == null) set = FindFirstObjectByType<SetCapas>();
        if (set == null) Debug.LogError("[Sets] CamaraPrototipo no encontró un SetCapas en la escena.");
    }

    private void Update()
    {
        if (set == null) return;
        float dt = Time.deltaTime;
        float fovBase = set.campoDeVision;

        if (demoAutomatica)
        {
            float t = Time.time;
            desplazamiento.x = Mathf.Sin(t * 0.5f) * set.rangoPaneoHorizontal;
            desplazamiento.y = Mathf.Sin(t * 0.35f) * set.rangoPaneoVertical * 0.5f;
            desplazamiento.z = Mathf.Lerp(-set.rangoAlejarse, set.rangoAcercarse, (Mathf.Sin(t * 0.25f) + 1f) * 0.5f);
        }
        else
        {
            LeerEntrada(out Vector3 mov, out float zoom, out bool reiniciar);

            if (reiniciar)
            {
                desplazamiento = Vector3.zero;
                cam.fieldOfView = fovBase;
            }

            desplazamiento.x += mov.x * velocidadPaneo * dt;
            desplazamiento.y += mov.y * velocidadPaneo * dt;
            desplazamiento.z += mov.z * velocidadDolly * dt;
            cam.fieldOfView -= zoom * velocidadZoom * dt;
        }

        // Nunca salir de la zona segura del set
        desplazamiento.x = Mathf.Clamp(desplazamiento.x, -set.rangoPaneoHorizontal, set.rangoPaneoHorizontal);
        desplazamiento.y = Mathf.Clamp(desplazamiento.y, -set.rangoPaneoVertical, set.rangoPaneoVertical);
        desplazamiento.z = Mathf.Clamp(desplazamiento.z, -set.rangoAlejarse, set.rangoAcercarse);
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, fovBase * zoomMaximo, fovBase);

        transform.SetPositionAndRotation(set.transform.position + desplazamiento, Quaternion.identity);
    }

    // mov.x = paneo horizontal, mov.y = paneo vertical, mov.z = dolly (+ acercarse)
    // zoom > 0 = acercar (cerrar el campo de visión)
    private void LeerEntrada(out Vector3 mov, out float zoom, out bool reiniciar)
    {
        mov = Vector3.zero;
        zoom = 0f;
        reiniciar = false;

#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return;
        mov.x = (k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f);
        mov.y = (k.upArrowKey.isPressed ? 1f : 0f) - (k.downArrowKey.isPressed ? 1f : 0f);
        mov.z = (k.wKey.isPressed ? 1f : 0f) - (k.sKey.isPressed ? 1f : 0f);
        zoom = (k.qKey.isPressed ? 1f : 0f) - (k.eKey.isPressed ? 1f : 0f);
        reiniciar = k.rKey.wasPressedThisFrame;
#else
        mov.x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        mov.y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        mov.z = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        zoom = (Input.GetKey(KeyCode.Q) ? 1f : 0f) - (Input.GetKey(KeyCode.E) ? 1f : 0f);
        reiniciar = Input.GetKeyDown(KeyCode.R);
#endif
    }
}
