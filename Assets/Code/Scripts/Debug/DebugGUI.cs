using UnityEngine;

public class DebugGUI : MonoBehaviour
{

    [SerializeField] private GodClass visualizer;

    bool visualizeEnabled = false;
    bool wasEnabled = false;

    void OnGUI ()
    {
        GUI.Box(new Rect(10,10,140,150), "VISUALIZE");
    
        if(GUI.Button(new Rect(20,40,120,20), "Instant Refresh"))
        {
            Debug.Log("Refresh");
            visualizer.OnRefreshPressed();
        }

        if(GUI.Button(new Rect(20,65,80,20), "Clear Field"))
        {
            Debug.Log("Clear Field");
            visualizer.OnClearFieldPressed();        
        }

        if(GUI.Button(new Rect(20,90,80,20), "Single Step"))
        {
            Debug.Log("Single Step");
            visualizer.OnSingleStepPressed();        
        }

        visualizeEnabled = GUI.Toggle(new Rect(20,115,80,20), visualizeEnabled, "Visualize");
        if (wasEnabled != visualizeEnabled) { 
            wasEnabled = visualizeEnabled;
            visualizer.SetSlowPathfindingMode(visualizeEnabled);
        }
    }
}
