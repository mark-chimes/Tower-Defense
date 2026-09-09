using UnityEngine;

public class DebugGUI : MonoBehaviour
{

    [SerializeField] private GodClass visualizer;

    bool autoRefreshEnabled = false;
    bool wasEnabled = false;

    void OnGUI ()
    {
        GUI.Box(new Rect(10,10,170,150), "VISUALIZE");
    
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

        if(GUI.Button(new Rect(20,115,80,20), "VISUALIZE"))
        {
            Debug.Log("VISUALIZE");
            visualizer.OnVisualizePressed();        
        }
        
        autoRefreshEnabled = GUI.Toggle(new Rect(20,140,120,20), autoRefreshEnabled, "Auto-Refresh");
        if (wasEnabled != autoRefreshEnabled) { 
            wasEnabled = autoRefreshEnabled;
            visualizer.SetAutoRefreshMode(autoRefreshEnabled);
        }
    }
}
