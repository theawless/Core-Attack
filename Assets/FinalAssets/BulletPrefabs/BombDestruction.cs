using UnityEngine;
using System.Collections;

public class BombDestruction : MonoBehaviour {
    //public GameObject explosion;
   AudioSource ass;
    void Start()
    {
        ass = GetComponent<AudioSource>();
        ass.Play();
        StartCoroutine("selfDestruct");
    }
    IEnumerator selfDestruct()
    {
        yield return new WaitForSeconds(5f);
        //Instantiate(explosion, transform.position, transform.rotation);
        
        Destroy(gameObject);
    }
}
