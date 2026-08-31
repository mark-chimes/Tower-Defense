using UnityEngine;


public class CellView : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMPro.TextMeshPro numberLabel;

    private int x,z;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void Initialize(int x, int z)
    {
        this.x = x;
        this.z = z;
        numberLabel.text = $"{x},{z}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
