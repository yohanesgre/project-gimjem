using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BakiakGameplayUIManager : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TMP_Text[] playerNameText;

    private CameraManager cameraManager;
    private BakiakGameplayManager gameplayManager;

    public void Awake()
    {
        winPanel.gameObject.SetActive(false);
        losePanel.gameObject.SetActive(false);
    }

    void Start()
    {
        // Find and store the CameraManager reference
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager != null)
        {
            cameraManager.OnCameraSwitch += OnCameraSwitch;
        }
        else
        {
            Debug.LogWarning("CameraManager not found in the scene.");
        }

        // Find and store the BakiakGameplayManager reference
        gameplayManager = FindObjectOfType<BakiakGameplayManager>();
        if (gameplayManager == null)
        {
            Debug.LogWarning("BakiakGameplayManager not found in the scene.");
        }

        gameplayManager.OnGameFinished += OnGameFinished;
    }

    private void OnDestroy()
    {
        gameplayManager.OnGameFinished -= OnGameFinished;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (winPanel.gameObject.activeSelf)
            {
                HideWinPanel();
                SceneManager.LoadScene("Carnival");
            }
            else if (losePanel.gameObject.activeSelf)
            {
                HideLosePanel();
                SceneManager.LoadScene("Carnival");
            }
        }
    }

    private void OnGameFinished(int playerIndex)
    {
        ShowWinPanel(playerIndex);
    }

    public void ShowWinPanel(int playerIndex)
    {
        winPanel.gameObject.SetActive(true);
        foreach (var e in playerNameText)
        {
            e.text = "Player " + (playerIndex + 1);
        }
    }

    public void ShowLosePanel()
    {
        losePanel.gameObject.SetActive(true);
    }

    public void HideWinPanel()
    {
        winPanel.gameObject.SetActive(false);
    }

    public void HideLosePanel()
    {
        losePanel.gameObject.SetActive(false);
    }


    private void OnCameraSwitch(Camera newCamera)
    {
        GetComponent<Canvas>().worldCamera = newCamera;
    }

}
