using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class YipeeAI : MonsterAI
{
    private Node topNode;
    public Transform player;
    public Transform bewareOf;
    public float detectionRange = 10f;
    public float stoppingDistance = 2f;
    public UnityEngine.AI.NavMeshAgent navMeshAgent;

    [SerializeField] float attackDistance = 1.5f;
    public Transform nest;
    [SerializeField] Transform Item;
    public bool setDesti = false;
    public float wanderRadius = 10f;
    public Vector3 itemPos;
    public GameObject detectedItem;
    public bool itemFind = false;
    public bool itemHave = false;
    public bool isAttacked = false;

    private DamageMessage damageMessage;
    private YipeeHealth yipeeHealth;
    [SerializeField] float attackCooltime = 2f;
    private float lastAttackTime;

    private bool startBeWatched = false;
    private float lastBeWatchedTime;
    public bool sawPlayer = false;
    public List<GameObject> item = new List<GameObject>();

    void Start()
    {
        
        navMeshAgent = GetComponent<NavMeshAgent>();
        ConstructBehaviorTree();

        yipeeHealth = GetComponent<YipeeHealth>();
        damageMessage = new DamageMessage();
        damageMessage.damage = 30;
        damageMessage.damager = gameObject;
    }

    void Update()
    {
        CheckWatched(); StolenItem();
        topNode.Evaluate();
    }

    private void ConstructBehaviorTree()
    {
        //죽음 시퀀스의 children Node
        ActionNode dead = new ActionNode(Dead);

        //공격 시퀀스의 children Node들
        ActionNode attackWill = new ActionNode(AttackWill);
        ActionNode moveToPlayer = new ActionNode(MoveToPlayer);
        ActionNode attackPlayer = new ActionNode(AttackPlayer);

        //배회 시퀀스의 children Node들
        ActionNode setDest = new ActionNode(SetDest);
        ActionNode moveToDest = new ActionNode(MoveToDest);

        //탐색 시퀀스의 children Node들
        ActionNode setDestToScrap = new ActionNode(SetDestToScrap);
        ActionNode moveToScrap = new ActionNode(MoveToScrap);
        ActionNode getScrap = new ActionNode(GetScrap);
        ActionNode moveToNest = new ActionNode(MoveToNest);
        ActionNode setScrap = new ActionNode(SetScrap);

        //위협 시퀀스의 children Node
        ActionNode threathen = new ActionNode(Threaten);

        //셀렉터 노드에 들어갈 시퀀스 노드들
        SequenceNode attackSequence = new SequenceNode(new List<Node> { attackWill, moveToPlayer, attackPlayer });
        SequenceNode wanderSequence = new SequenceNode(new List<Node> { setDest, moveToDest });
        SequenceNode detectSequence = new SequenceNode(new List<Node> { setDestToScrap, moveToScrap, getScrap, moveToNest, setScrap });
        topNode = new SelectorNode(new List<Node> { dead, attackSequence, threathen, /*wanderSequence,*/ detectSequence, });
    }

    //죽음 시퀀스 노드
    private Node.State Dead()
    {
        if (yipeeHealth.IsDead)
        {
            detectedItem.transform.parent = null;
            navMeshAgent.SetDestination(transform.position);
            return Node.State.SUCCESS;
        }
        else return Node.State.FAILURE;
    }

    //공격의지 활성화(공격 시퀀스)
    private Node.State AttackWill()
    {
        Debug.Log("여긴 들어올거잖어?");
        //1. 플레이어에게 공격 받았는지 bool변수 확인(YipeeHealth에서)
        if (isAttacked)
        {
            //YipeeHealth에서 플레이어 갱신. 
            Debug.Log("여기?");
            return Node.State.SUCCESS;
        }
        //2. 플레이어가 자신을 7초 이상 쳐다봤는지 확인
        //이거는 Update에서 CheckWatched를 통해 isAttacked를 바꿔준다. 그러면 1번 경우에 걸려서 공격함.

        //3. 플레이어가 둥지의 폐품 훔쳐간것을 봄.
        //이거도 Update에서 StolenItem으로 isAttacked를 바꿔줌.

        else
        {
            Debug.Log("아님 여기?");
            return Node.State.FAILURE;
        }
    }

    //플레이어에게 접근(공격 시퀀스)
    private Node.State MoveToPlayer()
    {
        Debug.Log("접근은 하는 것 같은데");
        float distance = Vector3.Distance(transform.position, player.position);
        Debug.Log(player.position + "," + transform.position + ", " + distance);
        if (distance > attackDistance)
        {
            if(detectedItem != null && detectedItem.transform.parent == transform) detectedItem.transform.parent = null;
            navMeshAgent.SetDestination(player.position);
            return Node.State.RUNNING;
        }
        else
        {
            return Node.State.SUCCESS;
        }
    }

    //플레이어를 공격(공격 시퀀스)
    private Node.State AttackPlayer()
    {
        Debug.Log("공격을 안하나?");
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            // 공격 로직 수행
            if (Time.time - lastAttackTime >= attackCooltime)
            {
                LivingEntity playerHealth = player.GetComponent<LivingEntity>();
                playerHealth.ApplyDamage(damageMessage);
                lastAttackTime = Time.time;
                if (playerHealth.dead) { isAttacked = false; player = null; }

                return Node.State.FAILURE;
            }
        }
        return Node.State.SUCCESS;
    }

    //위협 시퀀스 노드
    private Node.State Threaten()
    {
        if (bewareOf == null || itemHave) return Node.State.FAILURE;
        if (Vector3.Distance(bewareOf.position, nest.position) < 3f)
        {
            if(Vector3.Distance(transform.position, bewareOf.position) < 3f)
            {
                navMeshAgent.SetDestination(transform.position);
                Vector3 lookPosition = new Vector3(bewareOf.position.x, transform.position.y, bewareOf.position.z);
                transform.LookAt(lookPosition);
                Debug.Log("위협하는 동작");
                return Node.State.SUCCESS;
            }
            else return Node.State.RUNNING;
        }
        else return Node.State.RUNNING;
    }

    //랜덤 목적지 설정(한번만 실행)(돌아다니기 시퀀스)
    private Node.State SetDest()
    {
        Debug.Log("SetDest");
        if (itemFind) return Node.State.FAILURE;        //아이템을 찾아서 아이템을 추적하는 상태면,
        else if (setDesti) return Node.State.SUCCESS;   //이미 목적지 설정이 되어있으면,
        else
        {
            Vector3 newPos = RandomNavMeshMovement.RandomNavSphere(nest.position, wanderRadius, -1);
            navMeshAgent.SetDestination(newPos);
            setDesti = true;
            return Node.State.SUCCESS;
        }
    }


    //목적지로 이동(돌아다니기 시퀀스)
    private Node.State MoveToDest()
    {
        Debug.Log("MoveToDest");

        if (itemFind) return Node.State.FAILURE;

        //목적지에 도달함.
        else if (Vector3.Distance(transform.position, navMeshAgent.destination) <= 1f)
        {
            setDesti = false;
            return Node.State.SUCCESS;
        }
        else return Node.State.RUNNING;
    }

    //폐품으로 목적지 수정(탐색 시퀀스)
    private Node.State SetDestToScrap()
    {
        Debug.Log("SetDestToScrap");
        if (itemFind)
        {
            navMeshAgent.SetDestination(detectedItem.transform.position);
            return Node.State.SUCCESS;
        }
        else return Node.State.FAILURE;
    }

    //폐품으로 이동(탐색 시퀀스)
    private Node.State MoveToScrap()
    {
        Debug.Log("MoveToScrap");
        if (itemFind)
        {
            //도중에 아이템이 내가 아닌 누군가에게 주워지면 FAILURE.
            if (detectedItem.transform.parent != null && detectedItem.transform.parent != transform.GetChild(1)) return Node.State.FAILURE;

            if (Vector3.Distance(transform.position, navMeshAgent.destination) <= 0.5f)
            {
                return Node.State.SUCCESS;
            }
            else
            {
                Debug.Log("RUNNING이 의미가 있나?");
                return Node.State.RUNNING;
            }
        }
        else return Node.State.FAILURE;
    }


    //폐품 수집(탐색 시퀀스)
    private Node.State GetScrap()
    {
        Debug.Log("GetScrap");

        if (!itemHave)
        {
            //페품 들기

            //도중에 아이템이 내가 아닌 누군가에게 주워지면 FAILURE.
            if (detectedItem.transform.parent != null && detectedItem.transform.parent != transform) return Node.State.FAILURE;

            Debug.Log("들어올림");
            detectedItem.transform.position = transform.GetChild(1).position;
            detectedItem.transform.SetParent(transform.GetChild(1));
            itemHave = true;

            //둥지로 목적지 설정.
            navMeshAgent.SetDestination(nest.position);
            setDesti = true;
            return Node.State.SUCCESS;
        }
        else if (itemHave) return Node.State.SUCCESS;
        else return Node.State.FAILURE;

    }

    //폐품 들고 둥지로(탐색 시퀀스)
    private Node.State MoveToNest()
    {
        Debug.Log("MoveToNest");
        if (Vector3.Distance(transform.position, navMeshAgent.destination) <= 0.5f)
        {
            return Node.State.SUCCESS;
        }
        else { return Node.State.RUNNING; }
    }

    //페품 내려놓기(탐색 시퀀스)
    private Node.State SetScrap()
    {
        if(itemHave)
        {
            Debug.Log("내려놓음");
            detectedItem.transform.parent = null;
            setDesti = false;
            itemFind = false;
            itemHave = false;
            return Node.State.FAILURE;
        }
        else return Node.State.SUCCESS;
    }

    private void CheckWatched()
    {
        if (!startBeWatched && beWatched)
        {
            startBeWatched = true;
            lastBeWatchedTime = Time.time;
        }

        if(!beWatched)
        {
            Debug.Log("안보고 있음");
            startBeWatched = false;
        }

        if (beWatched && Time.time - lastBeWatchedTime > 7f)
        {
            Debug.Log("7초 쳐다 봤음");
            isAttacked = true;
            player = watchedBy.transform;
        }
    }

    private void StolenItem()
    {
        if (sawPlayer)
        {
            if(Vector3.Distance(transform.position, nest.position) < 4f)
            {
                foreach(var i in item)
                {
                    if(i.transform.parent != null)
                    {
                        isAttacked = true;
                        break;
                    }
                }
            }
        }
    }
}