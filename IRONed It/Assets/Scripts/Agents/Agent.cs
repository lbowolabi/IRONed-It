using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Component References")]
    Player playerScript;
    Enemy enemyScript;
    Motile motileScript;

    void Awake()
    {
        playerScript = GetComponent<Player>();
        enemyScript = GetComponent<Enemy>();
        motileScript = GetComponent<Motile>();
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
            if (motileScript != null && !motileScript.iFrames)
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
