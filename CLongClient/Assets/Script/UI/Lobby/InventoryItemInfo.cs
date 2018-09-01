using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemInfo : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemType iType;
    public Image itemImg;
    public GameObject equippedUI;
    public byte itemNum;

    private GameObject go;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (equippedUI.activeSelf) return;

        go = Instantiate(gameObject, LobbyUIManager.Instance.transform);
        foreach(var img in go.GetComponentsInChildren<Image>())
        {
            img.raycastTarget = false;
        }
        go.transform.position = Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (equippedUI.activeSelf) return;

        go.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (equippedUI.activeSelf) return;

        var hoverList = eventData.hovered;
        foreach(GameObject g in hoverList)
        {
            if(g.GetComponent<EquippedItemInfo>() != null)
            {
                var idr = g.GetComponent<EquippedItemInfo>();
                idr.SetItem(this);
                break;
            }
        }

        Destroy(go);
    }

    public void SetEquipped(bool b)
    {
        equippedUI.SetActive(b);
    }
}
