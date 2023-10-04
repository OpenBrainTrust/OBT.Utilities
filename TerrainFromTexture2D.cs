using Sirenix.OdinInspector;
using UnityEngine;

public class TerrainFromTexture2D : MonoBehaviour {
    [Header("Target Terrain")]
    public Terrain terrain;

    [Tooltip("Heightmap Texture [.TIFF R16 Format]")]
    public Texture2D terrainTexture;

    //Actual procedure for getting a TIFF in as terrain is to bring the exported TIFF into the project and set it to
    //R 16 Bit Format. Everything else in the Import Settings can be left as default. Then, run the CreateTerrain method

    /// <summary>
    /// Method to create a terrain from a Texture2D. The texture must be in R16 format.
    /// </summary>
    /// <param name="_terrainObj">The Input Terrain GameObject, leave null to create a new GameObject with a Terrain ad hoc.</param>
    /// <param name="_texture">The Input Texture2D, in R 16-Bit format.</param>
    /// <returns>The target Terrain's GameObject.</returns>
    [Button]
    public Terrain CreateTerrain(GameObject _terrainObj, Texture2D _texture ) {
        Debug.Log("CreateTerrain: " + _texture.height + "h x " + _texture.width + "w");
        //Create new TerrainData
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = _texture.width;
        terrainData.size = new Vector3(_texture.width, 50, _texture.height);
        terrainData.SetHeights(0, 0, GetHeights(_texture));

        Terrain _terrain = null;
        if (!_terrainObj) {
            //Create a new terrain game object from the terrain data and get the terrain component
            GameObject terrainGameObject = Terrain.CreateTerrainGameObject(terrainData);
            _terrain = terrainGameObject.GetComponent<Terrain>();
        } else {
            //Get the terrain component from the input terrain game object and set its terrain data
            _terrain = _terrainObj.GetComponent<Terrain>();
            _terrain.terrainData = terrainData;
            //Update the terrain by flushing its data, which will cause it to recalculate its normals and other features
            _terrain.Flush();
        }

        //Set the most recently applied terrain in the inspector. This is just for convenience.
        terrain = _terrain;
        return _terrain;
    }

    /// <summary>
    /// Method that returns a 2D array of floats from a Texture2D. The texture must be in R16 format in order for the BitConverter to work.
    /// </summary>
    /// <param name="_texture">The Input Texture2D, in R 16-Bit format.</param>
    /// <returns>A 2D array of floats representing the heightmap of the input texture.</returns>
    public float[,] GetHeights( Texture2D _texture ) {
        //Get the raw data from the texture
        byte[] raw = _texture.GetRawTextureData();

        //Convert the raw data to floats in a 1D array with length equal to the number of pixels in the texture
        float[] heights = new float[raw.Length / 2];
        for (int i = 0; i < heights.Length; i++) {
            heights[i] = System.BitConverter.ToUInt16(raw, i * 2) / 65535f;
        }

        //Convert the 1D array to a 2D array with dimensions equal to the dimensions of the texture
        float[,] heights2D = new float[_texture.width, _texture.height];
        for (int x = 0; x < _texture.width; x++) {
            for (int y = 0; y < _texture.height; y++) {
                heights2D[x, y] = heights[x + y * _texture.width];
            }
        }

        return heights2D;
    }
}