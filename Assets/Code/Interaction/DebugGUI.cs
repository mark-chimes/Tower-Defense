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
    bool visualizePathfindingEnabled = true;
    bool stopPathingEarly = false;

    public void Initialize(TerrainWithOverlay.VisualizationSettings visualization,
        bool stopPathingEarly,
        BoardInput boardInput,
        FlowBridge flowBridge,
        EnemyController enemyController,
        TowerController towerController,
        Action save,
        Action load,
        Func<bool> isSimRunning,
        Action<bool> setSimRunning
        )
    {
        visualizeDistanceEnabled = visualization.ShowDistance;
        visualizePathfindingEnabled = visualization.ShowPathfinding;

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
    private const int spacing = 5;

    void OnGUI()
    {
        // Left
        GUILayout.BeginArea(new Rect(margin, margin, boxWidth, Screen.height));
        DrawUsefulControls();
        DrawVisualizationControls();
        DrawBoatControls();
        DrawSaveLoadControls();
        GUILayout.EndArea();

        // Right 
        GUILayout.BeginArea(new Rect(Screen.width - boxWidth - margin, margin, boxWidth, Screen.height));
        DrawBuildingControls();
        DrawNewControls();
        GUILayout.EndArea();
    }


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

    private void DrawVisualizationControls()
    {
        BeginSection("VISUALIZE");

        Button("Instant Refresh", flowBridge.Refresh);
        Button("Clear Field", flowBridge.ClearField);
        Button("Single Step", flowBridge.SingleStep);

        Button("VISUALIZE", flowBridge.StartVisualize);
        Button("From-Start Mode (clears)", () => flowBridge.SetSearchDir(HexSearch.Dir.FromStart));
        Button("From-End Mode (clears)", () => flowBridge.SetSearchDir(HexSearch.Dir.FromEnd));


        Toggle("Auto-Refresh", flowBridge.IsAutoRefreshMode,
            flowBridge.SetAutoRefreshMode);

        Toggle("Pathfinding Arrows", visualizePathfindingEnabled,
            value => { visualizePathfindingEnabled = value; flowBridge.SetVisualizationVisible(value); });

        Toggle("Distance Numbers", visualizeDistanceEnabled,
            value => { visualizeDistanceEnabled = value; flowBridge.SetNumbersVisible(value); });

        Toggle("Stop Pathfinding Early", stopPathingEarly,
            value => { stopPathingEarly = value; flowBridge.ResetPathfindingWithEarlyStoppingMode(stopPathingEarly); });

        EndSection();
    }

    private void DrawBoatControls()
    {
        BeginSection("BOATS");

        Button("Spawn Boat", enemyController.OnSpawnBoatPressed);
        Button("Delete Boats", enemyController.OnDeleteBoatsPressed);
        Button("Follow existing path", enemyController.OnBoatsFollowExistingPathPressed);
        Button("Stop boats", enemyController.OnBoatsStopPressed);

        EndSection();
    }

    private void DrawSaveLoadControls()
    {
        BeginSection("SAVE/LOAD");
        Button("Save", save);
        Button("Load", load);
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

    private void DrawNewControls()
    {
        BeginSection("New");
        Toggle("Boats despawn at goal",
            enemyController.BoatsDespawnAtGoal,
            value => enemyController.BoatsDespawnAtGoal = value);
        Button($"Spawn {enemyController.SpawnBatchSize} Boats",
            enemyController.OnSpawnBatchPressed);
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

