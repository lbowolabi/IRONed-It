using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChelatedBy { None, Cholera, Coli, Heme, Sphaerogena };

public class LevelManager : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] float levelLengthInSeconds;

    [Header("RNG")] // one out of probability, called multiple times per second
    [SerializeField] int defaultDoxyProbability = -1;
    [SerializeField] int defaultEnergyProbability = -1;
    [SerializeField] int defaultIronProbability = -1;
    [SerializeField] int defaultHemeProbability = -1;
    [SerializeField] int defaultCholeraProbability = -1;
    [SerializeField] int defaultColiProbability = -1;
    [SerializeField] int defaultSphaerogenaProbability = -1;
    int currentDoxyProbability = -1, currentEnergyProbability = -1, currentIronProbability = -1, currentHemeProbability = -1;
    int currentCholeraProbability = -1, currentColiProbability = -1, currentSphaerogenaProbability = -1;

    [Header("Resources")]
    public Vector2 ironSpeedRange;
    [HideInInspector] public List<Transform> targetedIron = new List<Transform>();
    GameObject ironPrefab;

    [Header("Enemies")]
    public Vector2 choleraSpeedRange;
    GameObject choleraPrefab;
    public Vector2 coliSpeedRange;
    GameObject coliPrefab;
    public Vector2 sphaerogenaSpeedRange;
    GameObject sphaerogenaPrefab;

    [Header("Walls")]
    [SerializeField] float wallSpeed;
    float currentWallSpeed = 1;
    float wallSpeedSmoothing;
    float topWallY;
    float bottomWallY;
    LayerMask wallLayerMask;

    [Header("Player Interactions")]
    public float playerDeathSpeedMultiplier = .3f;

    [Header("Component References")]
    [SerializeField] Transform wallPool;
    [SerializeField] Transform ironPool;
    [Tooltip("unfortunate names <.<")]
    [SerializeField] Transform choleraPool;
    [SerializeField] Transform coliPool;
    [SerializeField] Transform sphaerogenaPool;
    public ParticleSystem doxycycline { get; private set; }
    public ParticleSystem energy { get; private set; }
    GameObject victoryGate;

    //Transform player;

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
        coliPrefab = (GameObject)Resources.Load("Prefabs/Coli");
        sphaerogenaPrefab = (GameObject)Resources.Load("Prefabs/Sphaerogena");

        currentIronProbability = defaultIronProbability;
        currentEnergyProbability = defaultEnergyProbability;
        StartCoroutine(Helpers.instance.Timer(startDoxySpawns => SetAllResourceSpawnsToDefault(), 7));
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //cameraBounds = GeometryUtility.CalculateFrustumPlanes(mainCam);
        //player = Player.instance.transform;

        victoryGate = GameObject.FindGameObjectWithTag("Victory");
        if (victoryGate != null) victoryGate.SetActive(false);

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
            if (Motile.playerInstance.agentCanMove)
            {
                levelLengthInSeconds -= Time.deltaTime;
                if (currentWallSpeed < wallSpeed - .05f || currentWallSpeed > wallSpeed + .05f)
                {
                    currentWallSpeed = Mathf.SmoothDamp(currentWallSpeed, wallSpeed, ref wallSpeedSmoothing, Motile.playerInstance.deathDuration * Time.deltaTime);
                }

                if (levelLengthInSeconds <= 0)
                {
                    Vector2 gateSpawnPosition = mainCam.ViewportToWorldPoint(new Vector2(1, .5f));
                    gateSpawnPosition.x += 5;
                    victoryGate.transform.position = gateSpawnPosition;
                    victoryGate.SetActive(true);
                    activeObjects.Add(victoryGate);
                }
            }
            else
            {
                currentWallSpeed = Mathf.SmoothDamp(currentWallSpeed, wallSpeed * playerDeathSpeedMultiplier, ref wallSpeedSmoothing, Motile.playerInstance.deathDuration * Time.deltaTime);

                // change particles' speeds too; work in progress
                var particles = new ParticleSystem.Particle[doxycycline.main.maxParticles];
                int aliveParticlesCount = doxycycline.GetParticles(particles);
                // Change only the particles that are alive
                for (int i = 0; i < aliveParticlesCount; i++)
                {
                    //particles[i].velocity = particles[i].velocity.normalized * (particles[i].velocity.magnitude * playerDeathSpeedMultiplier);
                }
                doxycycline.SetParticles(particles, aliveParticlesCount);

                particles = new ParticleSystem.Particle[energy.main.maxParticles];
                aliveParticlesCount = energy.GetParticles(particles);
                for (int i = 0; i < aliveParticlesCount; i++)
                {
                    //particles[i].velocity = particles[i].velocity.normalized * (particles[i].velocity.magnitude * playerDeathSpeedMultiplier);
                }
                energy.SetParticles(particles, aliveParticlesCount);
            }
        }

        for (int i = 0; i < activeObjects.Count; i++) // loop through all non-particle, non-player in-scene objects
        {
            Transform curr = activeObjects[i].transform;
            if (curr.position.x < -12) // if object has moved out of camera range
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

    public void RemoveFromActiveObjects(GameObject g)
    {
        activeObjects.Remove(g);
    }

    public void PlayerCanMove(bool newValue)
    {
        for (int i = 0; i < activeObjects.Count; i++)
        {
            if (activeObjects[i].transform.parent == wallPool)
            {
                continue;
            }
            activeObjects[i].GetComponent<TranslateSpeed>().MatchSpeedToPlayer(newValue);
        }
    }

    public void EndLevel()
    {
        StartCoroutine(EndLevelSequence());
    }

    IEnumerator EndLevelSequence()
    {
        Vector2 cameraEdge = mainCam.ViewportToWorldPoint(new Vector2(1, 1));
        float playerTranslateSpeed = 0;
        float smoothing = 0;
        while (Player.instance.transform.position.x < cameraEdge.x + 5)
        {
            Player.instance.transform.Translate(playerTranslateSpeed * Vector2.right);
            playerTranslateSpeed = Mathf.SmoothDamp(playerTranslateSpeed, wallSpeed / 8, ref smoothing, 3);
            yield return null;
        }
        // call victory, etc
    }

    void FixedUpdate() // putting this in fixed update to prevent item spawn rate from being affected by frame rate
    {
        if (wallSpeed > 0)
        {
            if (currentDoxyProbability != -1)
            {
                int r = (int)Random.Range(0, currentDoxyProbability / currentWallSpeed);
                if (r == 0) SpawnDoxy();
            }

            if (currentEnergyProbability != -1 && CanvasManager.instance.GetAtpBarFill().fillAmount < 1)
            {
                int r = (int)Random.Range(0, currentEnergyProbability / currentWallSpeed);
                if (r == 0) SpawnEnergy();
            }

            if (currentIronProbability != -1)
            {
                int r = (int)Random.Range(0, currentIronProbability / currentWallSpeed);
                if (r == 0) SpawnIron(false);
            }

            if (currentHemeProbability != -1)
            {
                int r = (int)Random.Range(0, currentHemeProbability / currentWallSpeed);
                if (r == 0) SpawnIron(true);
            }

            if (currentCholeraProbability != -1)
            {
                int r = (int)Random.Range(0, currentCholeraProbability / currentWallSpeed);
                if (r == 0) SpawnCholera();
            }

            if (currentColiProbability != -1)
            {
                int r = (int)Random.Range(0, currentColiProbability / currentWallSpeed);
                if (r == 0) SpawnColi();
            }

            if (currentSphaerogenaProbability != -1)
            {
                int r = (int)Random.Range(0, currentSphaerogenaProbability / currentWallSpeed);
                if (r == 0) SpawnSphaerogena();
            }
        }
    }

    public void SetAllResourceSpawnsToDefault()
    {
        SetIronSpawnProbability(true);
        SetEnergySpawnProbability(true);
        SetDoxySpawnProbability(true);
        SetHemeSpawnProbability(true);

        SetColiSpawnProbability(true);
        SetCholeraSpawnProbability(true);
        SetSphaerogenaSpawnProbability(true);
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

    public void SetHemeSpawnProbability(bool resetToDefault, int newHeme = -1)
    {
        if (resetToDefault)
        {
            currentHemeProbability = defaultHemeProbability;
        }
        else
        {
            currentHemeProbability = newHeme;
        }
    }

    public void SetCholeraSpawnProbability(bool resetToDefault, int newCholera = -1)
    {
        if (resetToDefault)
        {
            currentCholeraProbability = defaultCholeraProbability;
        }
        else
        {
            currentCholeraProbability = newCholera;
        }
    }

    public void SetColiSpawnProbability(bool resetToDefault, int newColi = -1)
    {
        if (resetToDefault)
        {
            currentColiProbability = defaultColiProbability;
        }
        else
        {
            currentColiProbability = newColi;
        }
    }

    public void SetSphaerogenaSpawnProbability(bool resetToDefault, int newSphaerogena = -1)
    {
        if (resetToDefault)
        {
            currentSphaerogenaProbability = defaultSphaerogenaProbability;
        }
        else
        {
            currentSphaerogenaProbability = newSphaerogena;
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
        RaycastHit2D hit = Physics2D.Raycast(p + (Vector2.right * 3), Vector2.up, 20, wallLayerMask);
        if (hit.collider != null)
        {
            r.x = hit.point.y;
        }

        p = mainCam.ViewportToWorldPoint(new Vector2(1, .5f));
        hit = Physics2D.Raycast(p + (Vector2.right * 3), Vector2.down, 20, wallLayerMask);
        if (hit.collider != null)
        {
            r.y = hit.point.y;
        }

        return r;
    }

    public void SpawnIron(bool heme, float _spawnY = -100)
    {
        float spawnX = mainCam.ViewportToWorldPoint(new Vector2(1, 1)).x + 1;
        if (_spawnY == -100)
        {
            Vector2 ySpawnRange = SpawnRange();
            _spawnY = Random.Range(ySpawnRange.x - ironPrefab.GetComponent<CircleCollider2D>().bounds.size.y / 2, ySpawnRange.y + ironPrefab.GetComponent<CircleCollider2D>().bounds.size.y / 2);
        }
        for (int i = 0; i < ironPool.childCount; i++)
        {
            var curr = ironPool.GetChild(i);
            if (!curr.gameObject.activeInHierarchy)
            {
                curr.position = new Vector2(spawnX, _spawnY);
                curr.gameObject.SetActive(true);
                activeObjects.Add(curr.gameObject);
                if (heme)
                {
                    StartCoroutine(Helpers.instance.WaitOneFrame(changeIronType => curr.GetComponent<Iron>().HemeIron()));
                }
                return;
            }
        }
        GameObject newIron = Instantiate(ironPrefab, new Vector2(spawnX, _spawnY), Quaternion.identity);
        activeObjects.Add(newIron);
        newIron.transform.parent = ironPool;
        if (heme)
        {
            StartCoroutine(Helpers.instance.WaitOneFrame(changeIronType => newIron.GetComponent<Iron>().HemeIron()));
        }
    }

    public void SpawnCholera(float _spawnY = -100)
    {
        float spawnX = mainCam.ViewportToWorldPoint(new Vector2(1, 1)).x + 3;
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

    public void SpawnColi(float _spawnY = -100)
    {
        float spawnX = mainCam.ViewportToWorldPoint(new Vector2(1, 1)).x + 3;
        if (_spawnY == -100)
        {
            Vector2 ySpawnRange = SpawnRange();
            _spawnY = Random.Range(ySpawnRange.x, ySpawnRange.y);
        }
        for (int i = 0; i < coliPool.childCount; i++)
        {
            var curr = coliPool.GetChild(i);
            if (!curr.gameObject.activeInHierarchy)
            {
                curr.position = new Vector2(spawnX, _spawnY);
                curr.gameObject.SetActive(true);
                activeObjects.Add(curr.gameObject);
                return;
            }
        }
        GameObject newColi = Instantiate(coliPrefab, new Vector2(spawnX, _spawnY), Quaternion.identity);
        activeObjects.Add(newColi);
        newColi.transform.parent = coliPool;
    }

    public void SpawnSphaerogena(float _spawnY = -100)
    {
        float spawnX = mainCam.ViewportToWorldPoint(new Vector2(1, 1)).x + 3;
        if (_spawnY == -100)
        {
            Vector2 ySpawnRange = SpawnRange();
            _spawnY = Random.Range(ySpawnRange.x - sphaerogenaPrefab.GetComponent<SpriteRenderer>().bounds.size.y / 2, ySpawnRange.y + sphaerogenaPrefab.GetComponent<SpriteRenderer>().bounds.size.y / 2);
        }
        for (int i = 0; i < sphaerogenaPool.childCount; i++)
        {
            var curr = sphaerogenaPool.GetChild(i);
            if (!curr.gameObject.activeInHierarchy)
            {
                curr.position = new Vector2(spawnX, _spawnY);
                curr.gameObject.SetActive(true);
                activeObjects.Add(curr.gameObject);
                return;
            }
        }
        GameObject newSphaerogena = Instantiate(sphaerogenaPrefab, new Vector2(spawnX, _spawnY), Quaternion.identity);
        activeObjects.Add(newSphaerogena);
        newSphaerogena.transform.parent = sphaerogenaPool;
    }
}
