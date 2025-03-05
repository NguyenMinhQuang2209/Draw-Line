using UnityEngine;

public class PrefabController : Singleton<PrefabController>
{
    [field: SerializeField] public PathPoint pathPoint { get; private set; }
}
