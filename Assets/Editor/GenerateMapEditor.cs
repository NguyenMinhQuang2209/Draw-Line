using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

[CustomEditor(typeof(GenerateMapController))]
public class GenerateMapEditor : Editor
{
    private int inputIndex = 0;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GenerateMapController controller = (GenerateMapController)target;
        if (GUILayout.Button("Generate Map To Json"))
        {
            List<PathPoint> pointList = controller.GetPoints();

            List<MapDataNodeItem> nodeItemList = new List<MapDataNodeItem>();
            Dictionary<PathPoint, int> nodeItemDict = new Dictionary<PathPoint, int>();
            for (int i = 0; i < pointList.Count; i++)
            {
                PathPoint pathPoint = pointList[i];
                Vector2 currentPos = pathPoint.CurrentPosition();
                MapDataNodeItem node = new MapDataNodeItem()
                {
                    nodeID = i,
                    positionX = currentPos.x,
                    positionY = currentPos.y,
                };
                nodeItemList.Add(node);
                nodeItemDict[pathPoint] = i;
            }
            List<MapDataConnectedNodeItem> mapDataConntectedList = new List<MapDataConnectedNodeItem>();

            for (int i = 0; i < pointList.Count; i++)
            {

                PathPoint pathPoint = pointList[i];
                if (nodeItemDict.TryGetValue(pathPoint, out int currentIndex))
                {
                    MapDataConnectedNodeItem connected = new MapDataConnectedNodeItem()
                    {
                        connectedNodeList = new List<int>(),
                        nodeID = currentIndex
                    };

                    foreach (PathPoint point in pathPoint.GetDefaultConnectedList())
                    {
                        if (nodeItemDict.TryGetValue(point, out int index))
                        {
                            connected.connectedNodeList.Add(index);
                        }
                    }
                    mapDataConntectedList.Add(connected);
                }
            }

            MapData mapData = new MapData();
            mapData.nodeItemList = nodeItemList;
            mapData.nodeConnectedList = mapDataConntectedList;


            string jsonData = JsonConvert.SerializeObject(mapData);

            string path = Path.Combine(Application.persistentDataPath, "MapData.json");

            File.WriteAllText(path, jsonData);
        }
        GUILayout.Label("Map Index", EditorStyles.boldLabel);

        inputIndex = EditorGUILayout.IntField(new GUIContent("Enter Map Index:"), inputIndex);
        if (GUILayout.Button("Load Json Map Data"))
        {
            string fileName = $"MapData_{inputIndex}";
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
            controller.SpawnPoints(pathPointItemList, pathPointConnections);
        }
    }
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