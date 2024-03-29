using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HughGeneric;
public class SinglePlayManager : Singleton<SinglePlayManager>
{
    private AudioSource audio;
    // audio
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip overSound;

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

    [Tooltip("Exit Button, Can Load Main Scene")]
    [SerializeField] private Button exitBt;

    // game result flag
    public bool isStart = false;
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
            isStart = false;

            topPanel.SetActive(true);
            bottomPanel.SetActive(true);
            resultPanel.SetActive(false);

            roundText.text = 1.ToString();

            startBt.onClick.AddListener(GameStart);
            restartBt.onClick.AddListener(Restart);
            exitBt.onClick.AddListener(ExitGame);
        }
    }

    private void Update()
    {
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

    private void GameStart()
    {
        startPanel.SetActive(false);

        audio.Stop();
        Time.timeScale = 1;
        isStart = true;
    }

    private void Restart()
    {
        string SceneName = HughSceneManager.GetInstace.GetActiveSceneName();
        if (SceneName == "SinglePlay")
        {
            HughSceneManager.GetInstace.LoadSinglePlayScene();
        }
        else if (SceneName == "MultiPlay")
        {
            HughSceneManager.GetInstace.LoadMultiPlayScene();
        }

        ResetSetting();
    }

    private void ResetSetting()
    {
        //SceneManager.LoadScene(0);

        // panel setting
        topPanel.SetActive(true);
        bottomPanel.SetActive(true);
        resultPanel.SetActive(false);

        // reset the game info
        score = 0;
        breakEnemyCount = 20;
        isOver = false;
        isClear = false;
        isStart = true;

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

    private void ExitGame()
    {
        ResetSetting();

        GameManager.GetInstance.GoToMainScene();

        topPanel.SetActive(false);
        bottomPanel.SetActive(false);
        resultPanel.SetActive(false);
        startPanel.SetActive(false);
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
