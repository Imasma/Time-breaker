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

    // Référence pour bloquer le slow
    private PlayerMovement playerMovement;

    private void Awake()
    {
        // On récupère le script de mouvement sur le même objet
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        interactAction.Enable();
        interactAction.performed += _ => OnInteract(); // Déclenché quand la touche est pressé est pressé
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

        // --- Nettoyage de l'ancien clone solide ---
        if (activeSolidClone != null)
        {
            Destroy(activeSolidClone);
        }

        // Créer le clone transparent sur la position la plus ANCIENNE enregistrée
        activeGhost = Instantiate(transparentGhostPrefab, recordedPositions[0], Quaternion.identity);

        //  passer la RÉFÉRENCE de la liste
        activeGhost.GetComponentInChildren<Ghost>().SetPathReference(recordedPositions);
        
        isGhostDeployed = true;

        //  Changement d'état du Joueur 
        if (playerMovement != null) playerMovement.isGhostActive = true; // Désactive le slow
        if (playerRenderer != null) playerRenderer.material = ghostActiveMaterial; // Change Material
        if (playerRenderer != null) trailRenderer.material = ghostActiveTrailMaterial; // Change Material
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

        // --- Retour à l'état Normal ---
        if (playerMovement != null) playerMovement.isGhostActive = false; // Réactive le slow
        if (playerRenderer != null) playerRenderer.material = normalMaterial; // Remet Material normal
        if (playerRenderer != null) trailRenderer.material = normalTrailMaterial; // Remet Material normal
    }
}