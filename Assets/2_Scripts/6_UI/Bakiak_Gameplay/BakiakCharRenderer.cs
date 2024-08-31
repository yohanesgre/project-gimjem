using System;
using TMPro;
using UnityEngine;

public class BakiakCharRenderer : MonoBehaviour
{
    public TMP_Text displayText;
    [SerializeField] private char currentChar;
    private System.Random rnd = new System.Random();

    private CameraManager cameraManager;
    private BakiakGameplayManager gameplayManager;
    private bool isInputEnabled = true;

    void Start()
    {
        GenerateNewCharacter();

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
    }

    void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed
        if (cameraManager != null)
        {
            cameraManager.OnCameraSwitch -= OnCameraSwitch;
        }
    }

    void Update()
    {
        if (isInputEnabled && Input.anyKeyDown)
        {
            string input = Input.inputString.ToUpper();
            if (input.Length > 0)
            {
                CheckInput(input[0]);
            }
        }
    }

    void GenerateNewCharacter()
    {
        currentChar = (char)rnd.Next(65, 91); // ASCII values for A-Z
        displayText.text = currentChar.ToString();
    }

    void CheckInput(char input)
    {
        if (input == currentChar)
        {
            if (gameplayManager != null)
            {
                gameplayManager.HandleCorrectInput(currentChar);

                if (!gameplayManager.IsAnyPlayerFinished())
                {
                    GenerateNewCharacter();
                }
                else
                {
                    // Handle game end scenario
                    // For example: disable input, show game over UI, etc.  
                    isInputEnabled = false;
                }
            }
        }
    }

    private void OnCameraSwitch(Camera newCamera)
    {
        GetComponent<Canvas>().worldCamera = newCamera;
    }
}