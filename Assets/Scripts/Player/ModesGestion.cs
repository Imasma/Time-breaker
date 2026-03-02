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
    public Material normalMaterial;    
    public Material normalTrailMaterial; 
    public Material ghostActiveMaterial; 
    public Material ghostActiveTrailMaterial; 

    [Header("Settings")]
    public int maxRecordedPositions = 300; 
    [Tooltip("Temps avant que le premier clone ne disparaisse quand un deuxième est créé")]
    public float cloneLifeDuration = 7f; // <--- MODIFIABLE DANS L'INSPECTEUR

    [Header("Input")]
    public InputAction toggleModeAction; 
    public InputAction placeCloneAction;  

    private List<Vector3> recordedPositions = new List<Vector3>();
    private List<GameObject> activeClones = new List<GameObject>(); 
    private GameObject activeGhost;      
    private bool isGhostDeployed = false;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        toggleModeAction.Enable();
        placeCloneAction.Enable();
        toggleModeAction.performed += _ => OnToggleMode();
        placeCloneAction.performed += _ => OnPlaceClone();
    }

    private void OnDisable()
    {
        toggleModeAction.Disable();
        placeCloneAction.Disable();
        toggleModeAction.performed -= _ => OnToggleMode();
        placeCloneAction.performed -= _ => OnPlaceClone();
    }

    void FixedUpdate()
    {
        recordedPositions.Add(transform.position);
        if (!isGhostDeployed && recordedPositions.Count > maxRecordedPositions)
        {
            recordedPositions.RemoveAt(0);
        }
    }

    private void OnToggleMode()
    {
        if (!isGhostDeployed) EnterBlueMode();
        else ExitBlueMode();
    }

    private void EnterBlueMode()
    {
        if (recordedPositions.Count == 0) return;

        // SÉCURITÉ : Détruit le fantôme précédent s'il existe pour éviter les doublons
        if (activeGhost != null) Destroy(activeGhost);

        activeGhost = Instantiate(transparentGhostPrefab, recordedPositions[0], Quaternion.identity);
        activeGhost.GetComponentInChildren<Ghost>().SetPathReference(recordedPositions);
        isGhostDeployed = true;

        if (playerMovement != null) {
            playerMovement.isGhostActive = true; 
            playerMovement.SetSlowMode(false); 
        }

        if (playerRenderer != null) playerRenderer.material = ghostActiveMaterial; 
        if (trailRenderer != null) trailRenderer.material = ghostActiveTrailMaterial; 
    }

    private void ExitBlueMode()
    {
        if (activeGhost != null) Destroy(activeGhost);
        
        foreach (GameObject clone in activeClones) {
            if (clone != null) Destroy(clone);
        }
        activeClones.Clear();

        recordedPositions.Clear();
        isGhostDeployed = false;

        if (playerMovement != null) {
            playerMovement.isGhostActive = false; 
            playerMovement.SetSlowMode(true); 
        }

        if (playerRenderer != null) playerRenderer.material = normalMaterial; 
        if (trailRenderer != null) playerRenderer.material = normalTrailMaterial; 
    }

    private void OnPlaceClone()
    {
        if (!isGhostDeployed || activeGhost == null)
            return;

        Ghost movingGhost = activeGhost.GetComponentInChildren<Ghost>();

        GameObject newClone = Instantiate(
            replacementPrefab,
            movingGhost.transform.position,
            movingGhost.transform.rotation
        );

        activeClones.Add(newClone);

        // Si on a maintenant PLUS de 2 clones → on détruit immédiatement le plus ancien
        if (activeClones.Count > 2)
        {
            GameObject oldest = activeClones[0];
            activeClones.RemoveAt(0);

            if (oldest != null)
                Destroy(oldest);
        }

        // Si on a EXACTEMENT 2 clones → on lance le timer sur le PLUS ANCIEN
        if (activeClones.Count == 2)
        {
            GameObject oldest = activeClones[0];

            SolidClone sc = oldest.GetComponentInChildren<SolidClone>();
            if (sc != null)
            {
                sc.StartLifeTimer(cloneLifeDuration);
            }
        }
    }

    public void RemoveCloneFromList(GameObject clone)
    {
        if (activeClones.Contains(clone)) activeClones.Remove(clone);
    }
}