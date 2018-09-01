using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType { WEAPON, THROWBLE, CONSUMABLE }

public class EquippedItemInfo : MonoBehaviour
{
    public enum EquipType { WEAPON1, WEAPON2, THROWBLE1, THROWBLE2, CONSUMABLE }

    public EquipType equipType;
    public ItemType iType;
    public Image equippedItemImg;
    public byte itemID;
    public InventoryItemInfo refItemInfo;

    public void SetItem(InventoryItemInfo refItem)
    {
        if (iType != refItem.iType)
            return;

        if(refItemInfo != null)
            refItemInfo.SetEquipped(false);

        refItemInfo = refItem;
        equippedItemImg.sprite = refItemInfo.itemImg.sprite;
        equippedItemImg.color = new Color(1, 1, 1, 1);
        itemID = refItemInfo.itemNum;
        refItemInfo.SetEquipped(true);
        LobbyUIManager.Instance.SetEquippedItem((int)equipType, itemID);
    }
}
