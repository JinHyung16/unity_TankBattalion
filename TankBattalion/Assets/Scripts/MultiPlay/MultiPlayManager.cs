using UnityEngine;
using UnityEngine.UI;

public class MultiPlayManager : MonoBehaviour
{
    #region SingleTon
    private static MultiPlayManager instance;

    public static MultiPlayManager Instance
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

    // health
    [SerializeField] private int health = 3;

    // score
    public int score = 0;

    // panel
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject bottomPanel;
    [SerializeField] private GameObject resultPanel;

    // img
    [Tooltip("player HP img")]
    [SerializeField] private Image[] healthImgs; // player HP img

    [Tooltip("게임 결과 Text")]
    [SerializeField] private Text resultText;

    // button
    [Tooltip("Game Start Buttom")]
    [SerializeField] private Button startBt;

    [Tooltip("Exit Game Button")]
    [SerializeField] private Button exitBt;

    // game result flag
    public bool isStart = false;
    public bool isOver = false;

    // Multi Player spawn position
    [SerializeField] private Transform[] spawnsPostions;

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

            bottomPanel.SetActive(true);
            resultPanel.SetActive(false);

            startBt.onClick.AddListener(GameStart);
            exitBt.onClick.AddListener(ExitGame);

            //spawn point 전달
            GameManager.Instance.GetSpawnPosition(this.spawnsPostions);
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


    public void GameOver()
    {
        isOver = true;
        resultPanel.SetActive(true);
        GameManager.Instance.ControlWinDisPlayPanel(true);

        // audio
        PlaySound("Over");

        Time.timeScale = 0;
    }

    public void ExitGame()
    {
        GameManager.Instance.ControlWinDisPlayPanel(false);
        GameManager.Instance.GoToMainScene();
    }
}
