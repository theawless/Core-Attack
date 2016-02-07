using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{

    private CapsuleCollider col;
    public bool walkByDefault = false;

    private CharMove character;
    private Transform cam;
    private Vector3 camForward;
    private Vector3 move;
    [SerializeField]
    private bool aim;
    private float aimingWeight;
    public Vector3 normalState = new Vector3(0, 0, -2f);
    public Vector3 aimingState = new Vector3(0, 0, -0.5f);
    public bool lookInCameraDirection = true;
    Vector3 lookPos;
    Animator anim;
    public bool debugShoot;
    [SerializeField]
    WeaponManager weaponManager;
    WeaponManager.WeaponType weaponType;
    float startHeight;



    [Serializable]
    public class IK
    {
        public Transform spine;
        public float aimingZ = -213.46f;
        public float aimingX = -65.93f;
        public float aimingY = 20.1f;
        public float point = 30;
        public bool DebugAim;
    }
    [SerializeField]
    public IK ik;

    FreeCameraLook cameraFunctions;

    //itemsystem
    public bool CanPickUp;
    public GameObject Item;
    public GameObject pickText;
    public Text curAmmo;
    public Text carryingAmmo;

    void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        col = GetComponent<CapsuleCollider>();
        startHeight = col.height;
        character = GetComponent<CharMove>();
        anim = GetComponent<Animator>();

        weaponManager = GetComponent<WeaponManager>();
        cameraFunctions = Camera.main.transform.root.GetComponent<FreeCameraLook>();
        offsetCross = cameraFunctions.crosshairOffsetWiggle;
    }

    void CorrectIK()
    {
        weaponType = weaponManager.weaponType;
        if (!ik.DebugAim)
        {
            switch (weaponType)
            {
                case WeaponManager.WeaponType.Pistol:
                    ik.aimingZ = 212.19f;
                    ik.aimingX = -63.8f;
                    ik.aimingY = 16.65f;
                    break;
                case WeaponManager.WeaponType.Rifle:
                    ik.aimingX = 212.19f;
                    ik.aimingX = -66.8f;
                    ik.aimingY = 14.65f;
                    break;
            }
        }
    }

    void AdditionalInput()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            anim.SetTrigger("Vault");

        }
    }
    void HandleCurves()
    {
        float sizeCurve = anim.GetFloat("ColliderSize");
        float newYcenter = 0.3f;
        float lerpCenter = Mathf.Lerp(1, newYcenter, sizeCurve);
        col.center = new Vector3(0, lerpCenter, 0);
        col.height = Mathf.Lerp(startHeight, 0.5f, sizeCurve);
    }

    void Update()
    {

        if (!ik.DebugAim)
        {
            aim = Input.GetMouseButton(1);
        }
        weaponManager.aim = aim;
        bool canFire = SharedFunctions.CheckAmmo(weaponManager.ActiveWeapon);
        if (aim)
        {
            if (!weaponManager.ActiveWeapon.CanBurst)
            {
                if (Input.GetMouseButtonDown(0) || debugShoot)
                {
                    if (canFire)
                    // weaponManager.FireActiveWeapon();
                    {
                        anim.SetTrigger("Fire");
                        ShootRay();
                        cameraFunctions.WiggleCrosshairAndCamera(weaponManager.ActiveWeapon, true);
                        weaponManager.ActiveWeapon.curAmmo--;
                    }
                    else
                    {
                        weaponManager.ReloadActiveWeapon();
                        anim.SetTrigger("Reload");
                    }
                }
            }
            else
            {
                if ((Input.GetMouseButton(0) && !anim.GetCurrentAnimatorStateInfo(2).IsTag("Reload")) || debugShoot)
                {
                    if (canFire)
                    // weaponManager.FireActiveWeapon();
                    {
                        anim.SetTrigger("Fire");
                        ShootRay();
                        cameraFunctions.WiggleCrosshairAndCamera(weaponManager.ActiveWeapon, true);
                        weaponManager.ActiveWeapon.curAmmo--;
                    }
                    else
                    {
                        weaponManager.ReloadActiveWeapon();
                        anim.SetTrigger("Reload");
                    }
                }
            }

        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (Input.GetAxis("Mouse ScrollWheel") < -0.0f)
            {
                // Debug.Log("MouseWheel");
                weaponManager.ChangeWeapon(false);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0.0f)
            {
                // Debug.Log("MouseWheel2");
                weaponManager.ChangeWeapon(true);
            }
        }
        AdditionalInput();
        //HandleCurves();
        PickupItem();
        UpdateUI();
    }

    void UpdateUI()
    {
        curAmmo.text = weaponManager.ActiveWeapon.curAmmo.ToString();
        carryingAmmo.text = weaponManager.ActiveWeapon.curCarryingAmmo.ToString();
    }

    void PickupItem()
    {
        if (CanPickUp)
        {
            if (!pickText.activeInHierarchy)
            {
                pickText.SetActive(true);
            }
            if (Input.GetKeyUp(KeyCode.F))
            {
                SharedFunctions.PickupItem(this.gameObject, Item);
                CanPickUp = false;
            }
        }
        else
        {
            if (pickText.activeInHierarchy)
            {
                pickText.SetActive(false);
            }
        }
    }

    public GameObject bulletPrefab;
    void ShootRay()
    {
        float x = Screen.width / 2;
        float y = Screen.height / 2;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;
        GameObject go = Instantiate(bulletPrefab, transform.position, Quaternion.identity) as GameObject;
        LineRenderer line = go.GetComponent<LineRenderer>();
        Vector3 startPos = weaponManager.ActiveWeapon.bulletSpawn.TransformPoint(Vector3.zero);
        Vector3 endpos = Vector3.zero;
        int mask = ~(1 << 8);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            float distance = Vector3.Distance(weaponManager.ActiveWeapon.bulletSpawn.transform.position, hit.point);
            RaycastHit[] hits = Physics.RaycastAll(startPos, hit.point - startPos, distance);
            foreach (RaycastHit hit2 in hits)
            {
                if (hit2.transform.GetComponent<Rigidbody>())
                {
                    Vector3 direction = hit2.transform.position - transform.position;
                    direction = direction.normalized;
                    Debug.Log("hitting this mofo" + hit2.transform.name);
                    hit2.transform.GetComponent<Rigidbody>().AddForce(direction * 2000);
                }
                //else
            }
            endpos = hit.point;
        }
        else
        {
            Debug.Log("HiitingTheSkyAsshole");
            endpos = ray.GetPoint(200);
        }
        line.SetPosition(0, startPos);
        line.SetPosition(1, endpos);

    }
    private void LateUpdate()
    {
        aimingWeight = Mathf.MoveTowards(aimingWeight, aim ? 1.0f : 0.0f, Time.deltaTime * 5);
        Vector3 pos = Vector3.Lerp(normalState, aimingState, aimingWeight);
        cam.localPosition = pos;

        if (aim)
        {
            Vector3 eulerAnglesOffset = Vector3.zero;
            eulerAnglesOffset = new Vector3(ik.aimingX, ik.aimingY, ik.aimingZ);
            Ray ray = new Ray(cam.position, cam.forward);
            Vector3 lookPosition = ray.GetPoint(ik.point);
            ik.spine.LookAt(lookPosition);
            ik.spine.Rotate(eulerAnglesOffset, Space.Self);
        }
    }

    float horizontal;
    float vertical;
    float offsetCross;

    void FixedUpdate()
    {

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (horizontal < -offsetCross || horizontal > offsetCross || vertical < -offsetCross || vertical > offsetCross)
        {
            cameraFunctions.WiggleCrosshairAndCamera(weaponManager.ActiveWeapon, false);
        }
        if (!aim)
        {
            if (cam != null)
            {
                camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                move = vertical * camForward + horizontal * cam.right;
            }
            else
            {
                move = vertical * Vector3.forward + horizontal * Vector3.right;
            }
        }
        else
        {
            move = Vector3.zero;
            Vector3 dir = lookPos - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
            anim.SetFloat("Forward", vertical);
            anim.SetFloat("Turn", horizontal);
        }
        if (move.magnitude > 1)
            move.Normalize();

        bool walkToggle = Input.GetKey(KeyCode.LeftShift) || aim;

        float walkMultiplier = 1;

        if (walkByDefault)
        {
            if (walkToggle)
            {
                walkMultiplier = 1;
            }
            else
            {
                walkMultiplier = 0.5f;
            }
        }
        else
        {
            if (walkToggle)
            {
                walkMultiplier = 0.5f;
            }
            else
            {
                walkMultiplier = 1;
            }
        }
        lookPos = lookInCameraDirection && cam != null ? transform.position + cam.forward * 100 : transform.position + transform.forward * 100;

        move *= walkMultiplier;
        character.Move(move, aim, lookPos);
    }

}