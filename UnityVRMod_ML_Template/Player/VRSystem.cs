using MelonLoader;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using AMPSonsVRMod_ML;

namespace UnityVRMod_ML_Template.Player
{
    public class VRSystems : MonoBehaviour
    {
        public VRSystems(IntPtr value) : base(value) { }

        public static VRSystems Instance { get; private set; }
        public static HarmonyLib.Harmony HarmonyInstance { get; set; }

        private void Awake()
        {

            MelonLogger.Error("########### INITIALIZE VRSYSTEM ########################");


            if (Instance)
            {
                MelonLogger.Error("Trying to create duplicate VRSystems class! Awake CAlled");
                enabled = false;
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (HarmonyInstance == null)
            {
                HarmonyInstance = new HarmonyLib.Harmony("it.ampowersoftware.SonsVRMod");
            }
            HarmonyInstance.PatchAll();

            UnityVRPluging.onSceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            CreateCameraRig();
        }

        private void CreateCameraRig()
        {
            if (!VRPlayer.Instance)
            {
                MelonLogger.Error("########### VRPLAYER INSTANCE IS NULL CRETATE RIG ########################");
                GameObject rig = new GameObject("[VRCameraRig]");
                rig.transform.parent = transform;
                rig.AddComponent<VRPlayer>();
            }
           
        }


        private void OnSceneLoaded(int buildindex, string sceneName)
        {
            if (VRPlayer.Instance == null)
            {
                CreateCameraRig();
            }
        }

        private void TogglePlayerCam(bool toggle)
        {
            if (toggle)
            {
                VRPlayer.Instance.StereoRender.LeftCam.cullingMask = 0;
                VRPlayer.Instance.StereoRender.RightCam.cullingMask = 0;
            }
            else
            {
                VRPlayer.Instance.StereoRender.LeftCam.cullingMask = StereoRender.defaultCullingMask;
                VRPlayer.Instance.StereoRender.RightCam.cullingMask = StereoRender.defaultCullingMask;
            }
        }

        private void OnDestroy()
        {
            UnityVRPluging.onSceneLoaded -= OnSceneLoaded;
        }
    }
}
