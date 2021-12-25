using FMODUnity;
using System.Collections;
using UnityEngine;
using Cinemachine;

public abstract class BossMonster : Monster
{
    [SerializeField] private BoolVariable isShowingSomething;
    public int npcID;
    public string nickName;
    protected CinemachineTargetGroup cinemachineTargetGroup;
    protected new CinemachineVirtualCamera camera;

    protected override void Awake()
    {
        base.Awake();
        camera = GameObject.Find("Cameras").transform.GetChild(1).GetComponent<CinemachineVirtualCamera>();
        cinemachineTargetGroup = GameObject.Find("Cameras").transform.GetChild(2).GetComponent<CinemachineTargetGroup>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(Ready());
    }

    private IEnumerator Ready()
    {
        isShowingSomething.RuntimeValue = true;
        yield return StartCoroutine(UIManager.Instance.SpeedWagon_BossOn(this));
        isShowingSomething.RuntimeValue = false;

        yield return new WaitForSeconds(2f);
        StartCoroutine(nameof(Attack));
    }

    protected abstract IEnumerator Attack();
    
    protected override IEnumerator Collapse()
    {
        isCollapsed = true;

        if (DataManager.Instance.CurGameData.killedOnceBoss[ID] == false)
        {
            if (Collection.Instance != null)
                Collection.Instance.Collect(this, true);
            DataManager.Instance.CurGameData.killedOnceBoss[ID] = true;
            DataManager.Instance.SaveGameData();
        }

        if (DataManager.Instance.CurGameData.rescuedNPC[npcID] == false)
        {
            DataManager.Instance.CurGameData.rescuedNPC[npcID] = true;
            DataManager.Instance.SaveGameData();
        }

        SpriteRenderer.material = originalMaterial;

        cinemachineTargetGroup.m_Targets[1].target = null;
        RuntimeManager.PlayOneShot($"event:/SFX/Monster/{(name.Contains("(Clone)") ? name.Remove(name.IndexOf("(", System.StringComparison.Ordinal), 7) : name)}_Collapse", transform.position);

        collider2D.enabled = false;
        Rigidbody2D.velocity = Vector2.zero;
        Rigidbody2D.bodyType = RigidbodyType2D.Static;
        Animator.SetTrigger("COLLAPSE");
        GameManager.Instance.enemyRunTimeSet.Remove(gameObject);
        onMonsterCollapse.Raise(transform);
        int randCount = Random.Range(0, 5);
        for (int i = 0; i < randCount; i++) ObjectManager.Instance.PopObject("ExpOrb", transform);

        ObjectManager.Instance.PopObject("LevelUpEffect", transform);
        yield return StartCoroutine(UIManager.Instance.SpeedWagon_BossOff(this));
        yield return new WaitForSeconds(3f);

        ObjectManager.Instance.PopObject("LevelUpEffect", transform);

        (StageManager.Instance.CurrentRoom as BossRoom)?.RoomClear();
        DataManager.Instance.CurGameData.rescuedNPC[ID] = true;
        DataManager.Instance.SaveGameData();
        gameObject.SetActive(false);
    }
}
