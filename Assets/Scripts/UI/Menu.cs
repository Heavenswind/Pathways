using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private CanvasGroup menuScreen = null;
    [SerializeField] private CanvasGroup loadingScreen = null;

    public void StartGame()
    {
        StartCoroutine(LoadGameScene());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator LoadGameScene()
    {
        DontDestroyOnLoad(gameObject);
        menuScreen.interactable = false;
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.alpha = 1;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        yield return new WaitUntil(() => asyncLoad.isDone);
        loadingScreen.alpha = 0;
        Destroy(gameObject);
    }
}
