using UnityEngine;
using UnityEngine.InputSystem; 

public class Cheat : MonoBehaviour
{
    public bool godMode = false;
    public InputAction godModeAction;

    private void OnEnable()
    {
        godModeAction.Enable();
    }

    private void OnDisable()
    {
        godModeAction.Disable();
    }

    private void Update() // .triggered renvoie true UNIQUEMENT sur la frame où on appuie.

    {
        if (godModeAction.triggered)
        {
            godMode = !godMode;
            Debug.Log("God Mode : " + (godMode ? "ACTIVÉ" : "DÉSACTIVÉ"));
        }
    }
}