using UnityEngine;
using FMODUnity;
public class Box : MonoBehaviour, IHitable
{
    [SerializeField] private GameObject[] fragments;

    public void ReceiveHit(int damage, HitType hitType = HitType.Normal)
    {
        RuntimeManager.PlayOneShot("event:/SFX/ETC/Box", Wakgood.Instance.AttackPosition.position);
        ObjectManager.Instance.PopObject("HealObject", transform);
        foreach (GameObject fragment in fragments)
        {
            fragment.SetActive(true);
            fragment.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(300f, 500f) * (1 + -2 * Random.Range(0, 2)), Random.Range(300f, 500f) * (1 + -2 * Random.Range(0, 2))));
        }
        gameObject.SetActive(false);
    }
}