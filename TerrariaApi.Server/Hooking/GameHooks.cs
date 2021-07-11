﻿using Microsoft.Xna.Framework;
using OTAPI;

namespace TerrariaApi.Server.Hooking
{
	internal static class GameHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Game hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.Main.Update += OnUpdate;
			On.Terraria.Main.Initialize += OnInitialize;
			On.Terraria.Netplay.StartServer += OnStartServer;

			Hooks.WorldGen.HardmodeTilePlace += OnHardmodeTilePlace;
			Hooks.WorldGen.HardmodeTileUpdate += OnHardmodeTileUpdate;
			Hooks.Item.MechSpawn += OnItemMechSpawn;
			Hooks.NPC.MechSpawn += OnNpcMechSpawn;
		}

		private static void OnUpdate(On.Terraria.Main.orig_Update orig, Terraria.Main self, GameTime gameTime)
		{
			_hookManager.InvokeGameUpdate();
			orig(self, gameTime);
			_hookManager.InvokeGamePostUpdate();
		}

		private static void OnHardmodeTileUpdate(object sender, Hooks.WorldGen.HardmodeTileUpdateEventArgs e)
		{
			if (_hookManager.InvokeGameHardmodeTileUpdate(e.x, e.y, e.type))
			{
				e.Result = HookResult.Cancel;
			}
		}

		private static void OnHardmodeTilePlace(object sender, Hooks.WorldGen.HardmodeTilePlaceEventArgs e)
		{
			if (_hookManager.InvokeGameHardmodeTileUpdate(e.x, e.y, e.type))
			{
				e.Result = HardmodeTileUpdateResult.Cancel;
			}
		}

		private static void OnInitialize(On.Terraria.Main.orig_Initialize orig, Terraria.Main self)
		{
			HookManager.InitialiseAPI();
			_hookManager.InvokeGameInitialize();
			orig(self);
		}

		private static void OnStartServer(On.Terraria.Netplay.orig_StartServer orig)
		{
			_hookManager.InvokeGamePostInitialize();
			orig();
		}


		private static void OnItemMechSpawn(object sender, Hooks.Item.MechSpawnEventArgs e)
		{
			if (!_hookManager.InvokeGameStatueSpawn(e.num2, e.num3, e.num, (int)(e.x / 16f), (int)(e.y / 16f), e.type, false))
			{
				e.Result = HookResult.Cancel;
			}
		}


		private static void OnNpcMechSpawn(object sender, Hooks.NPC.MechSpawnEventArgs e)
		{
			if (!_hookManager.InvokeGameStatueSpawn(e.num2, e.num3, e.num, (int)(e.x / 16f), (int)(e.y / 16f), e.type, true))
			{
				e.Result = HookResult.Cancel;
			}
		}
	}
}
