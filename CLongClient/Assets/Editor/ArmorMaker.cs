using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ArmorMaker : EditorWindow
{
    private enum ARMOR_TYPE
    {
        HELMET,
        VEST
    }

    private ARMOR_TYPE _type;
    private string _name = "UNKNOWN";
    private float _reduceValue = 0;

    private static ArmorMaker window;

    [MenuItem("Custom Tool/Item Maker/Armor")]
    static void Init()
    {
        window = (ArmorMaker)EditorWindow.GetWindow<ArmorMaker>();
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Base Properties", EditorStyles.boldLabel);

        _name = EditorGUILayout.TextField("Name", _name);
        _type = (ARMOR_TYPE)EditorGUILayout.EnumPopup("Type", _type);
        _reduceValue = EditorGUILayout.Slider("Reload Time", _reduceValue, 0, 1);

        GUILayout.Label("Detail Properties", EditorStyles.boldLabel);

        switch (_type)
        {
            case ARMOR_TYPE.HELMET:
                break;
            case ARMOR_TYPE.VEST:
                break;
        }

        if (GUILayout.Button("Confirm"))
        {
            // Make Json data
        }
        else if(GUILayout.Button("Cancel"))
        {
            // Cancel
            window.Close();
        }
    }
}
