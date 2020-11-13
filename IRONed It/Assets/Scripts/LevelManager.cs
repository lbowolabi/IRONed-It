using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public float scrollSpeed;

    [Header("RNG")] // one out of probability
    public int doxyProbability;
    public int energyProbability;
    public int ironProbability;

    [Header("Component References")]
    public Transform wallPool;
    public Transform ironPool;
    ParticleSystem doxycycline;
    ParticleSystem energy;

    Camera mainCam;
    Plane[] cameraBounds;

    List<GameObject> activeObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        cameraBounds = GeometryUtility.CalculateFrustumPlanes(mainCam);

        for (int i = 0; i < wallPool.childCount; i++)
        {
            if (wallPool.GetChild(i).gameObject.activeInHierarchy)
            {
                activeObjects.Add(wallPool.GetChild(i).gameObject);
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
        if (scrollSpeed > 0)
        {
            for (int i = 0; i < activeObjects.Count; i++) // loop through all in-scene objects
            {
                Transform curr = activeObjects[i].transform;
                curr.Translate(Vector2.left * scrollSpeed * Time.deltaTime); // translate them to the left
                if (curr.position.x < 0) // if they haven't just spawned on the right
                {
                    if (!GeometryUtility.TestPlanesAABB(cameraBounds, curr.GetComponent<Collider2D>().bounds)) // and if they're out of camera range
                    {
                        curr.gameObject.SetActive(false); // deactivate
                        if (curr.parent == wallPool) // if object is a wall
                        {
                            if (curr.position.y > 0) SpawnWall(true); // spawn a new upper wall
                            else SpawnWall(false); // or spawn a new lower wall
                        }
                        activeObjects.RemoveAt(i); // remove object from list active objects
                        i--; // decrease loop count
                    }
                }
            }

            int r = Random.Range(0, doxyProbability);
            if (r == 0) SpawnDoxy();

            r = Random.Range(0, energyProbability);
            if (r == 0) SpawnEnergy();

            r = Random.Range(0, ironProbability);
            if (r == 0) SpawnIron();
        }
    }

    public void ChangeScrollSpeed(float newSpeed)
    {

    }

    void SpawnWall(bool top)
    {

    }

    void SpawnDoxy()
    {

    }

    void SpawnEnergy()
    {

    }

    void SpawnIron()
    {

    }
}
