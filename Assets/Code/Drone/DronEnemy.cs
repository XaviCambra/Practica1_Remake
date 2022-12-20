using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DronEnemy : MonoBehaviour
{
    public enum TState
    {
        IDLE,
        PATROL,
        ALERT,
        CHASE,
        ATTACK,
        HIT,
        DIE
    }

    public TState m_State;
    NavMeshAgent m_NavMeshAgent;
    public List<Transform> m_PatrolTargets;
    int m_CurrentPatrolTargetId = 0;

    private void Awake()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        SetIdleState();
    }

    private void Update()
    {
        switch (m_State)
        {
            case TState.IDLE:
                UpdateStateIdle();
                break;
            case TState.PATROL:
                UpdateStatePatrol();
                break;
            case TState.ALERT:
                UpdateStateAlert();
                break;
            case TState.CHASE:
                UpdateStateChase();
                break;
            case TState.ATTACK:
                UpdateStateAttack();
                break;
            case TState.HIT:
                UpdateStateHit();
                break;
            case TState.DIE:
                UpdateStatedDie();
                break;
        }
    }

    void SetIdleState()
    {
        m_State = TState.IDLE;
    }

    void UpdateStateIdle()
    {
       SetPatrolState();
    }

    void SetPatrolState()
    {
        m_State = TState.PATROL;
        m_NavMeshAgent.destination = m_PatrolTargets[m_CurrentPatrolTargetId].position;
    }

    void UpdateStatePatrol()
    {
        if (PatrolTargetPositionArrived())
            MoveToNextPatrolPosition();
    }

    bool PatrolTargetPositionArrived()
    {
        return !m_NavMeshAgent.hasPath && !m_NavMeshAgent.pathPending && m_NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete;
    }

    void MoveToNextPatrolPosition()
    {
        ++m_CurrentPatrolTargetId;
        if (m_CurrentPatrolTargetId >= m_PatrolTargets.Count)
            m_CurrentPatrolTargetId = 0;
        m_NavMeshAgent.destination = m_PatrolTargets[m_CurrentPatrolTargetId].position;
    }

    void SetAlertState()
    {
        m_State = TState.ALERT;
    }

    void UpdateStateAlert()
    {

    }

    void SetChaseState()
    {
        m_State = TState.CHASE;
    }

    void UpdateStateChase()
    {

    }

    void SetAttackState()
    {
        m_State = TState.ATTACK;
    }

    void UpdateStateAttack()
    {

    }

    void SetHitState()
    {
        m_State = TState.HIT;
    }

    void UpdateStateHit()
    {

    }

    void SetDietate()
    {
        m_State = TState.DIE;
    }

    void UpdateStatedDie()
    {
        
    }
}
