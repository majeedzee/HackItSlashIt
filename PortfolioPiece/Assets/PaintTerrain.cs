using UnityEngine;
using System.Collections;

public class PaintTerrain : MonoBehaviour {

    [System.Serializable]
    public class SplatHeights
    {
        public int textureIndex;
        public int startingHeight;
        public int overlap;
    }

    public SplatHeights[] splatHeights;

    TerrainData terrainData;

    float[,] newHeightData;

    [Header("Perlin Noise Settings")]
    [Range(0.000f,0.01f)]
    public float bumps;
    [Range(0.000f, 1.000f)]
    public float damping;

    [Header("Mountain Settings")]
    public int numMountains;
    [Range(0.001f, 0.5f)]
    public float heightChange;
    [Range(0.0001f, 0.05f)]
    public float sideSlope;

    [Header("Hole Settings")]
    public int numHoles;
    [Range(0.0f, 1.0f)]
    public float holeDepth;
    [Range(0.001f, 0.5f)]
    public float holeChange;
    [Range(0.0001f, 0.05f)]
    public float holeSlope;

    [Header("River Settings")]
    public int numRivers;
    [Range(0.001f, 0.05f)]
    public float digDepth;
    [Range(0.001f, 1.0f)]
    public float maxDepth;
    [Range(0.0001f, 0.05f)]
    public float bankSlope;

    [Header("Rough Settings")]
    [Range(0.000f, 0.05f)]
    public float roughAmount;
    [Range(0, 5)]
    [Header("Smooth Settings")]
    public int smoothAmount;
    void Normalize(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }
        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    public float map(float value, float sMin, float sMax, float mMin, float mMax)
    {
        return (value - sMin) * (mMax - mMin) / (sMax - sMin) + mMin;
    }

    void Mountains(int x, int y, float height, float slope)
    {
        if (x <= 0 || x >= terrainData.alphamapWidth) return;
        if (y <= 0 || y >= terrainData.alphamapHeight) return;
        if (height <= 0) return;
        if (newHeightData[x, y] >= height) return;
        newHeightData[x, y] = height;

        Mountains(x - 1, y, height - Random.Range(0.001f, slope), slope);
        Mountains(x + 1, y, height - Random.Range(0.001f, slope), slope);
        Mountains(x, y - 1, height - Random.Range(0.001f, slope), slope);
        Mountains(x, y + 1, height - Random.Range(0.001f, slope), slope);
    }

    void River(int x, int y, float height, float slope)
    {
        if (x <= 0 || x >= terrainData.alphamapWidth) return;
        if (y <= 0 || y >= terrainData.alphamapHeight) return;
        if (height <= maxDepth) return;
        if (newHeightData[x, y] <= height) return;
        newHeightData[x, y] = height;

        River(x + 1, y, height + Random.Range(slope, slope + 0.01f), slope);
        River(x - 1, y, height + Random.Range(slope, slope + 0.01f), slope);

        River(x + 1, y + 1, height + Random.Range(slope, slope + 0.01f), slope);
        River(x - 1, y + 1, height + Random.Range(slope, slope + 0.01f), slope);

        River(x, y - 1, height + Random.Range(slope, slope + 0.01f), slope);
        River(x, y + 1, height + Random.Range(slope, slope + 0.01f), slope);

    }

    void Holes(int x, int y, float height, float slope)
    {
        if (x <= 0 || x >= terrainData.alphamapWidth) return;
        if (y <= 0 || y >= terrainData.alphamapHeight) return;
        if (height <= holeDepth) return;
        if (newHeightData[x, y] <= height) return;
        newHeightData[x, y] = height;

        Holes(x - 1, y, height + Random.Range(slope, slope + 0.01f), slope);
        Holes(x + 1, y, height + Random.Range(slope, slope + 0.01f), slope);
        Holes(x, y - 1, height + Random.Range(slope, slope + 0.01f), slope);
        Holes(x, y + 1, height + Random.Range(slope, slope + 0.01f), slope);
    }

