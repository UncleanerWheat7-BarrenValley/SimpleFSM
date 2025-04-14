using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleFSM : FSM
{
    public enum FSMState
    {
        None, Patrol, Chase, Attack, Dead,
    }

    public FSMState currentState = FSMState.Patrol;

    public FSM realFSM;

    [SerializeField]
    private float speed = 2.0f;
    private float rotateSpeed = 2.0f;
    private bool isDead = false;
    private int health = 100;

    new private Rigidbody rigidbody;

    protected Transform playerTransform;

    protected Vector3 targetPosition;

    protected GameObject[] pointList;

    protected float shootRate = 3;
    protected float elapsedTime = 0;
    public float maxFireAimError = 0.001f;

    public float patrolRadius = 100;
    public float attackRadius = 200;
    public float playerNearRadius = 300;

    protected override void Initialize()
    {
        pointList = GameObject.FindGameObjectsWithTag("WanderPoint");

        FindNextPoint();

        realFSM = GetComponent<FSM>();

        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        rigidbody = GetComponent<Rigidbody>();
        playerTransform = objPlayer.transform;
        if (!playerTransform)
        {
            print("Player not found. does your player obj have the tag 'Player'?");
        }
    }

    protected override void FSMUpdate()
    {
        switch (currentState)
        {
            case FSMState.Patrol:
                Patrol();
                break;
            case FSMState.Chase:
                Chase();
                break;
            case FSMState.Attack:
                Attack();
                break;
            case FSMState.Dead:
                Dead();
                break;
        }

        elapsedTime += Time.deltaTime;

        if (health <= 0)
        {
            isDead = true;
            currentState = FSMState.Dead;
        }
    }

    private void Dead()
    {
        if (!isDead)
        {
            isDead = true;
            Explode();
        }
    }

    private void Attack()
    {
        targetPosition = playerTransform.position;
        Vector3 frontVector = Vector3.forward;

        float dist = Vector3.Distance(transform.position, targetPosition);
        if (dist >= attackRadius && dist < playerNearRadius)
        {
            currentState = FSMState.Chase;
        }
        else if (dist >= playerNearRadius)
        {
            currentState = FSMState.Patrol;
        }

        Quaternion targetRotation = Quaternion.FromToRotation(frontVector, targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }

    private void Chase()
    {
        targetPosition = playerTransform.position;
        Vector3 frontVector = Vector3.forward;

        float dist = Vector3.Distance(transform.position, targetPosition);
        if (dist <= attackRadius)
        {
            currentState = FSMState.Attack;
        }
        else if (dist >= playerNearRadius)
        {
            currentState = FSMState.Patrol;
        }

        Quaternion targetRotation = Quaternion.FromToRotation(frontVector, targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, targetPosition) <= patrolRadius)
        {
            print("Reached patrol point");
            FindNextPoint();
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) <= playerNearRadius)
        {
            print("switch to chase");
            currentState = FSMState.Chase;
        }

        // rotate to target
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    void FindNextPoint()
    {
        print("finding next point");
        int randomIndex = Random.Range(0, pointList.Length);
        float randomRadius = 10;
        Vector3 randomPosition = Vector3.zero;
        targetPosition = pointList[randomIndex].transform.position + randomPosition;

        if (IsInCurrentRange(targetPosition))
        {
            randomPosition = new Vector3(Random.Range(-randomRadius, randomRadius), 0, Random.Range(-randomRadius, randomRadius));
            targetPosition = pointList[randomIndex].transform.position + randomPosition;
        }
    }

    bool IsInCurrentRange(Vector3 pos)
    {
        float xPos = Mathf.Abs(pos.x - transform.position.x);
        float zPos = Mathf.Abs(pos.z - transform.position.z);

        if (xPos <= patrolRadius && zPos <= patrolRadius) return true;

        return false;
    }

    void Explode()
    {
        float randomX = Random.Range(10, 30);
        float randomZ = Random.Range(10, 30);
        for (int i = 0; i < 3; i++)
        {
            rigidbody.AddExplosionForce(10000, transform.position - new Vector3(randomX, 0, randomZ), 40, 10);
            rigidbody.linearVelocity = transform.TransformDirection(new Vector3(randomX, 20, randomZ));
        }

        Destroy(gameObject, 2.0f);
    }

    protected override void FSMFixedUpdate()
    {
        
    }
}
