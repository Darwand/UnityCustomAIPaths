using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PTPPoint : ISerializationCallbackReceiver
{
    Dictionary<PTPPoint, float> connectedPointWeight;

    PTPSet owningSet;

    [SerializeField]
    private int[] keys;
    [SerializeField]
    private float[] values;

    [SerializeField]
    Vector3 location;

    public void OnBeforeSerialize()
    {
        var c = connectedPointWeight.Count;

        keys = new int[c];
        values = new float[c];

        int i = 0;
        using (var e = connectedPointWeight.GetEnumerator())
        while (e.MoveNext())
        {
            var kvp = e.Current;
            keys[i] = owningSet.GetIndexOfPoint(kvp.Key);
            values[i] = kvp.Value;
            i++;
        }

    }
    
    public void Deserialize(PTPSet owner)
    {
        owningSet = owner;

        //Debug.Log("Deserializing");

        var c = keys.Length;
        connectedPointWeight = new Dictionary<PTPPoint, float>(c);
        for (int i = 0; i < c; i++)
        {
            connectedPointWeight[owningSet.GetPointsInSet()[keys[i]]] = values[i];
        }
        keys = null;
        values = null;
    }

    public PTPPoint(Vector3 location, PTPSet owner)
    {
        connectedPointWeight = new Dictionary<PTPPoint, float>();
        this.location = location;
        owningSet = owner;
    }

    public void AddConnection( PTPPoint point, float weight )
    {
        //Set connection for self
        SetConnection(point, weight);

        //set connection for other point
        point.SetConnection(this, weight);
    }

    void SetConnection( PTPPoint point, float weight )
    {
        if (connectedPointWeight.ContainsKey(point))
        {
            connectedPointWeight[point] = weight;
        }
        else
        {
            connectedPointWeight.Add(point, weight);
        }
    }

    public void RemoveConnection( PTPPoint point )
    {
        if(connectedPointWeight.ContainsKey(point))
        {
            DeleteConnection(point);
            point.DeleteConnection(this);
        }
    }

    void DeleteConnection( PTPPoint point )
    {
        connectedPointWeight.Remove(point);
    }

    public Vector3 GetLocation()
    {
        return location;
    }

    public void SetLocation( Vector3 location )
    {
        this.location = location;
    }

    public Dictionary<PTPPoint, float> GetConnections()
    {
        return connectedPointWeight;
    }

    public void OnAfterDeserialize()
    {
        
    }
}
