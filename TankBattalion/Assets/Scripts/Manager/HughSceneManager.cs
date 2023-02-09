using HughGeneric;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HughSceneManager : LazySingleton<HughSceneManager>
{
    public string GetActiveSceneName()
    {
        string curSceneName = SceneManager.GetActiveScene().name;
        return curSceneName;
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }

    public void LoadSinglePlayScene()
    {
        SceneManager.LoadScene("SinglePlay");
    }

    public void LoadMultiPlayScene()
    {
        SceneManager.LoadScene("MultiPlay");
    }
}
