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

    [Header("Input")]
    public InputAction interactAction; // la touche dans l'inspecteur Unity

    private List<Vector3> recordedPositions = new List<Vector3>();
    private GameObject activeGhost;      // Le clone transparent qui suit
    private GameObject activeSolidClone; // Le clone solide actuel sur la map
    private bool isGhostDeployed = false;

    // Référence pour piloter le slow
    private PlayerMovement playerMovement;

    private void Awake()
    {
        // On récupère le script de mouvement
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        interactAction.Enable();
        interactAction.performed += _ => OnInteract(); 
    }

    private void OnDisable()
    {
        interactAction.Disable();
        interactAction.performed -= _ => OnInteract();
    }

    void FixedUpdate()
    {
        // Enregistrer la position en permanence à chaque frame physique
        recordedPositions.Add(transform.position);
    }

    private void OnInteract()
    {
        if (!isGhostDeployed)
        {
            DeployGhost();
        }
        else
        {
            ReplaceAndDestroyGhost();
        }
    }

    private void DeployGhost()
    {
        if (recordedPositions.Count == 0) return;

        // kill ancien clone solide 
        if (activeSolidClone != null)
        {
            Destroy(activeSolidClone);
        }

        // Créer le clone transparent sur la position la plus ANCIENNE enregistrée
        activeGhost = Instantiate(transparentGhostPrefab, recordedPositions[0], Quaternion.identity);

        // Passer la RÉFÉRENCE de la liste
        activeGhost.GetComponentInChildren<Ghost>().SetPathReference(recordedPositions);
        
        isGhostDeployed = true;

        // Passage au mode clone
        if (playerMovement != null) 
        {
            playerMovement.isGhostActive = true; 
            playerMovement.SetSlowMode(false); // On désactive la capacité de slow en mode bleu
        }

        if (playerRenderer != null) playerRenderer.material = ghostActiveMaterial; 
        if (trailRenderer != null) trailRenderer.material = ghostActiveTrailMaterial; 
    }

    private void ReplaceAndDestroyGhost()
    {
        if (activeGhost != null)
        {
            // Cherche le composant Ghost qui a réellement bougé (l'enfant)
            Ghost movingPart = activeGhost.GetComponentInChildren<Ghost>();
        
            Vector3 finalPos = movingPart.transform.position;
            Quaternion finalRot = movingPart.transform.rotation;

            // Crée le clone solide à cet endroit précis
            activeSolidClone = Instantiate(replacementPrefab, finalPos, finalRot);

            // Détruit tout le groupe du fantôme
            Destroy(activeGhost);
        }

        // Vider la mémoire pour le prochain cycle
        recordedPositions.Clear();
        isGhostDeployed = false;

        // --- Retour à l'état Normal (Passage au mode ORANGE) ---
        if (playerMovement != null) 
        {
            playerMovement.isGhostActive = false; 
            playerMovement.SetSlowMode(true); // On réactive la capacité de slow en mode orange
        }

        if (playerRenderer != null) playerRenderer.material = normalMaterial; 
        if (trailRenderer != null) trailRenderer.material = normalTrailMaterial; 
    }
}