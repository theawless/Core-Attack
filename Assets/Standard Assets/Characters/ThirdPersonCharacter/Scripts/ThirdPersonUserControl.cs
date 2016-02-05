using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
        [SerializeField]
        private bool aim;
        [SerializeField]
        private float aimingWeight;
        public Vector3 normalState = new Vector3(0, 0, -2f);
        public Vector3 aimingState = new Vector3(0, 0, -0.5f);
        public bool lookInCameraDirection;
        Vector3 lookPos;
        Animator anim;
        bool crouch;
        //IK initializations
        public Transform spine;
        public float aimingZ = -70f;
        public float aimingX = -0f;
        public float aimingY = 70f;
        public float point = 10;

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
            m_Character = GetComponent<ThirdPersonCharacter>();
            anim = GetComponent<Animator>();
        }

        private void LateUpdate()
        {
            aimingState.y = (crouch) ? -0.5f : 0f;
            aimingWeight = Mathf.MoveTowards(aimingWeight, aim ? 1.0f : 0.0f, Time.deltaTime * 5);
            Vector3 pos = Vector3.Lerp(normalState, aimingState, aimingWeight);
            m_Cam.localPosition = pos;

            if (aim)
            {
                Vector3 eulerAnglesOffset = Vector3.zero;
                eulerAnglesOffset = new Vector3(aimingX, aimingY, aimingZ);
                Ray ray = new Ray(m_Cam.position, m_Cam.forward);
                Vector3 lookPosition = ray.GetPoint(point);
                spine.LookAt(lookPosition);
                spine.Rotate(eulerAnglesOffset, Space.Self);
            }
        }


        private void Update()
        {

            aim = Input.GetMouseButton(1);
            //aim = !Input.GetButton("ChangeView");
            if (aim && Input.GetMouseButton(0))
            {
                anim.SetTrigger("Fire");
            }
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            crouch = Input.GetKey(KeyCode.C);

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

            if (aim)
            {
                Vector3 dir = lookPos - transform.position;
                dir.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
                anim.SetFloat("Forward", v);
                anim.SetFloat("Turn", h);
            }
#if !MOBILE_INPUT
            // walk speed multiplier
            if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

            lookPos = lookInCameraDirection && m_Cam != null ? transform.position + m_Cam.forward * 100 : transform.position + transform.forward * 100;
            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump, aim, lookPos);
            m_Jump = false;
        }
    }
}
