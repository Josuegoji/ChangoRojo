using UnityEngine;
using Unity.InferenceEngine;
using UnityEngine.UI;
using TMPro;

public class DetectarPlatanos : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    
    [SerializeField] private float umbralConicidencia = 0.8f;

    private WebCamTexture webcamTexture;
    private Worker worker;
    
    [SerializeField] private RawImage pantallaCamara;
    [SerializeField] private TMP_Text textoResultado;

    void Start()
    {
        BackendType backend;

        #if UNITY_IOS
        backend = BackendType.GPUCompute;
        #elif UNITY_ANDROID
        backend = BackendType.GPUCompute;
        #else
        backend = BackendType.CPU;
        #endif

        webcamTexture = new WebCamTexture(1920, 1080, 30);
        webcamTexture.Play();
        
        var model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, backend);
        
        if (pantallaCamara != null)
        {
            pantallaCamara.texture = webcamTexture;
            pantallaCamara.transform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);
            
            if (webcamTexture.videoVerticallyMirrored)
                pantallaCamara.transform.localScale = new Vector3(1, -1, 1);
        }
    }

    void Update()
    {
        if (webcamTexture.didUpdateThisFrame)
            EjecutarInferencia();
    }

    void EjecutarInferencia()
    {
        Texture2D texCompleta = new Texture2D(webcamTexture.width, webcamTexture.height);
        texCompleta.SetPixels32(webcamTexture.GetPixels32());
        texCompleta.Apply();
        
        int tamCrop = Mathf.Min(webcamTexture.width, webcamTexture.height);
        int startX  = (webcamTexture.width  - tamCrop) / 2;
        int startY  = (webcamTexture.height - tamCrop) / 2;
        
        Color[] pixelsCrop = texCompleta.GetPixels(startX, startY, tamCrop, tamCrop);
        Texture2D texCrop = new Texture2D(tamCrop, tamCrop);
        texCrop.SetPixels(pixelsCrop);
        texCrop.Apply();
        
        TextureScale.Bilinear(texCrop, 224, 224);
        
        var inputTensor = new Tensor<float>(new TensorShape(1, 224, 224, 3));
        Color[] pixels = texCrop.GetPixels();

        for (int y = 0; y < 224; y++)
        for (int x = 0; x < 224; x++)
        {
            int i = y * 224 + x;
            inputTensor[0, y, x, 0] = pixels[i].r;
            inputTensor[0, y, x, 1] = pixels[i].g;
            inputTensor[0, y, x, 2] = pixels[i].b;
        }

        worker.Schedule(inputTensor);

        using var output = worker.PeekOutput() as Tensor<float>;
        var resultados = output.DownloadToArray();

        float platano = resultados[0];
        float noPlatano   = resultados[1];

        Debug.Log($"Platano: {platano:P0} | No platano: {noPlatano:P0}");

        if (platano >= umbralConicidencia)
            MostrarMensaje(true);
        else
            MostrarMensaje(false);

        inputTensor.Dispose();
        Destroy(texCompleta);
        Destroy(texCrop);
    }

    void MostrarMensaje(bool hayPlatano)
    {
        if (textoResultado == null) return;

        if (hayPlatano)
        {
            textoResultado.gameObject.SetActive(true);
            textoResultado.text = "UN PLATANO";
            textoResultado.color = Color.yellow;
        }
        else
        {
            textoResultado.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        worker?.Dispose();
    }
}


public class TextureScale
{
    public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
    {
        Color[] newPixels = new Color[newWidth * newHeight];
        float scaleX = (float)tex.width / newWidth;
        float scaleY = (float)tex.height / newHeight;

        for (int y = 0; y < newHeight; y++)
        for (int x = 0; x < newWidth; x++)
            newPixels[y * newWidth + x] = tex.GetPixelBilinear(
                x / (float)newWidth,
                y / (float)newHeight
            );

        tex.Reinitialize(newWidth, newHeight);
        tex.SetPixels(newPixels);
        tex.Apply();
    }
}
