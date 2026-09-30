using UnityEngine;

public class TowerController : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefab;

    private TreasureMap treasureMap;

    private bool isInitialized;

    public void Initialize(TreasureMap treasureMap)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        this.treasureMap = treasureMap;
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        foreach (Transform child in transform) Destroy(child.gameObject);
        treasureMap = null;
    }

}
