using UnityEngine;

public class LookAtCenterTrain : MonoBehaviour
{
    public GameObject train;
    public GameObject target; // Object the camera looks at
    public float distance = 10f; // Distance from the target

    private Vector3 offsetDirection;
    void Update()
    {
        if (!target) return;
        
        // Calculate the offset direction based on the target's forward vector
        offsetDirection = target.transform.forward * -1; // Look behind the target
        
        // Calculate position behind the target
        transform.position = train.transform.position + offsetDirection.normalized * distance;

        // Look at the target
        transform.LookAt(target.transform);
    }
}