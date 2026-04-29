using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Custom Time Scale")]
    [Range(0f, 1f)]
    public float customTimeScale = 1f;

    private void Awake()
    {
        // Mise en place du Singleton
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Update()
    {
        // synchronise le paramètre global FMOD avec le temps de Unity
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameTimeScale", Time.timeScale);
    }
}