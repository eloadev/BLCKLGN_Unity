using UnityEngine;
using UnityEngine.InputSystem;

public class AudioTest : MonoBehaviour
{
    float sfxVol = 0.8f;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.spaceKey.wasPressedThisFrame)
            AudioManager.Instance.Handle_Tag("sfx: aplausos");

        if (kb.upArrowKey.wasPressedThisFrame)
        {
            sfxVol = Mathf.Clamp01(sfxVol + 0.2f);
            AudioManager.Instance.SetSFXVolume(sfxVol);
        }
        if (kb.downArrowKey.wasPressedThisFrame)
        {
            sfxVol = Mathf.Clamp01(sfxVol - 0.2f);
            AudioManager.Instance.SetSFXVolume(sfxVol);
        }
    }
}