using UnityEngine;

[CreateAssetMenu(fileName = "Tile", menuName = "WFC/Tile")]
public class TileData : ScriptableObject {
    public string tileName;
    public GameObject prefab;

    public string[] allowedUp;
    public string[] allowedDown;
    public string[] allowedLeft;
    public string[] allowedRight;
}