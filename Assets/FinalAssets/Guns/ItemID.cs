using UnityEngine;
using System.Collections;

public class ItemID : MonoBehaviour {

    public ItemType itemType;
    public enum ItemType
    {
        Weapon,
        Health,
        Wearable
    }
}
