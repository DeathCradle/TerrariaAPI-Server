﻿using System;
using OTAPI;
using Terraria;

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
			Hooks.Main.CommandProcess += OnProcess;
		}

		static void RemoteClient_Reset(On.Terraria.RemoteClient.orig_Reset orig, RemoteClient self)
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

		static void Main_startDedInput(On.Terraria.Main.orig_startDedInput orig)
		{
			if (Console.IsInputRedirected == true)
			{
				Console.WriteLine("TerrariaServer is running in the background and input is disabled.");
				return;
			}

			orig();
		}

		static void OnProcess(object sender, Hooks.Main.CommandProcessEventArgs e)
		{
			if (_hookManager.InvokeServerCommand(e.command))
			{
				e.Result = HookResult.Cancel;
			}
		}
	}
}
