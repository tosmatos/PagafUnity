using System.Collections.Generic;
using UnityEngine;

public class Cell {
    public List<TileData> possibleTiles;
    public List<int> weight;
    public bool isCollapsed => possibleTiles.Count == 1;

    public Cell(TileData[] allTiles) {
        possibleTiles = new List<TileData>(allTiles);
    }

    public bool CollapseRandomly() {
        if (!isCollapsed) {
            if (possibleTiles.Count == 0) {
                Debug.Log("No possible tiles to collapse. Redoing...");
                return false;
            }
            var chosen = possibleTiles[UnityEngine.Random.Range(0, possibleTiles.Count)];
            possibleTiles = new List<TileData> { chosen };
            return true;
        }
        return false;
    }

    public TileData GetCollapsedTile() {
        return isCollapsed ? possibleTiles[0] : null;
    }
}