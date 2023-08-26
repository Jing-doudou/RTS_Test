using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public enum AIState
{
    Idle,
    Move,
    Attack, Hurt, Die
}
public class Unit_Ctrl : MonoBehaviour
{
    private CoreManager manager;
    Slider hp;
    GameObject hpParent;
    GameObject marker;
    private float hpMax = 100;
    public float hpValue;
    public List<Skill> skillEventList;
    public Vector3 targetPos;//技能指向坐标
    public UnityEngine.AI.NavMeshAgent nav;//自动寻路
    public Animator animator;

    public Transform cameraTransform;

    public LineRenderer lineRenderer;
    public Unit_Info unit_Info;
    #region 状态机-----------------------------------------------
    private AIState state;
    AIState oldState;
    public AIState State
    {

        get => state;
        set
        {
            if (state != AIState.Hurt)
            {
                oldState = state;
            }
            state = value;
            switch (state)
            {
                case AIState.Idle:
                    animator.CrossFadeInFixedTime("Idle", 0.2f);
                    break;
                case AIState.Move:
                    animator.CrossFadeInFixedTime("Move", 0.2f);
                    break;
                case AIState.Attack:
                    animator.CrossFadeInFixedTime("Attack", 0.2f);
                    break;
                case AIState.Hurt:
                    animator.CrossFadeInFixedTime("Hurt", 0.2f);
                    break;
                case AIState.Die:
                    nav.SetDestination(transform.position);
                    animator.CrossFadeInFixedTime("Die", 0.2f);
                    Dead();
                    break;
                default:
                    break;
            }
        }
    }
    private void StateOnUpdate()
    {
        switch (State)
        {
            case AIState.Move:
                if (nav.remainingDistance < 1)
                {
                    State = AIState.Idle;
                }
                break;
            default:
                break;
        }
    }
    #endregion
    public float HPValue
    {
        get => hpValue; set
        {
            hpValue = Mathf.Clamp(value, -1, hpMax);
            //满血则关闭血条
            hpParent.SetActive(hpValue != hpMax);
            //
            //血条更新
            hp.value = hpValue;
            //
            if (hpValue <= 0)
            {
                State = AIState.Die;
            }
        }
    }
    private void Dead()
    {
        Destroy(gameObject, 1f);
    }
    public void Hurt(float value = 0)
    {
        HPValue -= value;
        State = AIState.Hurt;
    }
    void Start()
    {
        cameraTransform = transform.Find("UI/HeadVideoPoint");
        hpParent = transform.Find("UI/HP").gameObject;
        hp = hpParent.transform.Find("HP").GetComponent<Slider>();
        marker = transform.Find("UI/Marker").gameObject;
        manager = FindObjectOfType<CoreManager>();
        lineRenderer = transform.Find("UI/TargetPoint").GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        HPValue = 100;
        State = AIState.Idle;
        hp.maxValue = hpMax;

        transform.name = "Object_MagicUnit2";
        unit_Info = new Unit_Info(this);

    }
    void Update()
    {
        #region 血条看向摄像机
        hp.transform.LookAt(Camera.main.transform.position);
        #endregion
        StateOnUpdate();
        LineUpdate();

        //Text
        if (Input.GetKeyDown(KeyCode.P))
        {
            Hurt(10);
        }
        //
    }
    //void LineUpdate()
    //{空间小
    //    lineRenderer.positionCount = nav.path.corners.Length;
    //    Vector3[] vector3s = nav.path.corners;
    //    for (int i = 0; i < vector3s.Length; i++)
    //    {
    //        vector3s[i] = nav.path.corners[nav.path.corners.Length - 1 - i];
    //    }
    //    lineRenderer.SetPositions(vector3s);
    //}
    void LineUpdate()
    {
        if (nav.remainingDistance < 0.5f)
        {
            lineRenderer.gameObject.SetActive(false);
            return;
        }
        lineRenderer.gameObject.SetActive(true);
        lineRenderer.positionCount = nav.path.corners.Length;
        Vector3[] vector3s = nav.path.corners;
        Vector3 temp;
        for (int i = 0; i < vector3s.Length / 2; i++)
        {
            temp = vector3s[i];
            vector3s[i] = vector3s[vector3s.Length - 1 - i];
            vector3s[vector3s.Length - 1 - i] = temp;
        }
        lineRenderer.SetPositions(vector3s);
    }
    #region  unit选择状态
    /// <summary>
    /// 被选择状态
    /// </summary>
    public void IsSelect()
    {
        marker.SetActive(true);
        manager.AddThisTeam(this);
    }
    /// <summary>
    /// 被取消选择的状态
    /// </summary>
    public void IsNotSelect()
    {
        marker.SetActive(false);
        manager.RemoveThisTeam(this);
    }
    #endregion
    #region Skill
    /// <summary>
    /// 执行指令
    /// </summary>
    public void UseSkill(KeyCode keyName, Vector3 pos)
    {
        targetPos = pos;
        for (int i = 0; i < skillEventList.Count; i++)
        {
            if (skillEventList[i].keyName == keyName)
            {
                skillEventList[i].skillEvent.Invoke();
            }
        }
    }
    public void Move()
    {
        State = AIState.Move;
        nav.SetDestination(targetPos);
    }

    #endregion
    #region 动画事件
    void HurtEnd()
    {
        State = oldState;
    }
    #endregion
}
