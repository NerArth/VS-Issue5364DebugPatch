using System.Diagnostics;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Vintagestory;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Issue5364DebugPatch
{
    public class Issue5364DebugPatchModSystem : ModSystem
    {

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            //api.Logger.Notification("Hello from template mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            //api.Logger.Notification("Hello from template mod server side: " + Lang.Get("issue5364debugpatch:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            TimeManager.api = api;
            api.Logger.Notification("ISSUE5364 client side");
            Harmony harmony = new Harmony("issue5364debugpatch");
            harmony.PatchAll();

            api.Event.RegisterGameTickListener(EverySecond, 1000);
        }

        public override void Dispose()
        {
            TimeManager.ResetExecutionFlag();
        }

        private void EverySecond(float dt)
        {
            var api = TimeManager.api;
            // This method is called every second
            // You can use it to reset the execution flag or perform other periodic tasks
            TimeManager.ResetExecutionFlag();
            //api.Logger.Debug("ISSUE5364 EverySecond called, resetting execution flag");
        }
    }

    [HarmonyPatch(typeof(WorldMapManager), "OnMapLayerDataReceivedClient")]
    public static class MapDataPrefixPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            bool recentlyExecuted = TimeManager.GetExecutionFlag();
            if (recentlyExecuted)
            {
                Debug.Print("ISSUE5364 WorldMapManager.OnMapLayerDataReceivedClient recently executed, skipping this call");
                return false; // Skip the original method if it has already been executed
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(WorldMapManager), "OnMapLayerDataReceivedClient")]
    public static class MapDataPostfixPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!TimeManager.GetExecutionFlag())
            {
                TimeManager.SetExecutionFlag();
            }
        }
    }

    public static class TimeManager
    {
        private static bool recentlyExecuted = false;
        public static ICoreClientAPI api { get; set; } = null;
        public static void ResetExecutionFlag()
        {
            recentlyExecuted = false;
            if (api is not null)
            {
                api?.Logger.Debug("ISSUE5364 recentlyExecuted flag reset");
            }
            else
            {
                Debug.Print("ISSUE5364 recentlyExecuted flag reset");
            }
        }
        public static void SetExecutionFlag()
        {
            recentlyExecuted = true;
            if (api is not null)
            {
                api?.Logger.Debug("ISSUE5364 recentlyExecuted flag set");
            }
            else
            {
                Debug.Print("ISSUE5364 recentlyExecuted flag set");
            }
        }

        public static bool GetExecutionFlag()
        {
            return recentlyExecuted;
        }
    }
}
