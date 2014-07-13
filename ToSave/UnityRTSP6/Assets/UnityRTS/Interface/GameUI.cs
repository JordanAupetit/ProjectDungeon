using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour {

    public Player Player;
    public SelectionManager Selection;

    public void Awake() {
        if (Selection == null) Selection = GameObject.FindObjectOfType<SelectionManager>();
    }

    public void OnGUI() {
        using (var screen = new UIUtility.Group(new Rect(0, 0, Screen.width, Screen.height))) {
            // Menu bar along the top
            using (var topRect = screen.HeightSlice(22)) {
                GUI.Box(topRect.Bounds, "TODO: Top bar!");
            }
            // Bottom panel with resource/selection/map info
            using (var bottomPanel = screen.HeightSlice(190, true)) {
                var heroSelected = Selection.Selected.Count > 0 ? Selection.Selected[0] : null;
                var heroPlayer = heroSelected != null ? heroSelected.Player : null;

                GUI.Box(bottomPanel, "");
                var bottomBounds = bottomPanel.Bounds;
                using (var playerInfo = new UIUtility.Group(new Rect(bottomBounds.center.x - 120, bottomBounds.yMin - 30, 240, 30))) {
                    GUI.Box(playerInfo, heroPlayer != null ? heroPlayer.Name : "");
                }
                // Resorces (200px fixed-size) along left
                using (screen.WidthSlice(Mathf.Min(140, bottomPanel.width * 0.2f)))
                using (var resources = UIUtility.Inset(5))
                using (var grid = new UIUtility.Grid(1, Player.Resources.Count)) {
                    foreach (var resource in Player.Resources.Resources) {
                        using (var row = grid.Cell()) {
                            GUI.Box(row.Bounds, resource.Name + ": " + resource.Amount);
                        }
                    }
                }
                // Map (square) along right
                using (var mapItem = screen.WidthSlice(Mathf.Min(bottomPanel.Height * 1.4f, bottomPanel.Width * 0.3f), true)) {
                    GUI.Box(mapItem, "");
                }
                // Others (percentage) in centre
                using (var remaining = UIUtility.Group.Clone()) {
                    // 40% unit options (buildables, etc.)
                    using (var unitOptions = remaining.WidthSlice(0.45f)) {
                        GUI.Box(unitOptions, "Buildables");
                    }
                    // 30% unit information
                    using (var unitInfo = remaining.WidthSlice(0.3f)) {
                        GUI.Box(unitInfo, "");
                        if (heroSelected != null) {
                            using (var unitName = unitInfo.HeightSlice(24)) {
                                GUI.Box(unitName, heroSelected.name);
                            }
                            using (var bounds = unitInfo.HeightSlice(24)) {
                                GUI.Box(bounds, heroSelected.Health + " / " + heroSelected.MaxHealth);
                            }
                            var gatherer = heroSelected.Simulation.GetComponent<Simulation.SAGather>();
                            if (gatherer != null) {
                                using (var bounds = unitInfo.HeightSlice(24)) {
                                    if (gatherer.HoldingType != null) {
                                        GUI.Box(bounds, "Holding " + gatherer.HoldingAmount + " " + gatherer.HoldingType);
                                    }
                                }
                            }
                            var resourceSite = heroSelected.Simulation.GetComponent<Simulation.SCResourceSite>();
                            if (resourceSite != null) {
                                using (var grid = new UIUtility.Grid(1, resourceSite.Resources.Count)) {
                                    foreach (var resource in resourceSite.Resources.Resources) {
                                        using (var row = grid.Cell()) {
                                            GUI.Box(row, resource.Name + " " + resource.Amount);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // 25% selection information
                    using (var selection = remaining.WidthSlice(0.25f)) {
                        GUI.Box(selection, "");
                        var itemSize = new Vector2(50, 50);
                        int xcount = UIUtility.Arrange(selection, itemSize, Selection.Selected.Count);
                        for (int s = 0; s < Selection.Selected.Count; ++s) {
                            var selected = Selection.Selected[s];
                            using (var bounds = new UIUtility.Group(UIUtility.ArrangeH(selection, itemSize, s, xcount, Selection.Selected.Count))) {
                                GUI.Button(bounds, selected.name);
                            }
                        }
                    }
                }
            }
        }
    }

}
