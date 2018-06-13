using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("AI/PointToPointSet")]
public class PTPSet : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    List<PTPPoint> pointsInSet;

    public PTPSet()
    {
        if(pointsInSet == null)
        {
            pointsInSet = new List<PTPPoint>();

            //AddPoint(new Vector3(0, 0, 0));
            //AddPoint(new Vector3(2, 0, 0));
            //AddPoint(new Vector3(2, 2, 0));

            //pointsInSet[0].AddConnection(pointsInSet[1], 2);
            //pointsInSet[1].AddConnection(pointsInSet[2], 2);
            //pointsInSet[2].AddConnection(pointsInSet[0], 2);
        }
    }


    [MenuItem("GameObject/AI/Point To Point Set")]
    public static  void EditorAddPTPSet()
    {
        GameObject gameObject = new GameObject("PTP Set");
        gameObject.AddComponent<PTPSet>();
    }

    public void OnAfterDeserialize()
    {
        foreach(PTPPoint point in pointsInSet)
        {
            point.Deserialize(this);
        }
    }

    public void AddPoint( Vector3 location )
    {
        PTPPoint newPoint = new PTPPoint(location, this);

        pointsInSet.Add(newPoint);
    }

    public void RemovePoint( int index )
    {
        pointsInSet.RemoveAt(index);
    }

    public List<PTPPoint> GetPointsInSet()
    {
        return pointsInSet;
    }

    public int GetIndexOfPoint( PTPPoint point )
    {
        return pointsInSet.IndexOf(point);
    }

    public void OnBeforeSerialize()
    {
        
    }
}
