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
        int boxBuffer = 10;
        int boxX = 10;
        int boxWidth = 210;
        int buttonX = boxX + boxBuffer;
        int buttonWidth = boxWidth - 2*boxBuffer;
        int buttonHeight = 20;

        int yBetweenButtons = buttonHeight;

        int numVisControls = 10;
        int guiVisY = yBetweenButtons;
        int guiVisHeight = (numVisControls+2) * yBetweenButtons  + boxBuffer;
        int guiVisEnd = guiVisY + guiVisHeight;

        int numBoatControls = 4;
        int guiBoatY = guiVisEnd + yBetweenButtons + boxBuffer;
        int guiBoatHeight = (numBoatControls+2) * yBetweenButtons + boxBuffer;
        int guiBoatEnd = guiBoatY + guiBoatHeight;

        int numSaveLoadControls = 2;
        int guiSaveLoadY = guiBoatEnd + yBetweenButtons + boxBuffer;
        int guiSaveLoadHeight = (numSaveLoadControls+2) * yBetweenButtons +  + boxBuffer;

        GUI.Box(new Rect(boxX, guiVisY, boxWidth, guiVisHeight), "VISUALIZE");
        guiVisY += yBetweenButtons + boxBuffer;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "Instant Refresh"))
        {
            Debug.Log("Refresh");
            godClass.OnRefreshPressed();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "Clear Field"))
        {
            Debug.Log("Clear Field");
            godClass.OnClearFieldPressed();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "Single Step"))
        {
            Debug.Log("Single Step");
            godClass.OnSingleStepPressed();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "VISUALIZE"))
        {
            Debug.Log("VISUALIZE");
            godClass.OnVisualizePressed();
        }
        guiVisY += yBetweenButtons;


        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "From-Start Mode (clears)"))
        {
            Debug.Log("From-Start Mode");
            godClass.OnFromStartModePressed();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "From-End Mode (clears)"))
        {
            Debug.Log("From-End Mode");
            godClass.OnFromEndModePressed();
        }
        guiVisY += yBetweenButtons + boxBuffer;

        autoRefreshEnabled = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), autoRefreshEnabled, "Auto-Refresh");
        if (wasEnabled != autoRefreshEnabled)
        {
            wasEnabled = autoRefreshEnabled;
            godClass.SetAutoRefreshMode(autoRefreshEnabled);
        }
        guiVisY += yBetweenButtons;

        visualizeDistanceEnabled = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), visualizeDistanceEnabled, "Distance Numbers");
        if (wasvisualizeDistanceEnabled != visualizeDistanceEnabled)
        {
            wasvisualizeDistanceEnabled = visualizeDistanceEnabled;
            godClass.SetNumbersVisible(visualizeDistanceEnabled);
        }
        guiVisY += yBetweenButtons;

        visualizePathfindingEnabled = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), visualizePathfindingEnabled, "Pathfinding Arrows");
        if (wasvisualizePathfindingEnabled != visualizePathfindingEnabled)
        {
            wasvisualizePathfindingEnabled = visualizePathfindingEnabled;
            godClass.SetVisualizationVisible(visualizePathfindingEnabled);
        }
        guiVisY += yBetweenButtons;

        stopPathingEarly = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), stopPathingEarly, "Stop pathfinding early");
        if (wasStopPathingEarly != stopPathingEarly)
        {
            wasStopPathingEarly = stopPathingEarly;
            godClass.SetPathfindingStopOnPathFound(stopPathingEarly);
        }
        
        
        // === //


        GUI.Box(new Rect(boxX, guiBoatY, boxWidth, guiBoatHeight), "BOATS");

        guiBoatY += yBetweenButtons + boxBuffer;

        if (GUI.Button(new Rect(buttonX, guiBoatY, buttonWidth, buttonHeight), "Spawn Boat"))
        {
            Debug.Log("Spawn boats pressed");
            godClass.OnSpawnBoatPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiBoatY, buttonWidth, buttonHeight), "Delete Boats"))
        {
            Debug.Log("Delete boats pressed");
            godClass.OnDeleteBoatsPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiBoatY, buttonWidth, buttonHeight), "Follow existing path"))
        {
            Debug.Log("Follow existing path pressed");
            godClass.OnBoatsFollowExistingPathPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiBoatY, buttonWidth, buttonHeight), "Stop boats"))
        {
            Debug.Log("Stop boats pressed");
            godClass.OnBoatsStopPressed();
        }

        // === //

        GUI.Box(new Rect(boxX, guiSaveLoadY, boxWidth, guiSaveLoadHeight), "SAVE/LOAD");
        guiSaveLoadY += yBetweenButtons + boxBuffer;

        if (GUI.Button(new Rect(buttonX, guiSaveLoadY, buttonWidth, buttonHeight), "Save"))
        {
            Debug.Log("Save pressed");
            // TODO
        }

        guiSaveLoadY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiSaveLoadY, buttonWidth, buttonHeight), "Load"))
        {
            Debug.Log("Load pressed");
            // TODO
        }

    }
}
