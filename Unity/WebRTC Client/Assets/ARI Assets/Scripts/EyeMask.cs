// EyeMask.cs
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class EyeMask : MonoBehaviour
{
    public enum Eye { Left, Right }
    [Tooltip("Which eye this camera should render into")]
    public Eye eye = Eye.Left;

    private void Awake()
    {
        ApplyMask();
    }

    void ApplyMask()
    {
        // <-- get the single Camera, not an array:
        var cam = GetComponent<Camera>();

        // now this property exists on UnityEngine.Camera
        cam.stereoTargetEye =
             eye == Eye.Left ? StereoTargetEyeMask.Left
           : eye == Eye.Right ? StereoTargetEyeMask.Right
                              : StereoTargetEyeMask.Both;
    }
}
