using Cinemachine;
using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// Todo : 방종마다 강해지는 요소 > 방송용으로, 아무튼 클리어는 해야하니까
// Todo : 인트로, 진짜 왁굳, 엔딩 크레딧

public class Wakgood : MonoBehaviour, IHitable
{
    public static Wakgood Instance { get; private set; }

    [SerializeField] private Wakdu wakdu;
    [SerializeField] private IntVariable exp, level;
    [SerializeField] private IntVariable hpCur;
    [SerializeField] private MaxHp hpMax;
    [SerializeField] private IntVariable powerInt;
    public TotalPower totalPower;
    public FloatVariable attackSpeed;
    [SerializeField] private FloatVariable moveSpeed, evasion;
    [SerializeField] private BoolVariable canEvasionOnDash;
    [SerializeField] private GameEvent onDamage, onCollapse, onLevelUp;

    private Transform attackPositionParent;
    public Transform AttackPosition { get; private set; }
    public Transform WeaponPosition { get; private set; }
    private CinemachineTargetGroup cinemachineTargetGroup;

    private bool isHealthy;

    private SpriteRenderer spriteRenderer;
    private WakgoodCollider wakgoodCollider;
    public WakgoodMove WakgoodMove { get; private set; }

    private Vector3 worldMousePoint;

    public int CurWeaponNumber { get; private set; }
    public Weapon[] Weapon { get; } = new Weapon[2];
    [SerializeField] private Weapon hochi, hand;

    private static readonly int collapse = Animator.StringToHash("Collapse");

    public bool IsSwitching { get; private set; }
    public bool IsCollapsed { get; private set; }
    [SerializeField] private BoolVariable isFocusOnSomething;

    private GameObject chat;
    private TextMeshProUGUI chatText;

    private void Awake()
    {
        Instance = this;

        // attackPosition.transform.position = new Vector3(0, attackPosGap, 0);

        attackPositionParent = transform.Find("AttackPosParent");
        AttackPosition = attackPositionParent.GetChild(0);
        WeaponPosition = transform.Find("WeaponPos");

        spriteRenderer = GetComponent<SpriteRenderer>();
        wakgoodCollider = transform.GetChild(0).GetComponent<WakgoodCollider>();
        WakgoodMove = GetComponent<WakgoodMove>();
        cinemachineTargetGroup = GameObject.Find("CM TargetGroup").GetComponent<CinemachineTargetGroup>();

        chat = transform.Find("Canvas").Find("Chat").gameObject;
        chatText = chat.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable() => Initialize();

    public void Initialize()
    {
        StopAllCoroutines();

        transform.position = Vector3.zero;

        hpMax.RuntimeValue = wakdu.baseHp;
        hpCur.RuntimeValue = hpMax.RuntimeValue;
        powerInt.RuntimeValue = wakdu.basePower;
        attackSpeed.RuntimeValue = wakdu.baseAttackSpeed;
        moveSpeed.RuntimeValue = wakdu.baseMoveSpeed;
        level.RuntimeValue = 1;
        exp.RuntimeValue = 0;
        isHealthy = true;
        IsSwitching = false;
        IsCollapsed = false;

        cinemachineTargetGroup.m_Targets[0].target = transform;

        if (WeaponPosition.childCount > 0) Destroy(WeaponPosition.GetChild(0).gameObject);
        if (Weapon[CurWeaponNumber] != null) Weapon[CurWeaponNumber].OnRemove();

        UIManager.Instance.SetWeaponUI(0, Weapon[0] = hochi);
        UIManager.Instance.SetWeaponUI(1, Weapon[1] = hand);

        if (CurWeaponNumber != 0)
        {
            CurWeaponNumber = 0;
            UIManager.Instance.StartCoroutine(UIManager.Instance.SwitchWeapon());
        }

        Weapon[CurWeaponNumber].OnEquip();
        Instantiate(Weapon[CurWeaponNumber].resource, WeaponPosition);

        wakgoodCollider.enabled = true;
        WakgoodMove.enabled = true;
        WakgoodMove.StopAllCoroutines();
        WakgoodMove.Initialize();
    }

    private void Update()
    {
        if (Time.timeScale == 0 || IsCollapsed) return;

        spriteRenderer.color = isHealthy ? Color.white : new Color(1, 1, 1, (float)100 / 255);
        if (isFocusOnSomething.RuntimeValue) return;

        spriteRenderer.flipX = transform.position.x > worldMousePoint.x;
        worldMousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Debug.Log(EventSystem.current.IsPointerOverGameObject());

        
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) Weapon[CurWeaponNumber].BaseAttack();
        else if (Input.GetKeyDown(KeyCode.Q)) Weapon[CurWeaponNumber].SkillQ();
        else if (Input.GetKeyDown(KeyCode.E)) Weapon[CurWeaponNumber].SkillE();
        else if (Input.GetKeyDown(KeyCode.R)) Weapon[CurWeaponNumber].Reload();
        else if (Input.GetKeyDown(KeyCode.F)) wakgoodCollider.GetNearestInteractiveObject()?.Interaction();

        if (Input.GetAxisRaw("Mouse ScrollWheel") != 0) SwitchWeapon(CurWeaponNumber == 0 ? 1 : 0);
        else if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);

        attackPositionParent.transform.rotation = Quaternion.Euler(0, 0,
            Mathf.Atan2(worldMousePoint.y - (transform.position.y + 0.8f), worldMousePoint.x - transform.position.x) *
            Mathf.Rad2Deg - 90);

