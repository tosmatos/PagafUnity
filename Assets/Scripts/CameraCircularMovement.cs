using UnityEngine;

public class CameraCircularMotion : MonoBehaviour
{
    public Vector3 center = new Vector3(100f, 0f, 100f); // Centre du carré
    public float radius = 90f;     // Rayon ≤ 100m pour rester dans le carré
    public float speed = 3f;      // Vitesse en degrés/seconde
    public float height = 4f;     // Hauteur de la caméra
    public GameObject target;
    
    private float angle = 0f;

    void Update()
    {
        // Incrémenter l’angle
        angle += speed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        // Calculer la position circulaire
        float rad = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * radius + center.x;
        float z = Mathf.Sin(rad) * radius + center.z;

        // Position avec la hauteur
        transform.position = new Vector3(x, center.y + height, z);

        // Regarder vers le centre
        transform.LookAt(target.transform);
    }
}