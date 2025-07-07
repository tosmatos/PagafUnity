using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WFCGenerator : MonoBehaviour {
    public int width = 10;
    public int height = 10;
    public TileData[] tileOptions;

    private Cell[,] grid;
    private int counter = 0;

    void Start() {
        GenerateGrid();
    }

    void GenerateGrid() {
        const int maxTotalRetries = 5;
        int totalRetries = 0;

        while (totalRetries < maxTotalRetries) {
            grid = new Cell[width, height];
            counter = 0;

            // Step 1: Initialize grid with all possibilities
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    grid[x, y] = new Cell(tileOptions);
                }
            }

            bool failed = false;

            // Step 2: Collapse loop
            while (!IsFullyCollapsed()) {
                var cellPos = GetCellWithLowestEntropy();
                if (cellPos == null) break;

                var (x, y) = cellPos.Value;
                var res = grid[x, y].CollapseRandomly();
                if (!res) {
                    Debug.Log("Failed to collapse cell, retrying...");
                    counter++;
                    if (counter > 10) {
                        Debug.LogWarning("Too many retries in this attempt, restarting entire generation...");
                        failed = true;
                        break;
                    }
                    continue; // If collapse failed, we need to retry
                }
                Propagate(x, y);
            }

            if (!failed) {
                // Step 3: Instantiate tiles
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        var tileData = grid[x, y].GetCollapsedTile();
                        if (tileData != null) {
                            var tileGO = Instantiate(tileData.prefab, new Vector3(x * 5, 0, y * 5), Quaternion.identity);
                            tileGO.name = $"Tile_{x}_{y}_{tileData.tileName}";
                        }
                    }
                }
                return; // Success
            }

            totalRetries++;
        }

        Debug.LogError("Wave Function Collapse failed after multiple attempts.");
    }

    bool IsFullyCollapsed() {
        foreach (var cell in grid)
            if (!cell.isCollapsed) return false;
        return true;
    }

    (int, int)? GetCellWithLowestEntropy() {
        int minEntropy = int.MaxValue;
        List<(int, int)> candidates = new();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var cell = grid[x, y];
                if (!cell.isCollapsed && cell.possibleTiles.Count < minEntropy) {
                    minEntropy = cell.possibleTiles.Count;
                    candidates.Clear();
                    candidates.Add((x, y));
                } else if (!cell.isCollapsed && cell.possibleTiles.Count == minEntropy) {
                    candidates.Add((x, y));
                }
            }
        }

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }

    void Propagate(int startX, int startY) {
        Queue<(int, int)> queue = new();
        queue.Enqueue((startX, startY));

        while (queue.Count > 0) {
            var (x, y) = queue.Dequeue();
            var current = grid[x, y];

            // Check 4 directions
            foreach (var dir in Vector2IntCardinals()) {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (IsInBounds(nx, ny)) {
                    var neighbor = grid[nx, ny];
                    if (neighbor.isCollapsed) continue;

                    bool changed = RestrictNeighbor(neighbor, dir, current);
                    if (changed) {
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }
    }

    bool RestrictNeighbor(Cell neighbor, Vector2Int dir, Cell source) {
        string direction = GetDirectionName(dir);
        Debug.Log($"Checking neighbor at direction '{direction}' with {neighbor.possibleTiles.Count} possible tiles.");
        List<TileData> validNeighbors = new();

        foreach (var tile in neighbor.possibleTiles) {
            bool anyValid = false;
            foreach (var srcTile in source.possibleTiles) {
                Debug.Log($" - Checking compatibility of source tile '{srcTile.tileName}' with neighbor tile '{tile.tileName}' in direction '{direction}'.");
                if (IsTileCompatible(srcTile, tile, direction)) {
                    Debug.Log($" - Tile '{tile.tileName}' is valid in direction '{direction}' due to match with source tile.");
                    anyValid = true;
                    break;
                }
            }
            if (anyValid)
                validNeighbors.Add(tile);
        }

        if (validNeighbors.Count < neighbor.possibleTiles.Count) {
            Debug.Log($"Reduced neighbor possibilities from {neighbor.possibleTiles.Count} to {validNeighbors.Count} in direction '{direction}'.");
            neighbor.possibleTiles = validNeighbors;
            return true; // possibilities changed
        }

        return false; // no change
    }

    bool IsTileCompatible(TileData src, TileData neighbor, string direction) {
        return direction switch {
            "up" => src.allowedUp.Contains(neighbor.tileName),
            "down" => src.allowedDown.Contains(neighbor.tileName),
            "left" => src.allowedLeft.Contains(neighbor.tileName),
            "right" => src.allowedRight.Contains(neighbor.tileName),
            _ => false
        };
    }

    bool IsInBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    string GetDirectionName(Vector2Int dir) {
        if (dir == Vector2Int.up) return "up";
        if (dir == Vector2Int.down) return "down";
        if (dir == Vector2Int.left) return "left";
        if (dir == Vector2Int.right) return "right";
        return "";
    }

    IEnumerable<Vector2Int> Vector2IntCardinals() {
        yield return Vector2Int.up;
        yield return Vector2Int.down;
        yield return Vector2Int.left;
        yield return Vector2Int.right;
    }
}