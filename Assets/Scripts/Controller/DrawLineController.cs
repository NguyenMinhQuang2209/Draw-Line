using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DrawLineController : Singleton<DrawLineController>
{
    [SerializeField] private Transform nodeContainer;
    [SerializeField] private LineRenderer lineRender;
    private List<PathPoint> pathPointList = new List<PathPoint>();
    private List<PathPoint> pathPointListTemp = new List<PathPoint>();

    private List<PathPoint> lineRenderPathPointList = new List<PathPoint>();



    private bool isDrawing = false;
    (PathPoint, PathPoint) nearestPoint;
    private PathPoint addPoint = null;
    private PathPoint startPoint;

    private Vector2 previousMousePos = Vector2.zero;
    private bool isShowingResult = false;

    private HashSet<(PathPoint, PathPoint)> comparedPointList = new HashSet<(PathPoint, PathPoint)>();
    private List<(PathPoint, PathPoint)> storedPointList = new List<(PathPoint, PathPoint)>();

    PathPoint previousPoint = null;

    private Dictionary<(PathPoint, PathPoint), int> checkedPathDict = new Dictionary<(PathPoint, PathPoint), int>();

    private PathPoint firstConnectPoint = null;

    private Dictionary<(PathPoint, PathPoint), int> pathPointLineRenderIndexDict = new Dictionary<(PathPoint, PathPoint), int>();
    private Dictionary<PathPoint, List<PathPoint>> connectionPathDict = new Dictionary<PathPoint, List<PathPoint>>();
    protected override void Awake()
    {
        base.Awake();
        GenerateNodeList();
    }
    private void GenerateNodeList()
    {
        foreach (Transform child in nodeContainer.transform)
        {
            if (child.TryGetComponent<PathPoint>(out var item))
            {
                pathPointList.Add(item);
            }
        }
        pathPointListTemp = new List<PathPoint>(pathPointList);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            Vector2 mousePos = GetMousePosition();
            nearestPoint = GetNearestPathPoints(mousePos);
            if (nearestPoint.Item1 == null || nearestPoint.Item2 == null)
            {
                return;
            }
            Vector2 touchPoint = GetPerpendicularProjection(nearestPoint.Item1.CurrentPosition(), nearestPoint.Item2.CurrentPosition(), mousePos);
            float sqrtDistanceItem1 = Vector2.SqrMagnitude(touchPoint - nearestPoint.Item1.CurrentPosition());
            float sqrtDistanceItem2 = Vector2.SqrMagnitude(touchPoint - nearestPoint.Item2.CurrentPosition());

            if (sqrtDistanceItem1 <= 0.1f || sqrtDistanceItem2 <= 0.1f)
            {
                if (sqrtDistanceItem1 <= 0.1f)
                {
                    AddPathPoint(nearestPoint.Item1);
                    firstConnectPoint = nearestPoint.Item2;
                }
                else
                {
                    AddPathPoint(nearestPoint.Item2);
                    firstConnectPoint = nearestPoint.Item1;
                }
            }
            else
            {
                PathPoint firstPoint = AddNewPathPoint(touchPoint);
                firstPoint.SetConnectionList(new List<PathPoint>() { nearestPoint.Item1, nearestPoint.Item2 });

                nearestPoint.Item1.RemoveConnectedPathPoint(nearestPoint.Item2);
                nearestPoint.Item1.AddConnectedPathPoint(firstPoint);

                nearestPoint.Item2.RemoveConnectedPathPoint(nearestPoint.Item1);
                nearestPoint.Item2.AddConnectedPathPoint(firstPoint);

                if (sqrtDistanceItem1 > sqrtDistanceItem2)
                {
                    nearestPoint.Item1 = firstPoint;
                    firstConnectPoint = nearestPoint.Item2;
                }
                else
                {
                    nearestPoint.Item2 = firstPoint;
                    firstConnectPoint = nearestPoint.Item1;
                }
                AddPathPoint(firstPoint);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            ClearPath();
        }
        if (isDrawing)
        {
            Vector2 mousePos = GetMousePosition();
            if (previousMousePos != mousePos)
            {
                previousMousePos = mousePos;
                UpdatePathPointLineRender();
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isShowingResult = true;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            EditorApplication.isPaused = true;
        }
    }
    private List<PathPoint> GetPathPointDict(PathPoint pathPoint)
    {
        if (connectionPathDict.TryGetValue(pathPoint, out var list))
        {
            return list;
        }
        return new List<PathPoint>();
    }
    private void AddPathPoint(PathPoint pathPoint)
    {
        if (startPoint != null)
        {
            bool canContinue = false;
            for (int i = 0; i < startPoint.GetConnectedPointList().Count; i++)
            {
                if (startPoint.GetConnectedPointList()[i] == pathPoint)
                {
                    canContinue = true;
                    break;
                }
            }
            if (!canContinue)
            {
                return;
            }
            previousPoint = startPoint;
            storedPointList.Add((startPoint, pathPoint));
            storedPointList.Add((pathPoint, startPoint));
        }
        List<PathPoint> storedListTemp = GetPathPointDict(pathPoint);
        storedListTemp.Add(startPoint);
        connectionPathDict[pathPoint] = storedListTemp;
        (PathPoint, PathPoint) currentConnected = (startPoint, pathPoint);
        (PathPoint, PathPoint) currentConnected2 = (pathPoint, startPoint);
        startPoint = pathPoint;
        lineRenderPathPointList.Add(pathPoint);
        pathPointLineRenderIndexDict[currentConnected] = lineRenderPathPointList.Count - 1;
        pathPointLineRenderIndexDict[currentConnected2] = lineRenderPathPointList.Count - 1;

        Dictionary<PathPoint, HashSet<PathPoint>> connectedDict = new Dictionary<PathPoint, HashSet<PathPoint>>();
        for (int i = 0; i < lineRenderPathPointList.Count; i++)
        {
            PathPoint currentPointCheck = lineRenderPathPointList[i];
            PathPoint previous = null;
            PathPoint next = null;
            if (i != 0)
            {
                previous = lineRenderPathPointList[i - 1];
            }
            if (i != lineRenderPathPointList.Count - 1)
            {
                next = lineRenderPathPointList[i + 1];
            }
            HashSet<PathPoint> listConnected;
            if (connectedDict.TryGetValue(currentPointCheck, out listConnected))
            {

            }
            else
            {
                listConnected = new HashSet<PathPoint>();
            }
            if (previous != null)
            {
                listConnected.Add(previous);
            }
            if (next != null)
            {
                listConnected.Add(next);
            }
            connectedDict[currentPointCheck] = listConnected;
        }
        bool isWinning = true;
        for (int i = 0; i < pathPointList.Count; i++)
        {
            PathPoint checkPath = pathPointList[i];
            if (connectedDict.TryGetValue(checkPath, out var listConnected))
            {
                for (int j = 0; j < checkPath.GetConnectedPointList().Count; j++)
                {
                    if (!listConnected.Contains(checkPath.GetConnectedPointList()[j]))
                    {
                        isWinning = false;
                        break;
                    }
                }
            }
            else
            {
                isWinning = false;
                break;
            }
            if (!isWinning)
            {
                break;
            }
        }
        if (isWinning)
        {
            Debug.Log("Winning");
        }
        else
        {
            if (connectedDict.TryGetValue(pathPoint, out var listConnected))
            {
                int currentCount = 0;
                for (int j = 0; j < pathPoint.GetConnectedPointList().Count; j++)
                {
                    if (listConnected.Contains(pathPoint.GetConnectedPointList()[j]))
                    {
                        currentCount += 1;
                    }
                }
                if (currentCount == pathPoint.GetConnectedPointList().Count)
                {
                    ClearPath();
                }
            }
        }
    }
    private void RemovePathPoint()
    {
        if (lineRenderPathPointList.Count > 0)
        {
            startPoint = lineRenderPathPointList[^1];
            if (lineRenderPathPointList.Count > 1)
            {
                previousPoint = lineRenderPathPointList[^2];
            }
            else
            {
                previousPoint = null;
            }
            nearestPoint.Item1 = startPoint;
            nearestPoint.Item2 = previousPoint != null ? previousPoint : firstConnectPoint;
            int index = lineRenderPathPointList.Count - 1;
            (PathPoint, PathPoint) result = (null, null);
            foreach (var (key, value) in pathPointLineRenderIndexDict)
            {
                if (value == index)
                {
                    result = key;
                }
            }
            (PathPoint, PathPoint) result2 = (result.Item2, result.Item1);
            pathPointLineRenderIndexDict.Remove(result);
            pathPointLineRenderIndexDict.Remove(result2);
            lineRenderPathPointList.RemoveAt(lineRenderPathPointList.Count - 1);
        }
    }
    private void UpdatePathPointLineRender()
    {
        Vector2 mousePos = GetMousePosition();

        (PathPoint, PathPoint) nearPoint = GetNearestPathPoints(mousePos);
        if (nearPoint.Item1 == null || nearPoint.Item2 == null)
        {
            return;
        }

        bool isNotCurrentPoint = nearPoint.Item1 != nearestPoint.Item2 && nearPoint.Item2 != nearestPoint.Item2;
        if (!isNotCurrentPoint)
        {
            isNotCurrentPoint = nearPoint.Item1 != nearestPoint.Item1 && nearPoint.Item2 != nearestPoint.Item1;
        }

        if (isNotCurrentPoint)
        {
            List<PathPoint> resultList1 = new List<PathPoint>();
            List<PathPoint> resultList2 = new List<PathPoint>();
            comparedPointList.Clear();
            GetPathPointToAVector(nearPoint, nearestPoint.Item1, ref resultList1);
            GetPathPointToAVector(nearPoint, nearestPoint.Item2, ref resultList2);

            List<PathPoint> resultList;
            if (resultList1.Count > resultList2.Count)
            {
                resultList = resultList2;
            }
            else if (resultList1.Count == resultList2.Count)
            {
                Vector2 touchPoint = GetPerpendicularProjection(nearPoint.Item1.CurrentPosition(), nearPoint.Item2.CurrentPosition(), mousePos);

                if (Vector2.SqrMagnitude(nearestPoint.Item1.CurrentPosition() - touchPoint) > Vector2.SqrMagnitude(nearestPoint.Item2.CurrentPosition() - touchPoint))
                {
                    resultList = resultList2;
                }
                else
                {
                    resultList = resultList1;
                }
            }
            else
            {
                resultList = resultList1;
            }
            nearestPoint = nearPoint;
            resultList.Remove(startPoint);
            for (int i = 0; i < resultList.Count; i++)
            {
                if (resultList[0] == previousPoint)
                {
                    RemovePathPoint();
                }
                else
                {
                    AddPathPoint(resultList[i]);
                }
            }
        }
        lineRender.positionCount = lineRenderPathPointList.Count + 1;
        for (int i = 0; i < lineRenderPathPointList.Count; i++)
        {
            lineRender.SetPosition(i, lineRenderPathPointList[i].CurrentPosition());
        }
        Vector2 mouseTouchPosition = GetPerpendicularProjection(nearestPoint.Item1.CurrentPosition(), nearestPoint.Item2.CurrentPosition(), mousePos);
        lineRender.SetPosition(lineRenderPathPointList.Count, mouseTouchPosition);
    }
    private void GetPathPointToAVector((PathPoint, PathPoint) comparePoints, PathPoint checkPoint, ref List<PathPoint> result)
    {
        Queue<List<PathPoint>> queue = new Queue<List<PathPoint>>();
        HashSet<PathPoint> visited = new HashSet<PathPoint>();

        queue.Enqueue(new List<PathPoint>() { checkPoint });
        while (queue.Count > 0)
        {
            List<PathPoint> path = queue.Dequeue();
            PathPoint lastPoint = path[^1];
            if (lastPoint == comparePoints.Item1 || lastPoint == comparePoints.Item2)
            {
                result.Clear();
                result.AddRange(path);
                return;
            }
            foreach (PathPoint connectPoint in lastPoint.GetConnectedPointList())
            {
                if (!visited.Contains(connectPoint))
                {
                    visited.Add(connectPoint);
                    List<PathPoint> newPath = new List<PathPoint>(path) { connectPoint };
                    queue.Enqueue(newPath);
                }
            }
        }
    }
    private PathPoint AddNewPathPoint(Vector2 position)
    {
        if (addPoint == null)
        {
            PathPoint prefab = PrefabController.Instance.pathPoint;
            addPoint = Instantiate(prefab, position, Quaternion.identity);
        }
        addPoint.gameObject.SetActive(true);
        addPoint.transform.localPosition = position;
        return addPoint;
    }
    private Vector2 GetPerpendicularProjection(Vector2 A, Vector2 B, Vector2 C)
    {
        Vector2 AB = B - A;
        Vector2 AC = C - A;

        float projectionScale = Vector2.Dot(AC, AB) / Vector2.Dot(AB, AB);
        Vector2 P = A + projectionScale * AB;
        return P;
    }

    private (PathPoint, PathPoint) GetNearestPathPoints(Vector2 mousePos)
    {
        (PathPoint, PathPoint) result = (null, null);
        checkedPathDict.Clear();
        float currentDistance = 0f;
        bool isFirstItem = true;

        for (int i = 0; i < pathPointList.Count; i++)
        {
            PathPoint currentPoint = pathPointList[i];
            for (int j = 0; j < currentPoint.GetConnectedPointList().Count; j++)
            {
                PathPoint comparePoint = currentPoint.GetConnectedPointList()[j];
                if (isFirstItem)
                {
                    if (!IsOutOfDistance(currentPoint.CurrentPosition(), comparePoint.CurrentPosition(), mousePos))
                    {
                        Vector2 touchLinePos = GetPerpendicularProjection(currentPoint.CurrentPosition(), comparePoint.CurrentPosition(), mousePos);
                        currentDistance = Vector2.SqrMagnitude(mousePos - touchLinePos);
                        isFirstItem = false;
                        result.Item1 = currentPoint;
                        result.Item2 = comparePoint;
                    }
                }
                else
                {
                    if (!checkedPathDict.ContainsKey((currentPoint, comparePoint)) && !checkedPathDict.ContainsKey((comparePoint, currentPoint)))
                    {
                        if (!IsOutOfDistance(currentPoint.CurrentPosition(), comparePoint.CurrentPosition(), mousePos))
                        {
                            Vector2 touchLinePos = GetPerpendicularProjection(currentPoint.CurrentPosition(), comparePoint.CurrentPosition(), mousePos);
                            float checkDistance = Vector2.SqrMagnitude(mousePos - touchLinePos);
                            if (checkDistance < currentDistance)
                            {
                                currentDistance = checkDistance;
                                result.Item1 = currentPoint;
                                result.Item2 = comparePoint;
                            }
                        }
                    }
                }
                checkedPathDict[(currentPoint, comparePoint)] = 1;
                checkedPathDict[(comparePoint, currentPoint)] = 1;
            }
        }
        return result;
    }
    private bool IsOutOfDistance(Vector2 startPoint, Vector2 endPoint, Vector2 mousePos)
    {
        Vector2 perpendicularVector = GetPerpendicularPoint(startPoint, endPoint, mousePos);

        float sqrt = Vector2.SqrMagnitude(startPoint - endPoint);
        float sqrt1 = Vector2.SqrMagnitude(perpendicularVector - endPoint);
        float sqrt2 = Vector2.SqrMagnitude(perpendicularVector - startPoint);

        return sqrt < sqrt1 || sqrt < sqrt2;
    }
    public Vector2 GetPerpendicularPoint(Vector2 A, Vector2 B, Vector2 C)
    {
        Vector2 direction = B - A;
        Vector2 AC = C - A;

        float t = Vector2.Dot(AC, direction) / Vector2.Dot(direction, direction);
        Vector2 projection = A + t * direction;

        return projection;
    }
    private float DistanceFromPointToLine(Vector2 A, Vector2 B, Vector2 P)
    {
        Vector2 AB = B - A;
        Vector2 AP = P - A;
        float crossProduct = AB.x * AP.y - AB.y * AP.x;
        float distance = Mathf.Abs(crossProduct) / AB.magnitude;
        return distance;
    }


    private void ClearPath()
    {
        lineRender.positionCount = 0;
        for (int i = 0; i < pathPointList.Count; i++)
        {
            pathPointList[i].ClearPathPoint();
        }
        pathPointList = new List<PathPoint>(pathPointListTemp);
        if (addPoint != null)
        {
            addPoint.gameObject.SetActive(false);
        }
        startPoint = null;
        storedPointList.Clear();
        comparedPointList.Clear();
        lineRenderPathPointList.Clear();
        connectionPathDict.Clear();
        pathPointLineRenderIndexDict.Clear();
    }
    public Vector2 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
[System.Serializable]
public struct ConnectedPath
{
    public PathPoint Item1;
    public PathPoint Item2;
}