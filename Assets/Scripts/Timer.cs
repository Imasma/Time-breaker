using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public GameTimer gameTimer;     // Référence à ton script de timer
    public TextMeshProUGUI timerText;

    void Update()
    {
        float time = gameTimer.GetTime();

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
