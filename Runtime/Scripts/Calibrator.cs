using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
    public class Calibrator : MonoBehaviour
    {
        public GameObject toCalibrate;
        public GameObject demoPrefab;

        private Vector3 _calibratedPosition;

        private List<Vector3> _sampledPositions = new List<Vector3>();
        private List<Vector3> _sampledDirections = new List<Vector3>();

        private bool _isRecording = false;
        private GameObject _centerMarker;

        private void Start()
        {
            _centerMarker = Instantiate(demoPrefab);
            _centerMarker.name = "calibrated Tip";
            _centerMarker.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _centerMarker.GetComponent<MeshRenderer>().enabled = false;
        }

        public Vector3 GetCalibratedCurrentPosition()
        {
            return _centerMarker.transform.position;
        }
        private void Update()
        {
            if (_isRecording)
            {
                SampleControllerData();
                if (_sampledPositions.Count > 3)
                {
                    _calibratedPosition = SphereNumericalSolver(_sampledPositions);
                }

                _centerMarker.transform.position = _calibratedPosition;
            }
        }

        public void StartRecording()
        {
            _isRecording = true;
            _sampledPositions.Clear();
            _sampledDirections.Clear();
            _centerMarker.transform.parent = null;
            _centerMarker.GetComponent<MeshRenderer>().enabled = true;
        }

        public void StopRecording()
        {
            _isRecording = false;
            if (_sampledPositions.Count > 3)
            {
                _calibratedPosition = SphereNumericalSolver(_sampledPositions);
            }

            _centerMarker.transform.position = _calibratedPosition;
            _centerMarker.transform.parent = toCalibrate.transform;
        }

        private void SampleControllerData()
        {
            _sampledPositions.Add(toCalibrate.transform.position);
            _sampledDirections.Add(toCalibrate.transform.forward);
        }

        private Vector3 SphereNumericalSolver(List<Vector3> inputPositions)
        {
            int n = inputPositions.Count;
            if (n < 4)
                throw new ArgumentException("Mindestens 4 Punkte für Kugelfit nötig.");

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
}