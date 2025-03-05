using System.Collections.Generic;
using UnityEngine;

public class DrawLineController : Singleton<DrawLineController>
{
    [SerializeField] private Transform nodeContainer;
    [SerializeField] private LineRenderer lineRender;
    private List<PathPoint> pathPointList = new List<PathPoint>();
    private List<PathPoint> pathPointListTemp = new List<PathPoint>();

    private List<PathPoint> lineRenderPathPointList = new List<PathPoint>();


    private List<ConnectedPath> connectedPathList = new List<ConnectedPath>();

    private bool isDrawing = false;
    (PathPoint, PathPoint) nearestPoint;
    private PathPoint addPoint = null;
    private PathPoint startPoint;

    private Vector2 previousMousePos = Vector2.zero;
    private bool isShowingResult = false;

    private List<(PathPoint, PathPoint)> comparedPointList = new List<(PathPoint, PathPoint)>();
    private List<(PathPoint, PathPoint)> storedPointList = new List<(PathPoint, PathPoint)>();

    PathPoint previousPoint = null;
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
            PathPoint firstPoint = AddNewPathPoint(touchPoint);
            float sqrtDistanceItem1 = Vector2.SqrMagnitude(touchPoint - nearestPoint.Item1.CurrentPosition());
            float sqrtDistanceItem2 = Vector2.SqrMagnitude(touchPoint - nearestPoint.Item2.CurrentPosition());

            firstPoint.SetConnectionList(new List<PathPoint>() { nearestPoint.Item1, nearestPoint.Item2 });
            nearestPoint.Item1.RemoveConnectedPathPoint(nearestPoint.Item2);
            nearestPoint.Item1.AddConnectedPathPoint(firstPoint);

            nearestPoint.Item2.RemoveConnectedPathPoint(nearestPoint.Item1);
            nearestPoint.Item2.AddConnectedPathPoint(firstPoint);

