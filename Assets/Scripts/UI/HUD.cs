using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD instance = null;

    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarPrefab = null;
    
    [Header("Game Progress")]
    [SerializeField] private Slider blueTeamProgress = null;
    [SerializeField] private Text blueTeamProgressLabel = null;
    [SerializeField] private Slider redTeamProgress = null;
    [SerializeField] private Text redTeamProgressLabel = null;
    [SerializeField] private Text timer = null;

    [Header("Menus")]
    [SerializeField] private CanvasGroup pauseMenu = null;
    [SerializeField] private CanvasGroup endGameMenu = null;
    [SerializeField] private Text endGameStatus = null;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (GameController.instance.gameEnded) return;

        // Poll user input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Update timer
        var secs = Time.timeSinceLevelLoad % 60;
        var mins = Time.timeSinceLevelLoad / 60;
        timer.text = string.Format("{0:0}:{1:00}", mins, secs);
    }

    public void TogglePause()
    {
        ToggleScreen(pauseMenu);
    }

    public void LoadScene(int sceneIndex)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public HealthBar CreateHealthBar(UnitController unit)
    {
        var instance = Instantiate(
            healthBarPrefab,
            unit.transform.position,
            healthBarPrefab.transform.rotation);
        var healthBar = instance.GetComponent<HealthBar>();
        healthBar.transform.SetParent(transform.GetChild(0));
        healthBar.ConnectTo(unit);
        return healthBar;
    }

    public void SetProgress(float blue, float red)
    {
        blueTeamProgress.value = Mathf.Min(blue, 1);
        blueTeamProgressLabel.text = string.Format("Blue team progress ({0}%)", Mathf.Round(blue * 100));
        redTeamProgress.value = Mathf.Min(red, 1);
        redTeamProgressLabel.text = string.Format("Red team progress ({0}%)", Mathf.Round(red * 100));
    }

    public void EndGame(bool victory)
    {
        endGameStatus.text = victory? "Victory!" : "Defeat!";
        ToggleScreen(endGameMenu);
    }

    private void ToggleScreen(CanvasGroup screen)
    {
        var shown = !(screen.alpha == 0);
        shown = !shown;
        screen.alpha = shown? 1 : 0;
        screen.interactable = shown;
        screen.blocksRaycasts = shown;
        Time.timeScale = shown? 0 : 1;
    }
}
