using UnityEngine;
using System.Collections.Generic;

public class FlockManager : MonoBehaviour
{
    [Header("Flock Settings")]
    [SerializeField] private int flockSize = 20;
    [SerializeField] private GameObject birdPrefab;
    [SerializeField] private float spawnRadius = 5.0f;
    
    [Header("Behavior")]
    [SerializeField] private float maxNeighborRadius = 15.0f;
    public Vector3 GoalPosition { get; set; } = new Vector3(0, 10, 0);
    
    [Header("Goal Settings")]
    [SerializeField] private bool useMovingGoal = true;
    [SerializeField] private float goalMoveSpeed = 5.0f;
    [SerializeField] private float goalMoveRadius = 20.0f;
    
    // Active birds in the flock
    private List<Bird> allBirds = new List<Bird>();
    private float goalAngle = 0f;
    
    void Start()
    {
        SpawnFlock();
    }
    
    void Update()
    {
        UpdateGoalPosition();
        UpdateFlock();
    }
    
    private void UpdateGoalPosition()
    {
        if (useMovingGoal)
        {
            goalAngle += goalMoveSpeed * Time.deltaTime;
            GoalPosition = new Vector3(
                Mathf.Cos(goalAngle) * goalMoveRadius,
                10f + Mathf.Sin(goalAngle * 0.5f) * 5f,
                Mathf.Sin(goalAngle) * goalMoveRadius
            );
        }
    }
    
    private void UpdateFlock()
    {
        // Critical: Three-phase update to prevent frame-dependent behavior
        
        // Phase 1: All birds observe current world state (neighbor finding)
        Bird[] birdArray = allBirds.ToArray(); // Snapshot for consistent neighbor calculations
        
        // Phase 2: All birds calculate forces based on same snapshot
        foreach (Bird bird in allBirds)
        {
            if (bird != null)
            {
                Bird[] neighbors = GetNeighbors(bird.transform.position, maxNeighborRadius);
                bird.CalculateForces(neighbors);
            }
        }
        
        // Phase 3: All birds move simultaneously
        foreach (Bird bird in allBirds)
        {
            if (bird != null)
            {
                bird.UpdateMovement(Time.deltaTime);
            }
        }
    }
    
    private void SpawnFlock()
    {
        allBirds.Clear();
        
        if (birdPrefab == null)
        {
            Debug.LogError("Bird prefab is not assigned!");
            return;
        }
        
        for (int i = 0; i < flockSize; i++)
        {
            // Random position within spawn sphere
            Vector3 spawnLocation = transform.position + Random.insideUnitSphere * spawnRadius;
            
            GameObject birdObject = Instantiate(birdPrefab, spawnLocation, Quaternion.identity);
            Bird birdComponent = birdObject.GetComponent<Bird>();
            
            if (birdComponent != null)
            {
                // Initialize with slightly different velocities for natural spreading
                Vector3 baseDirection = Vector3.forward;
                Vector3 randomOffset = Random.insideUnitSphere * 0.3f;
                Vector3 initialVelocity = (baseDirection + randomOffset).normalized * 
                    Random.Range(8.0f, 12.0f);
                
                birdComponent.SetVelocity(initialVelocity);
                allBirds.Add(birdComponent);
            }
            else
            {
                Debug.LogError("Bird prefab must have a Bird component!");
                Destroy(birdObject);
            }
        }
        
        Debug.Log($"Spawned {allBirds.Count} birds");
    }
    
    private Bird[] GetNeighbors(Vector3 position, float radius)
    {
        List<Bird> neighbors = new List<Bird>();
        
        // Simple brute force - check all birds
        // This is O(n²) but acceptable for small flocks (<100 birds)
        foreach (Bird bird in allBirds)
        {
            if (bird != null)
            {
                float distance = Vector3.Distance(position, bird.transform.position);
                if (distance <= radius && distance > 0.01f) // Exclude self (small threshold for floating point)
                {
                    neighbors.Add(bird);
                }
            }
        }
        
        // DEBUG: Occasionally log neighbor counts
        if (Random.Range(0.0f, 1.0f) < 0.01f)
        {
            Debug.Log($"Neighbors found: {neighbors.Count}, Radius: {radius:F1}");
        }
        
        return neighbors.ToArray();
    }
    
    // Public methods for runtime tweaking
    public void SetFlockSize(int newSize)
    {
        if (newSize != flockSize)
        {
            flockSize = newSize;
            ClearFlock();
            SpawnFlock();
        }
    }
    
    public void SetGoalPosition(Vector3 newGoal)
    {
        GoalPosition = newGoal;
        useMovingGoal = false; // Stop automatic movement when manually set
    }
    
    private void ClearFlock()
    {
        foreach (Bird bird in allBirds)
        {
            if (bird != null && bird.gameObject != null)
            {
                DestroyImmediate(bird.gameObject);
            }
        }
        allBirds.Clear();
    }
    
    void OnDestroy()
    {
        ClearFlock();
    }
    
    // Visualization helpers
    void OnDrawGizmos()
    {
        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Draw goal position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GoalPosition, 2.0f);
        
        // Draw neighbor radius for first bird (if exists)
        if (allBirds.Count > 0 && allBirds[0] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(allBirds[0].transform.position, maxNeighborRadius);
        }
    }
}