using OTAPI;
using System;
using Terraria;
using ModFramework;

namespace TerrariaApi.Server.Hooking
{
	internal static class ServerHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Server hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.Main.startDedInput += Main_startDedInput;
			On.Terraria.RemoteClient.Reset += RemoteClient_Reset;
			Hooks.Main.CommandProcess = OnProcess;
		}

		private static void RemoteClient_Reset(On.Terraria.RemoteClient.orig_Reset orig, RemoteClient self)
		{
			if (!Netplay.Disconnect)
			{
				if (self.IsActive)
				{
					_hookManager.InvokeServerLeave(self.Id);
				}
				_hookManager.InvokeServerSocketReset(self);
			}

			orig(self);
		}

		private static void Main_startDedInput(On.Terraria.Main.orig_startDedInput orig)
		{
			if (Console.IsInputRedirected == true)
			{
				Console.WriteLine("TerrariaServer is running in the background and input is disabled.");
				return;
			}

			orig();
		}

		static HookResult OnProcess(string lowered, string raw)
		{
			if (_hookManager.InvokeServerCommand(raw))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}
	}
}
