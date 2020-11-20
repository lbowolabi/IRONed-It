using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iron : MonoBehaviour
{
    // parent is on Iron layer, not trigger, tagged anything, has Iron script
    // child is on Default layer, isTrigger, tagged Iron
    // child collides with player, parent collides with particles

    public bool chelated { get; private set; }
    float speed;

    CircleCollider2D cc;
    SpriteRenderer childSR;

    private void Awake()
    {
        cc = GetComponent<CircleCollider2D>();
        childSR = transform.GetChild(0).GetComponent<SpriteRenderer>();
        childSR.enabled = false;
        chelated = false;
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector2.left * speed * Time.fixedDeltaTime);
    }

    private void OnParticleCollision(GameObject p)
    { // no need to check exactly which particle system collided, only chelators interact with iron
        chelated = true;
        cc.isTrigger = true; // this object can't collide with particles again
        childSR.transform.localScale *= 1.5f;
        childSR.enabled = true;
        
        // find specific particle that collided, destroy it
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

    void OnDisable() // reset values for reuse in next spawn
    {
        chelated = false;
        cc.isTrigger = false;
        childSR.enabled = false;
        childSR.transform.localScale = Vector3.one;
    }

    private void OnEnable()
    {
        speed = Random.Range(LevelManager.instance.ironSpeedRange.x, LevelManager.instance.ironSpeedRange.y);
    }
}
