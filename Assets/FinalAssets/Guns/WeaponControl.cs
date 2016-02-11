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

    public int weaponDamage;

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

    [Header("TPositions")]

    public Vector3 TEquipPosition;
    public Vector3 TEquipRotation;
    public Vector3 TUnEquipPosition;
    public Vector3 TUnEquipRotation;


    [Header("CTPositions")]
    public Vector3 CTEquipPosition;
    public Vector3 CTEquipRotation;
    public Vector3 CTUnEquipPosition;
    public Vector3 CTUnEquipRotation;


    Vector3 scale;
    public enum RestPosition { RightHip, Waist, LeftHip, Back };
    public RestPosition restPosition;
    public string owner;
    public bool hasOwner;
    private Vector3 EquipPosition;
    private Vector3 EquipRotation;
    private Vector3 UnEquipPosition;
    private Vector3 UnEquipRotation;

    // Use this for initialization
    void Start()
    {

        /*bulletSpawnGO = Instantiate(bulletPrefab, transform.position, Quaternion.identity) as GameObject;
        bulletSpawnGO.AddComponent<ParticleDirection>();
        bulletSpawnGO.GetComponent<ParticleDirection>().weapon = bulletSpawn;
        bulletPart = bulletSpawnGO.GetComponent<ParticleSystem>();*/

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
            var yo = GetComponentInParent<WeaponManager>();
            var go = yo.gameObject.GetComponentInChildren<Animator>();
            //Debug.Log("poop");
            if (yo.GetComponent<CharacterStats>().Id == "T")
            {
                EquipPosition = TEquipPosition;
                EquipRotation = TEquipRotation;
            }
            if (yo.GetComponent<CharacterStats>().Id == "CT")
            {
                EquipPosition = CTEquipPosition;
                EquipRotation = CTEquipRotation;
            }
            transform.parent = transform.GetComponentInParent<WeaponManager>().gameObject.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
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

                var yo = transform.GetComponentInParent<WeaponManager>().gameObject;
                var go = transform.GetComponentInParent<WeaponManager>().transform.GetComponentInChildren<Animator>();
                switch (restPosition)
                {
                    case RestPosition.RightHip:
                        transform.parent = transform.GetComponentInParent<WeaponManager>().transform.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightUpperLeg);
                        break;
                    case RestPosition.Waist:
                        transform.parent = transform.GetComponentInParent<WeaponManager>().transform.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Spine);
                        break;
                    case RestPosition.LeftHip:
                        transform.parent = transform.GetComponentInParent<WeaponManager>().transform.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                        break;
                    case RestPosition.Back:
                        transform.parent = transform.GetComponentInParent<WeaponManager>().transform.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftShoulder);
                        break;
                }
                if (yo.GetComponent<CharacterStats>().Id == "T")
                {
                    UnEquipPosition = TUnEquipPosition;
                    UnEquipRotation = TUnEquipRotation;
                }
                if (yo.GetComponent<CharacterStats>().Id == "CT")
                {
                    UnEquipPosition = CTUnEquipPosition;
                    UnEquipRotation = CTUnEquipRotation;
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
                    if (pickableItem.Owner)
                    {
                        if (pickableItem.Owner.GetComponent<HumanUserControl>())
                        {
                            pickableItem.Owner.GetComponent<HumanUserControl>().CanPickUp = true;
                            pickableItem.Owner.GetComponent<HumanUserControl>().Item = this.gameObject;
                        }
                        else if (pickableItem.Owner.GetComponent<EnemyAI>())
                        {

                            pickableItem.Owner.GetComponent<EnemyAI>().CanPickUp = true;
                            pickableItem.Owner.GetComponent<EnemyAI>().Item = this.gameObject;
                        }
                    }
                }
                else
                {
                    if (pickableItem.Owner)
                    {
                        {
                            if (pickableItem.Owner.GetComponent<HumanUserControl>())
                            {
                                pickableItem.Owner = null;
                            }
                            else if (pickableItem.Owner.GetComponent<EnemyAI>())
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
