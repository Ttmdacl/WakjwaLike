﻿using System.Collections;
using UnityEngine;

public class Panchi : NormalMonster
{
    private Coroutine attack;
    private Coroutine checkDistance;
    private Coroutine idle;

    protected override void OnEnable()
    {
        base.OnEnable();

        idle = StartCoroutine(Idle());
        checkDistance = StartCoroutine(CheckWakgood());
    }

    protected override void OnDisable()
    {
        if (idle != null)
        {
            StopCoroutine(idle);
        }

        if (checkDistance != null)
        {
            StopCoroutine(checkDistance);
        }

        if (attack != null)
        {
            StopCoroutine(attack);
        }

        base.OnDisable();
    }

    private IEnumerator Idle()
    {
        while (true)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            SpriteRenderer.flipX = direction.x > 0;
            Animator.SetBool("ISMOVING", true);
            for (int i = 0; i < 10; i++)
            {
                Rigidbody2D.velocity = direction * MoveSpeed;
                yield return ws01;
            }

            Animator.SetBool("ISMOVING", false);
            yield return ws1;
            yield return ws1;
        }
    }

    private IEnumerator CheckWakgood()
    {
        while (true)
        {
            if (Vector2.Distance(transform.position, Wakgood.Instance.transform.position) < 10)
            {
                StopCoroutine(idle);
                attack = StartCoroutine(Attack());
                break;
            }

            yield return ws01;
        }
    }

    protected override void _ReceiveHit()
    {
        base._ReceiveHit();
        if (attack == null)
        {
            if (idle != null)
            {
                StopCoroutine(idle);
            }

            if (checkDistance != null)
            {
                StopCoroutine(checkDistance);
            }

            attack = StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        Animator.SetBool("ISMOVING", false);
        Rigidbody2D.velocity = Vector2.zero;

        while (true)
        {
            if (Vector2.Distance(transform.position, Wakgood.Instance.transform.position) > 2)
            {
                Animator.SetBool("ISMOVING", true);
                SpriteRenderer.flipX = IsWakgoodRight();
                Rigidbody2D.velocity = GetDirection() * MoveSpeed;
                yield return ws01;
            }
            else
            {
                Animator.SetBool("ISMOVING", false);
                Rigidbody2D.velocity = Vector2.zero;
                Vector3 direction = GetDirection();
                yield return StartCoroutine(Casting(castingTime));
                Animator.SetTrigger("ATTACK");
                ObjectManager.Instance.PopObject("PanchiSlash", transform.position + Vector3.up + (direction * 1.5f),
                    GetRot());
                yield return ws1;
            }
        }
    }
}