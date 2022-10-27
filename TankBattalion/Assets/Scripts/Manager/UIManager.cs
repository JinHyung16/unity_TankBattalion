using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region SingleTon
    private static UIManager instance;

    public static UIManager Instace
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

    public GameObject titlePanel;
    public GameObject selectModePanel;

    public Button singlePlayBt;
    public Button multiPlayBt;

    private void Start()
    {
        if (this.gameObject != null)
        {
            singlePlayBt.onClick.AddListener(SinglePlayMode);
            multiPlayBt.onClick.AddListener(MultiPlayMode);
        }
    }

    private void SinglePlayMode()
    {
        SceneActiveWhenSceneLoad(false);
        HughSceneManager.GetInstace.LoadSinglePlayScene();
    }

    private async void MultiPlayMode()
    {
        SceneActiveWhenSceneLoad(false);
        HughSceneManager.GetInstace.LoadMultiPlayScene();
        await HughServer.GetInstace.ConnecToServer();
    }

    public async void GoToMainScene()
    {
        if (HughSceneManager.GetInstace.GetActiveSceneName() == "MultiPlay")
        {
            await HughServer.GetInstace.Disconnect();
        }

        HughSceneManager.GetInstace.LoadMainScene();

        SceneActiveWhenSceneLoad(true);
    }

    private void SceneActiveWhenSceneLoad(bool active)
    {
        titlePanel.SetActive(active);
        selectModePanel.SetActive(active);
    }


}
