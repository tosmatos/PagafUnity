using UnityEngine;
using UnityEngine.InputSystem;

public class TrainMovementRealistic : MonoBehaviour
{
    [Header("Rail Settings")]
    public Transform[] railPoints;
    public float speed = 5f;
    public bool loop = true;
    
    [Header("Curve Settings")]
    [Range(0f, 90f)]
    public float curveAngleThreshold = 15f; // Angle minimum pour déclencher une courbe
    [Range(0f, 1f)]
    public float curveStrength = 0.3f; // Intensité des courbes
    public bool showDebugInfo = true;
    public HandleCameras cameraHandler;

    [SerializeField]private float currentPosition = 0f;
    private int currentSegment = 0;
    private bool movingForward = true;
    private bool isMoving = false;
    private Keyboard keyboard;
    
    // Cache pour les types de segments
    private enum SegmentType { Straight, Curve }
    private SegmentType[] segmentTypes;
    
    void Start()
    {
        if (railPoints.Length < 2)
        {
            Debug.LogError("Il faut au moins 2 points pour créer une voie !");
            return;
        }
        
        keyboard = Keyboard.current;
        transform.position = railPoints[0].position;
        
        // Analyser les segments pour déterminer lesquels sont des courbes
        AnalyzeSegments();
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
    
    void AnalyzeSegments()
    {
        segmentTypes = new SegmentType[railPoints.Length - 1];
        
        for (int i = 0; i < railPoints.Length - 1; i++)
        {
            float angle = GetAngleAtPoint(i + 1);
            
            // Si l'angle est significatif, c'est une courbe
            if (angle > curveAngleThreshold)
            {
                segmentTypes[i] = SegmentType.Curve;
            }
            else
            {
                segmentTypes[i] = SegmentType.Straight;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Segment {i}: {segmentTypes[i]} (angle: {angle:F1}°)");
            }
        }
    }
    
    float GetAngleAtPoint(int pointIndex)
    {
        // Vérifier si on peut calculer l'angle (besoin de 3 points)
        if (pointIndex <= 0 || pointIndex >= railPoints.Length - 1)
            return 0f;
            
        Vector3 prevPoint = railPoints[pointIndex - 1].position;
        Vector3 currentPoint = railPoints[pointIndex].position;
        Vector3 nextPoint = railPoints[pointIndex + 1].position;
        
        Vector3 dir1 = (currentPoint - prevPoint).normalized;
        Vector3 dir2 = (nextPoint - currentPoint).normalized;
        
        float angle = Vector3.Angle(dir1, dir2);
        
        // Retourner l'angle de déviation (180° = ligne droite, 0° = demi-tour)
        return 180f - angle;
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
        Vector3 newPosition = GetPositionOnTrack(currentPosition);
        Vector3 direction = GetDirectionOnTrack(currentPosition);
        
        transform.position = newPosition;
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    Vector3 GetPositionOnTrack(float t)
    {
        if (currentSegment >= segmentTypes.Length)
            return railPoints[railPoints.Length - 1].position;
            
        // Choisir le type d'interpolation selon le segment
        if (segmentTypes[currentSegment] == SegmentType.Straight)
        {
            return GetStraightPosition(t);
        }
        else
        {
            return GetCurvePosition(t);
        }
    }
    
    Vector3 GetStraightPosition(float t)
    {
        // Interpolation linéaire simple
        Vector3 startPos = railPoints[currentSegment].position;
        Vector3 endPos = railPoints[currentSegment + 1].position;
        
        return Vector3.Lerp(startPos, endPos, t);
    }
    
    Vector3 GetCurvePosition(float t)
    {
        // Utiliser une courbe de Bézier pour les virages
        Vector3 p0 = railPoints[currentSegment].position;
        Vector3 p3 = railPoints[currentSegment + 1].position;
        
        // Points de contrôle pour une courbe naturelle
        Vector3 p1 = p0 + GetTangentAtPoint(currentSegment, true) * Vector3.Distance(p0, p3) * curveStrength;
        Vector3 p2 = p3 - GetTangentAtPoint(currentSegment + 1, false) * Vector3.Distance(p0, p3) * curveStrength;
        
        return CalculateBezierCubic(p0, p1, p2, p3, t);
    }
    
    Vector3 GetTangentAtPoint(int pointIndex, bool isOutgoing)
    {
        Vector3 tangent = Vector3.forward;
        
        if (pointIndex > 0 && pointIndex < railPoints.Length - 1)
        {
            Vector3 prev = railPoints[pointIndex - 1].position;
            Vector3 next = railPoints[pointIndex + 1].position;
            tangent = (next - prev).normalized;
        }
        else if (pointIndex == 0 && railPoints.Length > 1)
        {
            tangent = (railPoints[1].position - railPoints[0].position).normalized;
        }
        else if (pointIndex == railPoints.Length - 1 && railPoints.Length > 1)
        {
            tangent = (railPoints[pointIndex].position - railPoints[pointIndex - 1].position).normalized;
        }
        
        return tangent;
    }
    
    Vector3 CalculateBezierCubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
    }
    
    Vector3 GetDirectionOnTrack(float t)
    {
        float sampleDistance = 0.01f;
        float t1 = Mathf.Clamp01(t - sampleDistance);
        float t2 = Mathf.Clamp01(t + sampleDistance);
        
        Vector3 pos1 = GetPositionOnTrack(t1);
        Vector3 pos2 = GetPositionOnTrack(t2);
        
        return (pos2 - pos1).normalized;
    }
    
    float GetSegmentLength()
    {
        if (currentSegment >= railPoints.Length - 1) return 1f;
        return Vector3.Distance(railPoints[currentSegment].position, 
                               railPoints[currentSegment + 1].position);
    }
    
    void OnDrawGizmosSelected()
    {
        if (railPoints == null || railPoints.Length < 2) return;
        
        // Analyser les segments si pas encore fait
        if (segmentTypes == null || segmentTypes.Length != railPoints.Length - 1)
        {
            AnalyzeSegments();
        }
        
        // Points de contrôle
        Gizmos.color = Color.yellow;
        for (int i = 0; i < railPoints.Length; i++)
        {
            if (railPoints[i] != null)
            {
                Gizmos.DrawWireSphere(railPoints[i].position, 0.5f);
                
                // Afficher l'angle à chaque point
                if (i > 0 && i < railPoints.Length - 1)
                {
                    float angle = GetAngleAtPoint(i);
                    Vector3 labelPos = railPoints[i].position + Vector3.up * 1.5f;
                    
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(labelPos, $"{angle:F0}°");
                    #endif
                }
            }
        }
        
        // Dessiner les segments avec des couleurs différentes
        for (int i = 0; i < railPoints.Length - 1; i++)
        {
            if (railPoints[i] != null && railPoints[i + 1] != null)
            {
                // Couleur selon le type de segment
                if (segmentTypes != null && i < segmentTypes.Length)
                {
                    Gizmos.color = segmentTypes[i] == SegmentType.Straight ? Color.green : Color.red;
                }
                else
                {
                    Gizmos.color = Color.white;
                }
                
                Vector3 lastPos = railPoints[i].position;
                for (float t = 0.05f; t <= 1; t += 0.05f)
                {
                    int oldSegment = currentSegment;
                    currentSegment = i;
                    Vector3 newPos = GetPositionOnTrack(t);
                    Gizmos.DrawLine(lastPos, newPos);
                    lastPos = newPos;
                    currentSegment = oldSegment;
                }
            }
        }
    }
}
