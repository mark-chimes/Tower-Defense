using UnityEngine;

public class DebugGUI : MonoBehaviour
{

    [SerializeField] private GodClass visualizer;

    bool visualizeEnabled = false;
    bool wasEnabled = false;

    void OnGUI ()
    {
        GUI.Box(new Rect(10,10,140,110), "VISUALIZE");
    
        if(GUI.Button(new Rect(20,40,120,20), "Instant Refresh"))
        {
            Debug.Log("Refresh");
            visualizer.OnRefreshPressed();
        }

        // if(GUI.Button(new Rect(20,65,80,20), "Button"))
        // {
        //     Debug.Log("Button");
        // }

        visualizeEnabled = GUI.Toggle(new Rect(20,90,80,20), visualizeEnabled, "Visualize");
        if (wasEnabled != visualizeEnabled) { 
            wasEnabled = visualizeEnabled;
            visualizer.SetSlowPathfindingMode(visualizeEnabled);
        }
    }
}
