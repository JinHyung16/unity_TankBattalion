using HughSingleTon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HughSceneManager : LazySingleton<HughSceneManager>
{
    public string GetActiveSceneName()
    {
        string curSceneName = SceneManager.GetActiveScene().name;
#if UNITY_EDITOR
        Debug.Log("<color=green><b>[HughSceneManager]</b> Get Current Scene - name : {0} </color>" + curSceneName);
#endif
        return curSceneName;
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
#if UNITY_EDITOR
        Debug.Log("<color=green><b>[HughSceneManager]</b> Load Scene - Main : {0} </color>");
#endif
    }

    public void LoadSinglePlayScene()
    {
        SceneManager.LoadScene("SinglePlay");
#if UNITY_EDITOR
        Debug.Log("<color=green><b>[HughSceneManager]</b> Load Scene - Single Play : {0} </color>");
#endif
    }

    public void LoadMultiPlayScene()
    {
        SceneManager.LoadScene("MultiPlay");
#if UNITY_EDITOR
        Debug.Log("<color=green><b>[HughSceneManager]</b> Load Scene - Multi Play : {0} </color>");
#endif
    }
}
