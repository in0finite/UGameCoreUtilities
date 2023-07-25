using System.Collections.Generic;
using UnityEngine;

public class GLDebug : MonoBehaviour
{
    private struct Line
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public double startTime;
        public float duration;

        public Line(Vector3 start, Vector3 end, Color color, double startTime, float duration)
        {
            this.start = start;
            this.end = end;
            this.color = color;
            this.startTime = startTime;
            this.duration = duration;
        }

        public bool DurationExpired => Time.timeAsDouble - this.startTime >= this.duration;
    }

    private Material matZOn;
    private Material matZOff;

    public KeyCode toggleKey;
    public bool displayLines = true;
#if UNITY_EDITOR
    public bool displayGizmos = true;
#endif
    
    private List<Line> linesZOn = new List<Line>();
    private List<Line> linesZOff = new List<Line>();
    
    public Shader zOnShader;
    public Shader zOffShader;


    void Awake()
    {
        SetMaterial();

        if (null == this.GetComponent<Camera>())
        {
            Debug.LogError("There should be camera attached to the same game object");
        }

    }

    void SetMaterial()
    {
        matZOn = new Material(this.zOnShader);
        matZOn.hideFlags = HideFlags.HideAndDontSave;
        //matZOn.shader.hideFlags = HideFlags.HideAndDontSave;

        matZOff = new Material(this.zOffShader);
        matZOff.hideFlags = HideFlags.HideAndDontSave;
        //matZOff.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            displayLines = !displayLines;

        if (!displayLines)
        {
            linesZOn.RemoveAll(_ => _.DurationExpired);
            linesZOff.RemoveAll(_ => _.DurationExpired);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!displayGizmos || !Application.isPlaying)
            return;

        for (int i = 0; i < linesZOn.Count; i++)
        {
            Gizmos.color = linesZOn[i].color;
            Gizmos.DrawLine(linesZOn[i].start, linesZOn[i].end);
        }

        for (int i = 0; i < linesZOff.Count; i++)
        {
            Gizmos.color = linesZOff[i].color;
            Gizmos.DrawLine(linesZOff[i].start, linesZOff[i].end);
        }
    }
#endif

    void OnPostRender()
    {
        if (!displayLines)
            return;

        matZOn.SetPass(0);

        GL.Begin(GL.LINES);

        linesZOn.RemoveAll(line =>
        {
            GL.Color(line.color);
            GL.Vertex(line.start);
            GL.Vertex(line.end);
            return line.DurationExpired;
        });

        GL.End();

        matZOff.SetPass(0);

        GL.Begin(GL.LINES);

        linesZOff.RemoveAll(line =>
        {
            GL.Color(line.color);
            GL.Vertex(line.start);
            GL.Vertex(line.end);
            return line.DurationExpired;
        });

        GL.End();
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = false)
    {
        if (duration == 0 && !displayLines)
            return;
        if (start == end)
            return;
        if (depthTest)
            linesZOn.Add(new Line(start, end, color, Time.timeAsDouble, duration));
        else
            linesZOff.Add(new Line(start, end, color, Time.timeAsDouble, duration));
    }

    public void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawLine(start, end, color ?? Color.white, duration, depthTest);
    }

    public void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0, bool depthTest = false)
    {
        if (dir == Vector3.zero)
            return;
        DrawLine(start, start + dir, color, duration, depthTest);
    }

    public void DrawLineArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawArrow(start, end - start, arrowHeadLength, arrowHeadAngle, color, duration, depthTest);
    }

    public void DrawArrow(Vector3 start, Vector3 dir, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
    {
        if (dir == Vector3.zero)
            return;
        DrawRay(start, dir, color, duration, depthTest);
        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        DrawRay(start + dir, right * arrowHeadLength, color, duration, depthTest);
        DrawRay(start + dir, left * arrowHeadLength, color, duration, depthTest);
    }

    public void DrawSquare(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawSquare(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color, duration, depthTest);
    }
    
    public void DrawSquare(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawSquare(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
    }
    
    public void DrawSquare(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
    {
        Vector3
                p_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, .5f)),
                p_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, -.5f)),
                p_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, -.5f)),
                p_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f));

        DrawLine(p_1, p_2, color, duration, depthTest);
        DrawLine(p_2, p_3, color, duration, depthTest);
        DrawLine(p_3, p_4, color, duration, depthTest);
        DrawLine(p_4, p_1, color, duration, depthTest);
    }

    public void DrawCube(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawCube(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color, duration, depthTest);
    }
    
    public void DrawCube(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawCube(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
    }
    
    public void DrawCube(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
    {
        Vector3
                down_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, .5f)),
                down_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, -.5f)),
                down_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, -.5f)),
                down_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, .5f)),
                up_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, .5f)),
                up_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, -.5f)),
                up_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, -.5f)),
                up_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, .5f));

        DrawLine(down_1, down_2, color, duration, depthTest);
        DrawLine(down_2, down_3, color, duration, depthTest);
        DrawLine(down_3, down_4, color, duration, depthTest);
        DrawLine(down_4, down_1, color, duration, depthTest);

        DrawLine(down_1, up_1, color, duration, depthTest);
        DrawLine(down_2, up_2, color, duration, depthTest);
        DrawLine(down_3, up_3, color, duration, depthTest);
        DrawLine(down_4, up_4, color, duration, depthTest);

        DrawLine(up_1, up_2, color, duration, depthTest);
        DrawLine(up_2, up_3, color, duration, depthTest);
        DrawLine(up_3, up_4, color, duration, depthTest);
        DrawLine(up_4, up_1, color, duration, depthTest);
    }

    public void DrawCircle(Vector3 center, float radius, Color? color = null, float duration = 0, bool depthTest = false)
    {
        for (float theta = 0.0f; theta < (2 * Mathf.PI); theta += 0.2f)
        {
            Vector3 ci = (new Vector3(Mathf.Cos(theta) * radius + center.x, Mathf.Sin(theta) * radius + center.y, center.z));
            DrawLine(ci, ci + new Vector3(0, 0.02f, 0), color, duration, depthTest);
        }
    }

}

