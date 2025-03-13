using Newtonsoft.Json;
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
    public void GenerateMap(int index)
    {
        string fileName = $"MapData_{index}";
        string path = $"Map/{fileName}";
        TextAsset fileData = Resources.Load<TextAsset>(path);
        MapData mapData = JsonConvert.DeserializeObject<MapData>(fileData.text);
        List<PathPointItem> pathPointItemList = new List<PathPointItem>();
        for (int i = 0; i < mapData.nodeItemList.Count; i++)
        {
            MapDataNodeItem mapDataNodeItem = mapData.nodeItemList[i];
            PathPointItem currentPathPointItem = new PathPointItem();
            currentPathPointItem.pathPointID = mapDataNodeItem.nodeID;
            currentPathPointItem.pos = new Vector2(mapDataNodeItem.positionX, mapDataNodeItem.positionY);
            pathPointItemList.Add(currentPathPointItem);
        }
        List<PathPointConnection> pathPointConnections = new List<PathPointConnection>();
        for (int i = 0; i < mapData.nodeConnectedList.Count; i++)
        {
            MapDataConnectedNodeItem item = mapData.nodeConnectedList[i];
            PathPointConnection connect = new PathPointConnection();
            connect.pathPointID = item.nodeID;
            connect.connectedList = item.connectedNodeList;

            pathPointConnections.Add(connect);
        }
        SpawnPoints(pathPointItemList, pathPointConnections);
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
[System.Serializable]
public struct MapData
{
    public List<MapDataNodeItem> nodeItemList;
    public List<MapDataConnectedNodeItem> nodeConnectedList;
}
[System.Serializable]
public struct MapDataConnectedNodeItem
{
    public int nodeID;
    public List<int> connectedNodeList;
}
[System.Serializable]
public struct MapDataNodeItem
{
    public int nodeID;
    public float positionX;
    public float positionY;
}