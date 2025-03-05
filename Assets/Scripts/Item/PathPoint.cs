using System;
using System.Collections.Generic;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    [SerializeField] private List<PathPoint> connectList = new List<PathPoint>();

    private HashSet<PathPoint> connectedPathList = new HashSet<PathPoint>();
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
        connectedPathList.Clear();
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < connectList.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, connectList[i].transform.position);
        }
    }

    internal Vector2 CurrentPosition()
    {
        return transform.position;
    }
    public List<PathPoint> GetConnectedPointList()
    {
        return new List<PathPoint>(connectList);
    }
    public void SetConnectionList(List<PathPoint> newList)
    {
        connectList = newList;
    }
}
