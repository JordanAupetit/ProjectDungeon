using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ResourceCollection))]
public class ResourceCollectionEditor : PropertyDrawer {
    private static bool isFoldedOut = true;

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
        var resources = prop.FindPropertyRelative("Resources");
        //return prop.FindPropertyRelative("Resources").serializedObject * base.GetPropertyHeight(prop, label);
        return base.GetPropertyHeight(prop, label) +
            (isFoldedOut ? resources.arraySize * base.GetPropertyHeight(prop, label) : 0);
    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        //SerializedProperty me = prop.FindPropertyRelative("this");
        //SerializedProperty scale = prop.FindPropertyRelative("scale");
        //SerializedProperty curve = prop.FindPropertyRelative("curve");
        //EditorGUI.LabelField(pos, "hi" + resources);
        var rowRect = new Rect(pos.x, pos.y, pos.width, base.GetPropertyHeight(prop, label));
        isFoldedOut = EditorGUI.Foldout(new Rect(rowRect.x, rowRect.y, rowRect.width - 20, rowRect.height), isFoldedOut, "Resources");
        if (isFoldedOut) {
            var resources = prop.FindPropertyRelative("Resources");
            if (GUI.Button(new Rect(rowRect.x + rowRect.width - 22, rowRect.y + 1, 22, rowRect.height - 2), "+")) {
                resources.InsertArrayElementAtIndex(resources.arraySize);
            }
            rowRect.y += rowRect.height;
            for (int i = 0; i < resources.arraySize; ++i) {
                var item = resources.GetArrayElementAtIndex(i);
                var minusClick = GUI.Button(new Rect(rowRect.x, rowRect.y, 22, rowRect.height), "-");
                EditorGUI.PropertyField(new Rect(rowRect.x + 20, rowRect.y, rowRect.width - 21, rowRect.height), item);
                if (minusClick) {
                    if (Event.current.button == 1) {
				        // Now create the menu, add items and show it
                        var menu = new GenericMenu();
                        if (i > 0)
                            menu.AddItem(new GUIContent("Move Up"), false, (itm) => { resources.MoveArrayElement((int)itm, (int)itm - 1); }, i);
                        if (i < resources.arraySize - 1)
                            menu.AddItem(new GUIContent("Move Down"), false, (itm) => { resources.MoveArrayElement((int)itm, (int)itm + 1); }, i);
                        menu.ShowAsContext();
                    } else {
                        resources.DeleteArrayElementAtIndex(i--);
                    }
                }
                rowRect.y += rowRect.height;
            }
        }
    }

}

[CustomPropertyDrawer(typeof(Resource))]
public class ResourceItemEditor : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
        //var resources = prop.FindPropertyRelative("Resources");
        //return prop.FindPropertyRelative("Resources").serializedObject * base.GetPropertyHeight(prop, label);
        return base.GetPropertyHeight(prop, label) +
            0;
            //resources.arraySize * base.GetPropertyHeight(prop, label);
    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        var nameField = prop.FindPropertyRelative("Name");
        var amountField = prop.FindPropertyRelative("Amount");
        var nameRect = new Rect(pos.x, pos.y, pos.width * 0.5f - 2, pos.height);
        var amountRect = new Rect(pos.x + pos.width * 0.5f + 2, pos.y, pos.width * 0.5f - 2, pos.height);
        nameField.stringValue = EditorGUI.TextField(nameRect, nameField.stringValue);
        amountField.intValue = EditorGUI.IntField(amountRect, amountField.intValue);
    }

}
