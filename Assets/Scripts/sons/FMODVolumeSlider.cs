using UnityEngine;
using UnityEngine.UI;

public class FMODVolumeSlider : MonoBehaviour
{
    private FMOD.Studio.Bus masterBus;
    private Slider volumeSlider;

    void Start()
    {
        // 1. On récupère le "Master Bus" qui gère TOUS les sons du projet FMOD
        masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");

        // 2. On récupère le composant Slider sur ce GameObject
        volumeSlider = GetComponent<Slider>();

        if (volumeSlider != null)
        {
            // Optionnel : On initialise le slider avec le volume actuel de FMOD
            masterBus.getVolume(out float currentVolume);
            volumeSlider.value = currentVolume * 100f; // On passe de 0-1 à 0-100

            // 3. On écoute en temps réel quand le joueur bouge le slider
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float sliderValue)
    {
        // FMOD prend une valeur linéaire entre 0.0f (silence) et 1.0f (volume max).
        // Comme ton slider va de 0 à 100, on divise par 100.
        float normalizedVolume = sliderValue / 100f;

        masterBus.setVolume(normalizedVolume);
    }
}