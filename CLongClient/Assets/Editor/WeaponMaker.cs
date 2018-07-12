using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WeaponMaker : EditorWindow
{
    private enum WEAPON_TYPE
    {
        HG, SMG, AR, SR
    }

    // 유형
    private WEAPON_TYPE _type;
    // 이름
    private string _name = "UNKNOWN";
    // 장탄수
    private int _bulletNum = 10;
    // 탄창수 
    private int _maxMagazine = 3;
    // 총알 속도
    private float _bulletSpeed = 1f;
    // 연사 속도
    private float _fireSpeed = 1f;
    // 무게
    private float _weight = 1f;
    // 재장전 속도
    private float _reloadTime = 1f;
    // 반동
    private float _rebound = 1f;

    private static WeaponMaker window;

    [MenuItem("Custom Tool/Item Maker/Weapon")]
    static void Init()
    {
        window = (WeaponMaker)EditorWindow.GetWindow<WeaponMaker>();
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Properties", EditorStyles.boldLabel);

        _name = EditorGUILayout.TextField("Name", _name);
        _type = (WEAPON_TYPE)EditorGUILayout.EnumPopup("Weapon Type", _type);
        _bulletNum = EditorGUILayout.IntField("Number of Bullets", _bulletNum);
        _maxMagazine = EditorGUILayout.IntField("Maximum Magazine", _maxMagazine);
        _bulletSpeed = EditorGUILayout.Slider("Bullet Speed Factor", _bulletSpeed, 1, 5);
        _fireSpeed = EditorGUILayout.Slider("Fire Speed Factor", _fireSpeed, 1, 5);
        _weight = EditorGUILayout.FloatField("Weight", _weight);
        _reloadTime = EditorGUILayout.Slider("Reload Time", _reloadTime, 1, 5);
        _rebound = EditorGUILayout.FloatField("Rebound Value", _rebound);

        if(GUILayout.Button("Confirm"))
        {
            // Make Json data
        }
        else if (GUILayout.Button("Cancel"))
        {
            // Cancel
            window.Close();
        }
    }
}