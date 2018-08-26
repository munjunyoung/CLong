using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSetter : MonoBehaviour
{
    public Toggle[] toggles;

    private void Awake()
    {
        for(int i=0; i<toggles.Length; ++i)
        {
            toggles[i].onValueChanged.AddListener((bool isOn) =>
            {
                if (!isOn)
                    return;
                
                SelectCharacter();
            });
        }
    }

    private void SelectCharacter()
    {
        
        ToggleGroup tg = toggles[0].group;
        tg.ActiveToggles().FirstOrDefault();
    }

    private void Update()
    {
        
    }
}
