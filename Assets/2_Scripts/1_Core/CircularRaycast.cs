using UnityEngine;
using System.Collections.Generic;

public class CircularRaycast : MonoBehaviour
{
    public float radius = 5f;
    public int rayCount = 36;
    public float maxDistance = 10f;
    public LayerMask layerMask = Physics.DefaultRaycastLayers;

    private HNSGameplayManager gameplayManager;
    [SerializeField] private List<GameObject> hitObjects = new List<GameObject>();

    private void Start()
    {
        gameplayManager = FindObjectOfType<HNSGameplayManager>();
    }

    private void Update()
    {
        PerformCircularRaycast();
    }

    private void PerformCircularRaycast()
    {
        hitObjects.Clear();

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * 360f / rayCount;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Ray ray = new Ray(transform.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
            {
                if (!hitObjects.Contains(hit.collider.gameObject))
                {
                    hitObjects.Add(hit.collider.gameObject);
                }
            }
        }

        // Send hit objects to GameplayManager
        if (gameplayManager != null)
        {
            gameplayManager.HandleRaycastHits(hitObjects);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        DrawCircle(transform.position, radius, rayCount);

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * 360f / rayCount;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Ray ray = new Ray(transform.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, hit.point);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, direction * maxDistance);
            }
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + Vector3.forward * radius;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector3 newPoint = center + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }
}