//
// using System.Collections;
// using UnityEngine;
// using FidelityFX.FSR3;
//
// namespace CWX_MegaMod.NewContent
// {
//     /// <summary>
//     /// Small helper script to be used in conjunction with the Fsr3UpscalerImageEffect script.
//     /// The FSR3 Upscaler image effect needs to be the last effect in the post-processing chain but for render scaling to work properly, it also needs to be the first to execute OnPreCull.
//     /// Unfortunately altering the script execution order does not affect the order in which OnPreCull is executed. Only the order of scripts on the same game object matters. 
//     /// 
//     /// When combining FSR3 upscaling with other post-processing effects (most notably Unity's Post-Processing Stack V2),
//     /// this script should be added to the same camera and moved up above any other scripts that have an OnPreCull method. 
//     /// </summary>
//     [RequireComponent(typeof(Camera), typeof(Fsr3UpscalerImageEffect))]
//     public class Fsr3UpscalerImageEffectHelper : MonoBehaviour
//     {
//         private Camera _renderCamera;
//         private Fsr3UpscalerImageEffect _imageEffect;
//
//         private void OnEnable()
//         {
//             _renderCamera = GetComponent<Camera>();
//             _imageEffect = GetComponent<Fsr3UpscalerImageEffect>();
//         }
//
//         private void OnPreCull()
//         {
//             if (_imageEffect == null || !_imageEffect.enabled)
//                 return;
//             
//             var originalRect = _renderCamera.rect;
//             float upscaleRatio = Fsr3Upscaler.GetUpscaleRatioFromQualityMode(_imageEffect.qualityMode);
//
//             // Render to a smaller portion of the screen by manipulating the camera's viewport rect
//             _renderCamera.aspect = (float)_renderCamera.pixelWidth / _renderCamera.pixelHeight;
//             _renderCamera.rect = new Rect(0, 0, originalRect.width / upscaleRatio, originalRect.height / upscaleRatio);
//         }
//     }
// }