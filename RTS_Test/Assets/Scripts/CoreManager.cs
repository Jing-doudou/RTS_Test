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

    public List<RaycastHit> unit = new List<RaycastHit>();//��ѡ���Ķ���
    public List<RaycastHit> unitLast = new List<RaycastHit>();//�ϴο�ѡ���Ķ���
    public LineRenderer lineRenderer;//����Ⱦ��
    public Vector3 mouseLeftDown;//��ʼ����
    public Vector3 mouseLeftDrag;//��ǰ����
    public Transform GroundTransform;

    public Transform uiUnitListParent;
    public GameObject unitImagePrefab;
    private bool inUi = false;
    private void Start()
    {
        //��ö��ǿתint���ֵ��ʼ��
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
    #region �б�λ����
    /// <summary>
    /// �е�λ����ѡ�����
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
    /// ÿ�θ��ĵ�ǰ��������ʱ������ˢ��UI���Ķ����б�
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
    /// ��ն���
    /// </summary>
    public void ClearunitTeamList()
    {
        for (int i = unitTeamList.Count - 1; i >= 0; i--)
        {
            unitTeamList[i].IsNotSelect();
        }
    }
    /// <summary>
    /// �Զ��鷢��ָ��
    /// </summary>
    public void UseTeamSkill(KeyCode keyName, Vector3 pos)
    {
        for (int i = 0; i < unitTeamList.Count; i++)
        {
            unitTeamList[i].UseSkill(keyName, pos);
        }
    }
    #endregion

    #region ������
    /// <summary>
    /// ���0�������
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
        //������Ⱦ��
        lineRenderer.enabled = true;
        mouseLeftDown = ray.point;
        //����ϴε㣬��ֹ�߿򶶶�
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
    /// ��������ק
    /// </summary>
    /// <param name="ray"></param>
    public void OnMouseLeftDrag(RaycastHit ray)
    {
        if (inUi)
        {
            return;
        }
        //���ƿ�ѡ��
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
    /// ������̧��
    /// </summary>
    /// <param name="ray"></param>
    public void OnMouseLeftUp(RaycastHit ray)
    {
        //if�����ô���δ�ƶ�������Ҫ��ѡ����
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
        //�ر���Ⱦ��
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
                //ȡ��ѡ��״̬
            }
        }
    }
    /// <summary>
    /// ���1�������
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

    #region �������
    protected void UnitGroup()
    {
        for (int iFor = (int)KeyCode.Alpha0; iFor < (int)KeyCode.Alpha9; iFor++)
        {
            if (Input.GetKeyDown((KeyCode)iFor))
            {
                //�������
                if (Input.GetKey(KeyCode.Z))
                {
                    _unitTeamListDict[(KeyCode)iFor] = new List<Unit_Ctrl>(unitTeamList);
                    return;
                }
                //�������ӵ�ǰ��ʿ��
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
                //���ֻ������ּ���ѡ���Ӧ����
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
