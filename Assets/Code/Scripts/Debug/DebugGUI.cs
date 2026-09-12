using UnityEngine;

public class DebugGUI : MonoBehaviour
{

    [SerializeField] private GodClass godClass; // TODO improve this data flow

    bool autoRefreshEnabled = false;
    bool wasEnabled = false;

    void OnGUI ()
    {
        GUI.Box(new Rect(10,10,210,200), "VISUALIZE");
    
        if(GUI.Button(new Rect(20,40,120,20), "Instant Refresh"))
        {
            Debug.Log("Refresh");
            godClass.PathfindingManager.OnRefreshPressed();
        }

        if(GUI.Button(new Rect(20,65,80,20), "Clear Field"))
        {
            Debug.Log("Clear Field");
            godClass.PathfindingManager.OnClearFieldPressed();        
        }

        if(GUI.Button(new Rect(20,90,80,20), "Single Step"))
        {
            Debug.Log("Single Step");
            godClass.PathfindingManager.OnSingleStepPressed();        
        }

        if(GUI.Button(new Rect(20,115,80,20), "VISUALIZE"))
        {
            Debug.Log("VISUALIZE");
            godClass.PathfindingManager.OnVisualizePressed();        
        }
        
        autoRefreshEnabled = GUI.Toggle(new Rect(20,140,120,20), autoRefreshEnabled, "Auto-Refresh");
        if (wasEnabled != autoRefreshEnabled) { 
            wasEnabled = autoRefreshEnabled;
            godClass.PathfindingManager.SetAutoRefreshMode(autoRefreshEnabled);
        }

        if(GUI.Button(new Rect(20,160,160,20), "From-Start Mode (clears)"))
        {
            Debug.Log("From-Start Mode");
            godClass.PathfindingManager.OnFromStartModePressed();        
        }

        if(GUI.Button(new Rect(20,180,160,20), "From-End Mode (clears)"))
        {
            Debug.Log("From-End Mode");
            godClass.PathfindingManager.OnFromEndModePressed();        
        }
    }
}
