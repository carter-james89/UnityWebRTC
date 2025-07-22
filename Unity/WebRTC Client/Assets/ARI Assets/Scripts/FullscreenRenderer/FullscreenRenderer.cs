using UnityEngine;
using UnityEngine.UI;

public class FullscreenRenderer : MonoBehaviour
{
    public Material fullscreenMat;
    public Texture2D videoTexture;

    public RawImage Renderer;

    private void Awake()
    {
     //   fullscreenMat = Renderer.material;
       // videoTexture = fullscreenMat.mainTexture;
    }

    void Update()
    {
    //    if (fullscreenMat != null && videoTexture != null)
        {
            fullscreenMat.SetTexture("_BlitTexture", Renderer.mainTexture);
        }
    }

}
