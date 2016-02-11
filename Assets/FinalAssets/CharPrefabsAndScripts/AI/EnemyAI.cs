using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{

    NavMeshAgent agent;
    HumanCharacter charMove;
    public GameObject waypointHolder;
    public List<Transform> waypoints = new List<Transform>();


    int waypointIndex;

    Vector3 targetPos;

    Animator anim;
    WeaponManager weaponManager;

    float patrolTimer;
    public float waitingTime = 1;
    public float attackRate = 100; // change for changing difficulty
    float attackTimer;



    public List<GameObject> Enemies = new List<GameObject>();
    public GameObject EnemyToAttack;
    public List<Transform> AvailableCover = new List<Transform>();
    public GameObject closestCover;
    public enum AIstate
    {
        Patrol, Attack, GoToCover, InCover
    }
    public AIstate aiState;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        charMove = GetComponentInChildren<HumanCharacter>();
        anim = GetComponentInChildren<Animator>();
        weaponManager = GetComponent<WeaponManager>();
        Transform[] wayp = waypointHolder.GetComponentsInChildren<Transform>();
        foreach (Transform tr in wayp)
        {
            if (tr != waypointHolder.transform)
                waypoints.Add(tr);
        }

    }


    void Update()
    {
        // GetComponentInChildren<Animator>().gameObject.transform.localRotation = new Quaternion(0,0,0,0);
        DecideState();
        if (EnemyToAttack)
            LookForEnemy();
        switch (aiState)
        {
            case AIstate.Attack:
                Attack();
                break;
            case AIstate.Patrol:
                Patrolling();
                break;
            case AIstate.GoToCover:
                agent.speed = 3;
                TakeCover();
                break;
            case AIstate.InCover:
                InCover();

                break;

        }

    }
    void DecideState()
    {
        Enemies.RemoveAll(item => item == null);
        if (Enemies.Count > 0)
        {
            bool enemy2attackinlist = false;
            if (!EnemyToAttack)
            {
                foreach (GameObject enGo in Enemies)
                {
                    if (EnemyToAttack == enGo)
                    { enemy2attackinlist = true; }
                    if (enGo != null)
                    {
                        Vector3 direction = enGo.transform.position - transform.position;
                        float angle = Vector3.Angle(direction, transform.forward);
                        if (angle < 180f)
                        {
                            RaycastHit hit;
                            if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, 15))
                            {
                                if (hit.collider.gameObject.GetComponent<CharacterStats>())
                                {
                                    if (hit.collider.gameObject.GetComponent<CharacterStats>().Id != GetComponent<CharacterStats>().Id)
                                    {
                                        //Debug.Log("!!!");
                                        EnemyToAttack = hit.collider.gameObject;
                                    }
                                }
                                if (hit.collider.gameObject.GetComponentInParent<CharacterStats>())
                                {
                                    if (hit.collider.gameObject.GetComponentInParent<CharacterStats>().Id != GetComponent<CharacterStats>().Id)
                                    {
                                        //Debug.Log("!!!");
                                        EnemyToAttack = hit.collider.gameObject.GetComponentInParent<CharacterStats>().gameObject;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {


                    }
                }
                if(enemy2attackinlist==false)
                {
                    EnemyToAttack = null;
                }
            }
            else {
                if (!Decision)
                {   //Compare weapons
                    if (SharedFunctions.CompareWeapon(weaponManager.ActiveWeapon, EnemyToAttack.GetComponent<WeaponManager>().ActiveWeapon))
                    {   //Our weapon is superior, so attack
                       // Debug.Log("superior");
                        aiState = AIstate.Attack;
                    }
                    else {  //Our weapon is inferior, so take cover
                      //  Debug.Log("inferior");
                        aiState = AIstate.GoToCover;
                    }


                    Decision = true;
                }
            }

        }
        else
        {
            aiState = AIstate.Patrol;
            anim.SetBool("Aim", false);
        }





        //Are we in Cover?
        if (aiState == AIstate.GoToCover)
        {
            if (closestCover != null)
            {
                float distance = Vector3.Distance(transform.position, closestCover.transform.position);
                if (distance < 1.5f)
                {
                    //character might get stuck,try to increase 1.3


                    aiState = AIstate.InCover;

                }
                /*
				CoverObject cObj = closestCover.GetComponent<CoverObject> ();

				if (cObj.owner) 
				{
					if (cObj != this.gameObject) 
					{
						//AvailableCover.Clear ();
						//closestCover = null;
					}
					else 
					{	Debug.Log ("red");
							
					}
				} 
				else 
				{
					cObj.owner = this.gameObject;
				}*/


            }
        }


    }
    bool Decision;

    void TakeCover()
    {
        if (closestCover == null)
        {
            if (AvailableCover.Count > 0)
            {
              //  Debug.Log("AvailCov" + AvailableCover.Count);
                Transform clCover = AvailableCover[0];
                float distance = Vector3.Distance(transform.position, AvailableCover[0].transform.position);
                for (int i = 0; i < AvailableCover.Count; i++)
                {
                    float tempDistance = Vector3.Distance(transform.position, AvailableCover[i].transform.position);
                    if (tempDistance < distance)
                    {
                        clCover = AvailableCover[i];
                        distance = tempDistance;
                    }
                    if (i == AvailableCover.Count - 1)
                    {

                        closestCover = clCover.gameObject;
                        closestCover.GetComponent<CoverObject>().owner = this.gameObject;
                    }
                }
            }
            else
            {
                FindCovers();
            }
        }
        else
        {
            MoveTo(closestCover.transform.position, false, EnemyToAttack.transform.position);
        }

    }
    void FindCovers()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 30);
        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.GetComponent<CoverObject>())
                {
                    if (!hitColliders[i].gameObject.GetComponent<CoverObject>().owner)
                    {
                        Vector3 direction = EnemyToAttack.transform.position - hitColliders[i].transform.position;//seems wrong
                        Vector3 forward = hitColliders[i].transform.forward;
                        float angle = Vector3.Angle(direction, forward);
                        if (angle < 180f)
                        {
                            if (!AvailableCover.Contains(hitColliders[i].transform))
                            {
                                AvailableCover.Add(hitColliders[i].transform);
                            }
                        }

                    }
                }

            }
            if (AvailableCover.Count <= 0)
                aiState = AIstate.Attack;

        }
    }

    bool takingCover;
    bool attacking;
    int timesFired;
    void InCover()
    {
        agent.Stop();
        anim.SetFloat("Turn", 0);
        anim.SetFloat("Forward", 0);
        anim.SetBool("Crouch", takingCover);
        transform.rotation = closestCover.transform.rotation;
        if (!attacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer > attackRate)
            {
                takingCover = false;
                attacking = true;
                attackTimer = 0;
            }
            else
            {
                takingCover = true;
            }
        }
        else
        {
            anim.SetBool("Aim", true);
            attackTimer += Time.deltaTime;
            if (attackTimer > attackRate / 2)
            {
                if (InSight)
                    Attack();
               
                    //Debug.Log("No Enemy in sight");
            }
        }
        if (timesFired > 3)
        {
            attacking = false;
            takingCover = true;
            timesFired = 0;
            anim.SetBool("Aim", false);
        }
    }
    Vector3 lastKnownPosition;
    public bool InSight;
    void LookForEnemy()
    {
        //Debug.Log("LookingforEnemy");
        RaycastHit hit;
        Vector3 direction = EnemyToAttack.transform.position - transform.position;
        if (Physics.Raycast(transform.position, direction, out hit))
        {
            if (hit.transform.root.GetComponent<CharacterStats>())
            {
                if (hit.transform.root.GetComponent<CharacterStats>().Id != GetComponent<CharacterStats>().Id)
                {
                    lastKnownPosition = EnemyToAttack.transform.position;
                    InSight = true;
                }
                else
                {
                    InSight = false;
                }
            }
            else
            {
                InSight = false;
            }
        }
        else
        {
            InSight = false;
        }
    }
    void Attack()
    {
        agent.Stop();
        anim.SetFloat("Turn", 0);
        anim.SetFloat("Forward", 0);
        weaponManager.aim = true;
        anim.SetBool("Aim", true);
        charMove.Move(Vector3.zero, false, false, true, Vector3.zero);
        Vector3 direction = lastKnownPosition - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);
        if (angle < 180f)
        {
            //Debug.Log("Enemy looking at you!!!" + lastKnownPosition.ToString());
            transform.LookAt(lastKnownPosition);
        }
        attackTimer += Time.deltaTime;
        if (attackTimer > attackRate)
        {

            if (SharedFunctions.CheckAmmo(weaponManager.ActiveWeapon))
            {
                timesFired++;
                ShootRay();
            }
            else
            {

                if (SharedFunctions.ReturnRandomInt() <= 5)
                {
                    anim.SetTrigger("Reload");
                    weaponManager.ReloadActiveWeapon();
                }
                else
                {

                    weaponManager.ChangeWeapon(true);
                }

            }
            attackTimer = 0;
        }
    }

    void ShootRay()
    {
        RaycastHit hit;
        var bulletPrefab = weaponManager.ActiveWeapon.bulletPrefab;
        weaponManager.ActiveWeapon.gameObject.GetComponent<AudioSource>().Play();
        GameObject go = Instantiate(bulletPrefab, transform.position, Quaternion.identity) as GameObject;
        LineRenderer line = go.GetComponent<LineRenderer>();

        Vector3 startPos = weaponManager.ActiveWeapon.bulletSpawn.TransformPoint(Vector3.zero);
        Vector3 endPos = Vector3.zero;
        int mask = ~(1 << 10);//depends on layer numbers
        Vector3 directionToAttack = lastKnownPosition - transform.position;
        if (Physics.Raycast(startPos, directionToAttack, out hit, Mathf.Infinity, mask))
        {
            float distance = Vector3.Distance(weaponManager.ActiveWeapon.bulletSpawn.transform.position, hit.point);
            RaycastHit[] hits = Physics.RaycastAll(startPos, hit.point - startPos, distance);
            foreach (RaycastHit hit2 in hits)
            {
                if (hit2.transform.GetComponent<Rigidbody>())
                {
                    Vector3 direction = hit2.transform.position - transform.position;
                    direction = direction.normalized;
                    //hit2.transform.GetComponent<Rigidbody>().AddForce(direction*200);

                    if (hit2.transform.root.GetComponent<CharacterStats>())
                    {
                        hit2.transform.root.GetComponent<CharacterStats>().HitDetect(hit2.transform, weaponManager.ActiveWeapon.weaponDamage);
                    }
                    EnemyToAttack = null;


                }
                /*else if(hit2.transform.GetComponent<Destructible>())
					
				{
					hit2.transform.GetComponent<Destructible>().destruct=true;
				}
				*/
                //else if hit2.GetComponent<CharacterStats>())


            }
            endPos = hit.point;
        }
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);


        weaponManager.ActiveWeapon.curAmmo--;
    }
    void Patrolling()
    {
        agent.speed = 1;
        if (waypoints.Count > 0)
        {
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= waitingTime)
                {
                    if (waypointIndex == waypoints.Count - 1)
                        waypointIndex = 0;
                    else {
                        waypointIndex++;
                    }

                    patrolTimer = 0;
                }

            }
            else
            {
                patrolTimer = 0;
            }


            MoveTo(waypoints[waypointIndex].position, false, targetPos);
        }
        else
        {
            agent.transform.position = transform.position;
            Vector3 lookPos = (waypoints.Count > 0) ? waypoints[waypointIndex].position : Vector3.zero;
            charMove.Move(Vector3.zero, false, false, false, lookPos);
        }


    }
    void MoveTo(Vector3 destination, bool aim, Vector3 lookPos)
    {
        agent.transform.position = transform.position;
        agent.destination = destination;
        Vector3 velocity = agent.desiredVelocity * 0.5f;
        //Debug.Log (agent.desiredVelocity.ToString());
        charMove.Move(velocity, false, false, aim, lookPos);
    }



    public bool CanPickUp;
    public GameObject Item;
    // Use this for initialization
    void PickupItem()
    {
        if (CanPickUp)
        {

            if (Input.GetKeyUp(KeyCode.F))
            {
                SharedFunctions.PickupItem(this.gameObject, Item);
                CanPickUp = false;
            }
        }
    }
}
