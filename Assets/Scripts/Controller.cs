using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public Transform Player;
    public int Size;
    public float DistanceThreshold;
    public Material CubeMaterial;

    public float TerrainFactorA;
    public float TerrainFactorB;
    public GameObject CubePrefab;

    private int NumCubes;
    private BlockingCollection<Vector3> PositionQueue;
    private BlockingCollection<CubeData> CubeQueue;
    private BlockingCollection<int> ConsumptionQueue;
    private GameObject[] cubes;
    private CubeLoadingThread LoadingThread;


    void Awake()
    {
        PositionQueue = new BlockingCollection<Vector3>(new ConcurrentQueue<Vector3>());
        CubeQueue = new BlockingCollection<CubeData>(new ConcurrentQueue<CubeData>());
        ConsumptionQueue = new BlockingCollection<int>(new ConcurrentQueue<int>());
        LoadingThread = new CubeLoadingThread(
            PositionQueue, 
            CubeQueue,
            DistanceThreshold, 
            Size,
            TerrainFactorA,
            TerrainFactorB
        );
    }

    void Start()
    {
        LoadingThread.Start();
        int numCubes = Size * Size;
        cubes = new GameObject[numCubes];
        for (int i = 0; i < numCubes; i++)
        {
            cubes[i] = Instantiate(CubePrefab);
            cubes[i].GetComponent<MeshRenderer>().material = CubeMaterial;
        }
    }

    void Update()
    {
        PositionQueue.Add(Player.position);
        CubeData cube;
        float startTime = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup < startTime + 0.001f)
        {
            if (CubeQueue.TryTake(out cube))
            {
                cubes[cube.Index].GetComponent<CubeController>().SetNewPosition(cube.Position);
                cubes[cube.Index].GetComponent<MeshRenderer>().material.color = GetColor(cube.Position.y);
            } else {
                break;
            }
        }
    }

    Color GetColor(float y)
    {
        float height = y / TerrainFactorB;
        if (height < 0.25)
        {
            return Color.blue;
        } else if (height < 0.28) {
            return Color.yellow;
        } else if (height < 0.5) {
            return Color.green;
        } else if (height < 0.8) {
            return new Color(0.5f,0.2f,0.1f);
        } else {
            return Color.white;
        }
    }
}

public class CubeLoadingThread
{
    private BlockingCollection<Vector3> PositionQueue;
    private BlockingCollection<CubeData> CubeQueue;
    private Thread CubeThread;
    private Vector3Int PlayerPosition;
    private float DistanceThreshold;
    private bool[] IndexStatus;
    private int Size;
    private int[,] Field;
    private float TerrainFactorA;
    private float TerrainFactorB;

    public CubeLoadingThread(
        BlockingCollection<Vector3> positionQueue,
        BlockingCollection<CubeData> cubeQueue,
        float distanceThreshold,
        int size,
        float terrainFactorA,
        float terrainFactorB
    ) {
        CubeQueue = cubeQueue;
        PositionQueue = positionQueue;
        DistanceThreshold = distanceThreshold;
        Size = size;
        CubeThread = new Thread(ThreadLoop);
        PlayerPosition = Vector3Int.zero;
        IndexStatus = new bool[Size * Size];
        TerrainFactorA = terrainFactorA;
        TerrainFactorB = terrainFactorB;
        InitializeField();
    }

    void InitializeField()
    {
        Field = new int[Size, Size];
        int index = 0;
        Vector3Int center = Vector3Int.zero;
        int radius = (Size - 1) / 2;
        for (int x = 0; x < Size; x++)
        {
            for (int z = 0; z < Size; z++)
            {
                Field[x, z] = index;
                Vector3 pos = new Vector3(x - radius + center.x, 0, z - radius + center.z);
                pos.y = Mathf.PerlinNoise((pos.x + 500f)  * TerrainFactorA , (pos.z + 500f) * TerrainFactorA) * TerrainFactorB;
                CubeQueue.TryAdd(new CubeData(index, pos));
                index++;
            }
        }
    }


    void ThreadLoop()
    {
        while(true)
        {
            Vector3 newPosition;
            if (PositionQueue.TryTake(out newPosition))
            {
                UpdateField(Vector3Int.FloorToInt(newPosition));
            }
        }
    }

    void UpdateField(Vector3Int newPosition)
    {
        int[,] tempField = new int[Size, Size];
        Vector3Int distance = newPosition - PlayerPosition;
        if (distance.magnitude < DistanceThreshold) return;
        int radius = (Size - 1) / 2;
        for (int x = 0; x < Size; x++)
        {
            for (int z = 0; z < Size; z++)
            {
                tempField[x, z] = Field[(int)Mathf.Repeat(x + distance.x, Size), (int)Mathf.Repeat(z + distance.z, Size)];
                if (
                    x + distance.x >= Size || x + distance.x < 0 || z + distance.z >= Size || z + distance.z < 0
                ) {
                    Vector3 pos = new Vector3(x - radius + newPosition.x, 0, z - radius + newPosition.z);
                    pos.y = Mathf.PerlinNoise((pos.x + 500f)  * TerrainFactorA , (pos.z + 500f) * TerrainFactorA) * TerrainFactorB;
                    CubeQueue.TryAdd(new CubeData(tempField[x, z], pos));
                }

              
            }
        }
        Field = tempField;
        PlayerPosition = newPosition;
    }

    public void Start()
    {
        CubeThread.Start();
    }

    public void Stop()
    {
        CubeThread.Abort();
    }

    static int mod(int a, int b)
    {
        return a - b * (int)Mathf.Floor(a / b);
    }


}

public class CubeData
{
    public int Index;
    public Vector3 Position;

    public CubeData(int index, Vector3 position)
    {
        Index = index;
        Position = position;
    }
}
