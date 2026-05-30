using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    [Header("Configuration UI")]
    [SerializeField] private GameObject optionCanva; // Glisse ton Canvas OPTIONS ici
    [SerializeField] private GameObject mainMenuCanva; // Glisse ton Canvas MENU PRINCIPAL ici (laisser vide dans le niveau de jeu)
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape; // Touche pour ouvrir (Echap par défaut)

    [Header("Options")]
    [SerializeField] private bool pauseGame = true; // Est-ce qu'on fige le jeu quand le menu est ouvert ?

    private bool isMenuOpen = false;

    void Start()
    {
        // Sécurité : On s'assure que le menu Options est bien caché au lancement du jeu
        if (optionCanva != null)
        {
            optionCanva.SetActive(false);
        }

        // Sécurité : Si on est sur le menu principal, on s'assure qu'il est bien visible au départ
        if (mainMenuCanva != null)
        {
            mainMenuCanva.SetActive(true);
        }
    }

    void Update()
    {
        // Si le joueur presse la touche configurée
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        // On active ou désactive le Canvas des OPTIONS
        if (optionCanva != null)
        {
            optionCanva.SetActive(isMenuOpen);
        }

        // --- LA MAGIE DU MENU PRINCIPAL EST ICI ---
        if (mainMenuCanva != null)
        {
            // Le Menu Principal fait TOUJOURS l'inverse du menu Options
            mainMenuCanva.SetActive(!isMenuOpen);
        }

        // Gestion du temps (Pause)
        if (pauseGame)
        {
            Time.timeScale = isMenuOpen ? 0f : 1f;
        }

        // --- GESTION DE LA SOURIS ---
        // Si on est dans le menu principal ou que les options sont ouvertes, on libère la souris
        if (isMenuOpen || mainMenuCanva != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}