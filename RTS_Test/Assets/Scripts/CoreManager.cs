using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Skill
{
    public string name;
    public KeyCode keyName;
    public UnityEngine.Events.UnityEvent skillEvent;
}
public class CoreManager : MonoBehaviour
{
    [SerializeField] Camera Camera;
    [SerializeField] Camera headVideoCamera;
    private Transform headVideoCameraTransform;
    private List<Unit_Ctrl> unitTeamList = new List<Unit_Ctrl>();

    public Dictionary<KeyCode, List<Unit_Ctrl>> _unitTeamListDict = new Dictionary<KeyCode, List<Unit_Ctrl>>();

    public List<RaycastHit> unit = new List<RaycastHit>();//框选到的队伍
    public List<RaycastHit> unitLast = new List<RaycastHit>();//上次框选到的队伍
    public LineRenderer lineRenderer;//线渲染器
    public Vector3 mouseLeftDown;//起始坐标
    public Vector3 mouseLeftDrag;//当前坐标
    public Transform GroundTransform;

    public Transform uiUnitListParent;
    public GameObject unitImagePrefab;
    private bool inUi = false;
    private void Start()
    {
        //将枚举强转int，字典初始化
        for (int i = (int)KeyCode.Alpha0; i < (int)KeyCode.Alpha9; i++)
        {
            _unitTeamListDict.Add((KeyCode)i, new List<Unit_Ctrl>());
        }
        GroundTransform = GameObject.Find("Ground").transform;
        uiUnitListParent = GameObject.Find("Canvas/UnitListPanel/UnitListParent").transform;
    }
    private void Update()
    {
        UpdataCameraTransform();
        UnitGroup();
        RaycastHit ray;

        if (Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out ray))
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnMouseLeftDown(ray);
            }
            if (Input.GetMouseButton(0))
            {
                OnMouseLeftDrag(ray);
            }
            if (Input.GetMouseButtonUp(0))
            {
                OnMouseLeftUp(ray);
            }
            if (Input.GetMouseButtonDown(1))
            {
                OnMouseRightDown(ray);
            }
        }
    }
    public void UpdataCameraTransform()
    {
        if (headVideoCameraTransform)
        {
            headVideoCamera.gameObject.SetActive(true);
            headVideoCamera.transform.position = headVideoCameraTransform.position;
            headVideoCamera.transform.rotation = headVideoCameraTransform.rotation;
        }
        else
        {
            headVideoCamera.gameObject.SetActive(false);
        }
    }
    #region 列表单位操作
    /// <summary>
    /// 有单位进入选择队列
    /// </summary>
    public void AddThisTeam(Unit_Ctrl unit)
    {
        if (!unitTeamList.Contains(unit))
        {
            unitTeamList.Add(unit);
        }
        UpdateHeadImager();
        UpdateUiUnitList();
    }
    public void RemoveThisTeam(Unit_Ctrl unit)
    {
        unitTeamList.Remove(unit);
        UpdateHeadImager();
        UpdateUiUnitList();
    }
    public void UpdateHeadImager()
    {
        if (unitTeamList.Count > 0)
        {
            headVideoCameraTransform = unitTeamList[0].cameraTransform.transform;
        }
        else
        {
            headVideoCameraTransform = null;
        }
    }

    /// <summary>
    /// 每次更改当前队列内容时，都会刷新UI面板的队伍列表
    /// </summary>
    public void UpdateUiUnitList()
    {
        for (int i = 0; i < uiUnitListParent.childCount; i++)
        {
            Destroy(uiUnitListParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < unitTeamList.Count; i++)
        {
            GameObject btn = Instantiate(unitImagePrefab, uiUnitListParent);
            btn.GetComponent<Image>().sprite = Resources.Load<Sprite>(unitTeamList[i].unit_Info.path);
        }
    }
    /// <summary>
    /// 清空队列
    /// </summary>
    public void ClearunitTeamList()
    {
        for (int i = unitTeamList.Count - 1; i >= 0; i--)
        {
            unitTeamList[i].IsNotSelect();
        }
    }
    /// <summary>
    /// 对队伍发出指令
    /// </summary>
    public void UseTeamSkill(KeyCode keyName, Vector3 pos)
    {
        for (int i = 0; i < unitTeamList.Count; i++)
        {
            unitTeamList[i].UseSkill(keyName, pos);
        }
    }
    #endregion

    #region 鼠标操作
    /// <summary>
    /// 鼠标0点击功能
    /// </summary>
    public void OnMouseLeftDown(RaycastHit ray)
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            inUi = true;
            return;
        }
        else
        {
            inUi = false;
        }
        //开启渲染线
        lineRenderer.enabled = true;
        mouseLeftDown = ray.point;
        //清除上次点，防止线框抖动
        mouseLeftDrag = ray.point;

        Unit_Ctrl unit = ray.collider.GetComponent<Unit_Ctrl>();
        if (unit)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                unit.IsSelect();
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (unitTeamList.Contains(unit))
                    {
                        unit.IsNotSelect();
                    }
                    else
                    {
                        unit.IsSelect();
                    }
                }
                else
                {
                    ClearunitTeamList();
                    unit.IsSelect();
                }
            }
        }
        if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            if (ray.collider.GetComponent<Ground>())
            {
                ClearunitTeamList();
            }
        }

    }
    /// <summary>
    /// 鼠标左键拖拽
    /// </summary>
    /// <param name="ray"></param>
    public void OnMouseLeftDrag(RaycastHit ray)
    {
        if (inUi)
        {
            return;
        }
        //绘制框选线
        {
            mouseLeftDrag = ray.point;
            lineRenderer.SetPosition(0, mouseLeftDown + Vector3.up * .1f);
            lineRenderer.SetPosition(1, new Vector3(mouseLeftDown.x, GroundTransform.position.y, mouseLeftDrag.z) + Vector3.up * .1f);
            lineRenderer.SetPosition(2, new Vector3(mouseLeftDrag.x, GroundTransform.position.y, mouseLeftDrag.z) + Vector3.up * .1f);
            lineRenderer.SetPosition(3, new Vector3(mouseLeftDrag.x, GroundTransform.position.y, mouseLeftDown.z) + Vector3.up * .1f);
            lineRenderer.SetPosition(4, mouseLeftDown + Vector3.up * .1f);
        }
    }
    /// <summary>
    /// 鼠标左键抬起
    /// </summary>
    /// <param name="ray"></param>
    public void OnMouseLeftUp(RaycastHit ray)
    {
        //if相等那么鼠标未移动，则不需要框选方法
        if (mouseLeftDown == mouseLeftDrag)
        {
            return;
        }
        {
            unit.Clear();
            foreach (RaycastHit item in Physics.BoxCastAll(Vector3.Lerp(mouseLeftDown, mouseLeftDrag, 0.5f), Vector3.Max(mouseLeftDown - mouseLeftDrag, mouseLeftDrag - mouseLeftDown) * .5f, Vector3.up))
            {
                if (item.transform.GetComponent<Unit_Ctrl>())
                {
                    unit.Add(item);
                }
            }
            unitLast = new List<RaycastHit>(unit);
        }
        //关闭渲染线
        lineRenderer.enabled = false;

        foreach (RaycastHit item in unitLast)
        {
            item.transform.GetComponent<Unit_Ctrl>().IsSelect();
        }
        unitLast.Clear();
        foreach (RaycastHit item in unitLast)
        {
            if (!unit.Contains(item))
            {
                //取消选中状态
            }
        }
    }
    /// <summary>
    /// 鼠标1点击功能
    /// </summary>
    public void OnMouseRightDown(RaycastHit ray)
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) { return; }
        if (ray.collider.GetComponent<Ground>())
        {
            UseTeamSkill(KeyCode.Mouse1, ray.point);
        }
    }
    #endregion

    #region 队伍分配
    protected void UnitGroup()
    {
        for (int iFor = (int)KeyCode.Alpha0; iFor < (int)KeyCode.Alpha9; iFor++)
        {
            if (Input.GetKeyDown((KeyCode)iFor))
            {
                //保存队伍
                if (Input.GetKey(KeyCode.Z))
                {
                    _unitTeamListDict[(KeyCode)iFor] = new List<Unit_Ctrl>(unitTeamList);
                    return;
                }
                //相队伍添加当前组士兵
                if (Input.GetKey(KeyCode.X))
                {
                    foreach (Unit_Ctrl item in unitTeamList)
                    {
                        if (!_unitTeamListDict[(KeyCode)iFor].Contains(item))
                        {
                            _unitTeamListDict[(KeyCode)iFor].Add(item);
                        }
                    }
                }
                //如果只点击数字键则选择对应队伍
                ClearunitTeamList();
                if (_unitTeamListDict[(KeyCode)iFor] != null)
                {
                    foreach (var item in _unitTeamListDict[(KeyCode)iFor])
                    {
                        item.IsSelect();
                    }
                    unitTeamList = new List<Unit_Ctrl>(_unitTeamListDict[(KeyCode)iFor]);
                }
            }
        }

    }
    #endregion
}
