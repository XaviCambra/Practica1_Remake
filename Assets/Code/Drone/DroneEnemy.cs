using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DroneEnemy : MonoBehaviour
{
   public enum TState
    {
        IDLE=0,
        PATROL,
        ALERT,
        CHASE,
        ATACK,
        HIT,
        DIE
    }

    public TState m_State;
    public TState m_LastState;

    NavMeshAgent m_NavMeshAgent;
    public List<Transform> m_PatrolTargets;
    int m_CurrentPatrolTargetId = 0;
    float m_HearRangeDistance = 4.5f;

    float m_VisualConeAngle = 60.0f;
    float m_SightDistance = 8.0f;
    public LayerMask m_SightLayerMask;
    float m_EyesHeight = 1.8f;
    float m_EyesPlayerHeight = 1.8f;
    float m_MaxShootingRange = 4.0f;
    bool m_CanShoot;
    float m_ShootDamage = 0.3f;

    float m_Rotation;
    float m_RotationSpeed;

    public GameObject m_LifeUi;
    public Image m_LifeBarImage;
    public Transform m_LifeBarAnchorPosition;
    public RectTransform m_LifeBarRectTransform;
    public float m_Life;
    public float m_MaxLife = 1.0f;
    bool m_isAlive;

    public BoxCollider m_LeftCollider;
    public BoxCollider m_RightCollider;
    public BoxCollider m_CenterCollider;

    public List<GameObject> m_Drops = new List<GameObject>();
    GameObject m_Drop;

    private void Awake()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Start()
    {
        m_LifeBarImage.fillAmount = m_Life;
        SetIdleState();
        m_CanShoot = true;
        m_Life = m_MaxLife;
    }

    void Update()
    {
        switch (m_State)
        {
            case TState.IDLE:
                UpdateIdleState();
                break;
            case TState.PATROL:
                UpdatePatrolState();
                break;
            case TState.ALERT:
                UpdateAlertState();
                break;
            case TState.CHASE:
                UpdateChaseState();
                break;
            case TState.ATACK:
                UpdateAtackState();
                break;
            case TState.HIT:
                UpdateHitState();
                break;
            case TState.DIE:
                UpdateDieState();
                break;
        }
    }

    private void LateUpdate()
    {
        UpdateLifeBarPosition();
    }
    void SetIdleState()
    {
        m_isAlive = true;
        m_State = TState.IDLE;
        SetPatrolState();
    }

    void SetPatrolState()
    {
        m_State = TState.PATROL;
        m_NavMeshAgent.isStopped = false;
        if (m_isAlive)
            m_NavMeshAgent.destination = m_PatrolTargets[m_CurrentPatrolTargetId].position;
    }

    void SetAlertState()
    {
        m_State = TState.ALERT;
        m_Rotation = 0.0f;
    }
    void SetChaseState()
    {
        m_State = TState.CHASE;
    }
    void SetAtackState()
    {
        m_State = TState.ATACK;
    }
    void SetHitState()
    {
        m_State = TState.HIT;
    }
    void SetDieState()
    {
        m_isAlive = false;
        m_State = TState.DIE;
    }

    void UpdateIdleState()
    {
    }

    void UpdatePatrolState()
    {
        if (PatrolTargetPositionArrived())
        {
            MovetoNextPatrolPosition();
        }
        if (HearsPlayer())
        {
            SetAlertState();
        }
    }

    bool PatrolTargetPositionArrived()
    {
        return !m_NavMeshAgent.hasPath && !m_NavMeshAgent.pathPending && m_NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete;
    }

    void MovetoNextPatrolPosition()
    {
        ++m_CurrentPatrolTargetId;
        if(m_CurrentPatrolTargetId>= m_PatrolTargets.Count)
        {
            m_CurrentPatrolTargetId = 0;
        }
        m_NavMeshAgent.destination = m_PatrolTargets[m_CurrentPatrolTargetId].position;
    }

    void UpdateAlertState()
    {
        transform.eulerAngles += new Vector3(0, m_RotationSpeed * Time.deltaTime, 0);
        m_Rotation += Time.deltaTime * m_RotationSpeed;

        if (SeesPlayer())
        {
            if (RangePlayer())
            {
                SetAtackState();
            }
            else
            {
                SetChaseState();
            }
        }
        if(m_Rotation >= 360 && !SeesPlayer())
        {
            SetPatrolState();
        }
    }

    void UpdateChaseState()
    {
        Vector3 l_PlayerPosition = GameControler.GetGameController().GetPlayer().transform.position;
        float l_Distance = Vector3.Distance(l_PlayerPosition, transform.position);
        m_NavMeshAgent.destination = GameControler.GetGameController().GetPlayer().transform.position;

        if (l_Distance <= m_MaxShootingRange)
        {
            SetAtackState();
        }
        else if (l_Distance <= m_SightDistance)
        {
            transform.LookAt(l_PlayerPosition);
            transform.position = Vector3.MoveTowards(transform.position, l_PlayerPosition, m_NavMeshAgent.speed * Time.deltaTime);
        }
        else
        {
            SetPatrolState();
        }
    }

    void UpdateAtackState()
    {
        if (!RangePlayer())
        {
            m_CanShoot = true;
            SetChaseState();
        }
        if (m_CanShoot)
        {
            m_CanShoot = false;
            Attack();
        }
        
    }

    void UpdateHitState()
    {
        if(m_LastState == TState.PATROL || m_LastState == TState.IDLE)
        {
            m_LastState = TState.IDLE;
            SetAlertState();
        }
        else
        {
            m_State = m_LastState;
            m_LastState = TState.IDLE;
        }
    }

    void UpdateDieState()
    {
        gameObject.SetActive(false);
        var random = new System.Random();

        m_Drop = m_Drops[random.Next(m_Drops.Count)];

        Instantiate(m_Drop, transform.position , transform.rotation);
    }

    public void Hit(float damage)
    {
        Debug.Log("he entrao)");
        m_NavMeshAgent.isStopped = true;
        m_LastState = m_State;
        SetHitState();
        m_Life -= damage;
        if(m_Life <= 0)
        {
            SetDieState();
        }
    }
    void Attack()
    {
        GameControler.GetGameController().GetPlayer().Hit(m_ShootDamage);
        StartCoroutine(ShootCooldown(2.0f));
    }
    bool HearsPlayer()
    {
        Vector3 l_PlayerPosition = GameControler.GetGameController().GetPlayer().transform.position;
        return Vector3.Distance(l_PlayerPosition, transform.position) <= m_HearRangeDistance;
    }
    bool SeesPlayer()
    {
        Vector3 l_PlayerPosition = GameControler.GetGameController().GetPlayer().transform.position;
        Vector3 l_DirectionToPlayerXZ = l_PlayerPosition - transform.position;
        l_DirectionToPlayerXZ.y = 0.0f;
        l_DirectionToPlayerXZ.Normalize();
        Vector3 l_ForwardXZ = transform.forward;
        l_ForwardXZ.y = 0.0f;
        l_ForwardXZ.Normalize();
        Vector3 l_EyesPosition = transform.position + Vector3.up * m_EyesHeight;
        Vector3 l_PlayerEyesPosition = l_PlayerPosition + Vector3.up * m_EyesPlayerHeight;
        Vector3 l_Direction = l_PlayerEyesPosition - l_EyesPosition;

        float l_Length = l_Direction.magnitude;
        l_Direction /= l_Length;

        return Vector3.Distance(l_PlayerPosition, transform.position) < m_SightDistance && Vector3.Dot(l_ForwardXZ, l_DirectionToPlayerXZ) > Mathf.Cos(m_VisualConeAngle * Mathf.Deg2Rad / 2.0f);
    }

    void UpdateLifeBarPosition()
    {
        if(RangePlayer() && m_State != TState.DIE)
        {
            m_LifeUi.SetActive(true);
        }
        else
        {
            m_LifeUi.SetActive(false);
        }
        Vector3 l_Position = GameControler.GetGameController().GetPlayer().m_Camera.WorldToViewportPoint(m_LifeBarAnchorPosition.position);
        m_LifeBarRectTransform.anchoredPosition = new Vector3(l_Position.x * 1920.0f, -(1080.0f - l_Position.y * 1080.0f), 0.0f);
        m_LifeBarRectTransform.gameObject.SetActive(l_Position.z > 0.0f);
    }


    bool RangePlayer()
    {
        Vector3 l_playerPosition = GameControler.GetGameController().GetPlayer().transform.position;
        Vector3 l_DirectionToPlayerXZ = l_playerPosition - transform.position;
        l_DirectionToPlayerXZ.y = 0.0f;
        l_DirectionToPlayerXZ.Normalize();
        Vector3 l_ForwardXZ = transform.forward;
        l_ForwardXZ.y = 0.0f;
        l_ForwardXZ.Normalize();

        Vector3 l_EyesPosition = transform.position + Vector3.up * m_EyesHeight;
        Vector3 l_PlayerEyesPosition = l_playerPosition + Vector3.up * m_EyesPlayerHeight;
        Vector3 l_Direction = l_PlayerEyesPosition - l_EyesPosition;

        float l_Lenght = l_Direction.magnitude;
        l_Direction /= l_Lenght;


        Ray l_Ray = new Ray(l_EyesPosition, l_Direction);

        return Vector3.Distance(l_playerPosition, transform.position) <= m_MaxShootingRange && Vector3.Dot(l_ForwardXZ, l_DirectionToPlayerXZ)
            > Mathf.Cos(m_VisualConeAngle * Mathf.Deg2Rad / 2.0f) && !Physics.Raycast(l_Ray, l_Lenght, m_SightLayerMask.value);
    }

    bool Visible()
    {
        Vector3 l_ForwardXZ = GameControler.GetGameController().GetPlayer().transform.forward;
        l_ForwardXZ.y = 0.0f;
        l_ForwardXZ.Normalize();
        Vector3 l_DirectionXZ = transform.position - GameControler.GetGameController().GetPlayer().transform.position;
        l_DirectionXZ.y = 0.0f;
        l_DirectionXZ.Normalize();
        Vector3 l_EyesPosition = transform.position + Vector3.up * m_EyesHeight;
        Vector3 l_PlayerEyesPosition = GameControler.GetGameController().GetPlayer().transform.position + Vector3.up * m_EyesPlayerHeight;
        Vector3 l_Direction = l_PlayerEyesPosition - l_EyesPosition;

        float l_Length = l_Direction.magnitude;
        l_Direction /= l_Length;

        Ray l_Ray = new Ray(l_EyesPosition, l_Direction);

        return Vector3.Distance(transform.position, GameControler.GetGameController().GetPlayer().transform.position) < m_SightDistance && Vector3.Dot(l_ForwardXZ, l_DirectionXZ) > Mathf.Cos(m_VisualConeAngle * Mathf.Deg2Rad / 2.0f) && 
            !Physics.Raycast(l_Ray, l_Length, LayerMask.GetMask());
    }

    IEnumerator ShootCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        m_CanShoot = true;
    }
}
