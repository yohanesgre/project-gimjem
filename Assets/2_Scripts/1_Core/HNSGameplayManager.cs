using UnityEngine;
using System.Collections.Generic;
using GimJem.Utilities;

public class HNSGameplayManager : MonoBehaviour
{
    [SerializeField, ReadOnly] private List<GameObject> trees = new();
    [SerializeField] private Transform treeGroup;
    [SerializeField, ReadOnly] private bool isInsideTreeTriggerArea;
    private System.Random rnd;

    private void Awake()
    {
        rnd = new System.Random();
    }

    private void Start()
    {
        RegisterTrees();
    }

    private void Update()
    {
        ListenForNumericInput();
    }

    private void RegisterTrees()
    {
        foreach (Transform child in treeGroup)
        {
            trees.Add(child.gameObject);
            child.gameObject.GetComponent<HNSTreeController>().SetTreeNumber(rnd.Next(0, 9).ToString());
        }
    }

    private void ListenForNumericInput()
    {
        if (!isInsideTreeTriggerArea) return;

        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                HandleNumericInput(i);
            }
        }
    }

    private void HandleNumericInput(int number)
    {
        Debug.Log($"Numeric input: {number}");
        // Add your game logic for numeric input here
    }

    public void HandleRaycastHits(List<GameObject> hitObjects)
    {
        foreach (GameObject obj in hitObjects)
        {
            if (obj.CompareTag("Player"))
            {
                isInsideTreeTriggerArea = false;
                return;
            }
        }

        isInsideTreeTriggerArea = hitObjects.Count > 0 && !hitObjects.Exists(obj => obj.CompareTag("Player"));
    }

    private void OnValidate()
    {
        if (treeGroup == null)
        {
            throw new System.Exception("Tree Group is not set");
        }
    }
}