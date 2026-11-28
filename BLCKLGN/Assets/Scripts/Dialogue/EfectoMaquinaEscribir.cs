using System;
using System.Collections;
using UnityEngine;
using TMPro;

// =====================================================
// EfectoMaquinaEscribir — BLCKLGN
// Tarjeta "Efecto máquina de escribir".
// Muestra el texto letra por letra en un TMP_Text.
//
// Cómo funciona: pone el texto COMPLETO desde el inicio y va
// subiendo maxVisibleCharacters. Así el texto no "brinca" de
// renglón mientras se escribe y las etiquetas de formato
// (<i>, <color>...) funcionan bien.
//
// Lo usa el InkManager:
//   Mostrar(texto, alTerminar) -> empieza a escribir
//   Completar()                -> muestra la línea completa ya
//   EstaEscribiendo            -> true mientras faltan letras
// =====================================================
[RequireComponent(typeof(TMP_Text))]
public class EfectoMaquinaEscribir : MonoBehaviour
{
    [Tooltip("Letras por segundo. Más alto = más rápido.")]
    [SerializeField, Min(1f)] private float letrasPorSegundo = 40f;

    public bool EstaEscribiendo { get; private set; }

    private TMP_Text texto;
    private Coroutine rutina;
    private Action alTerminar;

    private void Awake()
    {
        texto = GetComponent<TMP_Text>();
    }

    // Empieza a escribir una línea. alTerminar se llama cuando ya se ve completa
    // (sea porque terminó sola o porque el jugador dio clic).
    public void Mostrar(string contenido, Action alTerminar = null)
    {
        Detener();
        this.alTerminar = alTerminar;

        texto.text = contenido;
        texto.maxVisibleCharacters = 0;
        texto.ForceMeshUpdate(); // para saber cuántas letras visibles tiene

        rutina = StartCoroutine(Escribir(texto.textInfo.characterCount));
    }

    // Clic a media línea: la muestra completa de inmediato
    public void Completar()
    {
        if (!EstaEscribiendo) return;
        Detener();
        Terminar();
    }

    // Borra el texto sin escribir nada (para las líneas @)
    public void Limpiar()
    {
        Detener();
        alTerminar = null;
        texto.text = "";
        texto.maxVisibleCharacters = 99999;
    }

    private IEnumerator Escribir(int totalLetras)
    {
        EstaEscribiendo = true;
        float visibles = 0f;

        while (visibles < totalLetras)
        {
            visibles += letrasPorSegundo * Time.deltaTime;
            texto.maxVisibleCharacters = Mathf.Min((int)visibles, totalLetras);
            yield return null;
        }

        rutina = null;
        Terminar();
    }

    private void Terminar()
    {
        texto.maxVisibleCharacters = 99999;
        EstaEscribiendo = false;

        Action callback = alTerminar;
        alTerminar = null;
        callback?.Invoke();
    }

    private void Detener()
    {
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
        EstaEscribiendo = false;
    }
}
