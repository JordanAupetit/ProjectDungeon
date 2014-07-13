using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UIExtensions;

public class UIEditor : EditorWindow {

    public Vector2 ScreenSize = new Vector2(800, 480);

    //private static Type[] selectionPriority = new[] { typeof(UIElement), typeof(UIConstraint), typeof(UIRail), };

    private UIRail draggingRail;

    public void OnInspectorUpdate() { Repaint(); }
    public void OnGUI() {
        var screen = GetComponentInParents<UIScreen>(Selection.activeTransform);
        if (screen == null) {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Select a UIScreen component!");
            return;
        }

        const float ControlsGutterWidth = 250;

        var active = Selection.activeObject;
        if (active is GameObject) {
            var gobj = (GameObject)active;
            active = gobj.GetComponent<UIElement>() ?? (Component)gobj.GetComponent<UIConstraint>() ?? gobj.GetComponent<UIRail>();
        }

        var screenBounds = new Rect(0, 0, Screen.width, Screen.height);
        var virtualSpace = UIUtility.Anchor(screenBounds.Inset(0, 0, ControlsGutterWidth, 100), ScreenSize, UIScale.UniformOrNone, new Vector2(0.5f, 0));
        var inspectorSpace = Rect.MinMaxRect(screenBounds.xMax - ControlsGutterWidth, screenBounds.yMin, screenBounds.xMax, screenBounds.yMax);
        var controlSpace = Rect.MinMaxRect(screenBounds.xMin, virtualSpace.yMax, inspectorSpace.xMin, screenBounds.yMax);

        GUI.Box(virtualSpace, (Texture2D)null);
        screen.Arrange(ScreenSize);
        for (int c = 0; c < screen.HConstraints.Length; ++c) {
            var constraint = screen.HConstraints[c];
            if (!constraint._CanFlex) {
                var y = 200 + c * 20;
                var leftV = constraint.Left._value;
                var rightV = constraint.Right._value;
                var left = Mathf.Min(leftV, rightV);
                var right = Mathf.Max(leftV, rightV);
                var rect = constraint.Direction == UIRail.Directions.Vertical ?
                Rect.MinMaxRect(left, y, Mathf.Max(left, right), y + 20) :
                Rect.MinMaxRect(y, left, y + 20, Mathf.Max(left, right));
                DrawItem(rect, virtualSpace, constraint);
            }
        }
        for (int e = 0; e < screen.Elements.Length; ++e) {
            var element = screen.Elements[e];
            DrawItem(element.GetBounds(), virtualSpace, element);
        }
        if (active is UIElement) {
            var element = (UIElement)active;
            var bounds = TransformRect(element.GetBounds(), ScreenSize, virtualSpace);
            for (int d = 0; d < 4; ++d) {
                const float HandleWidth = 4;

                var dir = UIScreen.Directions[d];
                var nrm = UIScreen.Directions[(d + 1) % 4];
                var center = bounds.center + dir * (Mathf.Abs(Vector2.Dot(bounds.Size(), dir)) / 2 - HandleWidth);
                var width = Mathf.Abs(Vector2.Dot(bounds.Size(), nrm));
                var corner = dir * HandleWidth + nrm * (width / 2 - HandleWidth * 2);
                corner.x = Mathf.Abs(corner.x);
                corner.y = Mathf.Abs(corner.y);

                var handleRect = Rect.MinMaxRect(center.x - corner.x, center.y - corner.y, center.x + corner.x, center.y + corner.y);
                var mhit = handleRect.Contains(Event.current.mousePosition) || draggingRail == element.GetEdge(d);
                if (Event.current.type == EventType.Repaint) {
                    var color = mhit ? new Color(1, 1, 1) : new Color(0.7f, 0.7f, 0.7f);
                    using (new UIUtility.GUIColor(color)) {
                        EditorStyles.textField.Draw(handleRect, true, true, false, false);
                    }
                }
                if (mhit && Event.current.type == EventType.MouseDown) {
                    if (Event.current.control) {
                        Debug.Log("Creating new constraint!");
                        var oldRail = element.GetEdge(d);
                        var rail = screen.NewRail(element.name + "_" + UIScreen.DirectionNames[d], UIScreen.RailDirections[d]);
                        element.SetEdge(d, rail);
                        var constraint = dir.x + dir.y > 0 ?
                            screen.NewConstraint(oldRail, rail, 5, 0) :
                            screen.NewConstraint(rail, oldRail, 5, 0);
                        screen.HConstraints = screen.HConstraints.Concat(new[] { constraint }).ToArray();
                    }
                    draggingRail = element.GetEdge(d);
                }
            }
        }

        if (Event.current.type == EventType.MouseUp) {
            draggingRail = null;
        }
        if (draggingRail != null) {
            if (Event.current.type == EventType.MouseDrag) {
                for (int c = 0; c < screen.HConstraints.Length; ++c) {
                    var constraint = screen.HConstraints[c];
                    if (constraint.HasEdge(draggingRail)) {
                        var delta = constraint.Direction == UIRail.Directions.Vertical ? Event.current.delta.x : Event.current.delta.y;
                        constraint.Width += delta * (constraint.Left == draggingRail ? -1 : 1);
                    }
                }
            }
        }

        GUI.Box(inspectorSpace, (Texture2D)null);
        if (active != null) {
            GUILayout.BeginArea(inspectorSpace);
            EditorGUIUtility.labelWidth = 100;
            GUILayout.Button(active.name + " <" + active.GetType().Name + ">", EditorStyles.toolbarButton);
            if (active is UIConstraint) {
                var constraint = (UIConstraint)active;
                constraint.Width = EditorGUILayout.FloatField("Width", constraint.Width);
                constraint.CollapseId = EditorGUILayout.IntField("Collapse ID", constraint.CollapseId);
            }
            GUILayout.EndArea();
        }

        GUI.Box(controlSpace, (Texture2D)null);
        {
            const int ButtonCount = 5;
            var buttonSize = new Vector2(100, 100);
            int xcount = UIUtility.Arrange(controlSpace, buttonSize, ButtonCount);
            for (int b = 0; b < ButtonCount; ++b) {
                var rect = UIUtility.ArrangeH(controlSpace, buttonSize, b, xcount, ButtonCount);
                GUI.Button(rect, "Btn" + b);
            }
        }
    }

    private void DrawItem(Rect rect, Rect virtualBounds, Component component) {
        if (Selection.Contains(component.gameObject)) {
            GUI.Box(TransformRect(rect, ScreenSize, virtualBounds), component.name);
        } else {
            if (GUI.Button(TransformRect(rect, ScreenSize, virtualBounds), component.name))
                Selection.activeObject = component;
        }
    }

    private Rect TransformRect(Rect rect, Vector2 designSize, Rect bounds) {
        return new Rect(
            bounds.x + rect.x * bounds.width / designSize.x,
            bounds.y + rect.y * bounds.height / designSize.y,
            rect.width * bounds.width / designSize.x,
            rect.height * bounds.height / designSize.y
        );
    }


    [MenuItem("Window/UI Editor")]
    public static void ShowWindow() {
        EditorWindow.GetWindow<UIEditor>();
    }

    private static T GetComponentInParents<T>(Transform c) where T : Component {
        var parent = c;
        while(parent != null) {
            var cmp = parent.GetComponent<T>();
            if (cmp != null) return cmp;
            parent = parent.parent;
        }
        return null;
    }

}
