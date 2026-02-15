using UnityEngine;

public class Patrol : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Parameters")] 
    [SerializeField] private float speed = 2f;

    private float lastT; // Sauvegarde de la valeur de t de la frame précédente

    void Update()
    {
            MovePlatform();
    }

    private void MovePlatform()
    {
        // On calcule un facteur qui varie entre 0 et 1 selon le temps et la vitesse
        float t = Mathf.PingPong(Time.time * speed, 1f);

        
        // Si t augmente, l'objet se déplace vers endPoint
        if (t > lastT)
        {
            transform.forward = (endPoint.position - startPoint.position).normalized;
        }
        // Si t diminue, l'objet fait demi-tour vers startPoint
        else if (t < lastT)
        {
            transform.forward = (startPoint.position - endPoint.position).normalized;
        }

        lastT = t; // On met à jour lastT pour la prochaine frame
        
        // On applique le déplacement
        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
    }
    
    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            
            // On dessine des sphères basées sur l'échelle X pour visualiser les points
            Gizmos.DrawWireSphere(startPoint.position, transform.localScale.x / 2);
            Gizmos.DrawWireSphere(endPoint.position, transform.localScale.x / 2);
        }
    }
}