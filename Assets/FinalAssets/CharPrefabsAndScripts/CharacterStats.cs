using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class CharacterStats : MonoBehaviour
{
    Animator anim;
    public string Id = "";
    public float Health = 100;
    public bool isDead = false;
    WeaponManager wM;
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        wM = GetComponent<WeaponManager>();
    }

    void Update()
    {
        if (Health <= 0)
        {
            isDead = true;
        }
        else
        {
            isDead = false;
        }
        if (isDead == true)
        {
            Die();
        }
    }

    public void HitDetect(Transform transform, float wpndmg)
    {
        Health = Health - wpndmg;
     //   Debug.Log("We are here");
        if (transform == anim.GetBoneTransform(HumanBodyBones.Head))
        {
            Health -= wpndmg;
        }

        if (transform == anim.GetBoneTransform(HumanBodyBones.Chest))
        {
            Health -= wpndmg / 5;
        }

        if (transform == (anim.GetBoneTransform(HumanBodyBones.LeftUpperArm) || transform == anim.GetBoneTransform(HumanBodyBones.RightUpperArm)))
        {
            Health -= wpndmg / 7.5f;
        }
        if (transform == (anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg) || transform == anim.GetBoneTransform(HumanBodyBones.RightUpperLeg)))
        {
            Health -= wpndmg / 10;
        }
        else
        {
            Health -= wpndmg / 11;
        }
    }
    public void Die()
    {
        foreach (var go in wM.WeaponList)
        {
            var yo = go.GetComponent<WeaponControl>();
            yo.hasOwner = false;
            yo.equip = false;
            yo.transform.parent = null;
            yo.owner = null;
            //wM.WeaponList.Remove(go);
            if (gameObject.GetComponent<HumanUserControl>())
            {
                menuScript.gameon = false;
                SceneManager.LoadScene("start");

            }
            Destroy(gameObject);
        }

    }
}
