using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Provides functionality for calibrating a stylus-like Gameobject.
/// </summary>
/// <remarks>
/// David Mertens, TH Koeln.
/// Algorith by: https://www.ims.tuwien.ac.at/publications/tr-1882-01n.pdf
/// </remarks>
/// 
public class Calibrator : MonoBehaviour
{
    [HideInInspector] public GameObject toCalibrate;
    private Vector3 _calibratedPosition;
    private List<Vector3> _sampledPositions = new List<Vector3>();
    private List<Vector3> _sampledDirections = new List<Vector3>();
    private bool _isRecording = false;
    private GameObject _centerMarker;

    private void Start()
    {
        _centerMarker = Helper.CreateSmallSphere();
        _centerMarker.name = "calibrated Tip";
        _centerMarker.transform.parent = transform;
        _centerMarker.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        _centerMarker.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Returns the current position of the calibrated Object.
    /// </summary>
    public Vector3 GetCalibratedCurrentPosition()
    {
        return _centerMarker.transform.position;
    }

    private void Update()
    {
        if (!_isRecording) return;

        SampleControllerData();
        if (_sampledPositions.Count > 3) _centerMarker.transform.position = SphereNumericalSolver(_sampledPositions);
    }

    /// <summary>
    /// Start the calibration process. The game object being calibrated should only move and rotate over the calibration tip until the
    /// recording ends.
    /// </summary>
    public void StartRecording()
    {
        _isRecording = true;
        _sampledPositions.Clear();
        _sampledDirections.Clear();
        _centerMarker.transform.parent = null;
        _centerMarker.GetComponent<MeshRenderer>().enabled = true;
    }

    /// <summary>
    /// Stops the calibration process and links a calibrated Object to the input Gameobject.
    /// </summary>
    /// <returns>The Calibrated Object</returns>
    public GameObject StopRecording()
    {
        _isRecording = false;
        if (_sampledPositions.Count > 3)
        {
            _calibratedPosition = SphereNumericalSolver(_sampledPositions);
        }

        _centerMarker.transform.position = _calibratedPosition;
        _centerMarker.transform.parent = toCalibrate.transform;
        _centerMarker.GetComponent<MeshRenderer>().enabled = false;
        _sampledPositions.Clear();
        return _centerMarker;
    }

    private void SampleControllerData()
    {
        _sampledPositions.Add(toCalibrate.transform.position);
        _sampledDirections.Add(toCalibrate.transform.forward);
    }

    /// <summary>
    /// Calculates the center of a sphere given a set of points.
    /// </summary>
    /// <param name="inputPositions">List of measured positions.</param>
    /// <returns>The calculated center of the sphere.</returns>
    private Vector3 SphereNumericalSolver(List<Vector3> inputPositions)
    {
        int n = inputPositions.Count;
        if (n < 4)
            throw new ArgumentException("Minimum 4 Points are required.");

        var A = Matrix<double>.Build.Dense(n, 4);
        var b = Vector<double>.Build.Dense(n);

        for (int i = 0; i < n; i++)
        {
            var p = inputPositions[i];
            A[i, 0] = 2 * p.x;
            A[i, 1] = 2 * p.y;
            A[i, 2] = 2 * p.z;
            A[i, 3] = 1;
            b[i] = p.x * p.x + p.y * p.y + p.z * p.z;
        }

        var x = A.Solve(b);

        double cx = x[0], cy = x[1], cz = x[2];

        return new Vector3((float)cx, (float)cy, (float)cz);
    }
}