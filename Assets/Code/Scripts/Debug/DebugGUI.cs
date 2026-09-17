using UnityEngine;

public class DebugGUI : MonoBehaviour
{

    private GridPathfindingIOManager pathfindingIOManager;
    private EnemyController enemyController;    // TODO Cross-coupling - move all relevant methods to own class
    private LevelSaveLoadSystem saveLoadSystem;
    private GridWalls gridWalls;

    bool autoRefreshEnabled = false;
    bool wasEnabled = false;

    bool visualizeDistanceEnabled = true;
    bool wasvisualizeDistanceEnabled = true;
    bool visualizePathfindingEnabled = true;
    bool wasvisualizePathfindingEnabled = true;
    bool stopPathingEarly = false;
    bool wasStopPathingEarly = false;

    public void Initialize(GridView.VisualizationSettings visualization, 
        bool stopPathingEarly,
        GridPathfindingIOManager pathfindingIOManager, 
        EnemyController enemyController, // TODO remove reference
        LevelSaveLoadSystem saveLoadSystem,
        GridWalls gridWalls)
    {
        visualizeDistanceEnabled = visualization.ShowDistance;
        wasvisualizeDistanceEnabled = visualizeDistanceEnabled;

        visualizePathfindingEnabled = visualization.ShowPathfinding;
        wasvisualizePathfindingEnabled = visualizePathfindingEnabled;

        this.stopPathingEarly = stopPathingEarly;
        
        this.pathfindingIOManager = pathfindingIOManager;
        this.enemyController = enemyController;
        this.saveLoadSystem = saveLoadSystem;
        this.gridWalls = gridWalls;
    }




