using UnityEngine;
using System.Collections;

class Tile
{
    public GameObject m_tile;
    public float creationTime;
    public Tile(GameObject _t, float _ct)
    {
        m_tile = _t;
        creationTime = _ct;
    }
}

public class GenerateLandscape : MonoBehaviour {

    public GameObject plane;
    public GameObject player;

    int planeSize = 4;
    int halfTilesX = 2;
    int halfTilesZ = 52;

    Vector3 startPos;

    Hashtable tiles = new Hashtable();

	// Use this for initialization
	void Start () {
        this.gameObject.transform.position = Vector3.zero;
        startPos = Vector3.zero;

        float updateTime = Time.realtimeSinceStartup;

        for (int i = -halfTilesX; i < halfTilesX; i++)
        {
            for (int z = -halfTilesZ; z < halfTilesZ; z++)
            {
                Vector3 pos = new Vector3((i * planeSize + startPos.x), 0, (z * planeSize + startPos.z));
                GameObject t = (GameObject)Instantiate(plane, pos, Quaternion.identity);

                string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();
                t.name = tilename;
                Tile tile = new Tile(t, updateTime);
                tiles.Add(tilename, tile);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        int xMove = (int)(player.transform.position.x - startPos.x);
        int zMove = (int)(player.transform.position.z - startPos.z);

        if (Mathf.Abs(xMove) >= planeSize || Mathf.Abs(zMove) >= planeSize)
        {
            float updateTime = Time.realtimeSinceStartup;

            int playerX = (int)(Mathf.Floor(player.transform.position.x / planeSize) * planeSize);
            int playerZ = (int)(Mathf.Floor(player.transform.position.z / planeSize) * planeSize);
            for (int i = -halfTilesX; i < halfTilesX; i++)
            {
                for (int z = -halfTilesZ; z < halfTilesZ; z++)
                {
                    Vector3 pos = new Vector3((i * planeSize + playerX), 0, (z * planeSize + playerZ));
                    string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();
                    if (!tiles.ContainsKey(tilename))
                    {
                        GameObject t = (GameObject)Instantiate(plane, pos, Quaternion.identity);
                        t.name = tilename;
                        Tile tile = new Tile(t, updateTime);
                        tiles.Add(tilename, tile);
                    }
                    else
                    {
                        (tiles[tilename] as Tile).creationTime = updateTime;
                    }
                }
            }
            Hashtable newTerrain = new Hashtable();
            foreach(Tile tls in tiles.Values)
            {
                if (tls.creationTime != updateTime)
                {
                    Destroy(tls.m_tile);
                }
                else
                {
                    newTerrain.Add(tls.m_tile.name, tls);
                }
            }
            tiles = newTerrain;
            startPos = player.transform.position;
        }
    }
}
