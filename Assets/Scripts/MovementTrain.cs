using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TrainMovementSimple : MonoBehaviour
{
    [Header("Rail Settings")]
    public Transform[] railPoints; // Points définissant la voie
    public float speed = 5f;
    public bool loop = true; // Boucle infinie ou aller-retour
    
    private float currentPosition = 0f; // Position sur la courbe (0 à 1)
    private int currentSegment = 0;
    private bool movingForward = true;
    private bool isMoving = false;
    
    // Input System
    private Keyboard keyboard;
    
    void Start()
    {
        if (railPoints.Length < 2)
        {
            Debug.LogError("Il faut au moins 2 points pour créer une voie !");
            return;
        }
        
        // Initialiser le clavier
        keyboard = Keyboard.current;
        
        // Initialiser la position
        transform.position = railPoints[0].position;
    }
    
    void Update()
    {
        HandleInput();
        
        if (isMoving)
        {
            MoveTrain();
        }
    }
    
    void HandleInput()
    {
        if (keyboard == null) return;
        
        if (keyboard.wKey.isPressed)
        {
            isMoving = true;
            movingForward = true;
        }
        else if (keyboard.sKey.isPressed)
        {
            isMoving = true;
            movingForward = false;
        }
        else
        {
            isMoving = false;
        }
    }
    
    void MoveTrain()
    {
        float moveDirection = movingForward ? 1f : -1f;
        float deltaPosition = (speed * moveDirection * Time.deltaTime) / GetSegmentLength();
        
        currentPosition += deltaPosition;
        
        // Gestion des limites
        if (currentPosition >= 1f)
        {
            if (currentSegment < railPoints.Length - 2)
            {
                currentSegment++;
                currentPosition = 0f;
            }
            else if (loop)
            {
                currentSegment = 0;
                currentPosition = 0f;
            }
            else
            {
                currentPosition = 1f;
            }
        }
        else if (currentPosition <= 0f)
        {
            if (currentSegment > 0)
            {
                currentSegment--;
                currentPosition = 1f;
            }
            else if (loop)
            {
                currentSegment = railPoints.Length - 2;
                currentPosition = 1f;
            }
            else
            {
                currentPosition = 0f;
            }
        }
        
        UpdateTrainPosition();
    }
    
    void UpdateTrainPosition()
    {
        Vector3 newPosition = GetPositionOnCurve(currentPosition);
        Vector3 direction = GetDirectionOnCurve(currentPosition);
        
        // Positionner le train
        transform.position = newPosition;
        
        // Orienter le train dans la direction du mouvement
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    Vector3 GetPositionOnCurve(float t)
    {
        // Courbe de Bézier quadratique entre 3 points
        if (railPoints.Length == 2)
        {
            return Vector3.Lerp(railPoints[currentSegment].position, 
                               railPoints[currentSegment + 1].position, t);
        }
        
        // Pour plus de 2 points, utiliser une courbe lisse
        Vector3 p0 = railPoints[currentSegment].position;
        Vector3 p1 = railPoints[currentSegment + 1].position;
        
        // Points de contrôle pour la courbe
        Vector3 controlPoint1 = p0;
        Vector3 controlPoint2 = p1;
        
        // Ajuster les points de contrôle pour une courbe plus naturelle
        if (currentSegment > 0)
        {
            Vector3 prevDirection = (p0 - railPoints[currentSegment - 1].position).normalized;
            controlPoint1 = p0 + prevDirection * Vector3.Distance(p0, p1) * 0.3f;
        }
        
        if (currentSegment < railPoints.Length - 2)
        {
            Vector3 nextDirection = (railPoints[currentSegment + 2].position - p1).normalized;
            controlPoint2 = p1 - nextDirection * Vector3.Distance(p0, p1) * 0.3f;
        }
        
        return CalculateBezierCurve(p0, controlPoint1, controlPoint2, p1, t);
    }
    
    Vector3 GetDirectionOnCurve(float t)
    {
        float sampleDistance = 0.01f;
        float t1 = Mathf.Clamp01(t - sampleDistance);
        float t2 = Mathf.Clamp01(t + sampleDistance);
        
        Vector3 pos1 = GetPositionOnCurve(t1);
        Vector3 pos2 = GetPositionOnCurve(t2);
        
        return (pos2 - pos1).normalized;
    }
    
    Vector3 CalculateBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector3 result = uuu * p0;
        result += 3 * uu * t * p1;
        result += 3 * u * tt * p2;
        result += ttt * p3;
        
        return result;
    }
    
    float GetSegmentLength()
    {
        if (currentSegment >= railPoints.Length - 1) return 1f;
        
        return Vector3.Distance(railPoints[currentSegment].position, 
                               railPoints[currentSegment + 1].position);
    }
    
    // Visualisation dans l'éditeur
    void OnDrawGizmosSelected()
    {
        if (railPoints == null || railPoints.Length < 2) return;
        
        Gizmos.color = Color.yellow;
        
        // Dessiner les points de contrôle
        for (int i = 0; i < railPoints.Length; i++)
        {
            if (railPoints[i] != null)
            {
                Gizmos.DrawWireSphere(railPoints[i].position, 0.5f);
            }
        }
        
        // Dessiner la courbe
        Gizmos.color = Color.red;
        for (int i = 0; i < railPoints.Length - 1; i++)
        {
            if (railPoints[i] != null && railPoints[i + 1] != null)
            {
                for (float t = 0; t < 1; t += 0.05f)
                {
                    int oldSegment = currentSegment;
                    currentSegment = i;
                    Vector3 pos1 = GetPositionOnCurve(t);
                    Vector3 pos2 = GetPositionOnCurve(t + 0.05f);
                    Gizmos.DrawLine(pos1, pos2);
                    currentSegment = oldSegment;
                }
            }
        }
    }
}
