using UnityEngine;

public class ShootPlatforme : MonoBehaviour
{
    public float speed;
    [HideInInspector] public Vector3 moveDirection; // La direction transmise par le canon

    private void FixedUpdate()
    {
        // IMPORTANT : Ne redéfinis pas "float speed = 5f" ici, 
        // sinon la valeur du canon sera ignorée !
        
        // On bouge dans la direction réelle du tir, pas le forward de la plateforme
        transform.position += moveDirection * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Suppression si on touche un mur
        if(other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            Destroy(gameObject);
        }
    }
}