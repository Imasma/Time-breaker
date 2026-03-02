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

    [Header("Input")]
    public InputAction toggleModeAction; 
    public InputAction placeCloneAction;  

    private List<Vector3> recordedPositions = new List<Vector3>();
    private List<GameObject> activeClones = new List<GameObject>(); // Liste des clones physiques sur la map
    private GameObject activeGhost;      
    private bool isGhostDeployed = false;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        // Récupération du script de mouvement pour synchroniser le slow-mo
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
        // Enregistre la position du joueur à chaque frame physique
        recordedPositions.Add(transform.position);

        // En mode Orange (normal), on limite la liste à maxRecordedPositions
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

        // Création du fantôme au début du tracé enregistré
        activeGhost = Instantiate(transparentGhostPrefab, recordedPositions[0], Quaternion.identity);
        activeGhost.GetComponentInChildren<Ghost>().SetPathReference(recordedPositions);
        isGhostDeployed = true;

        // Configuration du joueur (Désactivation du slow, changement de couleur)
        if (playerMovement != null) {
            playerMovement.isGhostActive = true; 
            playerMovement.SetSlowMode(false); 
        }

        // --- CORRECTION : Mise à jour des visuels ---
        if (playerRenderer != null) playerRenderer.material = ghostActiveMaterial; 
        if (trailRenderer != null) trailRenderer.material = ghostActiveTrailMaterial; 
    }

    private void ExitBlueMode()
    {
        // Destruction du fantôme lors du retour au mode Orange
        if (activeGhost != null) Destroy(activeGhost);
        
        // Nettoyage de TOUS les clones physiques présents
        foreach (GameObject clone in activeClones) {
            if (clone != null) Destroy(clone);
        }
        activeClones.Clear();

        recordedPositions.Clear();
        isGhostDeployed = false;

        // Configuration du joueur (Réactivation du slow, retour au visuel normal)
        if (playerMovement != null) {
            playerMovement.isGhostActive = false; 
            playerMovement.SetSlowMode(true); 
        }

        // --- CORRECTION : Mise à jour des visuels ---
        if (playerRenderer != null) playerRenderer.material = normalMaterial; 
        if (trailRenderer != null) trailRenderer.material = normalTrailMaterial; 
    }

    private void OnPlaceClone()
    {
        // Uniquement en mode bleu et si le fantôme est actif
        if (isGhostDeployed && activeGhost != null)
        {
            // Si on a déjà 2 clones, le 1er disparaît instantanément pour laisser place au 3ème
            if (activeClones.Count >= 2)
            {
                GameObject firstClone = activeClones[0];
                activeClones.RemoveAt(0);
                if (firstClone != null) Destroy(firstClone);
            }

            // Récupère la position actuelle du fantôme pour y placer le clone physique
            Ghost movingGhost = activeGhost.GetComponentInChildren<Ghost>();
            GameObject newClone = Instantiate(replacementPrefab, movingGhost.transform.position, movingGhost.transform.rotation);
            activeClones.Add(newClone);

            // Si on a maintenant exactement 2 clones, on lance le timer de 7s sur le plus ancien
            if (activeClones.Count == 2)
            {
                SolidClone sc = activeClones[0].GetComponent<SolidClone>();
                if (sc != null) sc.StartLifeTimer(7f);
            }
        }
    }

    // Utilisé par SolidClone.cs pour se retirer de la liste proprement à sa destruction
    public void RemoveCloneFromList(GameObject clone)
    {
        if (activeClones.Contains(clone)) activeClones.Remove(clone);
    }
}