using UnityEngine;
using UnityEngine.InputSystem;

public class HandleCameras : MonoBehaviour
{
    public GameObject mainCamera; // Référence à la caméra principale
    public GameObject trainCamera; // Référence à la caméra en vue de dessus
    public bool isTrainCameraActive = false; // Indique si la caméra de train est active
    
    private Keyboard keyboard;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        keyboard = Keyboard.current;
    }

    // Update is called once per frame
    void Update()
    {
        if (keyboard.cKey.wasPressedThisFrame)
        {
            if (mainCamera.activeSelf)
            {
                mainCamera.SetActive(false);
                trainCamera.SetActive(true);
                isTrainCameraActive = true;
            }
            else
            {
                mainCamera.SetActive(true);
                trainCamera.SetActive(false);
                isTrainCameraActive = false;
            }
        }
    }
}
