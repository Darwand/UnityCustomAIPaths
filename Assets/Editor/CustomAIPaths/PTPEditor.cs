using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

enum SearchReason { Start, End, None};
enum ConnectionAction { Delete, Add};

[CustomEditor(typeof(PTPSet))]
public class PTPEditor : Editor
{
    PTPSet set;

    bool pointEditMode = true;

    bool displayPoints = true;
    List<bool> displayPoint = new List<bool>();
    List<bool> displayConnections = new List<bool>();

    Tool LastTool = Tool.None;

    bool addingConnection = false;
    bool removingConnection = false;

    ConnectionAction action;
    SearchReason currentSearch = SearchReason.None;

    PTPPoint start;
    PTPPoint end;
    
    float endSnapDist = 1f;


    void OnEnable()
    {
        set = (PTPSet)target;
        LastTool = Tools.current;
    }

    void OnDisable()
    {
        Tools.current = LastTool;
    }

    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Tools.current = Tool.None;

        Event e = Event.current;

        if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
        {
            pointEditMode = !pointEditMode;
            e.Use();
        }
        

        Handles.BeginGUI();
        if (GUILayout.Button(pointEditMode ? "Edit connections" : "Edit points", GUILayout.Width(120), GUILayout.Height(50)))
        {
            pointEditMode = !pointEditMode;
        }
        
        Handles.EndGUI();
        List<PTPPoint> points = set.GetPointsInSet();


        if (pointEditMode)
        {
            
            for (int i = 0; i < points.Count; i++)
            {
                points[i].SetLocation(Handles.PositionHandle(points[i].GetLocation(), Quaternion.identity));


                Dictionary<PTPPoint, float> connections = points[i].GetConnections();

                if (connections != null && connections.Count > 0)
                {
                    foreach (KeyValuePair<PTPPoint, float> connection in connections)
                    {
                        int connectedId = points.IndexOf(connection.Key);

                        if (connectedId > i)
                        {
                            Handles.DrawLine(points[i].GetLocation(), connection.Key.GetLocation());

                            Handles.Label(Vector3.Lerp(points[i].GetLocation(), connection.Key.GetLocation(), .5f), connection.Value.ToString());
                        }
                    }
                }
            }
        }
        else
        {
            Vector2 screenLocation = e.mousePosition;

            float closestDist = float.MaxValue;

            CheckMouseEvent(e);

            for (int i = 0; i < points.Count; i++)
            {
                Handles.color = Color.white;
                Handles.SphereHandleCap(i, points[i].GetLocation(), Quaternion.identity, .75f, EventType.Repaint);

                if(currentSearch != SearchReason.None && Camera.current != null)
                {
                    Vector2 screen = Camera.current.WorldToScreenPoint(points[i].GetLocation());

                    float dist = Vector2.Distance(screen, screenLocation);
                    if(currentSearch == SearchReason.Start)
                    {
                        if(dist < closestDist)
                        {
                            start = points[i];
                            closestDist = dist;
                        }
                    }
                    else
                    {
                        if(dist < closestDist && dist < endSnapDist)
                        {
                            end = points[i];
                            closestDist = dist;
                        }
                    }
                }

                DrawPointWithConnections(points, i);
               
            }

            UpdateDragHandles(e);

            DrawDragLine(e);
            
            CheckEndOfDrag(e);
        }

        Debug.Log(start != null);
    }

    void CheckMouseEvent( Event e )
    {
        //start a connection action
        if (e.button == 0 && e.type == EventType.MouseDown)
        {
            //add connection
            if (e.shift)
            {
                action = ConnectionAction.Add;
                currentSearch = SearchReason.Start;
                e.Use();

                Debug.Log("Starting");
            }
            //remove connection
            else if (e.control)
            {
                action = ConnectionAction.Delete;
                currentSearch = SearchReason.Start;
                e.Use();
            }
        }
        else if (e.button == 0 && e.type == EventType.MouseDrag)
        {
            currentSearch = SearchReason.End;
            e.Use();
        }
    }

    void DrawPointWithConnections( List<PTPPoint> points, int index )
    {
        Dictionary<PTPPoint, float> connections = points[index].GetConnections();

        if (connections != null && connections.Count > 0)
        {
            foreach (KeyValuePair<PTPPoint, float> connection in connections)
            {
                int connectedId = points.IndexOf(connection.Key);

                if (connectedId > index)
                {
                    Handles.DrawLine(points[index].GetLocation(), connection.Key.GetLocation());

                    Handles.Label(Vector3.Lerp(points[index].GetLocation(), connection.Key.GetLocation(), .5f), connection.Value.ToString());
                }
            }
        }
    }

    void DrawDragLine( Event e )
    {
        if (e.button == 0 && e.type == EventType.MouseDrag && start != null)
        {
            if (end != null)
            {
                Handles.DrawLine(start.GetLocation(), end.GetLocation());
            }
            else
            {
                Debug.Log("No End");
                Handles.DrawLine(start.GetLocation(), Camera.current.ScreenToViewportPoint(e.mousePosition));
            }
        }
    }

    void UpdateDragHandles( Event e)
    {
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftShift || e.keyCode == KeyCode.RightShift)
        {
            action = ConnectionAction.Add;
        }
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
        {
            action = ConnectionAction.Delete;
        }

        if (action == ConnectionAction.Add)
        {
            Handles.color = Color.blue;
        }
        else
        {
            Handles.color = Color.black;
        }
    }

    void CheckEndOfDrag( Event e )
    {
        //Check the event type and make sure it's left click.
        if (e.type == EventType.mouseUp && e.button == 0)
        {
            if (start != null && end != null)
            {
                if (action == ConnectionAction.Add)
                {
                    start.AddConnection(end, 1);
                }
                else
                {
                }
            }

            Debug.Log("Released");

            start = null;
            end = null;
            e.Use();
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();

        displayPoints = EditorGUILayout.Foldout(displayPoints, "Points");

        if(displayPoints)
        {
            EditorGUI.indentLevel++;
            List<PTPPoint> points = set.GetPointsInSet();

            for (int i = 0; i < points.Count; i++)
            {
                
                if(displayPoint.Count >= i)
                {
                    displayPoint.Add(true);
                }

                displayPoint[i] = EditorGUILayout.Foldout(displayPoint[i], "Point " + i);

                if(displayPoint[i])
                {
                    EditorGUI.indentLevel++;
                    points[i].SetLocation(EditorGUILayout.Vector3Field("Location", points[i].GetLocation()));

                    if (displayConnections.Count >= i)
                    {
                        displayConnections.Add(false);
                    }

                    displayConnections[i] = EditorGUILayout.Foldout(displayConnections[i], "Connections");

                    if (displayConnections[i])
                    {
                        EditorGUI.indentLevel++;
                        

                        if(points[i].GetConnections() != null && points[i].GetConnections().Count > 0)
                        {
                            Dictionary<PTPPoint, float> connections = new Dictionary<PTPPoint, float>(points[i].GetConnections());
                            foreach (KeyValuePair<PTPPoint, float> connection in connections)
                            {
                                points[i].AddConnection(connection.Key, EditorGUILayout.FloatField("Point " + points.IndexOf(connection.Key), connection.Value));
                            }
                        }
                        

                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
            }

            if(GUILayout.Button("Add new point"))
            {
                set.AddPoint(Vector3.zero);
            }

            EditorGUI.indentLevel--;
        }


        EditorGUILayout.EndVertical();
    }
}
