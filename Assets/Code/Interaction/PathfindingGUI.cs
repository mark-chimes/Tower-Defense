using UnityEngine;

public class PathfindingGUI : MonoBehaviour
{

    private FlowBridge flowBridge;
    private EnemyController enemyController;
    private BoardInput boardInput;
    private TowerController towerController;


    private GameMaster gameMaster; // // TODO Cross-dependency code smell not ideal, but I'll fix this later

    bool visualizeDistanceEnabled = true;
    bool wasVisualizeDistanceEnabled = true;
    bool visualizePathfindingEnabled = true;
    bool wasVisualizePathfindingEnabled = true;
    bool stopPathingEarly = false;
    bool wasStopPathingEarly = false;

    public void Initialize(TerrainWithOverlay.VisualizationSettings visualization,
        bool stopPathingEarly,
        BoardInput boardInput,
        FlowBridge flowBridge,
        EnemyController enemyController, // TODO remove reference
        TowerController towerController,
        GameMaster gameMaster
        )
    {
        visualizeDistanceEnabled = visualization.ShowDistance;
        wasVisualizeDistanceEnabled = visualizeDistanceEnabled;

        visualizePathfindingEnabled = visualization.ShowPathfinding;
        wasVisualizePathfindingEnabled = visualizePathfindingEnabled;

        this.stopPathingEarly = stopPathingEarly;

        this.flowBridge = flowBridge;
        this.enemyController = enemyController;
        this.boardInput = boardInput;
        this.towerController = towerController;
        this.gameMaster = gameMaster; // TODO Cross-dependency code smell 
    }




    void OnGUI()
    {
        int boxBuffer = 10;
        int leftBoxX = 10;
        int boxWidth = 210;
        int leftButtonX = leftBoxX + boxBuffer;
        int rightBoxX = Screen.width - boxWidth - leftBoxX;
        int rightButtonX = rightBoxX + boxBuffer;

        int buttonWidth = boxWidth - 2 * boxBuffer;
        int buttonHeight = 20;
        int boxX = leftBoxX;
        int buttonX = leftButtonX;

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
            flowBridge.Refresh();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "Clear Field"))
        {
            Debug.Log("Clear Field");
            flowBridge.ClearField();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "Single Step"))
        {
            Debug.Log("Single Step");
            flowBridge.SingleStep();
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "VISUALIZE"))
        {
            Debug.Log("VISUALIZE");
            flowBridge.StartVisualize();
        }
        guiVisY += yBetweenButtons;


        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "From-Start Mode (clears)"))
        {
            Debug.Log("From-Start Mode");
            flowBridge.SetSearchDir(HexSearch.Dir.FromStart);
        }
        guiVisY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), "From-End Mode (clears)"))
        {
            Debug.Log("From-End Mode");
            flowBridge.SetSearchDir(HexSearch.Dir.FromEnd);
        }
        guiVisY += yBetweenButtons + boxBuffer;

        Rect rect = new Rect(buttonX, guiVisY, buttonWidth, buttonHeight);
        bool isAutoRefreshWanted = GUI.Toggle(rect, flowBridge.IsAutoRefreshMode, "Auto-Refresh");
        if (isAutoRefreshWanted != flowBridge.IsAutoRefreshMode) flowBridge.SetAutoRefreshMode(isAutoRefreshWanted);
        guiVisY += yBetweenButtons;

        visualizeDistanceEnabled = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), visualizeDistanceEnabled, "Distance Numbers");
        if (wasVisualizeDistanceEnabled != visualizeDistanceEnabled)
        {
            wasVisualizeDistanceEnabled = visualizeDistanceEnabled;
            flowBridge.SetNumbersVisible(visualizeDistanceEnabled);
        }
        guiVisY += yBetweenButtons;

        visualizePathfindingEnabled = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), visualizePathfindingEnabled, "Pathfinding Arrows");
        if (wasVisualizePathfindingEnabled != visualizePathfindingEnabled)
        {
            wasVisualizePathfindingEnabled = visualizePathfindingEnabled;
            flowBridge.SetVisualizationVisible(visualizePathfindingEnabled);
        }
        guiVisY += yBetweenButtons;

        stopPathingEarly = GUI.Toggle(new Rect(buttonX, guiVisY, buttonWidth, buttonHeight), stopPathingEarly, "Stop pathfinding early");
        if (wasStopPathingEarly != stopPathingEarly)
        {
            wasStopPathingEarly = stopPathingEarly;
            flowBridge.ResetPathfindingWithEarlyStoppingMode(stopPathingEarly);
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

        // // === //

        GUI.Box(new Rect(boxX, guiSaveLoadY, boxWidth, guiSaveLoadHeight), "SAVE/LOAD");
        guiSaveLoadY += yBetweenButtons + boxBuffer;

        if (GUI.Button(new Rect(buttonX, guiSaveLoadY, buttonWidth, buttonHeight), "Save"))
        {
            Debug.Log("Save pressed");
            gameMaster.OnSave();
        }

        guiSaveLoadY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiSaveLoadY, buttonWidth, buttonHeight), "Load"))
        {
            Debug.Log("Load pressed");
            gameMaster.OnLoad();
        }

        // // === //
        // On right-side 

        boxX = rightBoxX;
        buttonX = rightButtonX;
        guiBuildingY = yBetweenButtons;

        GUI.Box(new Rect(boxX, guiBuildingY, boxWidth, guiBuildingHeight),
            "Building");
        guiBuildingY += yBetweenButtons + boxBuffer;

        string[] buildModeLabels = { "Land", "Tower" };
        int current = (int)boardInput.BuildMode;
        int selected = GUI.Toolbar(new Rect(buttonX, guiBuildingY, buttonWidth, buttonHeight), current, buildModeLabels);
        if (selected != current) boardInput.SetBuildMode((BoardInput.BuildType)selected);

        guiBuildingY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiBuildingY, buttonWidth, buttonHeight),
            "Remove All Towers"))
        {
            Debug.Log("Remove All Towers pressed");
            towerController.RemoveAllTowers();
        }


    }
}
