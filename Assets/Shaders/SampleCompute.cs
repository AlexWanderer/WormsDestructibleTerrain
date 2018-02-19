using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleCompute : MonoBehaviour
{
    public ComputeShader compute;
    public Color color;
    public RenderTexture result;

    // Update is called once per frame
    void Update()
    {
        int kernel = compute.FindKernel("CSMain");

        result = new RenderTexture(512, 512, 24);
        result.enableRandomWrite = true;
        result.Create();

        compute.SetTexture(kernel, "Result", result);
        compute.SetVector("color", color);
        compute.Dispatch(kernel, 512 / 8, 512 / 8, 1);
    }
}
