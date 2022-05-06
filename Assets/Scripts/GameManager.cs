using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

sealed class GameManager : MonoBehaviour
{
    #region SingleTon
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

    }
    #endregion

    AudioSource audio;
    // audio
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip overSound;

    // enemy respawn (상 하 좌 우 4개 귀퉁이 TransPos 받아서 넣어놓기)
    public GameObject enemy;

    [SerializeField] private Transform[] enemyRespawnPos;
    [SerializeField] private float enemyRespawnTime = 4.0f;
    private float curTime = 0.0f;

    // health
    [SerializeField] private int health = 3;

    // enemy count that destory mission
    [SerializeField] private int breakEnemyCount = 20;

    // score
    public int score = 0;

    // panel
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject topPanel;
    [SerializeField] private GameObject bottomPanel;
    [SerializeField] private GameObject resultPanel;

    // img
    [Tooltip("player HP img")]
    [SerializeField] private Image[] healthImgs; // player HP img

    [Tooltip("파괴해야 하는 enemy img")]
    [SerializeField] private Image[] enemyCountImgs; // enemy count img

    // text
    [Tooltip("최고 점수 Text")]
    [SerializeField] private Text highScoreText;

    [Tooltip("현재 라운드 Text")]
    [SerializeField] private Text roundText;

    [Tooltip("결과창에 나오는 점수 Text")]
    [SerializeField] private Text resultScoreText;

    [Tooltip("게임 결과 Text")]
    [SerializeField] private Text resultText;

    // button
    [Tooltip("Game Start Buttom")]
    [SerializeField] private Button startBt;

    [Tooltip("Restart Button")]
    [SerializeField] private Button restartBt;

    // game result flag
    public bool isOver = false;
    public bool isClear = false;
    public bool isDefend = false;

    private void Start()
    {
        if (this.gameObject != null)
        {
            audio = GetComponent<AudioSource>();

            PlaySound("Start");

            // panel setting
            startPanel.SetActive(true);
            Time.timeScale = 0;

            topPanel.SetActive(true);
            bottomPanel.SetActive(true);
            resultPanel.SetActive(false);

            roundText.text = 1.ToString();

            startBt.onClick.AddListener(GameStart);
            restartBt.onClick.AddListener(Restart);
        }
    }

    private void Update()
    {
        curTime += Time.deltaTime;
        if ((curTime > enemyRespawnTime) && !isOver)
        {
            EnemyRespawn();
            curTime = 0.0f;
        }

        if(isDefend)
        {
            Debug.Log("GameManager: 방어만 하여 적을 더 빨리 생성한다.");
            enemyRespawnTime = 3.0f;
        }
        else
        {
            Debug.Log("GameManager: 다시 원상복귀");
            enemyRespawnTime = 5.0f;
        }

        // update score text display
        ScoreUpdate();

        // update game result text display
        if (isClear)
        {
            resultText.text = "Game Clear";
        }
        else
        {
            resultText.text = "Game Over";
        }

    }

    #region Game Manager Function

    private void EnemyRespawn()
    {
        int posIndex = Random.Range(0, 4);
        GameObject e = Instantiate(enemy, enemyRespawnPos[posIndex]);
        e.name = "Enemy";
    }

    private void GameStart()
    {
        startPanel.SetActive(false);

        audio.Stop();
        Time.timeScale = 1;
    }

    private void Restart()
    {
        SceneManager.LoadScene(0);

        // panel setting
        topPanel.SetActive(true);
        bottomPanel.SetActive(true);
        resultPanel.SetActive(false);

        // reset the game info
        score = 0;
        breakEnemyCount = 20;
        isOver = false;
        isClear = false;

        for (int i = 0; i < healthImgs.Length; i++)
        {
            healthImgs[i].color = new Color(1, 1, 1, 1);
        }
        for (int i = 0; i < enemyCountImgs.Length; i++)
        {
            enemyCountImgs[i].color = new Color(0, 1, 1, 1);
        }
        Time.timeScale = 1;
    }

    private void PlaySound(string name)
    {
        switch (name)
        {
            case "Start":
                audio.clip = startSound;
                break;
            case "Over":
                audio.clip = overSound;
                break;
        }

        audio.Play();
    }
    public void ScoreUpdate()
    {
        highScoreText.text = score.ToString();
    }

    public void HealthDown()
    {
        if (health > 0)
        {
            health--;
            healthImgs[health].color = new Color(0, 0, 0, 0);
        }
        else
        {
            healthImgs[0].color = new Color(0, 0, 0, 0);
            GameOver();
        }
    }

    public void EnemyDown()
    {
        score += 300;
        if (breakEnemyCount > 0)
        {
            breakEnemyCount--;
            enemyCountImgs[breakEnemyCount].color = new Color(0, 0, 0, 0);
        }
        else
        {
            enemyCountImgs[0].color = new Color(0, 0, 0, 0);
            isClear = true;
            GameOver();
        }
    }

    public void GameOver()
    {
        isOver = true;
        resultPanel.SetActive(true);
        resultScoreText.text = "Score " + score.ToString();

        // audio
        PlaySound("Over");

        Time.timeScale = 0;
    }
    #endregion
}
