using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Alternate Scene manager to display a loading bar between scenes
 * 
 * AUTHORS:
 *  - Daniel Quarrell
*/

public class SceneHandler : MonoSingleton<SceneHandler>
{
    public enum SceneID
    {
        MainMenu = 0,
        Gameplay = 1,
        LevelEditor = 2,
        AttractState = 3,
    }

    [SerializeField] float loadingTime = 0.0f;
    [SerializeField] string loadingSceneName = "LoadingScene";
    private string GameSceneName = "LevelGameplay";

    private string currentSceneName;
    private SceneID currentScene;
    private bool isLoadingScene = false;

    private CameraMovement.InputType inputType;

    private void Start()
    {
        inputType = (CameraMovement.InputType)PlayerPrefs.GetInt("InputType", 0);
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadScene(int sceneIndex)
    {
        if (!isLoadingScene)
        {
            IEnumerator loadAsync = LoadAsyncronously(sceneIndex);
            isLoadingScene = true;
            StartCoroutine(loadAsync);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (!isLoadingScene)
        {
            IEnumerator loadAsync = LoadAsyncronously(sceneName);
            isLoadingScene = true;
            StartCoroutine(loadAsync);
        }
    }

    public void LoadGameSceneWithGUID(int guid)
    {
        if (!isLoadingScene)
        {
            IEnumerator loadAsync = LoadGameSceneAsyncronously(guid);
            isLoadingScene = true;
            StartCoroutine(loadAsync);
        }
    }

    IEnumerator LoadAsyncronously(int sceneIndex)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(loadingSceneName);

        yield return new WaitWhile(() => loadOperation.isDone == false);

        yield return new WaitForSecondsRealtime(loadingTime);

        AsyncOperation sceneOperation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!sceneOperation.isDone)
        {
            isLoadingScene = false;
            yield return null;
        }

        isLoadingScene = false;
    }

    IEnumerator LoadAsyncronously(string sceneName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(loadingSceneName);

        yield return new WaitWhile(() => loadOperation.isDone == false);

        yield return new WaitForSecondsRealtime(loadingTime);

        AsyncOperation sceneOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!sceneOperation.isDone)
        {
            isLoadingScene = false;
            yield return null;
        }

        isLoadingScene = false;
    }

    IEnumerator LoadGameSceneAsyncronously(int guid)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(loadingSceneName);

        yield return new WaitWhile(() => loadOperation.isDone == false);

        yield return new WaitForSecondsRealtime(loadingTime);

        AsyncOperation sceneOperation = SceneManager.LoadSceneAsync(GameSceneName, LoadSceneMode.Additive);

        StartCoroutine(LevelManager.Instance.Load(guid));

        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(loadingSceneName);

        yield return new WaitWhile(() => loadOperation.isDone == false);
        isLoadingScene = false;
    }

    public string GetCurrentSceneName()
    {
        return currentSceneName;
    }

    public SceneID GetCurrentSceneID()
    {
        return currentScene;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        switch (currentSceneName)
        {
            case "LevelGameplay":
                currentScene = SceneID.Gameplay;
                break;
            case "LevelEditor":
                currentScene = SceneID.LevelEditor;
                break;
            case "MainMenu":
                currentScene = SceneID.MainMenu;
                break;
            case "AttractState":
                currentScene = SceneID.AttractState;
                break;
        }
    }

    public CameraMovement.InputType GetInputType()
    {
        return inputType;
    }

    public void SetInputType(CameraMovement.InputType _inputType)
    {
        inputType = _inputType;
        PlayerPrefs.SetInt("InputType", (int)inputType);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerPrefs.Save();
    }
}