    void OnGUI()
    {
        int boxBuffer = 10;
        int boxX = 10;
        int boxWidth = 210;
        int buttonX = boxX + boxBuffer;
        int buttonWidth = boxWidth - 2 * boxBuffer;
        int buttonHeight = 20;

        int yBetweenButtons = buttonHeight;

        int numVisControls = 10;
        int guiVisY = yBetweenButtons;
        int guiVisHeight = (numVisControls + 2) * yBetweenButtons + boxBuffer;
        int guiVisEnd = guiVisY + guiVisHeight;

        int numBoatControls = 4;
        int guiBoatY = guiVisEnd + yBetweenButtons + boxBuffer;
        int guiBoatHeight = (numBoatControls + 2) * yBetweenButtons + boxBuffer;
        int guiBoatEnd = guiBoatY + guiBoatHeight;

        int numSaveLoadControls = 2;
        int guiSaveLoadY = guiBoatEnd + yBetweenButtons + boxBuffer;
        int guiSaveLoadHeight = (numSaveLoadControls + 2) * yBetweenButtons + boxBuffer;
        int guiSaveLoadEnd = guiSaveLoadY + guiSaveLoadHeight;

        int numBuildingControls = 2;
        int guiBuildingY = guiSaveLoadEnd + yBetweenButtons + boxBuffer;
        int guiBuildingHeight = (numBuildingControls + 2) * yBetweenButtons + boxBuffer;
        int guiBuildingEnd = guiBuildingY + guiBuildingHeight;

        GUI.Box(new Rect(boxX, guiVisY, boxWidth, guiVisHeight), "VISUALIZE");
        guiVisY += yBetweenButtons + boxBuffer;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "Instant Refresh"))
        {
            Debug.Log("Refresh");
            pathfindingIOManager.OnRefreshPressed();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "Clear Field"))
        {
            Debug.Log("Clear Field");
            pathfindingIOManager.OnClearFieldPressed();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "Single Step"))
        {
            Debug.Log("Single Step");
            pathfindingIOManager.OnSingleStepPressed();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "VISUALIZE"))
        {
            Debug.Log("VISUALIZE");
            pathfindingIOManager.OnVisualizePressed();
        }
        guiVisY += yBetweenButtons;


        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "From-Start Mode (clears)"))
        {
            Debug.Log("From-Start Mode");
            pathfindingIOManager.OnFromStartModePressed();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "From-End Mode (clears)"))
        {
            Debug.Log("From-End Mode");
            pathfindingIOManager.OnFromEndModePressed();
        }
        guiVisY += yBetweenButtons + boxBuffer;

        autoRefreshEnabled = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), autoRefreshEnabled, "Auto-Refresh");
        if (wasEnabled != autoRefreshEnabled)
        {
            wasEnabled = autoRefreshEnabled;
            pathfindingIOManager.SetAutoRefreshMode(autoRefreshEnabled);
        }
        guiVisY += yBetweenButtons;

        visualizeDistanceEnabled = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), visualizeDistanceEnabled, "Distance Numbers");
        if (wasvisualizeDistanceEnabled != visualizeDistanceEnabled)
        {
            wasvisualizeDistanceEnabled = visualizeDistanceEnabled;
            pathfindingIOManager.OnSetNumbersVisible(visualizeDistanceEnabled);
        }
        guiVisY += yBetweenButtons;

        visualizePathfindingEnabled = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), visualizePathfindingEnabled, "Pathfinding Arrows");
        if (wasvisualizePathfindingEnabled != visualizePathfindingEnabled)
        {
            wasvisualizePathfindingEnabled = visualizePathfindingEnabled;
            pathfindingIOManager.OnSetVisualizationVisible(visualizePathfindingEnabled);
        }
        guiVisY += yBetweenButtons;

        stopPathingEarly = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), stopPathingEarly, "Stop pathfinding early");
        if (wasStopPathingEarly != stopPathingEarly)
        {
            wasStopPathingEarly = stopPathingEarly;
            pathfindingIOManager.ResetPathfindingWithEarlyStoppingMode(stopPathingEarly);
        }


        // === //


        GUI.Box(new Rect(boxX, guiBoatY, boxWidth, guiBoatHeight), "BOATS");

        guiBoatY += yBetweenButtons + boxBuffer;

        if (GUI.Button(new Rect(buttonX, guiBoatY, buttonWidth, buttonHeight), "Spawn Boat"))
        {
            Debug.Log("Spawn boats pressed");
            enemyController.OnSpawnBoatPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiBoatY, buttonWidth, buttonHeight), "Delete Boats"))
        {
            Debug.Log("Delete boats pressed");
            enemyController.OnDeleteBoatsPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiBoatY, buttonWidth, buttonHeight), "Follow existing path"))
        {
            Debug.Log("Follow existing path pressed");
            enemyController.OnBoatsFollowExistingPathPressed();
        }

        guiBoatY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiBoatY, buttonWidth, buttonHeight), "Stop boats"))
        {
            Debug.Log("Stop boats pressed");
            enemyController.OnBoatsStopPressed();
        }

        // === //

        GUI.Box(new Rect(boxX, guiSaveLoadY, boxWidth, guiSaveLoadHeight), "SAVE/LOAD");
        guiSaveLoadY += yBetweenButtons + boxBuffer;

        if (GUI.Button(new Rect(buttonX, guiSaveLoadY, buttonWidth, buttonHeight), "Save"))
        {
            Debug.Log("Save pressed");
            saveLoadSystem.OnSave();
        }

        guiSaveLoadY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiSaveLoadY, buttonWidth, buttonHeight), "Load"))
        {
            Debug.Log("Load pressed");
            saveLoadSystem.OnLoad();
        }

        // === //

        GUI.Box(new Rect(boxX, guiBuildingY, boxWidth, guiBuildingHeight), "Building");
        guiBuildingY += yBetweenButtons + boxBuffer;

        if (GUI.Button(new Rect(buttonX, guiBuildingY, buttonWidth, buttonHeight), "Wall"))
        {
            Debug.Log("Build wall pressed");
            gridWalls.OnBuildWallsMode();
        }

        guiBuildingY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiBuildingY, buttonWidth, buttonHeight), "Tower"))
        {
            Debug.Log("Build tower pressed");
            gridWalls.OnBuildTowersMode();
        }

    }
}
