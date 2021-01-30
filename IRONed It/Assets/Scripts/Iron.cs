using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iron : MonoBehaviour
{
    // parent is on Iron layer, not trigger, tagged anything, has Iron script
    // child is on Default layer, isTrigger, tagged Iron
    // child collides with player, parent collides with particles

    public ChelatedBy chelatedBy { get; private set; } = ChelatedBy.None;

    Vector3 originalScale;

    CircleCollider2D cc;
    SpriteRenderer childSR;

    private void Awake()
    {
        cc = GetComponent<CircleCollider2D>();
        childSR = transform.GetChild(0).GetComponent<SpriteRenderer>();
        childSR.enabled = false;
        childSR.transform.GetChild(0).gameObject.SetActive(false);
        originalScale = childSR.transform.localScale;
    }

    public void HemeIron()
    {
        chelatedBy = ChelatedBy.Heme;
        cc.isTrigger = true;
        childSR.transform.localScale = originalScale * 1.5f;
        childSR.enabled = true;
        childSR.color = GameManager.instance.hutaColor;
        childSR.transform.GetChild(0).gameObject.SetActive(true);
    }

    private void OnParticleCollision(GameObject p)
    {
        if (chelatedBy == ChelatedBy.None)
        {
            if (p.name == "Vibriobactin")
            {
                chelatedBy = ChelatedBy.Cholera;
                childSR.color = GameManager.instance.viuaColor;
            }
            else if (p.name == "Enterobactin") // linear enterobactin
            {
                chelatedBy = ChelatedBy.Coli;
                childSR.color = GameManager.instance.irgaColor;
            }
            else if (p.name == "Ferrichrome")
            {
                chelatedBy = ChelatedBy.Sphaerogena;
                childSR.color = GameManager.instance.fhuaColor;
            }
            cc.isTrigger = true; // this object can't collide with particles again
            childSR.transform.localScale = originalScale * 1.5f;
            childSR.enabled = true;
            childSR.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    void OnDisable() // reset values for reuse in next spawn
    {
        chelatedBy = ChelatedBy.None;
        cc.isTrigger = false;
        childSR.enabled = false;
        childSR.transform.localScale = originalScale;
        childSR.transform.GetChild(0).gameObject.SetActive(false);
        if (LevelManager.instance.targetedIron.Contains(transform)) LevelManager.instance.targetedIron.Remove(transform);
    }
}
