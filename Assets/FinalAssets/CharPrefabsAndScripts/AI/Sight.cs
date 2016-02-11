using UnityEngine;
using System.Collections;

public class Sight : MonoBehaviour
{

    CharacterStats charStat;
    EnemyAI enAi;

    void Start()
    {
        charStat = GetComponentInParent<CharacterStats>();
        enAi = GetComponentInParent<EnemyAI>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterStats>())
        {
           // Debug.Log("1");
            if (other.GetComponent<CharacterStats>().Id != charStat.Id)
            {
            //    Debug.Log("2");
                if (!enAi.Enemies.Contains(other.gameObject))
                {
                    enAi.Enemies.Add(other.gameObject);
                }
            }
        }
        if (other.GetComponentInParent<CharacterStats>())
        {
           // Debug.Log("10");
            if (other.GetComponentInParent<CharacterStats>().Id != charStat.Id)
            {
            //    Debug.Log("20");
                if (!enAi.Enemies.Contains(other.GetComponentInParent<CharacterStats>().gameObject))
                {
                    enAi.Enemies.Add(other.GetComponentInParent<CharacterStats>().gameObject);
                }
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (enAi.Enemies.Contains(other.gameObject))
            enAi.Enemies.Remove(other.gameObject);
    }

}
