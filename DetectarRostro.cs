using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Mediapipe.Tasks.Vision.FaceLandmarker;
using Mediapipe.Unity.Sample.FaceLandmarkDetection;

public class DetectorRostro : MonoBehaviour
{
    [Header("Configuración")] 
    [Range(0.0f, 1.0f)] [SerializeField] private float umbralParpadeo = 0.65f;
    [Range(0.0f, 1.0f)] [SerializeField] private float umbralSonrisa = 0.65f;

    [SerializeField] private FaceLandmarkerRunner runner;
    [Header("UI")]
    [SerializeField] private TMP_Text textoResultado;
    [SerializeField] private Button botonMiradas, botonRisas;
    private bool miradas = false, sonrisa = false;
    
    private bool parpadeo = false, sonrio = false;
    
    private void Awake()
    {
        runner.OnResultDetected.AddListener(OnFaceLandmarkerResult);
        botonMiradas.onClick.AddListener(PeleaDeMiradas);
        botonRisas.onClick.AddListener(SiTeRiesPierdes);
    }

    private void OnFaceLandmarkerResult(FaceLandmarkerResult result)
    {
        if (result.faceBlendshapes != null && result.faceBlendshapes.Count > 0)
        {
            var blendshapes = result.faceBlendshapes[0];

            float parpadeoIzquierdo = 0f;
            float parpadeoDerecho = 0f;

            float sonrisaIzquierda = 0f;
            float sonrisaDerecha = 0f;
            
            foreach (var classification in blendshapes.categories)
            {
                if(classification.categoryName == "eyeBlinkLeft")
                {
                    parpadeoIzquierdo = classification.score;
                }
                else if(classification.categoryName == "eyeBlinkRight")
                {
                    parpadeoDerecho = classification.score;
                }else if(classification.categoryName == "mouthSmileRight")
                {
                    sonrisaDerecha = classification.score;
                }else if (classification.categoryName == "mouthSmileLeft")
                {
                    sonrisaIzquierda = classification.score;
                }
            }
            
            bool estaParpadeando = (parpadeoIzquierdo > umbralParpadeo && parpadeoDerecho > umbralParpadeo);
            
            bool estaSonriendo = (sonrisaIzquierda > umbralSonrisa && sonrisaDerecha > umbralSonrisa);
            
            if(estaParpadeando && miradas)
            {
                Debug.Log("Parpadeaste");
                lock (this)
                {
                    parpadeo = true;
                }
            }

            if (estaSonriendo && sonrisa)
            {
                lock (this)
                {
                    sonrio = true;
                }
            }
        }
    }

    private void Update()
    {
        lock (this)
        {
            if (parpadeo && miradas)
            {
                MostrarMensaje(true);
            }
            
            if(sonrio && sonrisa)
            {
                MostrarMensaje(false);
            }
        }
    }


    private void MostrarMensaje(bool parpadeo)
    {
        if (textoResultado == null) return;
        if (parpadeo)
        {
            textoResultado.text = "Parpadeaste";
            textoResultado.color = Color.yellow;
        }
        else
        {
            textoResultado.text = "Sonreiste";
            textoResultado.color = Color.yellow;
        }
    }

    private void PeleaDeMiradas()
    {
        textoResultado.text = "";
        sonrisa = false;
        parpadeo = false;
        miradas = true;
    }
    
    private void SiTeRiesPierdes()
    {
        textoResultado.text = "";
        miradas = false;
        sonrio = false;
        sonrisa = true;
    }

    void OnDestroy()
    {
        runner.OnResultDetected.RemoveListener(OnFaceLandmarkerResult);
        botonMiradas.onClick.RemoveListener(PeleaDeMiradas);
        botonRisas.onClick.RemoveListener(SiTeRiesPierdes);
    }
}