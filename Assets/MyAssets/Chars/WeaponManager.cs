using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class WeaponManager : MonoBehaviour
{
    public List<GameObject> WeaponList = new List<GameObject>();
    public WeaponControl ActiveWeapon;
    int weaponNumber = 0;

    public int MaxCarryingWeapons = 3;
    public bool aim;

    public enum WeaponType
    {
        Pistol, Rifle
    }
    //customIK CustomIK;
    Animator anim;

    float IKweight;


    public WeaponType weaponType;

    // Use this for initialization
    void Start()
    {
        //Debug.Log(weaponNumber.ToString());
        ActiveWeapon = WeaponList[weaponNumber].GetComponent<WeaponControl>();
        ActiveWeapon.equip = true;
        anim = GetComponentInChildren<Animator>();
        foreach (GameObject go in WeaponList)
        {
            go.GetComponent<WeaponControl>().hasOwner = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        IKweight = Mathf.MoveTowards(IKweight, (aim) ? 1.0f : 0.0f, Time.deltaTime * 5);
        ActiveWeapon = WeaponList[weaponNumber].GetComponent<WeaponControl>();
        ActiveWeapon.equip = true;
        weaponType = ActiveWeapon.weaponType;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                anim.SetInteger("Weapon", 0);
                break;
            case WeaponType.Rifle:
                anim.SetInteger("Weapon", 1);
                break;
        }

    }

    void OnAnimatorIK()
    {
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, IKweight);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, IKweight);

        Vector3 pos = ActiveWeapon.HandPosition.transform.TransformPoint(Vector3.zero);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, ActiveWeapon.HandPosition.transform.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, ActiveWeapon.HandPosition.transform.rotation);

    }

    public void FireActiveWeapon()
    {
        if (ActiveWeapon != null)
        {
            ActiveWeapon.Fire();
        }
    }
    public void ChangeWeapon(bool Ascending)
    {
        if (WeaponList.Count > 1)
        {
            ActiveWeapon.equip = false;
            if (Ascending)
            {
                if (weaponNumber < WeaponList.Count - 1)
                {
                    weaponNumber++;
                }
                else
                {
                    weaponNumber = 0;
                }
            }
            else
            {
                if (weaponNumber > 0)
                {
                    weaponNumber--;
                }
                else
                {
                    weaponNumber = WeaponList.Count - 1;
                }
            }
        }
    }

    internal void ReloadActiveWeapon()
    {
        int cur = ActiveWeapon.maxCarryingAmmo - (ActiveWeapon.maxCarryingAmmo - ActiveWeapon.curCarryingAmmo);
        if(cur>0)
        {
            if(cur>ActiveWeapon.MaxClipAmmo)
            {
                ActiveWeapon.curAmmo = ActiveWeapon.MaxClipAmmo;
                ActiveWeapon.curCarryingAmmo -= ActiveWeapon.MaxClipAmmo;
            }
            else
            {
                ActiveWeapon.curAmmo = cur;
                ActiveWeapon.curCarryingAmmo -= cur;
            }
        }
    }
}
