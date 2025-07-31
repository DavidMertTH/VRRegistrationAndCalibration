using System.Collections.Generic;
using UnityEngine;

public class Registration
{
    public bool register;

    private Vector3 _srcCenter;
    private Vector3 _targetCenter;
    private Kabsch.Kabsch _kabsch = new();

    public void AlignMesh(List<Vector3> selectedPositions, RegiTarget toTransform)
    {
        for (int i = 0; i < 5; i++)
        {
            _targetCenter = ShiftCenterOfMesh(selectedPositions, toTransform);
            float angle = FitMeshRotation(selectedPositions, toTransform, _targetCenter);
        }

        toTransform.SetVisible(true);
    }

    public void AlignMeshKabsch(List<Vector3> selectedPositions, RegiTarget toTransform)
    {
        toTransform.transform.position = Vector3.zero;
        toTransform.transform.rotation = Quaternion.identity;
        _kabsch.ReferencePoints = selectedPositions.ToArray();
        _kabsch.InPoints = toTransform.GetActiveRelativeMarkerPositions();
        _kabsch.TargetObject = toTransform.gameObject;
        _kabsch.SolveKabsch();
        toTransform.SetVisible(true);
    }

    private float FitMeshRotation(List<Vector3> selectedPositions, RegiTarget toTransform, Vector3 trgCenter)
    {
        List<Vector3> meshPositions = toTransform.GetMarkerPositions();
        float angle = 0;

        for (int i = 0; i < meshPositions.Count; i++)
        {
            Vector3 selectedToMid = trgCenter - selectedPositions[i];
            Vector3 meshToMid = trgCenter - meshPositions[i];

            selectedToMid.y = 0;
            meshToMid.y = 0;
            angle += Vector3.SignedAngle(selectedToMid, meshToMid, Vector3.up);
        }

        angle /= meshPositions.Count;
        toTransform.transform.RotateAround(trgCenter, Vector3.up, -angle);
        return angle;
    }

    private Vector3 ShiftCenterOfMesh(List<Vector3> selectedPositions, RegiTarget toTransform)
    {
        _srcCenter = GetCenter(selectedPositions);
        Vector3 trgCenter = GetCenter(toTransform.GetMarkerPositions());

        Vector3 translation = trgCenter - _srcCenter;
        toTransform.transform.position = toTransform.transform.position - translation;

        return trgCenter;
    }

    private Vector3 GetCenter(List<Vector3> positions)
    {
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < positions.Count; i++)
        {
            sum += positions[i];
        }

        return sum / positions.Count;
    }


       

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(_srcCenter, 0.01f);

        Gizmos.color = Color.yellowNice;
        Gizmos.DrawSphere(_targetCenter, 0.01f);
    }
}