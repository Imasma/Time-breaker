using UnityEngine;

public class ObjectPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float speed = 2f;
    public float minIntensity = 0.5f;
    public float maxIntensity = 2f;

    [Header("Glow Color")]
    public Color glowColor = Color.white;

    [Header("Emission Property")]
    public string emissionProperty = "_EmissionColor";

    private Material mat;
    private Color baseEmission;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // ✅ On crée une instance du material pour ne pas affecter tous les objets
            mat = renderer.material;
            mat.EnableKeyword("_EMISSION");
            baseEmission = mat.GetColor(emissionProperty);
        }
    }

    void Update()
    {
        if (mat == null) return;

        float pulse = Mathf.Lerp(minIntensity, maxIntensity,
                      (Mathf.Sin(Time.time * speed) + 1f) / 2f);

        mat.SetColor(emissionProperty, glowColor * pulse);
    }

    void OnDestroy()
    {
        // ✅ Nettoyage de l'instance material pour éviter les fuites mémoire
        if (mat != null)
            Destroy(mat);
    }
}