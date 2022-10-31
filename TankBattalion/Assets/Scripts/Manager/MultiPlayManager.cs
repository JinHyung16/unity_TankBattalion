using Nakama;
using Nakama.TinyJson;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

sealed class MultiPlayManager : MonoBehaviour
{
    #region SingleTon
    private static MultiPlayManager instance;

    public static MultiPlayManager GetInstance
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

    private AudioSource audio;
    // audio
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip overSound;

    // panel
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject topPanel;
    [SerializeField] private GameObject bottomPanel;
    [SerializeField] private GameObject resultPanel;

    [Tooltip("게임 결과 Text")]
    [SerializeField] private Text resultText;

    // button
    [Tooltip("Game Start Buttom")]
    [SerializeField] private Button startBt;

    [Tooltip("Exit Game Button")]
    [SerializeField] private Button exitBt;

    [Tooltip("Top Button Exist Game")]
    [SerializeField] private Button topExistBt;

    // game result flag
    public bool isStart = false;
    public bool isOver = false;

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

            startBt.onClick.AddListener(GameStart);
            exitBt.onClick.AddListener(ExitGame);
            topExistBt.onClick.AddListener(TopExitGame);
        }
    }

    private void Update()
    {
        // update game result text display
        if (isOver)
        {
            resultText.text = "Game Over";
        }
    }

    private void GameStart()
    {
        startPanel.SetActive(false);

        audio.Stop();
        Time.timeScale = 1;
        isStart = true;
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

    public void GameOver()
    {
        isOver = true;
        resultPanel.SetActive(true);

        // audio
        PlaySound("Over");

        Time.timeScale = 0;
    }

    public async void TopExitGame()
    {
        await GameManager.GetInstance.QuickMatch();
        GameManager.GetInstance.GoToMainScene();
    }

    public async void ExitGame()
    {
        await GameManager.GetInstance.QuickMatch();
        GameManager.GetInstance.GoToMainScene();
    }
}
