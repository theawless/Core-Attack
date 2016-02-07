using UnityEngine;
using System.Collections;

public class customIK : MonoBehaviour
{

    Animator anim;

    //Our bones
    Transform upperArm;
    Transform forearm;
    Transform hand;

    //our targets
    public Transform target;
    public Transform elbowTarget;

    //is it enable
    public bool IsEnabled;
    public float Weight = 1;

    //Starting rotations
   Quaternion upperArmStartRotation, forearmStartRotation, handStartRotation;
  
    //relative starting positions
    Vector3 targetRelativeStartPosition, elbowTargetRelativeStartPosition;

    //helper gos that are used every frame
    GameObject upperArmAxisCorrection, forearmAxisCorrection, handAxisCorrection;

    //hold last positions so recalculation is only done if needed
    private Vector3 lastUpperArmPosition, lastTargetPosition, lastElbowTargetPosition;


    void Start()
    {
        anim = GetComponentInParent<Animator>();
       // hand = GetComponentInParent<CharMove>().Lefthand;

        //assign the starting rotations
        forearm = hand.parent;
        upperArm = forearm.parent;
        upperArmStartRotation = upperArm.rotation;
        forearmStartRotation = forearm.rotation;
        handStartRotation = hand.rotation;
      
        //assign realtive starting positions
        elbowTargetRelativeStartPosition = elbowTarget.position - upperArm.position;

        //Create helper GOs
        upperArmAxisCorrection = new GameObject("upperArmAxisCorrection");
        forearmAxisCorrection = new GameObject("forearmAxisCorrection");
        handAxisCorrection = new GameObject("handAxisCorrection");

        //set helper hierarchy
        upperArmAxisCorrection.transform.parent = transform;
        forearmAxisCorrection.transform.parent = upperArmAxisCorrection.transform;
        handAxisCorrection.transform.parent = forearmAxisCorrection.transform;

        //guarantee first-frame update
        lastUpperArmPosition = upperArm.position + 5 * Vector3.up;

    }

    void Update()
    {
        if (anim.GetCurrentAnimatorStateInfo(2).IsTag("Aim"))
        { IsEnabled = true; }
        else
        {
            IsEnabled = false;
        }
    }

    void LateUpdate()
    {
        if (IsEnabled)
        {
            return;
        }
        CalculateIK();
    }

    void CalculateIK()
    {
        //if we have no target then reset the relative position
        if (target == null)
        {
            targetRelativeStartPosition = Vector3.zero;
            return;
        }

        //if we have a target and the relative start position is zeroed
        if (targetRelativeStartPosition == Vector3.zero && target != null)
        {
            targetRelativeStartPosition = target.position - upperArm.position;
        }

        //save our positions
        lastUpperArmPosition = upperArm.position;
        lastTargetPosition = target.position;
        lastElbowTargetPosition = elbowTarget.position;

        //Calculate ikAngle variable which defines the angle that will be subtracted from the upperarm axis
        float upperArmLength = Vector3.Distance(upperArm.position, forearm.position);
        float forearmLength = Vector3.Distance(forearm.position, hand.position);

        //find the arm length
        float armLength = upperArmLength + forearmLength;
        //find the hypoethenuse,  is the longest side of a right-angled triangle 90o degrees angle
        float hypotenuse = upperArmLength;

        //find the distance betwen our upperarm and the target
        float targetDistance = Vector3.Distance(upperArm.position, target.position);

        //Do not allow target distance be further away than the arm's length.
        targetDistance = Mathf.Min(targetDistance, armLength - 0.0001f);

        //(of a pair of angles) formed on the same side of a straight line when intersected by another line.
        float adjacent = (hypotenuse * hypotenuse - forearmLength * forearmLength + targetDistance * targetDistance) / (2 * targetDistance);

        //find the ik Angle
        float ikAngle = Mathf.Acos(adjacent / hypotenuse) * Mathf.Rad2Deg;

        //Store pre-ik info.
        Vector3 targetPosition = target.position;
        Vector3 elbowTargetPosition = elbowTarget.position;

        //Store the parent of each bone
        Transform upperArmParent = upperArm.parent;
        Transform forearmParent = forearm.parent;
        Transform handParent = hand.parent;

        //Store the scale
        Vector3 upperArmScale = upperArm.localScale;
        Vector3 forearmScale = forearm.localScale;
        Vector3 handScale = hand.localScale;

        //Store the local positions
        Vector3 upperArmLocalPosition = upperArm.localPosition;
        Vector3 forearmLocalPosition = forearm.localPosition;
        Vector3 handLocalPosition = hand.localPosition;

        //Store the rotations
        Quaternion upperArmRotation = upperArm.rotation;
        Quaternion forearmRotation = forearm.rotation;
        Quaternion handRotation = hand.rotation;
        Quaternion handLocalRotation = hand.localRotation;

        //Reset arm so that the ik starts from a known postion
        target.position = targetRelativeStartPosition + upperArm.position;
        elbowTarget.position = elbowTargetRelativeStartPosition + upperArm.position;
        upperArm.rotation = upperArmStartRotation;
        forearm.rotation = forearmStartRotation;
        hand.rotation = handStartRotation;

        //Work with temporaty game objects and align & parent them to the arm.
        transform.position = upperArm.position;
        //position the elbow using as an up axis a vector from the upperArm position to the target of the elbow
        //that will orient the elbow to the correct orientation
        transform.LookAt(targetPosition, elbowTargetPosition - transform.position);

        upperArmAxisCorrection.transform.position = upperArm.position;
        upperArmAxisCorrection.transform.LookAt(forearm.position, upperArm.up);
        upperArm.parent = upperArmAxisCorrection.transform;

        forearmAxisCorrection.transform.position = forearm.position;
        forearmAxisCorrection.transform.LookAt(hand.position, forearm.up);
        forearm.parent = forearmAxisCorrection.transform;

        handAxisCorrection.transform.position = hand.position;
        hand.parent = handAxisCorrection.transform;

        //Reset targets.
        target.position = targetPosition;
        elbowTarget.position = elbowTargetPosition;

        //Apply rotation for temporary game objects.
        upperArmAxisCorrection.transform.LookAt(target, elbowTarget.position - upperArmAxisCorrection.transform.position);
        upperArmAxisCorrection.transform.localRotation = Quaternion.Euler(upperArmAxisCorrection.transform.localRotation.eulerAngles - new Vector3(ikAngle, 0, 0));
        forearmAxisCorrection.transform.LookAt(target, elbowTarget.position - upperArmAxisCorrection.transform.position);
        handAxisCorrection.transform.rotation = target.rotation;

        //Restore limbs.
        upperArm.parent = upperArmParent;
        forearm.parent = forearmParent;
        hand.parent = handParent;
        upperArm.localScale = upperArmScale;
        forearm.localScale = forearmScale;
        hand.localScale = handScale;
        upperArm.localPosition = upperArmLocalPosition;
        forearm.localPosition = forearmLocalPosition;
        hand.localPosition = handLocalPosition;

        //Transition.
        Weight = Mathf.Clamp01(Weight);
        upperArm.rotation = Quaternion.Slerp(upperArmRotation, upperArm.rotation, Weight);
        forearm.rotation = Quaternion.Slerp(forearmRotation, forearm.rotation, Weight);
        hand.rotation = target.rotation;

    }

}
