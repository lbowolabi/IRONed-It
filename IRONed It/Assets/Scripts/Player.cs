using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Resources")]
    public int lives = 3;
    TextMeshProUGUI lifeCount;
    Image fe3BarFill;
    public float fe3LossRateOverTime;
    public float fe3PickupWorth;
    Image atpBarFill;
    //public float atpLossRateOverTime;
    public float atpPickupWorth;
    public float atpCostToActivateGene;

    [Header("Movement")]
    public float maxYSpeed = 7;
    //public float maxXSpeed;
    [Range(0.01f, .5f)] public float timeToMaxSpeed = .22f; // arbitrary range, feel free to modify. (don't make it zero)

    [Header("Friction")]
    [Tooltip("bigger number is faster direction change")]
    public float directionChangeMultiplier = 3f;
    [Tooltip("bigger number is faster stop")]
    public float stopMultiplier = 4f;
    //private float xSpeedSmoothing;
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
    public bool playerCanMove = true;
    public bool expendingResources = true;
    bool iFrames;
    bool topBounced;
    bool bottomBounced;
    public bool viuaActive { get; private set; }

    [Header("Component References")]
    Rigidbody2D rb;
    BoxCollider2D bc;
    SpriteRenderer sr;
    Color defaultColor;

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

    private void Start()
    {
        lifeCount = CanvasManager.instance.lifeCount;
        fe3BarFill = CanvasManager.instance.fe3BarFill;
        atpBarFill = CanvasManager.instance.atpBarFill;
        lifeCount.text = lives.ToString();
    }

    void Update()
    {
        if (expendingResources)
        {
            ChangeIronCount(-Time.deltaTime * fe3LossRateOverTime);
            //ChangeEnergyCount(-Time.deltaTime * atpLossRateOverTime);
        }
    }

    public void ActivateGene()
    {
        if (fe3BarFill.fillAmount > 0 && !viuaActive)
        {
            viuaActive = true;
            ChangeEnergyCount(-atpCostToActivateGene);
            StartCoroutine(Helpers.instance.Timer(deactivateGene => viuaActive = false, 10));
        }
    }

    void ChangeLifeCount(int amount)
    {
        if (amount > 0)
        {

        }
        else if (amount < 0)
        {
            StartCoroutine(Death());
        }

        lives += amount;
        lifeCount.text = lives.ToString();
    }

    void ChangeIronCount(float amount)
    {
        if (amount > 0)
        {

        }
        else if (amount < 0)
        {

        }

        fe3BarFill.fillAmount += amount;
    }

    public void ChangeEnergyCount(float amount)
    {
        if (amount > 0)
        {
            if (atpBarFill.fillAmount + amount >= 1)
            {
                if (lives < 3 && atpBarFill.fillAmount != 1)
                {
                    ChangeLifeCount(1);
                }
                LevelManager.instance.SetEnergySpawnProbability(false); // turn off energy spawn if player at max energy
            }
        }
        else if (amount < 0)
        {
            if (atpBarFill.fillAmount == 1)
            {
                LevelManager.instance.SetEnergySpawnProbability(true); // turn energy spawn back on
            }
        }

        atpBarFill.fillAmount += amount;
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
                //Debug.Log("bottom collision");
                bottomBounced = true;
                StartCoroutine(Helpers.instance.Timer(endBounce => bottomBounced = false, bounceDuration));
                rb.velocity = new Vector2(rb.velocity.x, bounceForce);
            }
            hit = Physics2D.Raycast(new Vector2(transform.position.x + (i * (bc.bounds.size.x / 2)), transform.position.y + (bc.bounds.size.y / 2)), Vector2.up, .02f, wallLayerMask);
            if (hit.collider != null)
            {
                //Debug.Log("top collision");
                topBounced = true;
                StartCoroutine(Helpers.instance.Timer(endBounce => topBounced = false, bounceDuration));
                rb.velocity = new Vector2(rb.velocity.x, -bounceForce);
            }
            //hit = Physics2D.Raycast(new Vector2(transform.position.x + (bc.bounds.size.x / 2), transform.position.y - (i * (bc.bounds.size.y / 2))), Vector2.right, .02f, wallLayerMask);
            //if (hit.collider != null)
            //{
            //    Debug.Log("forward collision");
            //}
        }
    }

    void Movement()
    {
        Vector2 input = new Vector2(0, playerCanMove ? Input.GetAxisRaw("Vertical") : 0); // get player movement input

        //float xSpeed = maxXSpeed / timeToMaxSpeed * input.x * Time.fixedDeltaTime;
        float ySpeed = maxYSpeed / timeToMaxSpeed * input.y * Time.fixedDeltaTime; // amount to change current vertical speed by

        //if (input.x == 0)
        //{
        //    xSpeed = -Mathf.SmoothDamp(rb.velocity.x, 0, ref xSpeedSmoothing, .05f) * Time.fixedDeltaTime; // change horizontal speed by negative amount
        //}
        if (input.y == 0 || input.y == 1 && topBounced || input.y == -1 && bottomBounced) // if no input, or if trying to move back towards wall after bounce
        {
            ySpeed = -Mathf.SmoothDamp(rb.velocity.y, 0, ref ySpeedSmoothing, .05f) * Time.fixedDeltaTime * stopMultiplier; // change vertical speed by negative amount
        }

        // change current speed
        rb.velocity += Vector2.up * (input.y == rb.velocity.y / Mathf.Abs(rb.velocity.y) ? ySpeed : ySpeed * directionChangeMultiplier);

        if (!topBounced && !bottomBounced)
        {
            // limit to max speed
            rb.velocity = Vector2.up * (Mathf.Abs(rb.velocity.y) > maxYSpeed ? maxYSpeed * input.y : rb.velocity.y);
        }
    }

    IEnumerator Death()
    {
        playerCanMove = false;
        expendingResources = false;
        iFrames = true;
        if (lives != 0)
        {
            StartCoroutine(Helpers.instance.Timer(revive => playerCanMove = true, deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => expendingResources = true, deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => iFrames = false, deathDuration));
            while (!playerCanMove)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSecondsRealtime(deathFlashInterval);
            }
            sr.color = defaultColor;
        }
        else
        {
            while (deathFlashInterval < 3)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSecondsRealtime(deathFlashInterval);
                deathFlashInterval *= 1.3f;
            }
            sr.color = Color.grey;
        }
    }

    private void OnParticleCollision(GameObject p)
    {
        bool destroyParticle = true;

        if (p.CompareTag("Energy"))
        {
            ChangeEnergyCount(atpPickupWorth);
        }
        if (p.CompareTag("Doxycycline"))
        {
            if (!iFrames)
            {
                ChangeLifeCount(-1);
            }
            else
            {
                destroyParticle = false;
            }
        }

        if (destroyParticle)
        {
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
                    Debug.Log("hrm");
                    c.transform.parent.gameObject.SetActive(false);
                    ChangeIronCount(fe3PickupWorth);
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
                    ChangeIronCount(fe3PickupWorth);
                }
            }
        }
    }
}
