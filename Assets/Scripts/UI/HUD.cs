using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD instance = null;

    [SerializeField] private GameObject healthBarPrefab = null;
    [SerializeField] private Slider blueTeamProgress = null;
    [SerializeField] private Text blueTeamProgressLabel = null;
    [SerializeField] private Slider redTeamProgress = null;
    [SerializeField] private Text redTeamProgressLabel = null;
    [SerializeField] private Text timer = null;
    [SerializeField] private CanvasGroup pauseMenu = null;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
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
        TogglePause();
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
        healthBar.transform.SetParent(transform);
        healthBar.ConnectTo(unit);
        return healthBar;
    }

    public void SetProgress(float blue, float red)
    {
        blueTeamProgress.value = blue;
        blueTeamProgressLabel.text = string.Format("Blue team progress ({0}%)", Mathf.Round(blue * 100));
        redTeamProgress.value = red;
        redTeamProgressLabel.text = string.Format("Red team progress ({0}%)", Mathf.Round(red * 100));
    }

    private void ToggleScreen(CanvasGroup screen)
    {
        var shown = !(screen.alpha == 0);
        shown = !shown;
        screen.alpha = shown? 1 : 0;
        screen.interactable = shown;
        Time.timeScale = shown? 0 : 1;
    }
}
