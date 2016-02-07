using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ItemID))]
[RequireComponent(typeof(Rigidbody))]

public class WeaponControl : MonoBehaviour
{
    public bool equip;
    public WeaponManager.WeaponType weaponType;
    public int MaxAmmo;
    public int MaxClipAmmo = 30;

    public int curCarryingAmmo;
    public int maxCarryingAmmo;


    public int curAmmo;
    public bool CanBurst;
    public float Kickback = 0.3f;
    public GameObject HandPosition;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    GameObject bulletSpawnGO;
    ParticleSystem bulletPart;

    WeaponManager parentControl;

    bool fireBullet;
    AudioSource audioSource;
    Animator weaponAnim;

    Rigidbody rigidBody;
    BoxCollider boxCol;
    PickableItem pickableItem;

    [Header("Positions")]
    public bool hasOwner;
    public Vector3 EquipPosition;
    public Vector3 EquipRotation;
    public Vector3 UnEquipPosition;
    public Vector3 UnEquipRotation;
    Vector3 scale;
    public enum RestPosition { RightHip, Waist };
    public RestPosition restPosition;
    // Use this for initialization
    void Start()
    {

        /*bulletSpawnGO = Instantiate(bulletPrefab, transform.position, Quaternion.identity) as GameObject;
        bulletSpawnGO.AddComponent<ParticleDirection>();
        bulletSpawnGO.GetComponent<ParticleDirection>().weapon = bulletSpawn;
        bulletPart = bulletSpawnGO.GetComponent<ParticleSystem>();
        */
        curCarryingAmmo = maxCarryingAmmo;
        curAmmo = MaxClipAmmo;
        pickableItem = GetComponentInChildren<PickableItem>();
        curAmmo = MaxClipAmmo;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        boxCol = GetComponent<BoxCollider>();

        audioSource = GetComponent<AudioSource>();
        scale = transform.localScale;
        weaponAnim = GetComponent<Animator>();


    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = scale;
        if (equip)
        {
            if (pickableItem.gameObject.activeInHierarchy)
            {
                pickableItem.gameObject.SetActive(false);
            }
            transform.parent = transform.GetComponentInParent<WeaponManager>().transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
            transform.localPosition = EquipPosition;
            transform.localRotation = Quaternion.Euler(EquipRotation);
      
        }
        else
        {
            if (hasOwner)
            {
                //Debug.Log(pickableItem.name);
                if (pickableItem.gameObject.activeInHierarchy)
                {
                    pickableItem.gameObject.SetActive(false);
                }
                boxCol.isTrigger = true;
                rigidBody.isKinematic = true;

                switch (restPosition)
                {
                    case RestPosition.RightHip:
                        transform.parent = transform.GetComponentInParent<WeaponManager>().transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightUpperLeg);

                        break;
                    case RestPosition.Waist:
                        transform.parent = transform.GetComponentInParent<WeaponManager>().transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Spine);
                        break;
                }
                transform.localPosition = UnEquipPosition;
                transform.localRotation = Quaternion.Euler(UnEquipRotation);
            }
            else
            {
                if (!pickableItem.gameObject.activeInHierarchy)
                {
                    pickableItem.gameObject.SetActive(true);
                }
                boxCol.isTrigger = false;
                rigidBody.isKinematic = false;
                if (pickableItem.CharacterInTrigger)
                {
                    if (pickableItem.Owner.GetComponent<UserInput>())
                    {
                        pickableItem.Owner.GetComponent<UserInput>().CanPickUp = true;
                        pickableItem.Owner.GetComponent<UserInput>().Item = this.gameObject;

                    }
                }
                else
                {
                    if (pickableItem.Owner)
                    {
                        {
                            if (pickableItem.Owner.GetComponent<UserInput>())
                            {
                                pickableItem.Owner = null;
                            }
                        }

                    }
                }
            }
        }
    }
    public void Fire()
    {

        fireBullet = true;
    }
}
