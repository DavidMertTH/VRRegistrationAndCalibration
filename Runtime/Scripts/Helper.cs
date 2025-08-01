using UnityEngine;

public class Helper : MonoBehaviour
{
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    public static Color GetColorForIndex(int index)
    {
        switch (index)
        {
            case 0:
                return Color.blue;
            case 1:
                return Color.red;
            case 2:
                return Color.green;
            case 3:
                return Color.yellow;
            case 4:
                return Color.black;
            default:
                return Color.white;
        }
    }

    public static GameObject CreateSmallSphere()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(0, 0, 0);
        sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        return sphere;
    }

    public static void SetColor(GameObject go, Color color)
    {
        var render = go.GetComponent<Renderer>();
        if (render == null) return;

        var propertyBlock = new MaterialPropertyBlock();
        render.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColor, color);
        render.SetPropertyBlock(propertyBlock);
    }
}