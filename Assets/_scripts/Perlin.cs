using UnityEngine;

public class Perlin : MonoBehaviour
{
    public GameObject cubePrefab;       
    public int gridSizeX = 10;          
    public int gridSizeZ = 10;          
    public float noiseScale = 0.1f;     
    public float heightMultiplier = 10f; 
    public float cubeThreshold = 0.3f; 
    public Vector3 spacing = new Vector3(1f, 1f, 1f); 
    public Transform gridOrigin;      

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Vector3 originPosition = gridOrigin != null ? gridOrigin.position : transform.position;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                float perlinValue = Mathf.PerlinNoise((x + originPosition.x) * noiseScale, (z + originPosition.z) * noiseScale);

                if (perlinValue > cubeThreshold)
                {
                    float height = perlinValue * heightMultiplier;
                    Vector3 spawnPosition = originPosition + new Vector3(x * spacing.x, height, z * spacing.z);
                    Instantiate(cubePrefab, spawnPosition, Quaternion.identity, transform);
                }
            }
        }
    }
}