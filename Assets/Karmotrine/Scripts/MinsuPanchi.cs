using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MinsuPanchi : NormalMonster
{
    [SerializeField] private Note[] notes;
    private bool bIsAttacking;

    private bool bRecognizeWakgood;

    protected override void OnEnable()
    {
        bRecognizeWakgood = false;
        bIsAttacking = false;
        base.OnEnable();
        StartCoroutine(CheckDistance());
    }

    private IEnumerator CheckDistance()
    {
        while (true)
        {
            if (!bRecognizeWakgood)
            {
                if (Vector2.Distance(transform.position, Wakgood.Instance.transform.position) < 15)
                {
                    bRecognizeWakgood = true;
                    StartCoroutine(Attack());
                }
            }
            else if (!bIsAttacking)
            {
                SpriteRenderer.flipX = transform.position.x < Wakgood.Instance.transform.position.x;
            }

            yield return ws01;
        }
    }


    private IEnumerator Attack()
    {
        WaitForSeconds ws5 = new(5f);

        while (true)
        {
            bIsAttacking = true;
            Animator.SetTrigger("READY");
            yield return StartCoroutine(Casting(castingTime));
            Animator.SetTrigger("GO");
            StartCoroutine(Cry());
            bIsAttacking = false;

            yield return ws5;
        }
    }

    private IEnumerator Cry()
    {
        int random = Random.Range(0, notes.Length);

        for (int i = 0; i < notes[random].notes.Length; i++)
        {
            notes[random].notes[i].SetDirection((Wakgood.Instance.transform.position - transform.position).normalized);
            notes[random].notes[i].transform.localPosition = Vector3.zero;
            notes[random].notes[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(.2f);
        }
    }

    protected override void Collapse()
    {
        foreach (Note _notes in notes)
        foreach (BulletMove note in _notes.notes)
        {
            note.gameObject.SetActive(false);
        }

        base.Collapse();
    }

    [Serializable]
    private struct Note
    {
        public BulletMove[] notes;
    }
}