        if (transform.position.x < worldMousePoint.x)
        {
            WeaponPosition.localScale = Vector3.one;
            WeaponPosition.localPosition = new Vector3(.3f, .5f, 0);
            WeaponPosition.rotation = Quaternion.Euler(0, 0,
                Mathf.Atan2(worldMousePoint.y - WeaponPosition.position.y,
                    worldMousePoint.x - WeaponPosition.position.x) * Mathf.Rad2Deg);
        }
        else
        {
            WeaponPosition.localScale = new Vector3(-1, 1, 1);
            WeaponPosition.localPosition = new Vector3(-.3f, .5f, 0);
            WeaponPosition.rotation = Quaternion.Euler(0, 0,
                Mathf.Atan2(WeaponPosition.position.y - worldMousePoint.y,
                    WeaponPosition.position.x - worldMousePoint.x) * Mathf.Rad2Deg);
        }
    }

    public void SwitchWeapon(int targetWeaponNum, Weapon targetWeapon = null)
    {
        if (IsSwitching) return;
        IsSwitching = true;
        StartCoroutine(TtmdaclExtension.ChangeWithDelay(false, .3f, value => IsSwitching = value));

        if (targetWeapon == null)
        {
            Weapon[CurWeaponNumber].OnRemove();
            Destroy(WeaponPosition.GetChild(0).gameObject);

            CurWeaponNumber = targetWeaponNum;

            Instantiate(Weapon[CurWeaponNumber].resource, WeaponPosition);
            Weapon[CurWeaponNumber].OnEquip();

            UIManager.Instance.StartCoroutine(UIManager.Instance.SwitchWeapon());
        }
        else
        {
            if (CurWeaponNumber != targetWeaponNum)
            {
                Weapon[targetWeaponNum] = targetWeapon;
            }
            else
            {
                Weapon[CurWeaponNumber].OnRemove();
                Destroy(WeaponPosition.GetChild(0).gameObject);

                Weapon[CurWeaponNumber] = targetWeapon;

                Instantiate(Weapon[CurWeaponNumber].resource, WeaponPosition);
                Weapon[CurWeaponNumber].OnEquip();
            }

            UIManager.Instance.SetWeaponUI(targetWeaponNum, targetWeapon);
        }
    }

    public void ReceiveHit(int damage)
    {
        if (!isHealthy || (WakgoodMove.MbDashing && canEvasionOnDash.RuntimeValue))
        {
            return;
        }

        if (evasion.RuntimeValue >= Random.Range(1, 100 + 1))
        {
            RuntimeManager.PlayOneShot($"event:/SFX/Wakgood/Evasion", transform.position);
            ObjectManager.Instance.PopObject("AnimatedText", transform).GetComponent<AnimatedText>()
                .SetText("MISS", TextType.Critical);
        }
        else
        {
            RuntimeManager.PlayOneShot($"event:/SFX/Wakgood/Ahya", transform.position);
            onDamage.Raise();

            if ((hpCur.RuntimeValue -= damage) > 0)
            {
                isHealthy = false;
                StartCoroutine(TtmdaclExtension.ChangeWithDelay(true, .5f, value => isHealthy = value));
            }
            else
            {
                hpCur.RuntimeValue = 0;
                StopAllCoroutines();
                StartCoroutine(Collapse());
            }
        }
    }

    public void ReceiveHeal(int amount)
    {
        if (hpCur.RuntimeValue == hpMax.RuntimeValue)
        {
            return;
        }

        hpCur.RuntimeValue += amount;
        ObjectManager.Instance.PopObject("AnimatedText", transform).GetComponent<AnimatedText>()
            .SetText(amount.ToString(), TextType.Heal);
    }

    private IEnumerator Collapse()
    {
        WakgoodMove.StopAllCoroutines();

        IsCollapsed = true;
        WakgoodMove.PlayerRb.bodyType = RigidbodyType2D.Static;
        WakgoodMove.Animator.SetTrigger(collapse);
        WakgoodMove.enabled = false;
        wakgoodCollider.enabled = false;

        yield return new WaitForSeconds(2f);
        DataManager.Instance.CurGameData.deathCount++;
        onCollapse.Raise();
        enabled = false;
    }

    public void CheckCanLevelUp()
    {
        if (exp.RuntimeValue >= 100 * level.RuntimeValue) LevelUp();
    }

    private void LevelUp()
    {
        hpMax.RuntimeValue += wakdu.growthHp;
        powerInt.RuntimeValue += wakdu.growthPower;
        attackSpeed.RuntimeValue += wakdu.growthAttackSpeed;

        exp.RuntimeValue -= 100 * level.RuntimeValue;
        level.RuntimeValue++;
        onLevelUp.Raise();

        ObjectManager.Instance.PopObject("LevelUpEffect", transform);
        ObjectManager.Instance.PopObject("AnimatedText", transform).GetComponent<AnimatedText>()
            .SetText("Level Up!", TextType.Critical);
    }

    public void SetRigidBodyType(RigidbodyType2D rigidbodyType2D)
    {
        WakgoodMove.PlayerRb.bodyType = rigidbodyType2D;
    }

    public IEnumerator ShowChat(string msg)
    {
        chatText.text = msg;
        chat.SetActive(true);
        yield return new WaitForSeconds(3f);
        chat.SetActive(false);
    }
}