using UnityEngine;
using System.Collections;

public class PickableItem : MonoBehaviour {
    public bool CharacterInTrigger;
    public GameObject Owner;

    void OnTriggerEnter(Collider other)
    {
       if(other.GetComponent<CharacterStats>())
        {
            CharacterInTrigger = true;
            if(!Owner)
            {
                Owner = other.gameObject;
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.GetComponent<CharacterStats>())
        {
            if(Owner!=null)
            {
                if(other.GetComponent<CharacterStats>().gameObject==Owner)
                {
                    CharacterInTrigger = false;
                    //Owner = null;
                }
            }
        }
    }
}
