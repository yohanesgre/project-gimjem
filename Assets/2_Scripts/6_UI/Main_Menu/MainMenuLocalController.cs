using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLocalController : MonoBehaviour
{
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button playOfflineButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button creditsPanel;

    private void OnEnable()
    {
        creditsButton.onClick.AddListener(OnClickCreditsButton);
        playOfflineButton.onClick.AddListener(OnClickPlayOfflineButton);
        exitButton.onClick.AddListener(OnClickExitButton);
        creditsPanel.onClick.AddListener(OnClickCreditsPanel);
        HideCreditsPanel();
    }

    private void OnDisable()
    {
        creditsButton.onClick.RemoveListener(OnClickCreditsButton);
        playOfflineButton.onClick.RemoveListener(OnClickPlayOfflineButton);
        exitButton.onClick.RemoveListener(OnClickExitButton);
        creditsPanel.onClick.RemoveListener(OnClickCreditsPanel);
    }

    private void OnClickCreditsButton()
    {
        ShowCreditsPanel();
    }

    private void OnClickPlayOfflineButton()
    {
        SceneManager.LoadScene("Carnival");
    }

    private void OnClickExitButton()
    {
        Application.Quit();
    }

    private void ShowCreditsPanel()
    {
        creditsPanel.gameObject.SetActive(true);
    }

    private void HideCreditsPanel()
    {
        creditsPanel.gameObject.SetActive(false);
    }

    private void OnClickCreditsPanel()
    {
        HideCreditsPanel();
    }
}
