using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public Slider healthBar;
    public TMP_Text winText;

    public Color healthHigh;
    public Color healthMed;
    public Color healthLow;

    public Button playButton;
    public Button quitButton;
    public Button instructionsButton;
    public Button exitInstructionsButton;
    public Button resumeButton;
    public Button menuButton;

    public GameObject instructionsMenu;
    public GameObject pauseMenu;

    readonly float medHP = Mathf.Ceil(Player.maxHealth / 2);
    readonly float lowHP = Mathf.Ceil(Player.maxHealth / 4);

    public float HealthPercent
    {
        get { return healthBar.value; }

        set 
        { 
            if (value < lowHP)
                healthBar.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = healthLow;
            else if (value < medHP)
                healthBar.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = healthMed;
            else
                healthBar.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = healthHigh;

            healthBar.value = value;
        }
    }

    private void Awake()
    {
        Screen.fullScreen = true;

        if (SceneManager.GetActiveScene().buildIndex == 1)
            Cursor.visible = false;
        else
            Cursor.visible = true;
    }

    void Start()
    {
        if (playButton)
            playButton.onClick.AddListener(PlayGame);

        if (quitButton)
            quitButton.onClick.AddListener(QuitGame);

        if (instructionsButton)
            instructionsButton.onClick.AddListener(OpenInstructionsMenu);

        if (exitInstructionsButton)
            exitInstructionsButton.onClick.AddListener(CloseInstructionsMenu);

        if (menuButton)
            menuButton.onClick.AddListener(ReturnToMenu);

        if (resumeButton)
            resumeButton.onClick.AddListener(ResumeGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && pauseMenu)
        {
            if (Time.timeScale > 0)
                PauseGame();
            else
                ResumeGame();
        }
    }

    void PlayGame()
    {
        if (Time.timeScale <= 0)
            ResumeGame();

        SceneManager.LoadScene(1);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PauseGame(bool wonGame = false)
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;
        if (wonGame)
        {
            winText.gameObject.SetActive(true);
            resumeButton.gameObject.SetActive(false);
        }
    }

    void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
    }

    void OpenInstructionsMenu()
    {
        instructionsMenu.SetActive(true);
    }

    void CloseInstructionsMenu()
    {
        instructionsMenu.SetActive(false);
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
