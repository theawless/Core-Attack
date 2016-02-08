using System;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class New_TPUserControl : MonoBehaviour
{

    private CapsuleCollider col;
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

    float h;
    float v;
    float offsetCross;
    private GameObject bulletPrefab;

    public New_Movement m_CharacterAim;
    /////////////////////////////////////////////////////////////////////

    private Yo_TPCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    public bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
    private bool a_wanttorun;

    private void Start()
    {
        // get the transform of the main camera
        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning(
                "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
        }
        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<Yo_TPCharacter>();
        m_CharacterAim = GetComponent<New_Movement>();
        col = GetComponent<CapsuleCollider>();
        startHeight = col.height;
        anim = GetComponent<Animator>();

        weaponManager = GetComponent<WeaponManager>();
        cameraFunctions = Camera.main.transform.root.GetComponent<FreeCameraLook>();
        offsetCross = cameraFunctions.crosshairOffsetWiggle;
    }


    private void Update()
    {
        ShootManager();
        if (!m_Jump)
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");

        }
        m_CharacterAim.m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        //AdditionalInput();
        //HandleCurves();
        PickupItem();
        UpdateUI();
        
        ScrollManager();
    }


    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {

       
        // read inputs
        h = CrossPlatformInputManager.GetAxis("Horizontal");
        v = CrossPlatformInputManager.GetAxis("Vertical");
        bool crouch = Input.GetKey(KeyCode.C);
        if ((h < -offsetCross || h > offsetCross || v < -offsetCross || v > offsetCross) && aim)
        {
            cameraFunctions.WiggleCrosshairAndCamera(weaponManager.ActiveWeapon, false);
        }
        lookPos = lookInCameraDirection && m_Cam != null ? transform.position + m_Cam.forward * 100 : transform.position + transform.forward * 100;

        if (!aim)
        {
            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
#if !MOBILE_INPUT
            // walk speed multiplier
            if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump, aim, lookPos);
            m_Jump = false;

        }
        else
        {
            m_Move = Vector3.zero;
            Vector3 dir = lookPos - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
            m_CharacterAim.ChangeMovementScript(h, v, Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized, m_Cam.right, a_wanttorun);
            m_Jump = false;
            //anim.SetFloat("Forward", v);
            //anim.SetFloat("Turn", h);
        }


    }
    private void LateUpdate()
    {
        aimingWeight = Mathf.MoveTowards(aimingWeight, aim ? 1.0f : 0.0f, Time.deltaTime * 5);
        Vector3 pos = Vector3.Lerp(normalState, aimingState, aimingWeight);
        m_Cam.localPosition = pos;

        if (aim)
        {
            Vector3 eulerAnglesOffset = Vector3.zero;
            eulerAnglesOffset = new Vector3(ik.aimingX, ik.aimingY, ik.aimingZ);
            Ray ray = new Ray(m_Cam.position, m_Cam.forward);
            Vector3 lookPosition = ray.GetPoint(ik.point);
            ik.spine.LookAt(lookPosition);
            ik.spine.Rotate(eulerAnglesOffset, Space.Self);
        }
    }
    private void ShootManager()
    {
        if (!ik.DebugAim)
        {
            aim = Input.GetMouseButton(1);
            anim.SetBool("Aim", aim);
            //anim.enabled = false;
        }
        weaponManager.aim = aim;
        bool canFire = SharedFunctions.CheckAmmo(weaponManager.ActiveWeapon);
        if (aim)
        {
            if (!weaponManager.ActiveWeapon.CanBurst)
            {
                if (Input.GetMouseButtonDown(0)  && !anim.GetCurrentAnimatorStateInfo(2).IsTag("Reload") || debugShoot)
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
    }
    private void ScrollManager()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (Input.GetAxis("Mouse ScrollWheel") < -0.0f)
            {
                weaponManager.ChangeWeapon(false);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0.0f)
            {
                weaponManager.ChangeWeapon(true);
            }
        }
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
    void ShootRay()
    {
        float x = Screen.width / 2;
        float y = Screen.height / 2;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;
        bulletPrefab = weaponManager.ActiveWeapon.bulletPrefab;
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
                    Debug.Log("hitting this mofo: " + hit2.transform.name);
                    hit2.transform.GetComponent<Rigidbody>().AddForce(direction * 2000);
                }
            }
            endpos = hit.point;
        }
        else
        {
            Debug.Log("Hiiting The Sky Asshole");
            endpos = ray.GetPoint(200);
        }
        line.SetPosition(0, startPos);
        line.SetPosition(1, endpos);

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
}
