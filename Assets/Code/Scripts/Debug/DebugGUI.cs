using UnityEngine;

public class DebugGUI : MonoBehaviour
{

    [SerializeField] private GodClass godClass; // TODO improve this data flow

    bool autoRefreshEnabled = false;
    bool wasEnabled = false;

    // TODO how can I get the debug GUI to read these values from somewhere and 
    // set up initial conditions easily so I don't have to unselect them every time? 

    bool visualizeDistanceEnabled = true;

    bool wasvisualizeDistanceEnabled = true;

    bool visualizePathfindingEnabled = true;

    bool wasvisualizePathfindingEnabled = true;

    bool stopPathingEarly = false;

    bool wasStopPathingEarly = false;

    void Start()
    {
        ReadValuesFromGodClass();
    }

    void ReadValuesFromGodClass()
    {
        GridView.VisualizationSettings visualization = godClass.StartingVisualization;
        visualizeDistanceEnabled = visualization.ShowDistance;
        wasvisualizeDistanceEnabled = visualizeDistanceEnabled;

        visualizePathfindingEnabled = visualization.ShowPathfinding;
        wasvisualizePathfindingEnabled = visualizePathfindingEnabled;

        stopPathingEarly = godClass.StartingIsStopOnPathFound;
    }


    void OnGUI()
    {
        int guiVisY = 10;
        int guiVisHeight = 300;
        int guiVisEnd = guiVisY + guiVisHeight;

        int guiBoatY = guiVisEnd + 10;
        int guiBoatHeight = 280;

        GUI.Box(new Rect(10, guiVisY, 210, guiVisHeight), "VISUALIZE");

        if (GUI.Button(new Rect(20, 40, 120, 20), "Instant Refresh"))
        {
            Debug.Log("Refresh");
            godClass.OnRefreshPressed();
        }

        if (GUI.Button(new Rect(20, 65, 80, 20), "Clear Field"))
        {
            Debug.Log("Clear Field");
            godClass.OnClearFieldPressed();
        }

        if (GUI.Button(new Rect(20, 90, 80, 20), "Single Step"))
        {
            Debug.Log("Single Step");
            godClass.OnSingleStepPressed();
        }

        if (GUI.Button(new Rect(20, 115, 80, 20), "VISUALIZE"))
        {
            Debug.Log("VISUALIZE");
            godClass.OnVisualizePressed();
        }

        autoRefreshEnabled = GUI.Toggle(new Rect(20, 140, 120, 20), autoRefreshEnabled, "Auto-Refresh");
        if (wasEnabled != autoRefreshEnabled)
        {
            wasEnabled = autoRefreshEnabled;
            godClass.SetAutoRefreshMode(autoRefreshEnabled);
        }

        if (GUI.Button(new Rect(20, 160, 160, 20), "From-Start Mode (clears)"))
        {
            Debug.Log("From-Start Mode");
            godClass.OnFromStartModePressed();
        }

        if (GUI.Button(new Rect(20, 180, 160, 20), "From-End Mode (clears)"))
        {
            Debug.Log("From-End Mode");
            godClass.OnFromEndModePressed();
        }

        visualizeDistanceEnabled = GUI.Toggle(new Rect(20, 220, 160, 20), visualizeDistanceEnabled, "Distance Numbers");
        if (wasvisualizeDistanceEnabled != visualizeDistanceEnabled)
        {
            wasvisualizeDistanceEnabled = visualizeDistanceEnabled;
            godClass.SetNumbersVisible(visualizeDistanceEnabled);
        }

        visualizePathfindingEnabled = GUI.Toggle(new Rect(20, 240, 160, 20), visualizePathfindingEnabled, "Pathfinding Arrows");
        if (wasvisualizePathfindingEnabled != visualizePathfindingEnabled)
        {
            wasvisualizePathfindingEnabled = visualizePathfindingEnabled;
            godClass.SetVisualizationVisible(visualizePathfindingEnabled);
        }

        stopPathingEarly = GUI.Toggle(new Rect(20, 260, 160, 20), stopPathingEarly, "Stop pathfinding early");
        if (wasStopPathingEarly != stopPathingEarly)
        {
            wasStopPathingEarly = stopPathingEarly;
            godClass.SetPathfindingStopOnPathFound(stopPathingEarly);
        }


        var yBetweenButtons = 20;

        GUI.Box(new Rect(10, guiBoatY, 210, guiBoatHeight), "BOATS");
        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(20, guiBoatY, 160, 20), "Spawn Boat"))
        {
            Debug.Log("Spawn boats pressed");
            godClass.OnSpawnBoatPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(20, guiBoatY, 160, 20), "Delete Boats"))
        {
            Debug.Log("Delete boats pressed");
            godClass.OnDeleteBoatsPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(20, guiBoatY, 160, 20), "Follow existing path"))
        {
            Debug.Log("Follow existing path pressed");
            godClass.OnBoatsFollowExistingPathPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(20, guiBoatY, 160, 20), "Stop boats"))
        {
            Debug.Log("Stop boats pressed");
            godClass.OnBoatsStopPressed();
        }



    }
}
