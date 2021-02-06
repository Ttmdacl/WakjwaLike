﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    [HideInInspector] public static GameManager Instance { get { return instance; } }

    [HideInInspector] public int nyang = 0;
    [HideInInspector] public int monsterKill = 0;
    [HideInInspector] public bool isFighting = false;
    private int currentStageID = 0;
    [SerializeField] private int maxRoomCount = 0;
    [SerializeField] private int roomMoldLength = 0;
    private List<Room> roomList = new List<Room>();
    private Dictionary<Vector2, Room> roomDictionary = new Dictionary<Vector2, Room>();
    [HideInInspector] public Room currentRoom;

    [SerializeField] private GameObject stageGrid;
    private int roomCount = 0;
    // private Queue<RoomStack> roomStackQueue = new Queue<RoomStack>();
    private List<Room> roomDatas = new List<Room>();
    
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Text monsterKillText;
    [SerializeField] private Text nyangText;
    [SerializeField] private GameObject miniMapCamera;

    [SerializeField] private GameObject gameOverPanel; 

    [SerializeField] private GameObject pausePanel;
    
    [SerializeField] private GameObject mapPanel;
    [SerializeField] GridLayoutGroup mapGridLayoutGroup;
    private Dictionary<Vector2, GameObject> roomUiDictionary = new Dictionary<Vector2, GameObject>();  

    [SerializeField] private GameObject fadePanel;
    [SerializeField] private Animator fadePanelAnimator;

    [SerializeField] private GameObject bagPanel;

    [SerializeField] private Text noticeText;
    [HideInInspector] public List<GameObject> monsters;
    [SerializeField] private GameObject stageSpeedWagon;
    [SerializeField] private Text stageNumberText, stageNameText;
    [SerializeField] private GameObject bossSpeedWagon;
    [SerializeField] private GameObject roomClearSpeedWagon;
    [SerializeField] private GameObject undo;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // 뒤로가기 버튼을 눌렀을 때, 정지 및 재개 ★ Time을 통한 실질적인 게임 정지 및 재개
        // # 정지 > 귀환 (StopCoroutine) 오류 가능성 : 따라서 플래그를 통해 현재 정지 시킬 수 있는지 확인해야함
        if (Input.GetKeyDown(KeyCode.Escape)) PauseGame();
    }

    public void PauseGame()
    {
        if (Time.timeScale == 1) Time.timeScale = 0;
        else if (Time.timeScale == 0) Time.timeScale = 1;

        mapPanel.SetActive(false);
        pausePanel.SetActive(!pausePanel.activeSelf);
    }

    public void OpenAndCloseBag()
    {
        if (isFighting)
        {
            StopCoroutine(NoticeText("전투 중에는 열 수 없습니다.", 1.5f));
            StartCoroutine(NoticeText("전투 중에는 열 수 없습니다.", 1.5f));
        }
        else
        {
            StopCoroutine(NoticeText("전투 중에는 열 수 없습니다.", 1.5f));

            bagPanel.SetActive(!bagPanel.activeSelf);
        }
    }

    private IEnumerator NoticeText(string text, float time)
    {
        noticeText.text = text;
        noticeText.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        noticeText.gameObject.SetActive(false);
    }

    public IEnumerator EnterPortal()
    {
        fadePanel.SetActive(true);
        yield return new WaitForSeconds(0.2f);

        undo.SetActive(false);

        currentStageID++;
        GenerateStage(currentStageID);
    }

    private void GenerateStage(int stageIndex) // 스테이지 만들기 시작
    {
        DestroyStage();
        roomCount = maxRoomCount;
        roomList.Clear();
        roomDatas = new List<Room>(StageDataBase.Instance.stages[stageIndex].roomDatas); 
        roomDictionary.Clear();

        GenerateRoom(Vector2.zero , true);

        while (roomCount > 0)
        {
            Vector2 randomRoomCoordinate = roomList[Random.Range(0, roomList.Count)].coordinate;
            if (randomRoomCoordinate.y != (roomMoldLength - 1) / 2) GenerateRoomAndLink(randomRoomCoordinate, Vector2.up, 0, 1);
            if (randomRoomCoordinate.y != -(roomMoldLength - 1) / 2) GenerateRoomAndLink(randomRoomCoordinate, Vector2.down, 1, 0);
            if (randomRoomCoordinate.x != -(roomMoldLength - 1) / 2) GenerateRoomAndLink(randomRoomCoordinate, Vector2.left, 2, 3);
            if (randomRoomCoordinate.x != (roomMoldLength - 1) / 2) GenerateRoomAndLink(randomRoomCoordinate, Vector2.right, 3, 2);
        }

        StartCoroutine(StartStage());
    }

    private void GenerateRoom(Vector2 coordinate, bool isFirstTry = false)
    {
        int roomDataIndex = isFirstTry ? 0 : Random.Range(0, roomDatas.Count);
        Room r = Instantiate(roomDatas[roomDataIndex].gameObject, stageGrid.transform).GetComponent<Room>();
        r.transform.localPosition = coordinate * 100;
        r.SetRoomCoordinate(coordinate);
        roomList.Add(r);
        roomDictionary.Add(coordinate, r);
        roomDatas.RemoveAt(roomDataIndex);
        roomCount--;
    }

    private void GenerateRoomAndLink(Vector2 originalCoordinate, Vector2 direction, int originalRoomDoorIndex, int totalRoomDoorIndex)
    {
        Vector2 totalCoordinate = originalCoordinate + direction;
        if (roomCount <= 0 || Random.Range(0, 2) == 0 || roomDictionary.ContainsKey(totalCoordinate)) return;
        GenerateRoom(totalCoordinate);
        roomDictionary[originalCoordinate].isDoorOpen[originalRoomDoorIndex] = true;
        roomDictionary[totalCoordinate].isDoorOpen[totalRoomDoorIndex] = true;
    }

    private IEnumerator StartStage()
    {  
        currentRoom = roomDictionary[Vector2.zero];
        currentRoom.Enter();
        InitialzeMap();
        
        Traveller.Instance.transform.position = new Vector3(currentRoom.coordinate.x, currentRoom.coordinate.y, 0) * 100;
        miniMapCamera.transform.position = new Vector3(currentRoom.coordinate.x, currentRoom.coordinate.y, -1) * 100;

        fadePanelAnimator.SetTrigger("FadeIn");
        StartCoroutine("StageSpeedWagon");
        yield return new WaitForSeconds(0.2f);
        fadePanel.SetActive(false);
    }

    public void InitialzeMap()
    {     
        mapGridLayoutGroup.constraintCount = roomMoldLength;
        roomUiDictionary = new Dictionary<Vector2, GameObject>();

        for (int i = 0; i < mapGridLayoutGroup.transform.childCount; i++)
        {
            if (i <= roomMoldLength * roomMoldLength - 1)
                mapGridLayoutGroup.transform.GetChild(i).gameObject.SetActive(true);
            else if (i > roomMoldLength * roomMoldLength - 1)
                mapGridLayoutGroup.transform.GetChild(i).gameObject.SetActive(false);

            for (int j = 0; j < mapGridLayoutGroup.transform.GetChild(i).transform.childCount; j++)
                mapGridLayoutGroup.transform.GetChild(i).transform.GetChild(j).gameObject.SetActive(false);
        }

        int childIndex = 0;
        for (int y = (roomMoldLength - 1) / 2; y >= -(roomMoldLength - 1) / 2; y--)
        {
            for (int x = -(roomMoldLength - 1) / 2; x <= (roomMoldLength - 1) / 2; x++, childIndex++)
            {
                roomUiDictionary.Add(new Vector2(x, y), mapGridLayoutGroup.transform.GetChild(childIndex).gameObject);
            }
        }

        UpdateMap();
    }

    private void UpdateMap()
    {  
        roomUiDictionary[currentRoom.coordinate].transform.Find("CurrentRoom").gameObject.SetActive(true);
        roomUiDictionary[currentRoom.coordinate].transform.Find("StageTheme").gameObject.SetActive(true);
      
        UpdateRoomUI(0);
        UpdateRoomUI(1);
        UpdateRoomUI(2);
        UpdateRoomUI(3);
    }

    private void UpdateRoomUI(int doorIndex)
    {
        Vector2 totalCoordinate = currentRoom.coordinate;
        string originDoor = "";
        string totalDoor = "";

        if (doorIndex == 0) {totalCoordinate.y++; originDoor = "Up"; totalDoor = "Down";}
        else if (doorIndex == 1) {totalCoordinate.y--; originDoor = "Down"; totalDoor = "Up";}
        else if (doorIndex == 2) {totalCoordinate.x--; originDoor = "Left"; totalDoor = "Right";}
        else if (doorIndex == 3) {totalCoordinate.x++; originDoor = "Right"; totalDoor = "Left";}

        if (currentRoom.isDoorOpen[doorIndex] && !roomDictionary[totalCoordinate].isCleared)
        {
            roomUiDictionary[currentRoom.coordinate].transform.Find(originDoor).gameObject.SetActive(true);
            roomUiDictionary[totalCoordinate].transform.Find(totalDoor).gameObject.SetActive(true);

            if (roomDictionary[totalCoordinate].roomType == RoomType.Boss)
                roomUiDictionary[totalCoordinate].transform.Find("Boss").gameObject.SetActive(true);
        }
    }
    
    public IEnumerator MigrateRoom(Vector2 moveDirection, int totalDoorIndex)
    {
        fadePanel.SetActive(true);
        yield return new WaitForSeconds(0.2f);

        roomUiDictionary[currentRoom.coordinate].transform.Find("CurrentRoom").gameObject.SetActive(false);

        currentRoom = roomDictionary[currentRoom.coordinate + moveDirection];
        Traveller.Instance.transform.position = new Vector3(currentRoom.doors[totalDoorIndex].transform.position.x, currentRoom.doors[totalDoorIndex].transform.position.y, 0) + (Vector3)moveDirection * 2f;
        miniMapCamera.transform.position = new Vector3(currentRoom.coordinate.x, currentRoom.coordinate.y, -1) * 100;

        UpdateMap();

        if (currentRoom.isCleared == false)
        {
            bagPanel.SetActive(false);
            mapPanel.SetActive(false);  
        }
        currentRoom.Enter();
        
        fadePanelAnimator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(0.2f);
        fadePanel.SetActive(false);
    }

    public IEnumerator GameOver()
    {
        Debug.Log("GameOver");
        // 플레이어 스크립트에서 hp <= 0, Died 감지 > 마지막 처리 후 플레이어 스크립트 비활성화
        yield return new WaitForSeconds(2f); // 2초 동안 Player Died/Recall 애니메이션 실행

        gamePanel.SetActive(false); // 게임 Panel @비활성화
        gameOverPanel.SetActive(true); // 게임 결과 Panel @활성화
    }
    
    public void Recall()
    {
        DestroyStage();
        undo.SetActive(true);

        StopCoroutine("BossSpeedWagon");
        bossSpeedWagon.SetActive(false);
        StopCoroutine("StageSpeedWagon");
        stageSpeedWagon.SetActive(false);
        StopCoroutine("RoomClearSpeedWagon");
        roomClearSpeedWagon.SetActive(false);

        AbilityManager.Instance.selectAbilityPanel.SetActive(false);

        ObjectManager.Instance.InsertAll();
        // UpdateMap();
        monsters.Clear();

        Traveller.Instance.enabled = true;
        Traveller.Instance.Initialize();

        miniMapCamera.transform.position = new Vector3(0, 0, -100);

        isFighting = false;
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gamePanel.SetActive(true);

        Time.timeScale = 1;
    }

    public void AcquireNyang(int amount)
    {
        nyang += amount;
        nyangText.text = nyang.ToString();
    }

    public void AcquireKillCount()
    {
        monsterKill++;
        monsterKillText.text = monsterKill.ToString();
    }

    public void OpenAndCloseMap()
    {
        if (isFighting)
        {
            StopCoroutine(NoticeText("전투 중에는 열 수 없습니다.", 1.5f));
            StartCoroutine(NoticeText("전투 중에는 열 수 없습니다.", 1.5f));
        }
        else
        {
            StopCoroutine(NoticeText("전투 중에는 열 수 없습니다.", 1.5f));

            mapPanel.SetActive(!mapPanel.activeSelf);
        }    
    }

    public void QuitGame()
    { 
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    private IEnumerator StageSpeedWagon()
    {
        stageSpeedWagon.SetActive(true);
        stageNumberText.text = currentStageID.ToString();
        stageNameText.text = StageDataBase.Instance.stages[currentStageID].name;
        yield return new WaitForSeconds(2f);
        stageSpeedWagon.SetActive(false);
    }

    public IEnumerator BossSpeedWagon()
    {
        GameObject.Find("SlimeKing(Clone)").GetComponent<SlimeKing>().enabled = false;
        GameObject.Find("SlimeKing(Clone)").transform.Find("CM Camera1").GetComponent<CinemachineVirtualCamera>().Priority = 100;
        Traveller.Instance.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        bossSpeedWagon.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        bossSpeedWagon.gameObject.SetActive(false);
        Traveller.Instance.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        GameObject.Find("SlimeKing(Clone)").transform.Find("CM Camera1").GetComponent<CinemachineVirtualCamera>().Priority = 0;
        GameObject.Find("SlimeKing(Clone)").GetComponent<SlimeKing>().enabled = true;
    }

    public IEnumerator RoomClearSpeedWagon()
    {
        roomClearSpeedWagon.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        roomClearSpeedWagon.gameObject.SetActive(false);
    }
    
    public void DestroyStage()
    {
        for (int i = 0; i < stageGrid.transform.childCount; i++)
        {
            Destroy(stageGrid.transform.GetChild(i).gameObject);
        }
    }
}