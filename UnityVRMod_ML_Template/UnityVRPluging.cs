using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using UnityEngine;
using UnityVRMod_ML_Template.Player;
using Il2CppInterop.Runtime.Injection;

namespace UnityVRMod_ML_Template
{
    public static class BuildInfo
    {
        public const string Name = "UnityVRMod_ML_Template"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Unity Base VR StereoRender SteamVR Template"; // Description for the Mod.  (Set as null if none)
        public const string Author = "AnthonyMauri"; // Author of the Mod.  (MUST BE SET)
        public const string Company = "AMP"; // Company that made the Mod.  (Set as null if none)
        public const string Version = "0.0.1"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }


    public class UnityVRPluging : MelonMod
    {

        public static UnityVRPluging Instance { get; private set; }

        internal static bool vrEnabled;

        public delegate void OnSceneLoadedEvent(int buildindex, string sceneName);
        public static OnSceneLoadedEvent onSceneLoaded;
        public static bool blockVRplayerOnScreen = false;


        private void InitVR()
        {
            vrEnabled = true;
            // Register Mod Classes
            SetupIL2CPPClassInjections();
            // Load openvr_api dll
            LoadDll();
        }

        public static void LoadDll()
        {
            SetUnmanagedDllDirectory();
            var result = LoadLibrary("openvr_api.dll");
            MelonLogger.Msg("Load dll result: " + result);
            if ((Int64)result == 0)
            {
                MelonLogger.Error("Win32 ErrorInfo: " + Marshal.GetLastWin32Error());
            }
        }
        private void SetupIL2CPPClassInjections()
        {
            ClassInjector.RegisterTypeInIl2Cpp<VRSystems>();
            ClassInjector.RegisterTypeInIl2Cpp<VRPlayer>();
            ClassInjector.RegisterTypeInIl2Cpp<StereoRender>();
        }
        public static void SetUnmanagedDllDirectory()
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/../UserLibs";
            MelonLogger.Msg("SetUnmanagedDllDirectory: " + path);
            SetDllDirectory(path);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll", EntryPoint = "LoadLibrary", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            Logger($"********************************************************");
            Logger($"**** LOAD VRMod {UnityEngine.Application.version}    ****");
            Logger($"********************************************************");

            Instance = this;
            InitVR();

        }

        public override void OnApplicationLateStart() // Runs after OnApplicationStart.
        {
            MelonLogger.Msg("# OnApplicationLateStart #");

            if (!VRSystems.Instance)
            {
                new GameObject("VR_Globals").AddComponent<VRSystems>();
            }

        }

        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            MelonLogger.Msg("# OnSceneWasLoaded: " + buildindex.ToString() + " | " + sceneName + " #");

            if (onSceneLoaded != null)
            {
                onSceneLoaded.Invoke(buildindex, sceneName);
                
            }
        }

        public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
            MelonLogger.Msg("# OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName + " #");
            

        }

        public override void OnUpdate() // Runs once per frame.
        {
        }

        public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
        {
        }

        public override void OnLateUpdate() // Runs once per frame after OnUpdate and OnFixedUpdate have finished.
        {
            if (VRPlayer.Instance != null && VRPlayer.Instance.StereoRender != null && VRPlayer.Instance.StereoRender.stereoRenderPass != null)
            {
                VRPlayer.Instance.StereoRender.stereoRenderPass.Execute();
            }
        }

        public override void OnGUI() // Can run multiple times per frame. Mostly used for Unity's IMGUI.
        {
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
        }

        public override void OnPreferencesSaved() // Runs when Melon Preferences get saved.
        {
        }

        public override void OnPreferencesLoaded() // Runs when Melon Preferences get loaded.
        {
        }



        public void Logger(string message)
        {
            LoggerInstance.Msg(message);
        }
       
    }




}
