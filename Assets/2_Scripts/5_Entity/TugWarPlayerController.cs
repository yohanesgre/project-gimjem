using UnityEngine;
using System;

public class TugWarPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform finishLine;
    [SerializeField] private bool isFinished = false;
    private Vector3 centerPoint;

    public bool IsFinished => isFinished;

    public void Initialize(Vector3 center, Transform finishLine)
    {
        UpdateCenterPoint(center);
        this.finishLine = finishLine;
    }

    public void UpdateCenterPoint(Vector3 newCenter)
    {
        centerPoint = new Vector3(newCenter.x, transform.position.y, newCenter.z);
    }

    private void Update()
    {
        if (!IsAtCenter())
        {
            MoveTowardsCenter();
        }
    }

    private void MoveTowardsCenter()
    {
        Vector3 targetPosition = new Vector3(centerPoint.x, transform.position.y, centerPoint.z);
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.position = newPosition;
    }

    private bool IsAtCenter()
    {
        Vector2 currentPositionXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 centerPointXZ = new Vector2(centerPoint.x, centerPoint.z);
        return Vector2.Distance(currentPositionXZ, centerPointXZ) < 0.01f;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 GetCenterPoint()
    {
        return centerPoint;
    }

    public void SetFinished()
    {
        isFinished = true;
    }

    public Transform GetFinishLine()
    {
        return finishLine;
    }
}