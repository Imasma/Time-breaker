using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    public float startTime = 60f;   // 1 minute (modifiable)
    private float currentTime;

    public UnityEvent onTimerEnd;   // Optionnel (mort, game over, etc.)

    void Start()
    {
        currentTime = startTime;
    }

    void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            onTimerEnd?.Invoke();
            enabled = false;
        }
    }

    public float GetTime()
    {
        return currentTime;
    }
}