    void RoughTerrain()
    {
        for (int i = 0; i < terrainData.alphamapHeight; i++)
        {
            for (int j = 0; j < terrainData.alphamapWidth; j++)
            {
                newHeightData[j, i] += Random.Range(0, roughAmount);

            }
        }
    }

    void SmoothTerrain()
    {
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float avgheight = (newHeightData[x, y] +
                    newHeightData[x + 1, y] + newHeightData[x - 1, y] +
                    newHeightData[x + 1, y + 1] + newHeightData[x - 1, y - 1] +
                    newHeightData[x + 1, y - 1] + newHeightData[x - 1, y + 1] +
                    newHeightData[x, y + 1] + newHeightData[x, y - 1]) / 9.0f;
                newHeightData[x, y] = avgheight;
            }
        }
    }

    void ApplyRiver()
    {
        for (int i = 0; i < numRivers; i++)
        {
            int cx = Random.Range(10, terrainData.alphamapWidth - 10);
            int cy = 0;// Random.Range(10, terrainData.alphamapHeight - 10);
            int xDir = Random.Range(-1, 2);
            int yDir = Random.Range(-1, 2);

            while (cy >= 0 && cy < terrainData.alphamapHeight - 2 && cx > 0 && cx < terrainData.alphamapWidth)
            {
                River(cx, cy, newHeightData[cx, cy] - digDepth, bankSlope);
                if (Random.Range(0,50) < 5)
                {
                    xDir = Random.Range(-1, 2);
                }
                if (Random.Range(0, 50) < 5)
                {
                    yDir = Random.Range(0, 2);
                }
                cx += xDir;
                cy += yDir;
            }
        }
    }

    void ApplyHoles()
    {
        for (int i = 0; i < numHoles; i++)
        {
            int xPos = Random.Range(10, terrainData.alphamapWidth - 10);
            int yPos = Random.Range(10, terrainData.alphamapHeight - 10);
            float newHeight = newHeightData[xPos, yPos] - holeChange;
            Holes(xPos, yPos, newHeight, holeSlope);
        }
    }

    void ApplyMountains()
    {
        for (int i = 0; i < numMountains; i++)
        {
            int xPos = Random.Range(10, terrainData.alphamapWidth - 10);
            int yPos = Random.Range(10, terrainData.alphamapHeight - 10);
            float newHeight = newHeightData[xPos, yPos] + heightChange;
            Mountains(xPos, yPos, newHeight, sideSlope);
        }
    }
	
    void ApplyPerlin()
    {
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                newHeightData[x, y] = Mathf.PerlinNoise(x * bumps, y * bumps) * damping;
            }
        }
    }

	public void Start () {
        terrainData = Terrain.activeTerrain.terrainData;
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        newHeightData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight];

        ApplyPerlin();
        RoughTerrain();
        ApplyMountains();
        ApplyHoles();
        ApplyRiver();
        for (int i = 0; i < smoothAmount; i++)
        {
            SmoothTerrain();

        }
        terrainData.SetHeights(0, 0, newHeightData);

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float terrainHeight = terrainData.GetHeight(y, x);
                float[] splat = new float[splatHeights.Length];

                for (int i = 0; i < splatHeights.Length; i++)
                {
                    float thisNoise = map(Mathf.PerlinNoise(x * 0.05f, y * 0.05f), 0, 1, 0.5f, 1);
                    float thisHeightStart = splatHeights[i].startingHeight * thisNoise - splatHeights[i].overlap * thisNoise;
                    float nextHeightStart = 0;

                    if (i != splatHeights.Length -1)
                    {
                        nextHeightStart = splatHeights[i + 1].startingHeight * thisNoise + splatHeights[i + 1].overlap * thisNoise;
                    }

                    if (i == splatHeights.Length-1 && terrainHeight >= thisHeightStart)
                    {
                        splat[i] = 1;
                    }
                    else if (terrainHeight >= thisHeightStart && terrainHeight <= nextHeightStart)
                    {
                        splat[i] = 1;
                    }
                }
                Normalize(splat);
                for (int j = 0; j < splatHeights.Length; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
	}
	
}
