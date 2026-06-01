using UnityEngine;
using UnityEngine.UI;
using TMPro; // Si tu utilises TextMeshPro

public class TitlePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float speed = 2f;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1f;

    [Header("Glow Color")]
    public Color glowColor = Color.white;

    private TextMeshProUGUI tmpText;
    private Image image;

    void Start()
    {
        // Détecte automatiquement si c'est un Text ou une Image
        tmpText = GetComponent<TextMeshProUGUI>();
        image = GetComponent<Image>();
    }

    void Update()
    {
        // Calcule la valeur de pulsation entre min et max
        float pulse = Mathf.Lerp(minIntensity, maxIntensity,
                      (Mathf.Sin(Time.time * speed) + 1f) / 2f);

        Color currentColor = glowColor * pulse;
        currentColor.a = 1f;

        if (tmpText != null)
            tmpText.color = currentColor;
        else if (image != null)
            image.color = currentColor;
    }
}