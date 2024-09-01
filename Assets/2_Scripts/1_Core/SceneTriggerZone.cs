using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTriggerZone : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private List<GameObject> objectsInside;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (objectsInside.Count > 0)
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!objectsInside.Contains(other.gameObject))
            {
                objectsInside.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objectsInside.Remove(other.gameObject);
        }
    }
}