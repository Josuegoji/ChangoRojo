# Chango Rojo / Experimentos de visión por computadora en Unity

Dos pruebas cortas de IA aplicada a Unity: detección de gestos faciales con
MediaPipe y un modelo propio de clasificación de plátanos con YOLO.

---

## 1. Detección de sonrisa y parpadeo (MediaPipe)

> 🎥 **Video:** https://youtu.be/D-ryR5MdGt4

Uso el Face Landmarker de [MediaPipe Unity Plugin](https://github.com/homuler/MediaPipeUnityPlugin),
con los **blendshapes** activados, que devuelve un puntaje de 0 a 1 para cada gesto
facial. El script [`DetectorRostro.cs`](DetectarRostro.cs)
lee `eyeBlinkLeft` y `eyeBlinkRight` para el parpadeo, y `mouthSmileLeft` y
`mouthSmileRight` para la sonrisa. Considero que hubo el gesto cuando **ambos lados**
superan un umbral configurable (0.65 por defecto), para evitar falsos positivos por
gestos de un solo lado.

El resultado de MediaPipe llega desde otro hilo, así que guardo el gesto en banderas
protegidas con `lock` y actualizo la interfaz en `Update`, que corre en el hilo principal.

Hay dos modos de prueba:
- **Pelea de miradas:** pierdes si parpadeas.
- **Si te ríes pierdes:** pierdes si sonríes.

**Qué es mío y qué no:** el plugin y las escenas de ejemplo son de terceros. Mi parte
es `DetectorRostro.cs` y una pequeña modificación al sample: agregué un evento
(`OnResultDetected`) a `FaceLandmarkerRunner` para poder recibir los resultados desde
otro script.

---

## 2. Clasificación de plátanos con un modelo propio (YOLO)

> 🎥 **Video:** https://youtu.be/PqHtXwSzJiM

Entrené un modelo de **clasificación de imágenes** con YOLO que decide si en la
imagen de la cámara hay un plátano o no, y lo exporté a **ONNX** para ejecutarlo
dentro de Unity con **Unity Inference Engine**.

- **Dataset:** dataset de Roboflow Universe.
- **Entrenamiento:** Google Colab, YOLO, 10 épocas.
- **Resultados:** Funciona medianamente bien, pero aun es sensible a calcificar objetos que se parecen mucho a un plátano como uno.

### Cómo funciona en Unity

El script [`DetectarPlatanos.cs`](DetectarPlatanos.cs):

1. Captura la cámara con `WebCamTexture`.
2. Recorta un cuadrado al centro del fotograma y lo reescala a 224×224 (con una
   función propia de interpolación bilineal).
3. Convierte los píxeles a un tensor y ejecuta el modelo con un `Worker`.
4. Lee las dos probabilidades de salida. Si la de "plátano" supera un umbral
   configurable (0.8 por defecto), muestra el mensaje en pantalla.
---

## Tecnologías

- Unity 6000.3.9f1
- C#
- MediaPipe Unity Plugin (homuler)
- YOLO y ONNX
- Google Colab


## Estado

Pruebas de concepto tempranas.

Este repositorio contiene **solo mis scripts y archivos de modelo**, no el proyecto
completo.

