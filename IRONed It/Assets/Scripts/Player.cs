using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Resources")]
    public int lives = 3;
    public Image fe3BarFill;
    public float fe3LossRate;
    public Image atpBarFill;
    public float atpLossRate;
    int iron;

    [Header("Movement")]
    public float maxSpeed = 7;
    [Range(0.01f, .5f)] public float timeToMaxSpeed = .22f; // arbitrary range, feel free to modify. (don't make it zero)
    public float xRecoveryTime;

    [Header("Friction")]
    [Tooltip("bigger number is faster direction change")]
    public float directionChangeMultiplier = 3f;
    [Tooltip("bigger number is faster stop")]
    public float stopMultiplier = 4f;
    private float xSpeedSmoothing;
    private float ySpeedSmoothing;

    [Header("Wall Bounce")]
    public float bounceForce;
    [Tooltip("how long until player can move back in the direction of the bounced wall")]
    public float bounceDuration;
    LayerMask wallLayerMask;

    [Header("Miscellaneous")]
    public float deathDuration;
    public float deathFlashInterval;

    // player states
    public bool playerCanMove { get; private set; } = true;
    bool expendingResources = true;
    bool topBounced;
    bool bottomBounced;
    bool viuaActive;

    [Header("Component References")]
    Rigidbody2D rb;
    BoxCollider2D bc;
    SpriteRenderer sr;
    Color defaultColor;
    Transform mainCam;

    public static Player instance;

    void Awake()
    {
        instance = this;
        wallLayerMask = LayerMask.GetMask("Wall");
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
    }

    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void Update()
    {
        if (expendingResources)
        {
            fe3BarFill.fillAmount -= Time.deltaTime * fe3LossRate;
            atpBarFill.fillAmount -= Time.deltaTime * atpLossRate;
        }
    }

    public void ActivateGene()
    {
        if (iron > 0 && !viuaActive)
        {
            viuaActive = true;
            iron--;
            StartCoroutine(Helpers.instance.Timer(deactivateGene => viuaActive = false, 10));
        }
    }

    void FixedUpdate()
    {
        Movement();
        CheckForWallCollisions();
    }

    void CheckForWallCollisions()
    {
        for (int i = -1; i < 2; i += 2)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + (i * (bc.bounds.size.x / 2)), transform.position.y - (bc.bounds.size.y / 2)), Vector2.down, .02f, wallLayerMask);
            if (hit.collider != null)
            {
                Debug.Log("bottom collision");
                bottomBounced = true;
                StartCoroutine(Helpers.instance.Timer(endBounce => bottomBounced = false, bounceDuration));
                rb.velocity = new Vector2(rb.velocity.x, bounceForce);
            }
            hit = Physics2D.Raycast(new Vector2(transform.position.x + (i * (bc.bounds.size.x / 2)), transform.position.y + (bc.bounds.size.y / 2)), Vector2.up, .02f, wallLayerMask);
            if (hit.collider != null)
            {
                Debug.Log("top collision");
                topBounced = true;
                StartCoroutine(Helpers.instance.Timer(endBounce => topBounced = false, bounceDuration));
                rb.velocity = new Vector2(rb.velocity.x, -bounceForce);
            }
            hit = Physics2D.Raycast(new Vector2(transform.position.x + (bc.bounds.size.x / 2), transform.position.y - (i * (bc.bounds.size.y / 2))), Vector2.right, .02f, wallLayerMask);
            if (hit.collider != null)
            {
                Debug.Log("forward collision");
            }
        }
    }

    void Movement()
    {
        Vector2 input = new Vector2(playerCanMove && transform.position.x < 0 ? 1 : 0, playerCanMove ? Input.GetAxisRaw("Vertical") : 0); // get player movement input

        float xSpeed = maxSpeed / timeToMaxSpeed * input.x * Time.fixedDeltaTime;
        float ySpeed = maxSpeed / timeToMaxSpeed * input.y * Time.fixedDeltaTime; // amount to change current vertical speed by

        if (input.x == 1)
        {
            if (rb.velocity.x < 0)
            {
                xSpeed = (-Mathf.SmoothDamp(transform.position.x, 0, ref xSpeedSmoothing, .05f) * Time.fixedDeltaTime) * stopMultiplier; // change horizontal speed by negative amount
            }
            else
            {

            }
        }
        if (input.y == 0 || input.y == 1 && topBounced || input.y == -1 && bottomBounced)
        {
            ySpeed = (-Mathf.SmoothDamp(rb.velocity.y, 0, ref ySpeedSmoothing, .05f) * Time.fixedDeltaTime) * stopMultiplier; // change horizontal speed by negative amount
        }

        // change current speed
        rb.velocity += new Vector2(xSpeed, input.y == rb.velocity.y / Mathf.Abs(rb.velocity.y) ? ySpeed : ySpeed * directionChangeMultiplier);

        if (!topBounced && !bottomBounced)
        {
            // limit to max speed
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Abs(rb.velocity.y) > maxSpeed ? maxSpeed * input.y : rb.velocity.y);
        }
    }

    private void OnParticleCollision(GameObject p)
    {
        if (p.CompareTag("Energy"))
        {
            atpBarFill.fillAmount += .05f;
            if (atpBarFill.fillAmount == 1)
            {
                if (lives < 3)
                {
                    ChangeLifeCount(1);
                }
            }
        }
        if (p.CompareTag("Doxycycline"))
        {
            ChangeLifeCount(-1);
        }

        List<ParticleCollisionEvent> events = new List<ParticleCollisionEvent>();

        ParticleSystem m_System = p.GetComponent<ParticleSystem>();

        ParticleSystem.Particle[] m_Particles;
        m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];

        ParticlePhysicsExtensions.GetCollisionEvents(p.GetComponent<ParticleSystem>(), gameObject, events);
        foreach (ParticleCollisionEvent coll in events)
        {
            int numParticlesAlive = m_System.GetParticles(m_Particles);

            // Check only the particles that are alive
            for (int i = 0; i < numParticlesAlive; i++)
            {
                //If the collision was close enough to the particle position, destroy it
                if (Vector3.Magnitude(m_Particles[i].position - coll.intersection) < 0.05f)
                {
                    m_Particles[i].remainingLifetime = -1; //Kills the particle
                    m_System.SetParticles(m_Particles); // Update particle system
                    return;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Iron"))
        {
            if (!c.transform.parent.GetComponent<Iron>().chelated)
            {
                int r = Random.Range(0, 10);
                if (r == 0)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    iron++;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D c)
    {
        if (c.CompareTag("Iron"))
        {
            if (c.transform.parent.GetComponent<Iron>().chelated)
            {
                if (viuaActive)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    iron++;
                }
            }
        }
    }

    IEnumerator Death()
    {
        playerCanMove = false;
        expendingResources = false;
        if (lives != 0)
        {
            StartCoroutine(Helpers.instance.Timer(revive => playerCanMove = true, deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => expendingResources = true, deathDuration));
            while (!playerCanMove)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSecondsRealtime(deathFlashInterval);
            }
            sr.color = defaultColor;
        }
        else
        {

        }
    }

    void ChangeLifeCount(int amount)
    {
        lives += amount;
        if (amount > 0)
        {

        }
        else
        {
            StartCoroutine(Death());
        }
    }
}
