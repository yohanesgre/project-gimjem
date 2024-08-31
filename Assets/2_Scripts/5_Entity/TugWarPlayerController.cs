using UnityEngine;
using System;

public class TugWarPlayerController : MonoBehaviour
{
    [SerializeField] private float stepDistance = 1f;
    private Vector3 finishLine;
    private bool hasFinished = false;
    public bool IsFinished => hasFinished;
    private int playerIndex;

    private TugWarGameplayManager gameplayManager;

    public event Action<int> OnPlayerFinished;

    public void Initialize(TugWarGameplayManager manager, Vector3 finish, int index)
    {
        gameplayManager = manager;
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
        Vector3 directionToFinish = (finishLine - transform.position).normalized;
        Vector3 newPosition = transform.position + directionToFinish * stepDistance;

        // Ensure the new position doesn't overshoot the finish line
        if (Vector3.Distance(newPosition, finishLine) > Vector3.Distance(transform.position, finishLine))
        {
            newPosition = finishLine;
        }

        transform.position = newPosition;

        if (Vector3.Distance(transform.position, finishLine) < 0.01f)
        {
            hasFinished = true;
            OnPlayerFinished?.Invoke(playerIndex);
            Debug.Log($"Player {gameObject.name} reached the finish line!");
        }
    }

    public bool HasFinished()
    {
        return hasFinished;
    }
}