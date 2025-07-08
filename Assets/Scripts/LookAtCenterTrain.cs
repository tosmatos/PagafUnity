using UnityEngine;

public class LookAtCenterTrain : MonoBehaviour
{
    public GameObject train;
    public GameObject target; // Object the camera looks at (map center)
    public float distance = 10f; // Distance from the train
    public float height = 5f; // Height above the train

    void Update()
    {
        if (!train || !target) return;
        
        // Calculate direction from target (map center) to train
        Vector3 directionFromCenter = (train.transform.position - target.transform.position).normalized;
        
        // Position camera behind the train (away from the center)
        Vector3 targetPosition = train.transform.position + directionFromCenter * distance;
        targetPosition.y = train.transform.position.y + height; // Add height
        
        transform.position = targetPosition;

        // Look at the target (map center)
        transform.LookAt(target.transform);
    }
}