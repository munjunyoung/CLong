using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCharInfo : MonoBehaviour
{
    public AudioSource sound;
    public int id;
    Toggle tg;

	// Use this for initialization
	void Start ()
    {
        tg = GetComponent<Toggle>();
        tg.group = LobbyUIManager.Instance.mainCharacterToggleGroup;
        tg.onValueChanged.AddListener((bool isOn) =>
        {
            if (!isOn) return;

            LobbyUIManager.Instance.OnClickChangeCharacter(id);
            sound.Play();
        });
	}
}
