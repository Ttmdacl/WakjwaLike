using System.Collections;
using UnityEngine;

public class NormalRoom : Room
{
    [System.Serializable]
    private struct Wave
    {
        public MobSpawnData[] mobSpawnDatas;
    }
    [System.Serializable]
    private struct MobSpawnData
    {
        public GameObject monster;
        public Transform spawnPoint;
        public float spawnDuration;
        public float spawnDelay;
    }
    [SerializeField] private Wave[] waves;
    private int curWaveMonsterCount;
    private bool isCleared = false;
    private bool waveClearFlag = false;

    public override void Enter()
    {
        if (IsVisited == false) IsVisited = true;
        if (isCleared == false)
        {
            GameManager.Instance.IsFighting = true;
            foreach (var hider in DoorHiders) hider.SetActive(true);
            foreach (var particle in DoorParticles) particle.SetActive(false);
            StartCoroutine(StartWave());
        }
    }

    private IEnumerator StartWave()
    {
        foreach (var wave in waves)
        {
            yield return new WaitForSeconds(2f);
            curWaveMonsterCount = wave.mobSpawnDatas.Length;
            foreach (var mobSpawnData in wave.mobSpawnDatas)
            {
                yield return new WaitForSeconds(mobSpawnData.spawnDelay);
                StartCoroutine(SpawnMonster(mobSpawnData));              
            }
            while (!waveClearFlag) yield return null;
            waveClearFlag = false;
        }
        RoomClear();
    }

    private IEnumerator SpawnMonster(MobSpawnData mobSpawnData)
    {
        ObjectManager.Instance.PopObject("SpawnCircle", mobSpawnData.spawnPoint.position).GetComponent<Animator>().SetFloat("SPEED", 1 / mobSpawnData.spawnDuration);
        yield return new WaitForSeconds(mobSpawnData.spawnDuration);
        //EnemyRunTimeSet.Add());
        ObjectManager.Instance.PopObject(mobSpawnData.monster.name, mobSpawnData.spawnPoint.position);
    }

    public void CheckMonsterCount() { if (--curWaveMonsterCount <= 0) waveClearFlag = true; }

    private void RoomClear()
    {
        GameManager.Instance.IsFighting = false;
        isCleared = true;
        foreach (var hider in DoorHiders) hider.SetActive(false);
        foreach (var particle in DoorParticles) particle.SetActive(true);
        StartCoroutine(GameManager.Instance.RoomClearSpeedWagon());
        ObjectManager.Instance.PopObject("Chest", transform.Find("ChestPoint")).GetComponent<Chest>().InitializeRandom();
    }
}
