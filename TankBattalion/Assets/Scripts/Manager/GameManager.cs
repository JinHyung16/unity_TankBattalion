using Nakama;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HughGeneric;

public class GameManager : Singleton<GameManager>
{
    // Panel
    public GameObject titlePanel;
    public GameObject selectModePanel;

    public Button singlePlayBt;
    public Button multiPlayBt;

    private void Start()
    {
        singlePlayBt.onClick.AddListener(SinglePlayMode);
        multiPlayBt.onClick.AddListener(MultiPlayMode);
    }

    private void SinglePlayMode()
    {
        PanelActiveControlWhenMoveScene(false);
        HughSceneManager.GetInstace.LoadSinglePlayScene();
    }

    private void MultiPlayMode()
    {
        PanelActiveControlWhenMoveScene(false);
        HughSceneManager.GetInstace.LoadMultiPlayScene();
    }

    public void GoToMainScene()
    {
        HughSceneManager.GetInstace.LoadMainScene();

        PanelActiveControlWhenMoveScene(true);
    }

    private void PanelActiveControlWhenMoveScene(bool active)
    {
        titlePanel.SetActive(active);
        selectModePanel.SetActive(active);
    }
}
