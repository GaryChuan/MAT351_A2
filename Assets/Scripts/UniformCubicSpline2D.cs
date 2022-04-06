using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class UniformCubicSpline2D : MonoBehaviour
{

    [Header("Texts")]
    [SerializeField] Text errorText;
    [SerializeField] Text positionText;
    [SerializeField] Text timerText;

    [Header("Spline Attributes")]
    Vector2 initialVelocity;
    Vector2 finalVelocity;
    List<Vector2> points = new List<Vector2>(); 
    
    [SerializeField] TextAsset InputFile;
    [SerializeField] TextAsset OutputFile;


    // Spline data
    List<Vector2> data = new List<Vector2>();
    List<Quaternion> rotations = new List<Quaternion>();
    float[,] m;
    int N;
    
    float timer = 0;

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
        Debug.Log("N : "  + N);
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= N - 1)
        {
            timer -= N - 1 ;
        }
        Vector2 point = GeneratePoint(timer);
        Vector2 tangent = GenerateTangent(timer);

        float angle = (float)Mathf.Acos(tangent.x / tangent.magnitude);

        angle *=  tangent.y > 0 ? 1 : -1;

        gameObject.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

        Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + new Vector3(tangent.x, tangent.y, 0));
        gameObject.transform.position = new Vector3(point.x , point.y , 0);
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
            Vector2 point = GeneratePoint(t);
            lines.Add(new Vector3(point.x, point.y, 0));
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

    Vector2 GeneratePoint(float t)
    {
        Vector2 result = new Vector2();
        int ti = (int)t + 1;

        result = (data[ti]* (t - ti + 1)) / 6 * (((t - ti + 1) * (t - ti + 1)) - 1) +
                 (data[ti - 1] * (ti - t)) / 6 * (((ti - t) * (ti - t)) - 1) +
                 points[ti]* (t - ti + 1) + points[ti - 1] * (ti - t);
                

        return result;
    }

    Vector2 GenerateTangent(float t)
    {
        Vector2 result = new Vector2();
        int ti = (int)t + 1;

        result = (data[ti] / 6) * (3 * (t - ti + 1) * (t - ti + 1) - 1) -
                 (data[ti - 1] / 6) * (3 * (ti - t) * (ti - t) - 1) +
                 (points[ti] - points[ti - 1]);
        
        result.Normalize();

        return result;
    }

    Vector2 GenerateAcceleration(float t)
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

        initialVelocity = new Vector2(
            (float)Convert.ToDouble(initialVelocityInfo[0]),
            (float)Convert.ToDouble(initialVelocityInfo[1])
        );

        for(int i = 0; i < vertexCount; ++i)
        {
            var vertexInfo = inputStream.ReadLine()?.Split(' ');

            points.Add(
                    new Vector2(
                        (float)Convert.ToDouble(vertexInfo[0]),
                        (float)Convert.ToDouble(vertexInfo[1])
                    )
            );
        }

        var finalVelocityInfo = inputStream.ReadLine()?.Split(' ');

        finalVelocity = new Vector2(
            (float)Convert.ToDouble(finalVelocityInfo[0]),
            (float)Convert.ToDouble(finalVelocityInfo[1])
        );
    }
}
