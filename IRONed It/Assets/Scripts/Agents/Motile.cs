using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motile : MonoBehaviour
{
    [Header("Movement")]
    Vector2 movementVector;
    [SerializeField] private float maxYSpeed = 7;
    //public float maxXSpeed;
    [SerializeField] [Range(0.01f, .5f)] private float timeToMaxSpeed = .22f; // arbitrary range, feel free to modify. (don't make it zero)

    [Header("Friction")]
    [Tooltip("bigger number is faster direction change")]
    [SerializeField] private float directionChangeMultiplier = 3f;
    [Tooltip("bigger number is faster stop")]
    [SerializeField] private float stopMultiplier = 4f;
    //private float xSpeedSmoothing;
    private float ySpeedSmoothing;

    [Header("Wall Bounce")]
    [SerializeField] private float bounceForce;
    [Tooltip("how long until agent can bounce back in the direction of the bounced wall")]
    [SerializeField] private float bounceDuration;
    LayerMask wallLayerMask;

    [Header("Death")]
    public float lives = 3;
    public float deathDuration;
    public float deathFlashInterval;

    // agent states
    [HideInInspector] public bool agentCanMove = true;
    [HideInInspector] public bool iFrames = false;
    bool topBounced;
    bool bottomBounced;

    [Header("Component References")]
    Rigidbody2D rb;
    CapsuleCollider2D cc;
    Player playerScript;
    Enemy enemyScript;

    public static Motile playerInstance;

    void Awake()
    {
        wallLayerMask = LayerMask.GetMask("Wall");
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CapsuleCollider2D>();
        playerScript = GetComponent<Player>();
        enemyScript = GetComponent<Enemy>();

        if (playerScript != null) playerInstance = this;
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
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + (i * (cc.bounds.size.x / 2)), transform.position.y + cc.offset.y - (cc.bounds.size.y / 2)), Vector2.down, .02f, wallLayerMask);
            if (hit.collider != null)
            {
                //Debug.Log("bottom collision");
                bottomBounced = true;
                StartCoroutine(Helpers.instance.Timer(endBounce => bottomBounced = false, bounceDuration));
                rb.velocity = new Vector2(rb.velocity.x, bounceForce);
            }
            hit = Physics2D.Raycast(new Vector2(transform.position.x + (i * (cc.bounds.size.x / 2)), transform.position.y + cc.offset.y + (cc.bounds.size.y / 2)), Vector2.up, .02f, wallLayerMask);
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
            //    (problem rn is that forward collision also logs top and bottom collisions because the raycasts are starting from the same corners)
            //}
        }
    }

    public void SetMovementVector(Vector2 _movement)
    {
        if (_movement.x != 0) _movement.x /= Mathf.Abs(_movement.x);
        if (_movement.y != 0) _movement.y /= Mathf.Abs(_movement.y);
        movementVector = _movement;
    }

    void Movement()
    {
        Vector2 input = new Vector2(0, agentCanMove ? movementVector.y : 0); // get player movement input

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

    private void OnParticleCollision(GameObject p)
    {
        bool destroyParticle = true;

        if (p.CompareTag("Energy"))
        {
            if (playerScript != null) playerScript.EnergyPickup();
        }
        if (p.CompareTag("Doxycycline"))
        {
            if (!iFrames)
            {
                if (playerScript != null)
                {
                    playerScript.ChangeLifeCount(-1);
                    StartCoroutine(Helpers.instance.Shake(CameraFollow.instance.transform, .08f, .06f));
                }
                else if (enemyScript != null) destroyParticle = false; //enemyScript.ChangeLifeCount(-1);
            }
            else
            {
                //destroyParticle = false;
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
                    if (Vector3.Magnitude(m_Particles[i].position - coll.intersection) <= m_Particles[i].GetCurrentSize(m_System) / 2)
                    {
                        m_Particles[i].remainingLifetime = -1f; //Kills the particle
                        m_System.SetParticles(m_Particles); // Update particle system
                        //break;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Iron"))
        {
            if (c.transform.parent.GetComponent<Iron>().chelatedBy == ChelatedBy.None)
            {
                int r = Random.Range(0, 10);
                if (r == 0)
                {
                    LevelManager.instance.RemoveFromActiveObjects(c.transform.parent.gameObject);
                    c.transform.parent.gameObject.SetActive(false);
                    if (playerScript != null) playerScript.IronPickup();
                }
            }
        }
    }
}
