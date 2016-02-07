using UnityEngine;
using System.Collections;

public class SharedFunctions : MonoBehaviour
{
    public static void PickupItem(GameObject owner, GameObject item)
    {
        ItemID.ItemType id = item.GetComponent<ItemID>().itemType;

        switch (id)
        {
            case ItemID.ItemType.Health:
                break;
            case ItemID.ItemType.Weapon:
                WeaponManager wM = owner.GetComponent<WeaponManager>();
                if (wM.WeaponList.Count < wM.MaxCarryingWeapons)
                {

                    wM.WeaponList.Add(item);
                }
                else
                {
                    wM.ActiveWeapon.equip = false;
                    wM.ActiveWeapon.hasOwner = false;
                    wM.ActiveWeapon.transform.parent = null;
                    GameObject removeWeapon = null;
                    foreach (GameObject go in wM.WeaponList)
                    {
                        if (go == wM.ActiveWeapon.gameObject)
                        {
                            removeWeapon = go;
                        }
                    }
                    wM.WeaponList.Remove(removeWeapon);
                    wM.WeaponList.Add(item);
                }
                item.transform.parent = owner.transform;
                item.GetComponent<WeaponControl>().hasOwner = true;
                break;
            case ItemID.ItemType.Wearable:
                break;

        }

    }

    public static bool CheckAmmo(WeaponControl AW)
    {
        if(AW.curAmmo>0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
