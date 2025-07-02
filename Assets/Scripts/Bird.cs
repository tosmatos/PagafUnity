using UnityEngine;

public class Bird : MonoBehaviour
{
    [Header("Boid Settings")]
    [SerializeField] private float separationRadius = 3.0f;
    [SerializeField] private float alignmentRadius = 8.0f;
    [SerializeField] private float cohesionRadius = 15.0f;
    
    [SerializeField] private float maxSeparationForce = 50.0f;
    [SerializeField] private float maxAlignmentForce = 30.0f;
    [SerializeField] private float maxCohesionForce = 20.0f;
    
    [Header("Flight Settings")]
    [SerializeField] private float maxSpeed = 15.0f;  // Scaled down from UE4 units
    [SerializeField] private float minSpeed = 5.0f;
    
    [Header("Boid Weights")]
    [SerializeField] private float separationWeight = 3.0f;
    [SerializeField] private float alignmentWeight = 2.0f;
    [SerializeField] private float cohesionWeight = 1.5f;
    [SerializeField] private float goalWeight = 0.5f;
    [SerializeField] private float avoidanceWeight = 3.0f;
    
    [Header("Animation")]
    [SerializeField] private bool isFlapping = false;
    
    // Current flight state
    public Vector3 Velocity { get; private set; }
    
    // Boid behavior forces
    private Vector3 separationForce;
    private Vector3 alignmentForce;
    private Vector3 cohesionForce;
    private Vector3 goalForce;
    private Vector3 totalForce;
    
    private FlockManager flockManager;
    
    private Animator animator;
    
    void Start()
    {
        // Initialize with random velocity
        Vector3 baseDirection = Vector3.forward;
        Vector3 randomOffset = Random.insideUnitSphere * 0.3f;
        Velocity = (baseDirection + randomOffset).normalized * Random.Range(8.0f, 12.0f);
        
        flockManager = FindObjectOfType<FlockManager>();
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        // Movement is handled by FlockManager to ensure synchronized updates
        UpdateAnimation();
    }
    
    public Vector3 CalculateSeparationForce(Bird[] neighbors)
    {
        Vector3 separationVector = Vector3.zero;
        int count = 0;
        
        foreach (Bird neighbor in neighbors)
        {
            if (neighbor == this) continue;
            
            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
            
            if (distance > 0 && distance < separationRadius)
            {
                // Direction away from neighbor
                Vector3 difference = transform.position - neighbor.transform.position;
                difference.Normalize();
                
                // Weight closer birds more heavily
                difference /= distance;
                
                separationVector += difference;
                count++;
            }
        }
        
        if (count > 0)
        {
            separationVector /= count; // Average the vectors
            separationVector.Normalize();
            return separationVector * maxSeparationForce;
        }
        
        return Vector3.zero;
    }
    
    public Vector3 CalculateAlignmentForce(Bird[] neighbors)
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;
        
        foreach (Bird neighbor in neighbors)
        {
            if (neighbor == this) continue;
            
            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
            
            if (distance > 0 && distance < alignmentRadius)
            {
                averageVelocity += neighbor.Velocity;
                count++;
            }
        }
        
        if (count > 0)
        {
            averageVelocity /= count;
            averageVelocity.Normalize();
            return averageVelocity * maxAlignmentForce;
        }
        
        return Vector3.zero;
    }
    
    public Vector3 CalculateCohesionForce(Bird[] neighbors)
    {
        Vector3 centerOfMass = Vector3.zero;
        int count = 0;
        
        foreach (Bird neighbor in neighbors)
        {
            if (neighbor == this) continue;
            
            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
            
            if (distance > 0 && distance < cohesionRadius)
            {
                centerOfMass += neighbor.transform.position;
                count++;
            }
        }
        
        if (count > 0)
        {
            centerOfMass /= count; // Average position
            Vector3 desiredDirection = centerOfMass - transform.position;
            desiredDirection.Normalize();
            return desiredDirection * maxCohesionForce;
        }
        
        return Vector3.zero;
    }
    
    public Vector3 CalculateGoalForce()
    {
        Vector3 targetPosition = flockManager != null ? flockManager.GoalPosition : Vector3.up * 20f;
        Vector3 desiredDirection = targetPosition - transform.position;
        float distance = desiredDirection.magnitude;
        
        if (distance > 5.0f) // Reduced threshold for Unity scale
        {
            desiredDirection.Normalize();
            // Fixed: Much more reasonable force magnitude
            return desiredDirection * 100.0f;
        }
        
        return Vector3.zero;
    }
    
    public void CalculateForces(Bird[] neighbors)
    {
        separationForce = CalculateSeparationForce(neighbors);
        alignmentForce = CalculateAlignmentForce(neighbors);
        cohesionForce = CalculateCohesionForce(neighbors);
        goalForce = CalculateGoalForce();
        
        // Combine with weights
        totalForce = (separationForce * separationWeight) +
                     (alignmentForce * alignmentWeight) +
                     (cohesionForce * cohesionWeight) +
                     (goalForce * goalWeight);
    }
    
    public void UpdateMovement(float deltaTime)
    {
        // Apply force to velocity (F = ma, assume mass = 1)
        Vector3 targetVelocity = Velocity + totalForce * deltaTime;
        Velocity = Vector3.Lerp(Velocity, targetVelocity, deltaTime * 3.0f);
        
        // DEBUG: Print force magnitudes occasionally
        if (Random.Range(0.0f, 1.0f) < 0.01f) // 1% of frames
        {
            Debug.Log($"Bird forces - Sep: {separationForce.magnitude:F2}, " +
                     $"Align: {alignmentForce.magnitude:F2}, " +
                     $"Cohesion: {cohesionForce.magnitude:F2}, " +
                     $"Goal: {goalForce.magnitude:F2}, " +
                     $"Speed: {Velocity.magnitude:F2}");
        }
        
        // Clamp speed to reasonable bird flight range
        float speed = Velocity.magnitude;
        if (speed > 0.1f) // Only clamp if velocity has meaningful direction
        {
            if (speed > maxSpeed)
            {
                Velocity = Velocity.normalized * maxSpeed;
            }
            else if (speed < minSpeed)
            {
                Velocity = Velocity.normalized * minSpeed;
            }
        }
        else
        {
            // If velocity is too small, give a default direction
            Velocity = Vector3.forward * minSpeed;
        }
        
        // Update position
        transform.position += Velocity * deltaTime;
        
        // Orient bird in flight direction
        if (Velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * 5.0f);
        }
    }
    
    private void UpdateAnimation()
    {
        // Simple flapping based on speed
        if (Velocity.y > 2.0f)
            isFlapping = true;
        else isFlapping = false;

        if (animator != null)
            animator.SetBool("isFlapping", isFlapping);
    }
    
    public void SetVelocity(Vector3 newVelocity)
    {
        Velocity = newVelocity;
    }
}