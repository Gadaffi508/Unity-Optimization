using UnityEngine;

[ExecuteInEditMode]
public class ShaderDisolve : MonoBehaviour
{
    public float radius = 1.0f;

    void Update()
    {
        Shader.SetGlobalVector("_Position", transform.position);
        Shader.SetGlobalFloat("_Radius", radius);
    }

}
