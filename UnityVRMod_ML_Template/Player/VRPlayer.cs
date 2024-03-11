using MelonLoader;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using static UnityVRMod_ML_Template.UnityVRPluging;
using MelonCoroutines = MelonLoader.MelonCoroutines;


namespace UnityVRMod_ML_Template.Player
{
    public class VRPlayer : MonoBehaviour
    {
        public VRPlayer(IntPtr value) : base(value) { }

        public static VRPlayer Instance { get; private set; }
        public Transform Origin { get; private set; }
        public Transform Body { get; private set; }
        public Camera Camera { get; private set; }
        public Camera FPSCam { get; private set; }
        public StereoRender StereoRender { get; private set; }
        public string lastCameraUsed = "";
        private static bool setupLock = false;
        public bool isUIMode = false;
        public GameObject LocalPlayer;


        private void Awake()
        {
            if (Instance)
            {
                MelonLogger.Error("# Trying to create duplicate VRPlayer! Awake Called! #");
                enabled = false;
                return;
            }

            MelonLogger.Msg("# VRPlayer  create Instance #");

            Instance = this;

            Body = transform;
            Origin = transform.parent;


            UnityVRPluging.onSceneLoaded += OnSceneLoaded;
            SteamVR_Actions.PreInitialize();
            SteamVR.InitializeStandalone(EVRApplicationType.VRApplication_Scene);

                      
            SetupImmediately();
            DontDestroyOnLoad(Origin);
        }


        private void OnSceneLoaded(int buildIndex, string sceneName)
        {        
            
            MelonLogger.Msg("# VRPlayer ON ScreenLoaded Called  #");
            MelonLoader.MelonCoroutines.Start(Setup());
            LocalPlayer = GameObject.FindGameObjectWithTag("Player");
            if (LocalPlayer)
            {
              MelonLogger.Msg($"# Found Player {LocalPlayer.name} #");
            }
        }


        public IEnumerator Setup()
        {
            if (setupLock)
                yield break;
            setupLock = true;
            if (StereoRender)
            {
                if (StereoRender.Head)
                {
                    Destroy(StereoRender.Head.gameObject);
                }
                Destroy(StereoRender);
                MelonLogger.Msg("# STEREORENDER DESTROYED #");
            }
            yield return new WaitForSeconds(5.0f);

            Camera[] cameraList = GameObject.FindObjectsOfType<Camera>();

            if (cameraList.Length > 0)
            {
                foreach (var camera in cameraList)
                {
                    MelonLogger.Msg($"# Camera> : {camera.name} #");
                    if (camera.tag == "MainCamera")
                    {
                        FPSCam = camera.GetComponent<Camera>();
                        lastCameraUsed = camera.name;
                        MelonLogger.Msg($"# GET CAMERA : {FPSCam.name} #");
                        //Add stereo camera to player body
                        StereoRender = Body.gameObject.AddComponent<StereoRender>();
                        Origin.position = FPSCam.transform.position;
                        Origin.rotation = FPSCam.transform.rotation;
                        break;
                    }
                }
            }
            setupLock = false;  
        }


        private void LateUpdate()
        {
            if (StereoRender != null && Origin != null)
            {
                Origin.position = FPSCam.transform.position - Vector3.up * 1.0f;
                FPSCam.transform.rotation = StereoRender.Head.rotation;
                try
                {
                    Camera[] cameraList = GameObject.FindObjectsOfType<Camera>();
                    foreach (var camera in cameraList)
                    {
                        if(camera.tag == "MainCamera")
                        {
                            if (FPSCam == null ||  camera.name != lastCameraUsed)
                            {
                                MelonLogger.Msg($"# Camera Change! #");
                                //MelonLoader.MelonCoroutines.Start(Setup());
                                SetupImmediately();
                            }
                            break;
                        }
                    }

                    // Set VRCamera on Player transform
                    if (LocalPlayer != null)
                    {                      
                        float camYaw = FPSCam.transform.rotation.eulerAngles.y;
                        var playerTransform = LocalPlayer.transform;
                        LocalPlayer.transform.localRotation = Quaternion.Euler( playerTransform.rotation.x, camYaw, playerTransform.rotation.z );                     
                    }
                }catch (Exception e)
                {
                    

                }

            }
  
        }

        private void SetupImmediately()
        {
            if (StereoRender)
            {
                if (StereoRender.Head)
                {
                    Destroy(StereoRender.Head.gameObject);
                }
                Destroy(StereoRender);
                MelonLogger.Msg("# STEREORENDER DESTROYED #");
            }

            Camera[] cameraList = GameObject.FindObjectsOfType<Camera>();

            if (cameraList.Length > 0)
            {
                foreach (var camera in cameraList)
                {
                    MelonLogger.Msg($"# Camera> : {camera.name} #");
                    if (camera.tag == "MainCamera")
                    {
                        FPSCam = camera.GetComponent<Camera>();
                        lastCameraUsed = camera.name;
                        MelonLogger.Msg($"# GET CAMERA : {FPSCam.name} #");
                        //Add stereo camera to player body
                        StereoRender = Body.gameObject.AddComponent<StereoRender>();
                        Origin.position = FPSCam.transform.position;
                        Origin.rotation = FPSCam.transform.rotation;
                        break;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            UnityVRPluging.onSceneLoaded -= OnSceneLoaded;
        }

        private void SetOriginHome()
        {
            SetOriginPosRotScl(new Vector3(0f, 0f, 0f), new Vector3(0, 90, 0), new Vector3(1, 1, 1));
        }

        public void SetOriginPosRotScl(Vector3 pos, Vector3 euler, Vector3 scale)
        {
            Origin.position = pos;
            Origin.localEulerAngles = euler;
            Origin.localScale = scale;
        }

        public void SetOriginScale(float scale)
        {
            Origin.localScale = new Vector3(scale, scale, scale);
        }

        public Vector3 GetWorldForward()
        {
            return StereoRender.Head.forward;
        }

        public Vector3 GetFlatForwardDirection(Vector3 foward)
        {
            foward.y = 0;
            return foward.normalized;
        }

        public float GetPlayerHeight()
        {
            if (!StereoRender.Head)
            {
                return 1.8f;
            }
            return StereoRender.Head.localPosition.y;
        }
    }
}
