using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.WonderTools.Integration {
    public static class CelesteTasIntegration
    {
		private static bool IsCelesteTasInstalled = false;

		private static Type ConsoleCommand;
		private static Type TeleportRoomUtils;
		private static MethodInfo ConsoleCommand_CreateConsoleCommand;

		internal static void Load() {
			try {
				// Get type info and functions
				EverestModuleMetadata CelesteTAS = new()
				{
					Name = "CelesteTAS",
					Version = new Version(3, 36, 5)
				};
				bool CelesteTASLoaded = Everest.Loader.DependencyLoaded(CelesteTAS);
				WonderToolsModule.WonderLog($"CelesteTAS loaded {CelesteTASLoaded}");

				WonderToolsModule.WonderLog($"CelesteTAS loaded {CelesteTAS}");


				//ConsoleCommand = Type.GetType("Celeste.Mod.CelesteTAS-EverestInterop.TAS.Input.Commands.ConsoleCommand,CelesteTAS-EverestInterop");
				ConsoleCommand = Type.GetType("TAS.Input.Commands.ConsoleCommand,CelesteTAS-EverestInterop");
				if (ConsoleCommand == null) {
					WonderToolsModule.WonderLog("CONSOLE COMMAND FAILED");
					IsCelesteTasInstalled = false;
					return;
				}
				ConsoleCommand_CreateConsoleCommand = ConsoleCommand.GetMethod("CreateConsoleCommand", BindingFlags.Public | BindingFlags.Static);

				// Set up hooks

				// Misc
				IsCelesteTasInstalled = true;
			}
			catch (Exception e) {
                Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"{e} {IsCelesteTasInstalled}");
				IsCelesteTasInstalled = false;
			}
		}

		internal static void Unload() {
			return;
		}

		public static string CreateConsoleCommand(bool simple) {
			if (!IsCelesteTasInstalled) return null; 
			return  ConsoleCommand_CreateConsoleCommand?.Invoke(null, new object[] { simple }) as string;
        }
    }
}
