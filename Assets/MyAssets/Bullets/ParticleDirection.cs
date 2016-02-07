using UnityEngine;
using System.Collections;

public class ParticleDirection : MonoBehaviour
{

    public Transform weapon;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = weapon.TransformPoint(Vector3.zero);
        transform.forward = weapon.TransformDirection(Vector3.forward);
    }
    void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.GetComponent<Rigidbody>())
        {
            Vector3 direction = other.transform.position - transform.position;
            direction = direction.normalized;
            other.GetComponent<Rigidbody>().AddForce(direction * 50);
        }
    }
}
