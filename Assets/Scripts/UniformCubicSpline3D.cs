using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class UniformCubicSpline3D : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] Text errorText;
    [SerializeField] Text positionText;
    [SerializeField] Text timerText;

    [Header("Spline Attributes")]
    Vector3 initialVelocity;
    Vector3 finalVelocity;
    List<Vector3> points = new List<Vector3>(); 
    
    [SerializeField] TextAsset InputFile;
    [SerializeField] TextAsset OutputFile;


    // Spline data
    List<Vector3> data = new List<Vector3>();
    List<Quaternion> rotations = new List<Quaternion>();
    float[,] m;
    int N;
    
    float timer;

    [SerializeField] float timeMultiplier = 1f;

    void Awake()
    {
        if(File.Exists(AssetDatabase.GetAssetPath(InputFile)))
        {
            Load();
        }
        else
        {
            // errorText.gameObject.SetActive(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        N = points.Count;
        Initialize();
    }

    public void Exit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        if(timeMultiplier == 0)
        {
            return;
        }

        timer += Time.deltaTime * timeMultiplier;

        if(timer >= N - 1)
        {
            timer -= N - 1 ;
        }
        Vector3 point = GeneratePoint(timer);
        Vector3 tangent = GenerateTangent(timer);
        Vector3 acceleration = GenerateAcceleration(timer);
        Vector3 binormal = Vector3.Cross(tangent, acceleration);
        
        binormal.Normalize();

        Vector3 normal = Vector3.Cross(binormal, tangent);
        
        // N B T
        Matrix4x4 a = new Matrix4x4();

        a[0, 0] = normal.x;
        a[1, 0] = normal.y;
        a[2, 0] = normal.z;

        a[0, 1] = binormal.x;
        a[1, 1] = binormal.y;
        a[2, 1] = binormal.z;

        a[0, 2] = tangent.x;
        a[1, 2] = tangent.y;
        a[2, 2] = tangent.z;

        Quaternion q = new Quaternion();
        float trace = normal.x + binormal.y + tangent.z;
       
        if( trace > 0 ) {// I changed M_EPSILON to 0
            float s = 0.5f / Mathf.Sqrt(trace+ 1.0f);
            q.w = 0.25f / s;
            q.x = ( a[2, 1] - a[1, 2] ) * s;
            q.y = ( a[0, 2] - a[2, 0] ) * s;
            q.z = ( a[1, 0] - a[0, 1] ) * s;
        } 
        else if ( a[0, 0] > a[1, 1] && a[0, 0] > a[2, 2] ) 
        {
            float s = 2.0f * Mathf.Sqrt( 1.0f + a[0, 0] - a[1, 1] - a[2, 2]);
            q.w = (a[2, 1] - a[1, 2] ) / s;
            q.x = 0.25f * s;
            q.y = (a[0, 1] + a[1, 0] ) / s;
            q.z = (a[0, 2] + a[2, 0] ) / s;
        } 
        else if (a[1, 1] > a[2, 2]) {
            float s = 2.0f * Mathf.Sqrt( 1.0f + a[1, 1] - a[0, 0] - a[2, 2]);
            q.w = (a[0, 2] - a[2, 0] ) / s;
            q.x = (a[0, 1] + a[1, 0] ) / s;
            q.y = 0.25f * s;
            q.z = (a[1, 2] + a[2, 1] ) / s;
        } 
        else 
        {
            float s = 2.0f * Mathf.Sqrt( 1.0f + a[2, 2] - a[0, 0] - a[1, 1] );
            q.w = (a[1, 0] - a[0, 1] ) / s;
            q.x = (a[0, 2] + a[2, 0] ) / s;
            q.y = (a[1, 2] + a[2, 1] ) / s;
            q.z = 0.25f * s;
        }

        transform.rotation = q;

        // Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + new Vector3(tangent.x, tangent.y, 0));
        gameObject.transform.position = point;
    }

    void Initialize()
    {
        m = new float[N, N];
        
        // Populate the matrix
        m[0, 0] = 2;
        m[0, 1] = 1;

        for (int i = 1; i < N - 1; ++i)
        {
            m[i, i - 1] = 1;
            m[i, i]     = 4;
            m[i, i + 1] = 1;
        }

        m[N - 1, N - 2] = 1;
        m[N - 1, N - 1] = 2;

        // Populate the data
        data.Add(points[1] - points[0] - initialVelocity);

        for (int i = 1; i < N - 1; ++i)
        {
            data.Add(points[i + 1] - 2.0f * points[i] + points[i - 1]);
        }

        data.Add(finalVelocity - points[N - 1] + points[N - 2]);

        for (int i = 0; i < N; ++i)
        {
            data[i] *= 6;
        }

        Execute();
        
        List<Vector3> lines = new List<Vector3>();

        for(float t = 0f; t <= (N - 1); t += 0.01f)
        {
            lines.Add(GeneratePoint(t));
        }

        var lineRenderer = gameObject.GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = lines.Count;
        lineRenderer.SetPositions(lines.ToArray());
    }

    void Execute()
    {
        float tempMult = 1.0f / m[0, 0];

        m[0, 0] *= tempMult;
        m[0, 1] *= tempMult;

        data[0] *= tempMult;

        for (int i = 1; i < N - 1; ++i)
        {
            m[i, i - 1] -= m[i - 1, i - 1];
            m[i, i] -= m[i - 1, i];
            m[i, i + 1] -= m[i - 1, i + 1];

            data[i] -= data[i - 1];

            tempMult = 1.0f / m[i, i];

            m[i, i] *= tempMult;
            m[i, i + 1] *= tempMult;

            data[i] *= tempMult;
        }

        m[N - 1, N - 2] -= m[N - 2, N - 2];
        m[N - 1, N - 1] -= m[N - 2, N - 1];

        data[N - 1] -= data[N - 2];

        tempMult = 1.0f / m[N - 1, N - 1];

        m[N - 1, N - 1] *= tempMult;

        data[N - 1] *= tempMult;

        // Go upwards
        for (int i = N - 2; i >= 0; --i)
        {
            data[i] -= data[i + 1] * m[i, i + 1];
            m[i, i + 1] = 0.0f;
        }
    }

    Vector3 GeneratePoint(float t)
    {
        Vector3 result = new Vector3();
        int ti = (int)t + 1;

        result = (data[ti]* (t - ti + 1)) / 6 * (((t - ti + 1) * (t - ti + 1)) - 1) +
                 (data[ti - 1] * (ti - t)) / 6 * (((ti - t) * (ti - t)) - 1) +
                 points[ti]* (t - ti + 1) + points[ti - 1] * (ti - t);
                

        return result;
    }

    Vector3 GenerateTangent(float t)
    {
        Vector3 result = new Vector3();
        int ti = (int)t + 1;

        result = (data[ti] / 6) * (3 * (t - ti + 1) * (t - ti + 1) - 1) -
                 (data[ti - 1] / 6) * (3 * (ti - t) * (ti - t) - 1) +
                 (points[ti] - points[ti - 1]);
        
        result.Normalize();

        return result;
    }

    Vector3 GenerateAcceleration(float t)
    {
        int ti = (int)t + 1;
        return data[ti] * (t - ti + 1) + data[ti - 1] * (ti - t);
    }

    void Load()
    {
        StreamReader inputStream = File.OpenText(AssetDatabase.GetAssetPath(InputFile));

        Debug.Log("InputFile: " + InputFile.ToString());
        var vertexCountInfo = inputStream.ReadLine();
        var initialVelocityInfo = inputStream.ReadLine()?.Split(' ');
        
        int vertexCount = Convert.ToInt32(vertexCountInfo);

        initialVelocity = new Vector3(
            (float)Convert.ToDouble(initialVelocityInfo[0]),
            (float)Convert.ToDouble(initialVelocityInfo[1]),
            (float)Convert.ToDouble(initialVelocityInfo[2])
        );

        for(int i = 0; i < vertexCount; ++i)
        {
            var vertexInfo = inputStream.ReadLine()?.Split(' ');

            points.Add(
                    new Vector3(
                        (float)Convert.ToDouble(vertexInfo[0]),
                        (float)Convert.ToDouble(vertexInfo[1]),
                        (float)Convert.ToDouble(vertexInfo[2])
                    )
            );
        }

        var finalVelocityInfo = inputStream.ReadLine()?.Split(' ');

        finalVelocity = new Vector3(
            (float)Convert.ToDouble(finalVelocityInfo[0]),
            (float)Convert.ToDouble(finalVelocityInfo[1]),
            (float)Convert.ToDouble(finalVelocityInfo[2])
        );
    }

    public void SetTimeMultiplier(System.Single multiplier)
    {
        timeMultiplier = multiplier;
    }
}