            if (sqrtDistanceItem1 > sqrtDistanceItem2)
            {
                nearestPoint.Item1 = firstPoint;
            }
            else
            {
                nearestPoint.Item2 = firstPoint;
            }
            AddPathPoint(firstPoint);
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
            if (isShowingResult)
            {
                isShowingResult = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isShowingResult = true;
        }
    }
    private void AddPathPoint(PathPoint pathPoint)
    {
        if (startPoint != null)
        {
            previousPoint = startPoint;
            storedPointList.Add((startPoint, pathPoint));
            storedPointList.Add((pathPoint, startPoint));
        }
        startPoint = pathPoint;
        lineRenderPathPointList.Add(pathPoint);
    }
    private void RemovePathPoint()
    {
        if (lineRenderPathPointList.Count > 0)
        {
            startPoint = lineRenderPathPointList[^1];
            if (lineRenderPathPointList.Count > 1)
            {
                previousPoint = lineRenderPathPointList[^2];
                storedPointList.Remove((startPoint, previousPoint));
                storedPointList.Remove((previousPoint, startPoint));
            }
            else
            {
                previousPoint = null;
            }
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
            bool needCheck = true;
            if (previousPoint != null)
            {
                if ((nearPoint.Item1 == startPoint || nearPoint.Item2 == startPoint) &&
                (nearPoint.Item2 == previousPoint || nearPoint.Item1 == previousPoint))
                {
                    needCheck = false;
                    RemovePathPoint();
                }
                nearestPoint = nearPoint;
            }
            if (needCheck)
            {
                nearestPoint = nearPoint;
                List<PathPoint> resultList = new List<PathPoint>();
                comparedPointList.Clear();
                GetPathPointToAVector(true, nearestPoint, startPoint, ref resultList);
                for (int i = 0; i < resultList.Count; i++)
                {
                    AddPathPoint(resultList[i]);
                    Debug.Log(resultList[i]);
                }
                Debug.Log("------------");
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
    private void GetPathPointToAVector(bool isStartPoint, (PathPoint, PathPoint) comparePoints, PathPoint checkPoint, ref List<PathPoint> result)
    {
        if (checkPoint == comparePoints.Item1 || checkPoint == comparePoints.Item2)
        {
            if (checkPoint == comparePoints.Item1)
            {
                result.Add(comparePoints.Item1);
            }
            else
            {
                result.Add(comparePoints.Item2);
            }
            return;
        }
        else
        {
            result.Add(checkPoint);
            List<PathPoint> storePointListTemp = new List<PathPoint>();
            bool isInit = false;
            for (int i = 0; i < checkPoint.GetConnectedPointList().Count; i++)
            {
                List<PathPoint> tempPoint = new List<PathPoint>();
                (PathPoint, PathPoint) currentComparePoint = (checkPoint, checkPoint.GetConnectedPointList()[i]);
                if (!comparedPointList.Contains(currentComparePoint))
                {
                    comparedPointList.Add(currentComparePoint);
                    GetPathPointToAVector(false, comparePoints, checkPoint.GetConnectedPointList()[i], ref tempPoint);
                    if (!isInit)
                    {
                        isInit = true;
                        storePointListTemp = new List<PathPoint>(tempPoint);
                    }
                    else
                    {
                        if (tempPoint.Count < storePointListTemp.Count)
                        {
                            storePointListTemp = new List<PathPoint>(tempPoint);
                        }
                    }
                }
            }
            result.AddRange(storePointListTemp);
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
        float currentDistance = 0;
        bool haveValue = false;
        for (int i = 0; i < pathPointList.Count; i++)
        {
            PathPoint currentPoint = pathPointList[i];
            if (haveValue)
            {
                float checkDistance = Vector2.SqrMagnitude(pathPointList[i].CurrentPosition() - mousePos);
                if (checkDistance < currentDistance)
                {
                    for (int j = 0; j < pathPointList[i].GetConnectedPointList().Count; j++)
                    {
                        PathPoint connectPoint = pathPointList[i].GetConnectedPointList()[j];
                        if (!IsOutOfDistance(currentPoint.CurrentPosition(), connectPoint.CurrentPosition(), mousePos))
                        {
                            result.Item1 = pathPointList[i];
                            currentDistance = checkDistance;
                            continue;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < pathPointList[i].GetConnectedPointList().Count; j++)
                {
                    PathPoint connectPoint = pathPointList[i].GetConnectedPointList()[j];
                    if (!IsOutOfDistance(currentPoint.CurrentPosition(), connectPoint.CurrentPosition(), mousePos))
                    {
                        currentDistance = Vector2.SqrMagnitude(pathPointList[i].CurrentPosition() - mousePos);
                        haveValue = true;
                        result.Item1 = pathPointList[i];
                        continue;
                    }
                }
            }
        }
        if (result.Item1 == null)
        {
            return result;
        }
        List<PathPoint> connectedList = result.Item1.GetConnectedPointList();
        bool haveItem2 = false;
        float connectedDistance = 0;
        for (int i = 1; i < connectedList.Count; i++)
        {
            float compareDistance = DistanceFromPointToLine(result.Item1.CurrentPosition(), connectedList[i].CurrentPosition(), mousePos);
            if (haveItem2)
            {
                if (compareDistance < connectedDistance)
                {
                    if (!IsOutOfDistance(result.Item1.CurrentPosition(), connectedList[i].CurrentPosition(), mousePos))
                    {
                        result.Item2 = connectedList[i];
                        connectedDistance = compareDistance;
                    }
                }
            }
            else
            {
                if (!IsOutOfDistance(result.Item1.CurrentPosition(), connectedList[i].CurrentPosition(), mousePos))
                {
                    result.Item2 = connectedList[i];
                    connectedDistance = compareDistance;
                    haveItem2 = true;
                    continue;
                }
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
        pathPointListTemp = new List<PathPoint>(pathPointList);
        if (addPoint != null)
        {
            addPoint.gameObject.SetActive(false);
        }
        startPoint = null;
        storedPointList.Clear();
        comparedPointList.Clear();
        lineRenderPathPointList.Clear();

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