using UnityEngine;
using System.Collections;

public class BulletAutoDestruct : MonoBehaviour
{

    void Start()
    {
             StartCoroutine("selfDestruct");
    }
    IEnumerator selfDestruct()
    {
        yield return new WaitForSeconds(0.3f);
            Destroy(gameObject);
    }
}
