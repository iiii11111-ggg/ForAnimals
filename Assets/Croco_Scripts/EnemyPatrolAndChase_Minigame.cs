using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolAndChase_Minigame : MonoBehaviour
{
    public Transform[] waypoints;
    public float detectionRadius = 6f;
    public float captureDistance = 1.0f;
    public float patrolStopTime = 1.0f;
    private int currentWP = 0;
    private NavMeshAgent agent;
    private Transform player;
    private float waitTimer = 0f;
    private bool chasing = false;

    public Animator an;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        an.SetBool("isWalking", true);
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        // 에이전트가 NavMesh 위에 있는지 확인하고, 아니라면 근처 NavMesh로 스냅 시도
        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            // 반경 2.0f 안에서 샘플
            if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position); // 에이전트를 NavMesh 위로 옮김
            }
            else
            {
                Debug.LogWarning($"{name}: NavMesh 위에 있지 않습니다. NavMesh를 베이크했는지 확인하세요.");
            }
        }

        if (waypoints.Length > 0 && agent.isOnNavMesh)
            agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        if (player == null) return;

        // 안전: 에이전트가 NavMesh 위에 있는 경우에만 NavMesh 관련 호출 수행
        if (!agent.isOnNavMesh)
        {
            // 시도적으로 NavMesh 근처로 이동시키는 재시도 (옵션)
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 3.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= detectionRadius)
        {
            chasing = true;
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            if (chasing)
            {
                chasing = false;
                if (waypoints.Length > 0)
                    agent.SetDestination(waypoints[currentWP].position);
            }

            if (waypoints.Length == 0) return;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= patrolStopTime)
                {
                    waitTimer = 0f;
                    currentWP = (currentWP + 1) % waypoints.Length;
                    agent.SetDestination(waypoints[currentWP].position);
                }
            }
        }

        if (distToPlayer <= captureDistance)
        {
            GameManager_Croco_Minigame.Instance.PlayerCaught();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, captureDistance);
    }
}
