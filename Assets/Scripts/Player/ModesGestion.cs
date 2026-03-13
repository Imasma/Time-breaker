using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ModesGestion : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject transparentGhostPrefab; 
    public GameObject replacementPrefab;      

    [Header("Player Visuals")]
    public Renderer playerRenderer;      
    public Renderer trailRenderer;      
    public Material activeMaterial;    
    public Material activeTrailMaterial; 

    [Header("Settings")]
    public int maxRecordedPositions = 300; 
    [Tooltip("Nombre de frames de décalage entre le joueur et le fantôme (50 = env. 1 seconde)")]
    public int ghostDelayInFrames = 50; // <--- LE DÉLAI EST ICI
    public float cloneLifeDuration = 7f;

    [Header("Input")]
    public InputAction placeCloneAction;  

    private List<Vector3> recordedPositions = new List<Vector3>();
    private List<GameObject> activeClones = new List<GameObject>(); 
    private GameObject activeGhost;      

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        
        if (playerRenderer != null) playerRenderer.material = activeMaterial;
        if (trailRenderer != null) trailRenderer.material = activeTrailMaterial;
    }

    private void Start()
    {
        // 1. On pré-remplit la liste avec la position actuelle
        for (int i = 0; i < ghostDelayInFrames; i++)
        {
            recordedPositions.Add(transform.position);
        }
        
        // 2. On spawn le fantôme à la position actuelle
        activeGhost = Instantiate(transparentGhostPrefab, transform.position, Quaternion.identity);
        
        // 3. On lui donne la liste (il commencera à l'index 0, là où on a mis les positions immobiles)
        activeGhost.GetComponentInChildren<Ghost>().SetPathReference(recordedPositions);

        if (playerMovement != null) {
            playerMovement.isGhostActive = false; 
        }
    }

    private void OnEnable()
    {
        placeCloneAction.Enable();
        placeCloneAction.performed += _ => OnPlaceClone();
    }

    private void OnDisable()
    {
        placeCloneAction.Disable();
        placeCloneAction.performed -= _ => OnPlaceClone();
    }

    void FixedUpdate()
    {
        // Enregistrement permanent
        recordedPositions.Add(transform.position);
        
        // On garde toujours le nombre maximum de positions (buffer + historique)
        if (recordedPositions.Count > (maxRecordedPositions + ghostDelayInFrames))
        {
            recordedPositions.RemoveAt(0);
        }
    }

    private void OnPlaceClone()
    {
        if (activeGhost == null) return;

        Ghost movingGhost = activeGhost.GetComponentInChildren<Ghost>();
        
        GameObject newClone = Instantiate(
            replacementPrefab,
            movingGhost.transform.position,
            movingGhost.transform.rotation
        );

        activeClones.Add(newClone);

        if (activeClones.Count > 2)
        {
            GameObject oldest = activeClones[0];
            activeClones.RemoveAt(0);
            if (oldest != null) Destroy(oldest);
        }

        if (activeClones.Count == 2)
        {
            GameObject oldest = activeClones[0];
            SolidClone sc = oldest.GetComponentInChildren<SolidClone>();
            if (sc != null) sc.StartLifeTimer(cloneLifeDuration);
        }
    }

    public void RemoveCloneFromList(GameObject clone)
    {
        if (activeClones.Contains(clone)) activeClones.Remove(clone);
    }
}