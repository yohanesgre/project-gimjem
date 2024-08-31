using TMPro;
using UnityEngine;

public class HNSTreeController : MonoBehaviour
{
    [SerializeField] private string treeNumber;
    [SerializeField] private TMP_Text treeNumberText;

    private void Start()
    {
        treeNumberText.text = treeNumber;
    }

    public string GetTreeNumber()
    {
        return treeNumber;
    }

    public void SetTreeNumber(string number)
    {
        treeNumber = number;
        treeNumberText.text = treeNumber;
    }

    public void DestroyTree()
    {
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (treeNumberText == null)
        {
            treeNumberText = GetComponentInChildren<TMP_Text>();
        }
    }
}