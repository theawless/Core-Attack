using UnityEngine;
using System.Collections;

public class PickableItem : MonoBehaviour
{
    public bool CharacterInTrigger;
    public GameObject Owner;

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("A");
        //Debug.Log(other.name);
        if (other.GetComponent<CharacterStats>())
        {
            //  Debug.Log("B");
            CharacterInTrigger = true;
            if (!Owner)
            {
                ///    Debug.Log("C");
                Owner = other.gameObject;
            }
        }//Debug.Log("END");
        if (other.GetComponentInParent<CharacterStats>())
        {
            // Debug.Log("E");
            CharacterInTrigger = true;
            if (!Owner)
            {
                //   Debug.Log("C");
                Owner = other.GetComponentInParent<CharacterStats>().gameObject;
            }
        }
    }/*

    void OnTriggerEnter(Collider[] other)
    {
        Debug.Log(other.Length.ToString());
        foreach (var v in other)
        {
            Debug.Log(v.name);
            if (v.GetComponent<CharacterStats>())
            {
                Debug.Log("B");
                CharacterInTrigger = true;
                if (!Owner)
                {
                    Debug.Log("C");
                    Owner = v.gameObject;
                }
            }
        }
        Debug.Log("END");
    }*/
    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterStats>())
        {
            if (Owner != null)
            {
                if (other.GetComponent<CharacterStats>().gameObject == Owner)
                {
                    CharacterInTrigger = false;
                    //Owner = null;
                }
            }
        }
    }
}
