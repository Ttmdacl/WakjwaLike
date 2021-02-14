using UnityEngine;

public class DamagingObject : MonoBehaviour
{
    private enum Target
    {
        Monster,
        Traveller
    }
    [SerializeField] private IntVariable travellerAD;
    [SerializeField] private IntVariable TravellerCriticalChance;
    [SerializeField] private int monsterAD;
    [SerializeField] private Target target;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Monster") || other.CompareTag("Boss")) && target.Equals(Target.Monster))
        {
            if (Random.Range(0, 100) < TravellerCriticalChance.RuntimeValue)
            {
                other.gameObject.GetComponent<Monster>().ReceiveDamage(travellerAD.RuntimeValue * 4, DamageType.Critical);
            }
            else
            {
                other.gameObject.GetComponent<Monster>().ReceiveDamage(travellerAD.RuntimeValue);
            }
        }
        else if (other.CompareTag("Player") && target.Equals(Target.Traveller))
        {
            other.gameObject.GetComponent<TravellerController>().ReceiveDamage(monsterAD);
        }
    }
}