using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    private int currentItemSlot = 0;
    private int maxSlot = 4;
    public List<Slotdata> slots = new List<Slotdata>();

    [SerializeField] Transform pickTransform;


    private void Start()
    {
        for (int i = 0; i < maxSlot; i++)
        {
            slots.Add(new Slotdata());
        }
        ChangeItemSlot(0);
    }
    public Item GetCurrentItemComponent
    {
        get
        {
            if (slots[currentItemSlot].isEmpty != true)
            {
                return slots[currentItemSlot].slotObjComponent;
            }
            return null;
        }
    }
    public bool IsOutRange(int index) => (index < 0 || index >= maxSlot) ? true : false;

    public void AddItem(GameObject item)
    {
        if (item == null)
            return;
        if (slots[currentItemSlot].isEmpty == true)
        {
            slots[currentItemSlot].isEmpty = false;
            slots[currentItemSlot].slotObjComponent = item.GetComponent<Item>();
            slots[currentItemSlot].slotObjComponent.PickUp(pickTransform);
            GameRoomNetworkManager.Instance.OnSetObjectHierarchy(ObjectReference.Instance.GetIdByGameObject(item), ObjectReference.Instance.GetIdByGameObject(gameObject));
        }
    }
    public void RemoveItem()
    {
        if (slots[currentItemSlot].isEmpty == false)
        {
            slots[currentItemSlot].isEmpty = true;
            slots[currentItemSlot].slotObjComponent.PickDown(pickTransform);
            slots[currentItemSlot].slotObjComponent = null;
        }
    }
    public void ChangeItemSlot(int index)
    {
        if (IsOutRange(index)) return;

        var currentItem = GetCurrentItemComponent;
        if (currentItem != null && currentItem.IsBothHandGrab) return;

        SetCurrentItemActive(false);
        currentItemSlot = index;
        SetCurrentItemActive(true);
        UIController.Instance.ui_Game.ItemSelection(index);
    }
    public void UseItem()
    {
        GetCurrentItemComponent?.UseItem();
    }
    public void SetCurrentItemActive(bool isActive)
    {
        Item item = GetCurrentItemComponent;
        if (item != null)
            GameRoomNetworkManager.Instance.OnSetActiveObject(ObjectReference.Instance.GetIdByGameObject(item.gameObject), isActive);
    }
}
