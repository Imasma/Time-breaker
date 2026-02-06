using UnityEngine;

public class KillAreaMove : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Parameters")] 
    [SerializeField] private float speed = 2f;

    void Update()
    {
        MoveAlongX();
    }

    private void MoveAlongX()
    {
        // On calcule un facteur qui varie entre 0 et 1 selon le temps et la vitesse
        float t = Mathf.PingPong(Time.time * speed, 1f);

        // On interpole (Lerp) uniquement la valeur X entre le début et la fin
        float newX = Mathf.Lerp(startPoint.position.x, endPoint.position.x, t);

        // On applique la nouvelle position en gardant les Y et Z actuels de l'objet
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}