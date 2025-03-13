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
            controller.GenerateMap(inputIndex);
        }
    }
}
