using DunGen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EyelessDogAI : MonsterAI
{
    [SerializeField] EyelessDogHealth eyelessDogHealth;
    [SerializeField] UnityEngine.AI.NavMeshAgent navMeshAgent;
    [SerializeField] DamageMessage damageMessage;
    [SerializeField] Transform pivot;
    [SerializeField] float wanderRadius = 30f;
    [SerializeField] float hearingRange = 60f;
    [SerializeField] float stackRange = 8f;
    [SerializeField] int soundStack = 0;
    [SerializeField] float rushSpeed = 18f;
    [SerializeField] float defaultSpeed = 6f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackCooltime = 1f;

    private Node topNode;
    public bool setDesti = false;
    public Vector3 soundPoint = Vector3.positiveInfinity;
    private float lastSoundTime;
    private float stackCoolTime = 2f;
    public bool readyToRush = false;
    private float lastAttackTime;

    //구현이 복잡해서 그냥 상태머신 쓰기로
    public enum State
    {
        Dead,
        Attack,
        Rush,
        Move,
        Wander
    }
    public State currentState;


    void Start()
    {
        currentState = State.Wander;
        ConstructBehaviorTree();

        damageMessage = new DamageMessage();
        damageMessage.damage = 100;
        damageMessage.damager = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        topNode.Evaluate();
    }

    private void ConstructBehaviorTree()
    {
        //죽음 시퀀스의 children Node
        ActionNode dead = new ActionNode(Dead);

        //공격 시퀀스의 children Node들
        ActionNode attack = new ActionNode(Attack);

        //돌진 시퀀스
        ActionNode rush = new ActionNode(Rush);

        //경계 시퀀스의 children Node들
        ActionNode moveToRush = new ActionNode(MoveToRush);
        ActionNode moveToSound = new ActionNode(MoveToSound);

        //배회 시퀀스의 children Node들
        ActionNode detectSound = new ActionNode(DetectSound);
        ActionNode wander = new ActionNode(Wander);

        //셀렉터 노드에 들어갈 시퀀스 노드들
        SequenceNode moveSequence = new SequenceNode(new List<Node> { moveToRush, moveToSound });
        SequenceNode wanderSequence = new SequenceNode(new List<Node> { detectSound, wander });

        topNode = new SelectorNode(new List<Node> { dead, attack, rush, moveSequence, wanderSequence });
    }


    //[죽음 시퀀스]
    private Node.State Dead()
    {
        if (eyelessDogHealth.IsDead)
        {
            currentState = State.Dead;
            navMeshAgent.SetDestination(pivot.position);
            return Node.State.SUCCESS;
        }
        else return Node.State.FAILURE;
    }

    //[공격 시퀀스] 도달한 곳에 플레이어가 있으면(콜라이더로 감지) 공격.
    private Node.State Attack()
    {
        if (currentState != State.Attack) { return Node.State.FAILURE; }
        Debug.Log("공격준비");
        //범위 안의 가장 가까운 플레이어 대상으로 공격
        Collider[] hitColliders = Physics.OverlapSphere(pivot.position, attackRange);
        PlayerHealth nearestPlayer = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hitCollider in hitColliders)
        {
            Debug.Log("hitCollider: " + hitCollider.gameObject.name);
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsDead)
            {
                float distance = Vector3.Distance(pivot.position, playerHealth.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPlayer = playerHealth;
                }
            }
        }

        if (nearestPlayer != null)
        {
            Debug.Log("플레이어 감지");

            if (Time.time - lastAttackTime >= attackCooltime)
            {
                Debug.Log("공격한다.(즉사)");
                nearestPlayer.ApplyDamage(damageMessage);

                lastAttackTime = Time.time;
                currentState = State.Wander;
                return Node.State.SUCCESS;
            }
        }
        else
        {
            Debug.Log("못찾았어용");
        }
        return Node.State.FAILURE;

    }

    //[돌진 시퀀스] 속도를 변화시키고 달려간다.
    private Node.State Rush()
    {
        if(currentState != State.Rush) { return Node.State.FAILURE; }

        Debug.Log("돌진임");

        navMeshAgent.speed = rushSpeed;
        

        if (Vector3.Distance(pivot.position, navMeshAgent.destination) <= 1f)
        {
            currentState = State.Attack;
            navMeshAgent.speed = defaultSpeed;
            return Node.State.SUCCESS;
        }
        else
        {
            return Node.State.RUNNING;
        }
    }


    //[경계 시퀀스] 뛸 준비
    private Node.State MoveToRush()
    {
        Debug.Log("뛸 준비");
        Collider[] hitColliders = Physics.OverlapSphere(soundPoint, stackRange);
        AudioSource nearestSource = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hitCollider in hitColliders)
        {
            AudioSource audioSource = hitCollider.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.isPlaying)
            {
                float distance = Vector3.Distance(transform.position, audioSource.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSource = audioSource;
                }
            }
        }

        if (nearestSource != null)
        {
            Debug.Log("근처에서 또 소리를 감지");
            soundPoint = nearestSource.transform.position;
            navMeshAgent.SetDestination(soundPoint);

            if (Time.time - lastSoundTime >= stackCoolTime)
            {
                soundStack++;
                lastSoundTime = Time.time;
                Debug.Log("soundStack" + soundStack);
            }
        }

        if (soundStack > 2)
        {
            currentState = State.Rush;
            soundStack = 0;
            return Node.State.SUCCESS;
        }
        else
        {
            return Node.State.FAILURE;
        }
    }


    //[경계 시퀀스] 다가가기
    private Node.State MoveToSound()
    {
        Debug.Log("다가가기");

        if (Vector3.Distance(pivot.position, navMeshAgent.destination) <= 1f)
        {
            return Node.State.SUCCESS;
        }
        else
        {
            return Node.State.RUNNING;
        }
    }


    //[배회 시퀀스] 소리감지
    private Node.State DetectSound()
    {
        Debug.Log("소리 감지중..");
        Collider[] hitColliders = Physics.OverlapSphere(pivot.position, hearingRange);
        AudioSource nearestSource = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hitCollider in hitColliders)
        {
            AudioSource audioSource = hitCollider.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.isPlaying)
            {
                float distance = Vector3.Distance(transform.position, audioSource.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSource = audioSource;
                }
            }
        }

        if (nearestSource != null)
        {
            soundPoint = nearestSource.transform.position;
            navMeshAgent.SetDestination(soundPoint);
            currentState = State.Move;
            return Node.State.FAILURE;
        }
        else
        {
            return Node.State.SUCCESS;
        }
    }

    //[배회 시퀀스] 돌아다니기 
    private Node.State Wander()
    {
        if (!setDesti)
        {
            Debug.Log("목적지 설정");
            Vector3 newDest = RandomNavMeshMovement.RandomNavSphere(pivot.position, wanderRadius, -1);
            setDesti = true;
            navMeshAgent.SetDestination(newDest);
        }

        if (Vector3.Distance(pivot.position, navMeshAgent.destination) <= 1f)
        {
            Debug.Log("목적지 도착");
            setDesti = false;
            return Node.State.SUCCESS;
        }
        else return Node.State.RUNNING;
    }

}
