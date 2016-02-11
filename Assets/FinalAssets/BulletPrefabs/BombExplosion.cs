using UnityEngine;
using System.Collections;

public class BombExplosion : MonoBehaviour
{
    AudioSource blast;
    public GameObject explosion;
    AudioClip boom;
    void Start()
    {
        var col = gameObject.AddComponent<BoxCollider>();
        col.size = new Vector3(0.1f, 0.1f, 0.1f);
        blast = GetComponent<AudioSource>();
        
        StartCoroutine("selfDestruct");
    }
    IEnumerator selfDestruct()
    {
        
        yield return new WaitForSeconds(5f);
        blast.PlayOneShot(boom);
        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
