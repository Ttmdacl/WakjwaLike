using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EffectMonsterKillSpawn", menuName = "Effect/EffectMonsterKillSpawn")]
public class EffectMonsterKillSpawn : Effect
{
    [SerializeField] private GameEvent OnMonsterCollapse;
    [SerializeField] private GameObject spawnObject;
    [SerializeField] private int percentage;

    private void SpawnObject()
    {
        if (Random.Range(0, 100) < percentage)
            ObjectManager.Instance.PopObject(spawnObject.name, Wakgood.Instance.transform.position);
    }

    public override void _Effect() => OnMonsterCollapse.AddCollback(SpawnObject);

    public override void Return() => OnMonsterCollapse.RemoveCollback(SpawnObject);
}

