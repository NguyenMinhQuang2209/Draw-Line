using System;
using System.Collections.Generic;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    [SerializeField] private List<PathPoint> connectList = new List<PathPoint>();

    private List<PathPoint> connectedPathList = new List<PathPoint>();

    private void Awake()
    {
        connectedPathList = new List<PathPoint>(connectList);
    }
    public void AddConnectedPathPoint(PathPoint pathPoint)
    {
        connectedPathList.Add(pathPoint);
    }
    public void RemoveConnectedPathPoint(PathPoint pathPoint)
    {
        connectedPathList.Remove(pathPoint);
    }
    public void ClearPathPoint()
    {
        connectedPathList = new List<PathPoint>(connectList);
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < connectList.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, connectList[i].transform.position);
        }
    }
    public List<PathPoint> GetDefaultConnectedList()
    {
        return new List<PathPoint>(connectList);
    }

    public Vector2 CurrentPosition()
    {
        return transform.position;
    }
    public List<PathPoint> GetConnectedPointList()
    {
        return new List<PathPoint>(connectedPathList);
    }
    public void SetConnectionList(List<PathPoint> newList)
    {
        connectedPathList = new List<PathPoint>(newList);
    }
    public void SetDefaultConnectionList(List<PathPoint> pathPoints)
    {
        connectList = pathPoints;
        Transform lineRenderWrap = PrefabController.Instance.lineRenderWrap;
        LineRenderer prefab = PrefabController.Instance.lineRender;
        for (int i = 0; i < connectList.Count; i++)
        {
            PathPoint targetPoint = connectList[i];
            LineRenderer current = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, lineRenderWrap.transform);
            current.positionCount = 2;
            current.SetPosition(0, transform.position);
            current.SetPosition(1, targetPoint.transform.position);
        }
    }
}
