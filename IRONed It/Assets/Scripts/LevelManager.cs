using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChelatedBy { None, Cholera, Coli, Heme, Sphaerogena };

public class LevelManager : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] float levelLengthInSeconds;

    [Header("RNG")] // one out of probability, called multiple times per second
    [SerializeField] int defaultDoxyProbability;
    [SerializeField] int defaultEnergyProbability;
    [SerializeField] int defaultIronProbability;
    int currentDoxyProbability;
    int currentEnergyProbability;
    int currentIronProbability;

    [Header("Resources")]
    public Vector2 ironSpeedRange;
    [HideInInspector] public List<Transform> targetedIron = new List<Transform>();
    GameObject ironPrefab;

    [Header("Enemies")]
    public Vector2 choleraSpeedRange;
    GameObject choleraPrefab;
    GameObject coliPrefab;

    [Header("Walls")]
    [SerializeField] float wallSpeed;
    float currentWallSpeed = 1;
    float wallSpeedSmoothing;
    float topWallY;
    float bottomWallY;
    LayerMask wallLayerMask;

    [Header("Component References")]
    [SerializeField] Transform wallPool;
    [SerializeField] Transform ironPool;
    [SerializeField] Transform choleraPool; // unfortunate names <.<
    public ParticleSystem doxycycline { get; private set; }
    public ParticleSystem energy { get; private set; }

    Transform player;
    Motile playerMovement;
    Rigidbody2D playerRB;

    Camera mainCam;
    //Plane[] cameraBounds;

    List<GameObject> activeObjects = new List<GameObject>();

    public static LevelManager instance;

    private void Awake()
    {
        instance = this;
        wallLayerMask = LayerMask.GetMask("Wall");
        ironPrefab = (GameObject)Resources.Load("Prefabs/Iron");
        choleraPrefab = (GameObject)Resources.Load("Prefabs/Cholera");

        SetAllResourceSpawnsToDefault();
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //cameraBounds = GeometryUtility.CalculateFrustumPlanes(mainCam);

        player = Player.instance.transform;
        playerRB = player.GetComponent<Rigidbody2D>();
        playerMovement = player.GetComponent<Motile>();
        if (CanvasManager.instance.GetAtpBarFill().fillAmount == 1)
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
        if (levelLengthInSeconds > 0)
        {
            if (playerMovement.agentCanMove)
            {
                levelLengthInSeconds -= Time.deltaTime;
                // on hitting zero, spawn victory gate as part of level; when it moves past player, end game
                if (currentWallSpeed != wallSpeed)
                {
                    currentWallSpeed += Mathf.SmoothDamp(currentWallSpeed, wallSpeed, ref wallSpeedSmoothing, 2 * Time.deltaTime);
                }
            }
            else
            {
                currentWallSpeed += Mathf.SmoothDamp(currentWallSpeed, 1, ref wallSpeedSmoothing, 2 * Time.deltaTime);
            }
        }

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
                curr.Translate(Vector2.left * currentWallSpeed * Time.deltaTime);
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
        Debug.Log("no more walls in pool to take from");
    }

    public void SpawnDoxy()
    {
        doxycycline.Play();
    }

    public void SpawnEnergy()
    {
        energy.Play();
    }

    Vector2 SpawnRange()
    {
        Vector2 r = Vector2.zero;

        Vector2 p = mainCam.ViewportToWorldPoint(new Vector2(1, .5f));
        RaycastHit2D hit = Physics2D.Raycast(p + (Vector2.right * 5), Vector2.up, 20, wallLayerMask);
        if (hit.collider != null)
        {
            r.x = hit.point.y;
        }

        p = mainCam.ViewportToWorldPoint(new Vector2(1, .5f));
        hit = Physics2D.Raycast(p + (Vector2.right * 5), Vector2.down, 20, wallLayerMask);
        if (hit.collider != null)
        {
            r.y = hit.point.y;
        }

        return r;
    }

    public void SpawnIron(float _spawnY = -100)
    {
        float spawnX = mainCam.ViewportToWorldPoint(new Vector2(1, 1)).x + 5;
        if (_spawnY == -100)
        {
            Vector2 ySpawnRange = SpawnRange();
            _spawnY = Random.Range(ySpawnRange.x, ySpawnRange.y);
        }
        for (int i = 0; i < ironPool.childCount; i++)
        {
            var curr = ironPool.GetChild(i);
            if (!curr.gameObject.activeInHierarchy)
            {
                curr.position = new Vector2(spawnX, _spawnY);
                curr.gameObject.SetActive(true);
                activeObjects.Add(curr.gameObject);
                return;
            }
        }
        GameObject newIron = Instantiate(ironPrefab, new Vector2(spawnX, _spawnY), Quaternion.identity);
        activeObjects.Add(newIron);
        newIron.transform.parent = ironPool;
    }

    public void SpawnCholera(float _spawnY = -100)
    {
        float spawnX = mainCam.ViewportToWorldPoint(new Vector2(1, 1)).x + 5;
        if (_spawnY == -100)
        {
            Vector2 ySpawnRange = SpawnRange();
            _spawnY = Random.Range(ySpawnRange.x, ySpawnRange.y);
        }
        for (int i = 0; i < choleraPool.childCount; i++)
        {
            var curr = choleraPool.GetChild(i);
            if (!curr.gameObject.activeInHierarchy)
            {
                curr.position = new Vector2(spawnX, _spawnY);
                curr.gameObject.SetActive(true);
                activeObjects.Add(curr.gameObject);
                return;
            }
        }
        GameObject newCholera = Instantiate(choleraPrefab, new Vector2(spawnX, _spawnY), Quaternion.identity);
        activeObjects.Add(newCholera);
        newCholera.transform.parent = choleraPool;
    }
}
