using UnityEngine;

public class FlockCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float height = 20f;
    [SerializeField] private float followSpeed = 2f;
    [SerializeField] private float lookAheadDistance = 5f;
    
    private FlockManager flockManager;
    
    void Start()
    {
        flockManager = FindAnyObjectByType<FlockManager>();
    }
    
    void LateUpdate()
    {
        if (flockManager == null || flockManager.allBirds.Count == 0)
            return;
            
        Vector3 flockCenter = CalculateFlockCenter();
        Vector3 flockVelocity = CalculateAverageVelocity();
        
        // Look ahead in the direction the flock is moving
        Vector3 targetPosition = flockCenter + flockVelocity.normalized * lookAheadDistance;
        targetPosition.y = flockCenter.y + height;
        
        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        // Point camera down at flock
        Vector3 lookDirection = flockCenter - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
    
    Vector3 CalculateFlockCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (Bird bird in flockManager.allBirds)
        {
            if (bird != null)
                center += bird.transform.position;
        }
        return center / flockManager.allBirds.Count;
    }
    
    Vector3 CalculateAverageVelocity()
    {
        Vector3 avgVelocity = Vector3.zero;
        foreach (Bird bird in flockManager.allBirds)
        {
            if (bird != null)
                avgVelocity += bird.Velocity;
        }
        return avgVelocity / flockManager.allBirds.Count;
    }
}