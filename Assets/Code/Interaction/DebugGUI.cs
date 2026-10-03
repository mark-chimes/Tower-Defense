using System;
using UnityEngine;

// TODO neaten up the rest of this GUI later
public class DebugGUI : MonoBehaviour
{

    private FlowBridge flowBridge;
    private EnemyController enemyController;
    private BoardInput boardInput;
    private TowerController towerController;


    private Action save;
    private Action load;

    private Func<bool> isSimRunning;
    private Action<bool> setSimRunning;


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
        Action save,
        Action load,
        Func<bool> isSimRunning,
        Action<bool> setSimRunning
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
        this.save = save;
        this.load = load;
        this.isSimRunning = isSimRunning;
        this.setSimRunning = setSimRunning;
    }


    private const int boxBuffer = 10;
    private const int boxWidth = 210;

    private const int margin = 10;



    void OnGUI()
    {
        int leftBoxX = 10;
        int leftButtonX = leftBoxX + boxBuffer;
        int rightBoxX = Screen.width - boxWidth - leftBoxX;
        int rightButtonX = rightBoxX + boxBuffer;

        int buttonWidth = boxWidth - 2 * boxBuffer;
        int buttonHeight = 20;
        int boxX = leftBoxX;
        int buttonX = leftButtonX;

        int yBetweenButtons = buttonHeight;
        int yBetweenBuildingButtons = buttonHeight + boxBuffer;

        int numUsefulControls = 4;
        int usefulVisY = yBetweenButtons;
        int usefulVisHeight = (numUsefulControls + 2) * yBetweenButtons + boxBuffer;
        int guiUsefulEnd = usefulVisY + usefulVisHeight;

        int numVisControls = 10;
        int guiVisY = guiUsefulEnd + yBetweenButtons + boxBuffer;
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

        GUILayout.BeginArea(new Rect(margin, margin, boxWidth, Screen.height));
        DrawUsefulControls();
        GUILayout.EndArea();




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
            save.Invoke();
        }

        guiSaveLoadY += yBetweenButtons;

        if (GUI.Button(new Rect(buttonX, guiSaveLoadY, buttonWidth, buttonHeight), "Load"))
        {
            Debug.Log("Load pressed");
            load.Invoke();
        }

        // // === //
        // On right-side 
        GUILayout.BeginArea(new Rect(Screen.width - boxWidth - margin, margin, boxWidth, Screen.height));
        DrawBuildingControls();

        DrawSimRunningControls();

        GUILayout.EndArea();


    }

    int spacing = 5;

    private void DrawUsefulControls()
    {
        BeginSection("USEFUL");

        Toggle("Sim running", isSimRunning(), setSimRunning);
        GUILayout.Space(spacing);

        Toggle("Auto-Refresh", flowBridge.IsAutoRefreshMode,
            flowBridge.SetAutoRefreshMode);

        Toggle("Pathfinding Arrows", visualizePathfindingEnabled,
            value => { visualizePathfindingEnabled = value; flowBridge.SetVisualizationVisible(value); });

        Toggle("Boats follow path on spawn", enemyController.BoatsFollowPathOnSpawn,
              value => enemyController.BoatsFollowPathOnSpawn = value);

        Button("Follow existing path", enemyController.OnBoatsFollowExistingPathPressed);

        Button("Spawn Boat", enemyController.OnSpawnBoatPressed);

        EndSection();
    }
    private void DrawBuildingControls()
    {
        BeginSection("Building");

        string[] buildModeLabels = { "Land", "Tower" };
        int current = (int)boardInput.BuildMode;
        int selected = GUILayout.Toolbar(current, buildModeLabels);
        if (selected != current) boardInput.SetBuildMode((BoardInput.BuildType)selected);

        GUILayout.Space(spacing);

        Toggle("Land removal destroys tower", boardInput.DestroysTowerWithLand,
            value => boardInput.DestroysTowerWithLand = value);

        GUILayout.Space(spacing);

        Button("Remove All Towers", towerController.RemoveAllTowers);
        EndSection();
    }

    private void DrawSimRunningControls()
    {
        BeginSection("SIM");
        EndSection();
    }

    private void BeginSection(string title)
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label(title);
    }

    private void EndSection()
    {
        GUILayout.EndVertical();
        GUILayout.Space(boxBuffer);
    }

    private void Button(string label, Action onClick)
    {
        if (GUILayout.Button(label))
        {
            Debug.Log($"{label} pressed");
            onClick.Invoke();
        }
    }


    private void Toggle(string label, bool current, Action<bool> onChanged)
    {
        bool wanted = GUILayout.Toggle(current, label);
        if (wanted != current) onChanged(wanted);
    }
}

