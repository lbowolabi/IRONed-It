using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPickup : MonoBehaviour
{
    public ChelatedBy targetType;
    Enemy motileEnemy;
    Transform particleChild;

    void Awake()
    {
        motileEnemy = GetComponent<Enemy>();
        particleChild = transform.GetChild(0);
    }

    void Start()
    {
        particleChild.parent = LevelManager.instance.transform;
        particleChild.localScale = Vector3.one;
    }

    private void Update()
    {
        particleChild.position = transform.position;
    }

    private void OnEnable()
    {
        if (particleChild == null) transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        else particleChild.GetComponent<ParticleSystem>().Play();
    }

    private void OnDisable()
    {
        particleChild.GetComponent<ParticleSystem>().Stop();
    }

    private void OnTriggerStay2D(Collider2D c)
    {
        if (c.CompareTag("Iron"))
        {
            var currentIron = c.transform.parent.GetComponent<Iron>();
            if (currentIron.chelatedBy == targetType)
            {
                c.transform.parent.gameObject.SetActive(false);
                LevelManager.instance.RemoveFromActiveObjects(c.transform.parent.gameObject);
                if (motileEnemy != null) motileEnemy.target = null;
            }
        }
    }
}
