using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class TerrainAdapter : MonoBehaviour
{
    [SerializeField] private Terrain _workingTerrain = null;

    private Sketch _sketch;
    private List<Vector2> _CandidateTargets;
    private List<Vector2> _polyBrokenTargets;
    private const int _profileLength = 6;

    [Header("Debugging")]
    [SerializeField] private GameObject _debugShape = null;

    public TerrainAdapter(Terrain workingTerrain)
    {
        _workingTerrain = workingTerrain;
    }

    private void Awake()
    {
        _sketch = new Sketch();
        _CandidateTargets = new List<Vector2>();
        _polyBrokenTargets = new List<Vector2>();
    }

    private void Update()
    { }

    public void StartSketch()
    {

    }

    public void EndSketch()
    {

    }

    public void RunPPA() // Detecting the terrain features
    {
        TargetRecognition();
        //TargetConnection();
        PolygonBreaking();
    }

    private void TargetRecognition() // Detecting points that could be on a ridge
    {
        int mapWidth = _workingTerrain.terrainData.heightmapWidth;
        int mapHeight = _workingTerrain.terrainData.heightmapHeight;

        for (int indexY = 0; indexY < mapHeight; ++indexY)
        {
            for (int indexX = 0; indexX < mapWidth; ++indexX)
            {
                if (!IsCoordinateValid(_workingTerrain, indexX, indexY, _profileLength))
                    continue;

                if (IsTarget(_workingTerrain, indexX, indexY, _profileLength))
                {
                    Vector2 newTarget = new Vector2(indexX, indexY);
                    _CandidateTargets.Add(newTarget);
                }
            }
        }

        Debug.Log
            ($"{_CandidateTargets.Count} targets found during target-recognotion with profile-length {_profileLength}");
        DebugDrawTargets(_CandidateTargets);
    }

    //private void TargetConnection() // Connecting neighboring points from the candidates
    //{

    //}

    private void PolygonBreaking() // Breaking polygons (remove least important connection)
    {
        foreach (Vector2 target in _CandidateTargets)
        {
            List<Vector2> neighborhood = GetConnectedNeighborhood(target);
        }
    }

    private void BranchReduction() // Eliminating less important branches
    {

    }

    private bool IsCoordinateValid(Terrain terrain, int x, int y, int profileLength)
    {
        int mapWidth = terrain.terrainData.heightmapWidth;
        int mapHeight = terrain.terrainData.heightmapHeight;

        bool valid =
            (x - profileLength > 0) &&
            (x + profileLength < mapWidth) &&
            (y - profileLength > 0) &&
            (y + profileLength < mapHeight);

        return valid;
    }

    private bool IsTarget(Terrain terrain, int x, int y, int profileLength)
    {
        float centerHeight = terrain.terrainData.GetHeight(x, y);

        // Vertical target check
        float[] neightborHeights = new float[2 * profileLength];
        for (int count = 1; count < profileLength; ++count)
        {
            neightborHeights[count - 1] = terrain.terrainData.GetHeight(x, y + count);
            neightborHeights[count] = terrain.terrainData.GetHeight(x, y - count);
        }
        if (HeightCheck(centerHeight, neightborHeights))
            return true;

        // Horizontal height check
        for (int count = 1; count < profileLength; ++count)
        {
            neightborHeights[count - 1] = terrain.terrainData.GetHeight(x + count, y);
            neightborHeights[count] = terrain.terrainData.GetHeight(x - count, y);
        }
        if (HeightCheck(centerHeight, neightborHeights))
            return true;

        // Northwest --> southeast height check
        for (int count = 1; count < profileLength; ++count)
        {
            neightborHeights[count - 1] = terrain.terrainData.GetHeight(x - count, y + count);
            neightborHeights[count] = terrain.terrainData.GetHeight(x + count, y - count);
        }
        if (HeightCheck(centerHeight, neightborHeights))
            return true;

        // Northeast --> southwest height check
        for (int count = 1; count < profileLength; ++count)
        {
            neightborHeights[count - 1] = terrain.terrainData.GetHeight(x + count, y + count);
            neightborHeights[count] = terrain.terrainData.GetHeight(x - count, y - count);
        }
        if (HeightCheck(centerHeight, neightborHeights))
            return true;

        return false;
    }

    private bool HeightCheck(float centerValue, float[] heights)
    {
        foreach (float height in heights)
        {
            if (height > centerValue)
                return false;
        }

        return true;
    }

    private void DebugDrawTargets(List<Vector2> targets)
    {
        GameObject debugShapes = new GameObject("DebugShapes");

        foreach (Vector2 target in targets)
        {
            float heightValue = _workingTerrain.terrainData.GetHeight((int)target.x, (int)target.y);
            Vector3 terrainPos = _workingTerrain.GetPosition();
            Vector3 shapePos = new Vector3(terrainPos.x + target.x, heightValue, terrainPos.y + target.y);

            GameObject shape = Instantiate(_debugShape, debugShapes.transform);
            shape.transform.position = shapePos;
        }
    }

    private List<Vector2> GetConnectedNeighborhood(Vector2 target)
    {
        List<Vector2> neighborhood = new List<Vector2>();
        neighborhood.Add(target);

        Vector2 neighbor = new Vector2(target.x, target.y + 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);

        neighbor = new Vector2(target.x + 1, target.y + 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);

        neighbor = new Vector2(target.x + 1, target.y);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);

        neighbor = new Vector2(target.x + 1, target.y - 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);

        neighbor = new Vector2(target.x, target.y - 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);

        neighbor = new Vector2(target.x - 1, target.y - 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);

        neighbor = new Vector2(target.x - 1, target.y);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);

        neighbor = new Vector2(target.x - 1, target.y + 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);

        return neighborhood;
    }

    private bool IsValidNeighbor(Vector2 neighbor)
    {
        if (_CandidateTargets.Contains(neighbor) &&
            IsCoordinateValid(_workingTerrain, (int) neighbor.x, (int) neighbor.y, 1))
        {
            return true;
        }

        return false;
    }


    //private List<Vector2> PolyCheck(List<Vector2> neighborhood)
    //{
    //    foreach (Vector2 target in neighborhood)
    //    {
            
    //    }
    //}
}