using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    [Header("RNG")] // one out of probability, called multiple times per second
    public int defaultDoxyProbability;
    public int defaultEnergyProbability;
    public int defaultIronProbability;
    int currentDoxyProbability;
    int currentEnergyProbability;
    int currentIronProbability;

    [Header("Resources")]
    public Vector2 ironSpeedRange;
    public Vector2 ironSpawnYRange;
    GameObject ironPrefab;

    [Header("Walls")]
    public float wallSpeed;
    float topWallY;
    float bottomWallY;
    LayerMask wallLayerMask;

    [Header("Component References")]
    public Transform wallPool;
    public Transform ironPool;
    public ParticleSystem doxycycline { get; private set; }
    public ParticleSystem energy { get; private set; }

    Transform player;
    Rigidbody2D playerRB;

    Camera mainCam;
    Plane[] cameraBounds;

    List<GameObject> activeObjects = new List<GameObject>();

    public static LevelManager instance;

    private void Awake()
    {
        instance = this;
        wallLayerMask = LayerMask.GetMask("Wall");
        ironPrefab = (GameObject)Resources.Load("Prefabs/Iron");

        SetAllResourceSpawnsToDefault();
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        cameraBounds = GeometryUtility.CalculateFrustumPlanes(mainCam);

        player = Player.instance.transform;
        playerRB = player.GetComponent<Rigidbody2D>();
        if (CanvasManager.instance.atpBarFill.fillAmount == 1)
        {
            SetEnergySpawnProbability(false);
        }

        for (int i = 0; i < wallPool.childCount; i++)
        {
            if (wallPool.GetChild(i).gameObject.activeInHierarchy)
            {
                activeObjects.Add(wallPool.GetChild(i).gameObject);
                if (wallPool.GetChild(i).position.y > 0)
                {
                    topWallY = wallPool.GetChild(i).position.y;
                }
                else
                {
                    bottomWallY = wallPool.GetChild(i).position.y;
                }
            }
        }

        doxycycline = GameObject.FindGameObjectWithTag("Doxycycline").GetComponent<ParticleSystem>();
        energy = GameObject.FindGameObjectWithTag("Energy").GetComponent<ParticleSystem>();
        for (int i = 0; i < ironPool.childCount; i++)
        {
            if (ironPool.GetChild(i).gameObject.activeInHierarchy)
            {
                activeObjects.Add(ironPool.GetChild(i).gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < activeObjects.Count; i++) // loop through all in-scene objects
        {
            Transform curr = activeObjects[i].transform;
            if (curr.position.x < -15) // if object has moved out of camera range
            {
                curr.gameObject.SetActive(false); // deactivate
                if (curr.parent == wallPool) // if object is a wall
                {
                    if (curr.position.y > 0) SpawnWall(true); // spawn a new upper wall
                    else SpawnWall(false); // or spawn a new lower wall
                }
                activeObjects.RemoveAt(i); // remove object from list of active objects
                i--; // decrease loop count
            }
            else if (LayerMask.LayerToName(activeObjects[i].layer) == "Wall")
            {
                curr.Translate(Vector2.left * wallSpeed * Time.deltaTime);
            }
        }
    }

    void FixedUpdate() // putting this in fixed update to prevent item spawn rate from being affected by frame rate
    {
        if (playerRB.velocity.x > 0)
        {
            if (currentDoxyProbability != -1)
            {
                int r = (int)Random.Range(0, currentDoxyProbability / playerRB.velocity.x);
                if (r == 0) SpawnDoxy();
            }

            if (currentEnergyProbability != -1)
            {
                int r = (int)Random.Range(0, currentEnergyProbability / playerRB.velocity.x);
                if (r == 0) SpawnEnergy();
            }

            if (currentIronProbability != -1)
            {
                int r = (int)Random.Range(0, currentIronProbability / playerRB.velocity.x);
                if (r == 0) SpawnIron();
            }
        }
    }

    public void SetAllResourceSpawnsToDefault()
    {
        SetIronSpawnProbability(true);
        SetEnergySpawnProbability(true);
        SetDoxySpawnProbability(true);
    }

    public void SetIronSpawnProbability(bool resetToDefault, int newIron = -1)
    {
        if (resetToDefault)
        {
            currentIronProbability = defaultIronProbability;
        }
        else
        {
            currentIronProbability = newIron;
        }
    }

    public void SetDoxySpawnProbability(bool resetToDefault, int newDoxy = -1)
    {
        if (resetToDefault)
        {
            currentDoxyProbability = defaultDoxyProbability;
        }
        else
        {
            currentDoxyProbability = newDoxy;
        }
    }

    public void SetEnergySpawnProbability(bool resetToDefault, int newEnergy = -1)
    {
        if (resetToDefault)
        {
            currentEnergyProbability = defaultEnergyProbability;
        }
        else
        {
            currentEnergyProbability = newEnergy;
        }
    }

    void SpawnWall(bool top)
    {
        for (int i = 0; i < wallPool.childCount; i++)
        {
            var curr = wallPool.GetChild(i);
            if (!curr.gameObject.activeInHierarchy)
            {
                if (top)
                {
                    Vector2 p = mainCam.ViewportToWorldPoint(new Vector2(1, 1));
                    RaycastHit2D hit = Physics2D.Raycast(p, Vector2.down, 5, wallLayerMask);
                    if (hit.collider != null)
                    {
                        curr.gameObject.SetActive(true);
                        float newWallX = hit.transform.position.x + hit.collider.offset.x + hit.collider.bounds.size.x / 2;
                        newWallX += curr.GetComponent<BoxCollider2D>().offset.x + (curr.GetComponent<BoxCollider2D>().bounds.size.x / 2);
                        Vector2 newWallPosition = new Vector2(newWallX, topWallY);
                        curr.position = newWallPosition;
                        curr.localScale = new Vector2(curr.localScale.x, Mathf.Abs(curr.localScale.y));
                        curr.gameObject.SetActive(true);
                        activeObjects.Add(curr.gameObject);
                    }
                    else
                    {
                        Debug.Log("could not find rightmost upper wall");
                    }
                }
                else // if bottom wall
                {
                    Vector2 p = mainCam.ViewportToWorldPoint(new Vector2(1, 0));
                    RaycastHit2D hit = Physics2D.Raycast(p, Vector2.up, 5, wallLayerMask);
                    if (hit.collider != null)
                    {
                        curr.gameObject.SetActive(true);
                        float newWallX = hit.transform.position.x + hit.collider.offset.x + (hit.collider.bounds.size.x / 2);
                        newWallX += curr.GetComponent<BoxCollider2D>().offset.x + (curr.GetComponent<BoxCollider2D>().bounds.size.x / 2);
                        Vector2 newWallPosition = new Vector2(newWallX, bottomWallY);
                        curr.position = newWallPosition;
                        curr.localScale = new Vector2(curr.localScale.x, -Mathf.Abs(curr.localScale.y));
                        curr.gameObject.SetActive(true);
                        activeObjects.Add(curr.gameObject);
                    }
                    else
                    {
                        Debug.Log("could not find rightmost lower wall");
                    }
                }
                return;
            }
        }
        Debug.Log("no more walls in pool to pick from");
    }

    public void SpawnDoxy()
    {
        doxycycline.Play();
    }

    public void SpawnEnergy()
    {
        energy.Play();
    }

    public void SpawnIron()
    {
        float spawnX = mainCam.ViewportToWorldPoint(new Vector2(1, 1)).x + 5;
        float spawnY = Random.Range(ironSpawnYRange.x, ironSpawnYRange.y);
        for (int i = 0; i < ironPool.childCount; i++)
        {
            var curr = ironPool.GetChild(i);
            if (!curr.gameObject.activeInHierarchy)
            {
                curr.position = new Vector2(spawnX, spawnY);
                curr.gameObject.SetActive(true);
                activeObjects.Add(curr.gameObject);
                return;
            }
        }
        GameObject newIron = Instantiate(ironPrefab, new Vector2(spawnX, spawnY), Quaternion.identity);
        activeObjects.Add(newIron);
        newIron.transform.parent = ironPool;
    }
}
