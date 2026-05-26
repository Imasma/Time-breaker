using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// À placer sur le GameObject joueur.
/// Détecte les cristaux à portée et leur envoie le facteur de proximité.
/// Envoie aussi la position globale à tous les shaders via Shader.SetGlobalVector.
/// </summary>
public class PlayerProximityEmitter : MonoBehaviour
{
    [Header("Détection")]
    [Tooltip("Rayon de détection max (doit être >= GlowRadius du shader)")]
    public float detectionRadius = 8f;

    [Tooltip("Layer(s) sur lesquels se trouvent les cristaux")]
    public LayerMask crystalLayer;

    [Tooltip("Tag des cristaux (optionnel, si pas de layer dédié)")]
    public string crystalTag = "Crystal";

    [Header("Smoothing")]
    [Tooltip("Vitesse de transition du glow (lerp)")]
    [Range(1f, 20f)]
    public float transitionSpeed = 5f;

    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = new Color(0.3f, 0.9f, 1f, 0.25f);

    // ??? Privé ????????????????????????????????????????????????????????????????

    // Cache des cristaux avec leur état courant de proximité (0..1)
    private Dictionary<Renderer, float> _crystalStates = new Dictionary<Renderer, float>();

    // Résultats du OverlapSphere (réutilisé chaque frame)
    private Collider[] _hitBuffer = new Collider[32];

    // IDs de propriétés shader (cache pour éviter les string lookups)
    private static readonly int _playerPosID = Shader.PropertyToID("_PlayerPos");
    private static readonly int _proximityID = Shader.PropertyToID("_PlayerProximity");
    private static readonly int _glowEnabledID = Shader.PropertyToID("_GlowEnabled");

    // ??? Unity Lifecycle ??????????????????????????????????????????????????????

    void Update()
    {
        // 1. Broadcast position globale ? tous les shaders reçoivent _PlayerPos
        Shader.SetGlobalVector(_playerPosID, transform.position);

        // 2. Cherche les cristaux dans le rayon
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            _hitBuffer,
            crystalLayer
        );

        // Ensemble des renderers touchés ce frame
        HashSet<Renderer> activeThisFrame = new HashSet<Renderer>();

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _hitBuffer[i];

            // Filtre par tag si renseigné
            if (!string.IsNullOrEmpty(crystalTag) && !col.CompareTag(crystalTag))
                continue;

            Renderer rend = col.GetComponent<Renderer>();
            if (rend == null) continue;

            activeThisFrame.Add(rend);

            // Calcul de la proximité normalisée (1 = tout près, 0 = au bord)
            float dist = Vector3.Distance(transform.position, col.transform.position);
            float targetProx = 1f - Mathf.Clamp01(dist / detectionRadius);

            // Récupère ou initialise l'état courant
            if (!_crystalStates.TryGetValue(rend, out float currentProx))
                currentProx = 0f;

            // Lerp smooth vers la valeur cible
            float newProx = Mathf.Lerp(currentProx, targetProx, Time.deltaTime * transitionSpeed);
            _crystalStates[rend] = newProx;

            // Envoi au matériau de ce cristal spécifiquement
            // (en complément du SetGlobalVector qui gère le calcul dans le shader)
            rend.material.SetFloat(_proximityID, newProx);
        }

        // 3. Fade out les cristaux qui ne sont plus dans le rayon
        List<Renderer> toRemove = new List<Renderer>();
        foreach (var kvp in _crystalStates)
        {
            Renderer rend = kvp.Key;

            if (activeThisFrame.Contains(rend)) continue;

            // Rend est null (objet détruit) ? marquer pour suppression
            if (rend == null) { toRemove.Add(rend); continue; }

            float faded = Mathf.Lerp(kvp.Value, 0f, Time.deltaTime * transitionSpeed);
            _crystalStates[rend] = faded;
            rend.material.SetFloat(_proximityID, faded);

            // Nettoyage si complètement éteint
            if (faded < 0.001f) toRemove.Add(rend);
        }

        foreach (var r in toRemove)
            _crystalStates.Remove(r);
    }

    // ??? Gizmos (debug dans la Scene View) ???????????????????????????????????

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Sphère de détection
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, detectionRadius);

        // Contour
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Lignes vers les cristaux actifs
        Gizmos.color = Color.cyan;
        foreach (var kvp in _crystalStates)
        {
            if (kvp.Key == null) continue;
            Gizmos.DrawLine(transform.position, kvp.Key.transform.position);
        }
    }
}