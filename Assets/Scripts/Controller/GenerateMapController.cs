using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class GenerateMapController : Singleton<GenerateMapController>
{
    public Transform pointWrap;
    public Transform point2Wrap;
    public PathPoint pathPointPrefab;

    private Dictionary<int, PathPoint> pathPointDict = new Dictionary<int, PathPoint>();
    public List<PathPoint> GetPoints()
    {
        List<PathPoint> results = new List<PathPoint>();
        foreach (Transform child in pointWrap.transform)
        {
            if (child.TryGetComponent<PathPoint>(out var point))
            {
                results.Add(point);
            }
        }
        return results;
    }
    public void SpawnPoints(List<PathPointItem> pathPoints, List<PathPointConnection> connectedList)
    {
        pathPointDict.Clear();
        foreach (Transform child in point2Wrap.transform)
        {
            DestroyImmediate(child.gameObject);
        }
        for (int i = 0; i < pathPoints.Count; i++)
        {
            PathPointItem currentPointItem = pathPoints[i];
            PathPoint currentPathPoint = Instantiate(pathPointPrefab, currentPointItem.pos, Quaternion.identity, point2Wrap.transform);
            currentPathPoint.name = currentPointItem.pathPointID.ToString();
            pathPointDict[currentPointItem.pathPointID] = currentPathPoint;
        }

        for (int i = 0; i < connectedList.Count; i++)
        {
            PathPointConnection connection = connectedList[i];
            if (pathPointDict.TryGetValue(connection.pathPointID, out PathPoint currentPoint))
            {
                List<PathPoint> conntectedPathPointList = new List<PathPoint>();
                for (int j = 0; j < connection.connectedList.Count; j++)
                {
                    int connectedIndex = connection.connectedList[j];
                    if (pathPointDict.TryGetValue(connectedIndex, out PathPoint connectedPath))
                    {
                        conntectedPathPointList.Add(connectedPath);
                    }
                }
                currentPoint.SetDefaultConnectionList(conntectedPathPointList);
            }
        }

    }

}
[System.Serializable]
public struct PathPointItem
{
    public Vector2 pos;
    public int pathPointID;
}
[System.Serializable]
public struct PathPointConnection
{
    public int pathPointID;
    public List<int> connectedList;
}