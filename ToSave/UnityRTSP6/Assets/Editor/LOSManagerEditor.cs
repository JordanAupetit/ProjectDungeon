using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LOSManager))]
public class LOSManagerEditor : Editor {

    private static bool showQuality = true;
    private static bool showFeatures = true;
    private static bool showDecayRates = true;

    public override void OnInspectorGUI() {
        bool doUpdate = Event.current.type == EventType.MouseUp;

        LOSManager manager = target as LOSManager;

        // Allow selection of terrain
        manager.Size.Terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", manager.Size.Terrain, typeof(Terrain), true);
        if (manager.Terrain != null) {
            // If terrain is active, do not allow size override
            manager.Size.Width = -1;
            manager.Size.Height = -1;
        } else {
            // Otherwise require a valid size
            if (manager.Size.Width == -1) manager.Size.Width = 64;
            if (manager.Size.Height == -1) manager.Size.Height = 64;
            // And allow size modification
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Size");
            manager.Size.Width = EditorGUILayout.IntField(manager.Size.Width);
            EditorGUILayout.LabelField("x", GUILayout.MinWidth(0));
            manager.Size.Height = EditorGUILayout.IntField(manager.Size.Height);
            EditorGUILayout.EndHorizontal();
        }

        // Adjust detail settings
        showQuality = EditorGUILayout.Foldout(showQuality, "Quality");
        if (showQuality) {
            EditorGUI.indentLevel++;
            manager.Size.Scale = RoundToPow2(EditorGUILayout.Slider("Pixels Per Unit", manager.Size.Scale, 0.125f, 4));
            manager.Size.HighDetailTexture = EditorGUILayout.Toggle("32bit texture", manager.Size.HighDetailTexture);
            EditorGUI.indentLevel--;
        }

        // Enable/configure features
        showFeatures = EditorGUILayout.Foldout(showFeatures, "Features");
        if (showFeatures) {
            EditorGUI.indentLevel++;
            manager.Visual.AOIntensity = Mathf.RoundToInt(ToggleableRange("AO Shadows", manager.Visual.AOIntensity, 255));
            manager.Visual.InterpolationRate = Mathf.RoundToInt(ToggleableRange("Fade", manager.Visual.InterpolationRate, 1024));
            manager.Visual.FringeSize = ToggleableRange("Fringe Size", manager.Visual.FringeSize, 16);
            manager.HeightBlockers.Enable = EditorGUILayout.Toggle("Height blockers", manager.HeightBlockers.Enable);
            manager.Visual.EntityDiscoverMode = (LOSManager.OnDiscover)EditorGUILayout.EnumPopup("Entity Discover", manager.Visual.EntityDiscoverMode);
            EditorGUI.indentLevel--;
        }
        // Rates of feature decay
        showDecayRates = EditorGUILayout.Foldout(showDecayRates, "Decay rates");
        if (showDecayRates) {
            EditorGUI.indentLevel++;
            manager.Visual.GrayscaleDecayDuration = ToggleableRangeDecay("Grayscale Decay", manager.Visual.GrayscaleDecayDuration, 360);
            manager.Visual.RevealedDecayDuration = ToggleableRangeDecay("Revealed Decay", manager.Visual.RevealedDecayDuration, 360);
            manager.Visual.VisibleDecayDuration = ToggleableRangeDecay("Visible Decay", manager.Visual.VisibleDecayDuration, 360);
            EditorGUI.indentLevel--;
        }

        if (doUpdate) EditorUtility.SetDirty(target);

        // Toggle editor preview
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Preview");
        manager.PreviewInEditor = GUI.Toolbar(EditorGUILayout.GetControlRect(GUILayout.Height(22), GUILayout.Width(60)), manager.PreviewInEditor ? 1 : 0, new[] { "Off", "On", }) == 1;
        EditorGUILayout.EndHorizontal();

        // Display scale parameters
        EditorGUILayout.HelpBox(
            "Map Size " + Mathf.RoundToInt(manager.MapWidth) + " x " + Mathf.RoundToInt(manager.MapHeight) + "\n" +
            "Resolution " + manager.ActualWidth + " x " + manager.ActualHeight,
            MessageType.Info);

        //base.OnInspectorGUI();
    }

    private float ToggleableRange(string name, float value, float max) {
        EditorGUILayout.BeginHorizontal();
        if (EditorGUILayout.Toggle(name, value != 0) != (value != 0)) {
            value = value != 0 ? 0 : max / 2;
        }
        int oldIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        value = EditorGUILayout.Slider(value, 0, max);
        EditorGUI.indentLevel = oldIndent;
        EditorGUILayout.EndHorizontal();
        return value;
    }

    private float ToggleableRangeDecay(string name, float value, float max) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(name);
        int oldIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var max2 = Mathf.Pow(max, 0.5f);
        var val2 = GUILayout.HorizontalSlider(Mathf.Pow(value, 0.5f), 0, max2);
        if (val2 != max2) value = Mathf.Pow(val2, 2); else value = float.PositiveInfinity;

        string label = "";
        if (value == 0) label = "Instant";
        else if (float.IsInfinity(value)) label = "Off";
        else if (value < 5) label = Mathf.Round(value * 10) / 10 + "s";
        else label = (value > 60 ? Mathf.FloorToInt(value / 60) + "m" : "") + (value % 60 > 1 ? Mathf.FloorToInt(value % 60) + "s" : "");
        EditorGUILayout.LabelField(label, GUILayout.Width(50));

        EditorGUI.indentLevel = oldIndent;
        EditorGUILayout.EndHorizontal();
        return value;
    }

    private int IntField(int value, int dispVal) {
        var oldCol = GUI.color;
        if (dispVal != value) GUI.color = new Color(1, 1, 1, 0.5f);
        var newValue = EditorGUILayout.IntField(dispVal, EditorStyles.label);
        GUI.color = oldCol;
        if (newValue != dispVal) return newValue;
        return value;
    }

    private float RoundToPow2(float value) {
        var l2 = Mathf.Log(value, 2);
        l2 = Mathf.Round(l2);
        return Mathf.Pow(2, l2);
    }

}
