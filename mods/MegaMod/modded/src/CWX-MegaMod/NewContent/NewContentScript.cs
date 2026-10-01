// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Comfort.Common;
// using System.Linq;
// using EFT;
// using FidelityFX.FSR3;
// using UnityEngine;
// using UnityEngine.Rendering.PostProcessing;
// using Object = UnityEngine.Object;
//
// namespace CWX_MegaMod.NewContent
// {
//     public class TempFuckaroundScript : MonoBehaviour
//     {
//         private GameWorld _gameWorld;
//         private List<GameObject> _allObjects;
//         private List<GameObject> _windowGameObjects;
//         private List<GameObject> _windowsGameObjects;
//         private List<GameObject> _glassGameObjects;
//         private List<GameObject> _doorGameObjects;
//         private List<GameObject> _decalGameObjects;
//         private IBotGame _botGame;
//
//         private static CameraClass _cameraClass;
//         private static Camera _camera;
//         private Fsr3UpscalerImageEffectHelper _fsrHelper;
//         private Fsr3UpscalerImageEffect _fsrImageEffect;
//         private AssetBundle _assetBundle;
//         private Fsr3UpscalerAssets _fsrAssets;
//         private Object[] _assetBundleLoaded;
//
//         private void Awake()
//         {
//             _gameWorld = Singleton<GameWorld>.Instance;
//             _allObjects = FindObjectsOfType<GameObject>(true).ToList();
//             _windowGameObjects = _allObjects.Where(x => x.name.ToLower().Contains("window")).ToList();
//             _windowsGameObjects = _allObjects.Where(x => x.name.ToLower() == "windows").ToList();
//             _glassGameObjects = _allObjects.Where(x => x.name.ToLower().Contains("glass")).ToList();
//             _doorGameObjects = _allObjects.Where(x => x.name.ToLower().Contains("door")).ToList();
//             _decalGameObjects = _allObjects.Where(x => x.name.ToLower().Contains("decal")).ToList();
//             _cameraClass = CameraClass.Instance;
//             _camera = _cameraClass.Camera;
//             _botGame = Singleton<IBotGame>.Instance;
//         }
//
//         public async Task StartTask()
//         {
//             // do nothing for now
//         }
//
//         public async Task RemovePostProcessLayer()
//         {
//             var postProcessLayer = _cameraClass.Camera.gameObject.GetComponent<PostProcessLayer>();
//             postProcessLayer.StopAllCoroutines();
//             postProcessLayer.enabled = false;
//         }
//
//         public async Task LoadFSRAssets()
//         {
//             // path to bundle
//             var path = "C:/SPT/4.0.0/BepInEx/plugins/CWX/fsr.bundle";
//             _assetBundle = AssetBundle.LoadFromFile(path);
//             _assetBundleLoaded = _assetBundle.LoadAllAssets();
//             _fsrAssets = (Fsr3UpscalerAssets) _assetBundleLoaded.FirstOrDefault(x => x.name == "FSR3 Upscaler Assets");
//             _fsrAssets.shaders = new Fsr3UpscalerShaders
//             {
//                 prepareInputsPass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_prepare_inputs_pass") as ComputeShader,
//                 lumaPyramidPass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_luma_pyramid_pass") as ComputeShader,
//                 shadingChangePyramidPass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_shading_change_pyramid_pass") as ComputeShader,
//                 shadingChangePass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_shading_change_pass") as ComputeShader,
//                 prepareReactivityPass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_prepare_reactivity_pass") as ComputeShader,
//                 lumaInstabilityPass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_luma_instability_pass") as ComputeShader,
//                 accumulatePass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_accumulate_pass") as ComputeShader,
//                 sharpenPass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_rcas_pass") as ComputeShader,
//                 autoGenReactivePass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_autogen_reactive_pass") as ComputeShader,
//                 tcrAutoGenPass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_tcr_autogen_pass") as ComputeShader,
//                 debugViewPass = _assetBundleLoaded.FirstOrDefault(x => x.name == "ffx_fsr3upscaler_debug_view_pass") as ComputeShader
//             };
//         }
//
//         public async Task AddFSR31()
//         {
//             _fsrHelper = _cameraClass.Camera.gameObject.AddComponent<Fsr3UpscalerImageEffectHelper>();
//             _fsrImageEffect = _cameraClass.Camera.gameObject.AddComponent<Fsr3UpscalerImageEffect>();
//         }
//
//         public async Task ApplyFSRAssets()
//         {
//             _fsrImageEffect.assets = _fsrAssets;
//         }
//
//         public async Task DisableHDR()
//         {
//             _cameraClass.Camera.allowHDR = false;
//         }
//
//         public async Task DisableBotController()
//         {
//             _botGame.BotsController.Disable();
//         }
//     }
// }