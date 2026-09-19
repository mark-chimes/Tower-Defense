using UnityEngine;

// TODO find better name for this class
public class HexGodClass : MonoBehaviour
{
    [SerializeField] private HexAuthor gridAuthor;

    [SerializeField] private Mesh hexMesh;

    [SerializeField] private bool isWire;


    CameraControl camControl;

    void Awake()
    {
        camControl = new CameraControl();
        camControl.Initialize();
    }

    void Start()
    {
    }

    void Update()
    {
        camControl.ControlCamera();
    }

    void OnDrawGizmos()
    {
        HexGizmo.Draw(gridAuthor, hexMesh, transform,isWire);
    }
}
