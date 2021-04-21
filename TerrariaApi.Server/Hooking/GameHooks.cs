using System;
using Microsoft.Xna.Framework;
using ModFramework;
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

			Hooks.WorldGen.HardmodeTilePlace = OnHardmodeTilePlace;
			Hooks.WorldGen.HardmodeTileUpdate = OnHardmodeTileUpdate;
			Hooks.Item.MechSpawn = OnItemMechSpawn;
			Hooks.NPC.MechSpawn = OnNpcMechSpawn;
		}

		private static void OnUpdate(On.Terraria.Main.orig_Update orig, Terraria.Main self, GameTime gameTime)
		{
			_hookManager.InvokeGameUpdate();
			orig(self, gameTime);
			_hookManager.InvokeGamePostUpdate();
		}

		static HookResult OnHardmodeTileUpdate(int x, int y, ushort type)
		{
			if (_hookManager.InvokeGameHardmodeTileUpdate(x, y, type))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}

		static HardmodeTileUpdateResult OnHardmodeTilePlace(ref int x, ref int y, ref int type, ref bool mute, ref bool forced, ref int plr, ref int style)
		{
			if (_hookManager.InvokeGameHardmodeTileUpdate(x, y, type))
			{
				return HardmodeTileUpdateResult.Cancel;
			}
			return HardmodeTileUpdateResult.Continue;
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

		static HookResult OnItemMechSpawn(float x, float y, int type, int num, int num2, int num3)
		{
			if (_hookManager.InvokeGameStatueSpawn(num2, num3, num, (int)(x / 16f), (int)(y / 16f), type, false))
			{
				return HookResult.Continue;
			}
			return HookResult.Cancel;
		}

		static HookResult OnNpcMechSpawn(float x, float y, int type, int num, int num2, int num3)
		{
			if (_hookManager.InvokeGameStatueSpawn(num2, num3, num, (int)(x / 16f), (int)(y / 16f), type, true))
			{
				return HookResult.Continue;
			}
			return HookResult.Cancel;
		}
	}
}
