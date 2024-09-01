using UnityEngine;
using System;
using System.Collections;

public class BakiakPlayerController : MonoBehaviour
{
    [SerializeField] private float stepDistance = 1f;
    [SerializeField] private float rotationSpeed = 5f; // New field for rotation speed
    private Vector3 startingPosition;
    private Vector3 finishLine;
    private bool hasFinished = false;
    public bool IsFinished => hasFinished;
    private int playerIndex;
    private int playerScore = 0;
    public int PlayerScore => playerScore;
    private BakiakGameplayManager gameplayManager;

    public void Initialize(BakiakGameplayManager manager, Vector3 start, Vector3 finish, int index)
    {
        gameplayManager = manager;
        startingPosition = start;
        finishLine = finish;
        playerIndex = index;

        if (gameplayManager != null)
        {
            gameplayManager.OnCorrectInput += OnCorrectInput;
        }
        else
        {
            Debug.LogError("GameplayManager is null!");
        }

        // Initial rotation towards finish line
        RotateTowardsFinishLine();
    }

    public void ResetPlayer()
    {
        hasFinished = false;
        transform.position = startingPosition;
        RotateTowardsFinishLine(); // Reset rotation when resetting player
    }

    private void OnDestroy()
    {
        if (gameplayManager != null)
        {
            gameplayManager.OnCorrectInput -= OnCorrectInput;
        }
    }

    private void OnCorrectInput(int index, char correctChar)
    {
        if (index == playerIndex && !hasFinished)
        {
            MoveStep();
        }
    }

    private void MoveStep()
    {
        StartCoroutine(IE_MoveStep());
        RotateTowardsFinishLine();

        if (Vector3.Distance(transform.position, finishLine) < 0.01f)
        {
            hasFinished = true;
            playerScore++;
            Debug.Log($"Player {gameObject.name} reached the finish line!");
        }
    }

    private IEnumerator IE_MoveStep()
    {
        Vector3 directionToFinish = (finishLine - transform.position).normalized;
        Vector3 newPosition = transform.position + directionToFinish * stepDistance;

        while (Vector3.Distance(transform.position, newPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, newPosition, stepDistance * Time.deltaTime);
            yield return null;
        }
    }

    private void RotateTowardsFinishLine()
    {
        StartCoroutine(IE_RotateTowradsFinishLine());
    }

    private IEnumerator IE_RotateTowradsFinishLine()
    {
        Vector3 directionToFinish = (finishLine - transform.position).normalized;
        directionToFinish.y = 0; // Ensure rotation is only on Y-axis

        while (directionToFinish != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToFinish);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public bool HasFinished()
    {
        return hasFinished;
    }
}