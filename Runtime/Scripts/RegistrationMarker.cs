using UnityEngine;

[ExecuteAlways]
public class RegiMarker : MonoBehaviour
{
    public Color color;
    
    private void OnDrawGizmos()
    {
        Color transparent = color;
        transparent.a = 0.2f;
        
        Gizmos.color = transparent;
        Gizmos.DrawSphere(transform.position, 0.03f);
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, 0.007f);
    }
}