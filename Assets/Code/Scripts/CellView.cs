using UnityEngine;


public class CellView : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMPro.TextMeshPro numberLabel;

    private CellCoord coord;

    public void Initialize(CellCoord coord)
    {
        this.coord = coord;
        numberLabel.text = coord.ToStringNumbersOnly();
    }

}
