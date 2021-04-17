using ModFramework;
using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class WorldHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI World hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.IO.WorldFile.SaveWorld_bool_bool += WorldFile_SaveWorld;
			On.Terraria.WorldGen.StartHardmode += WorldGen_StartHardmode;
			On.Terraria.WorldGen.SpreadGrass += WorldGen_SpreadGrass;
			On.Terraria.Main.checkXMas += Main_checkXMas;
			On.Terraria.Main.checkHalloween += Main_checkHalloween;

			Hooks.Collision.PressurePlate = OnPressurePlate;
			Hooks.WorldGen.Meteor = OnDropMeteor;
		}

		static HookResult OnPressurePlate(int x, int y, Entity entity)
		{
			if (entity is NPC npc)
			{
				if (_hookManager.InvokeNpcTriggerPressurePlate(npc, x, y))
					return HookResult.Cancel;
			}
			else if (entity is Player player)
			{
				if (_hookManager.InvokePlayerTriggerPressurePlate(player, x, y))
					return HookResult.Cancel;
			}
			else if (entity is Projectile projectile)
			{
				if (_hookManager.InvokeProjectileTriggerPressurePlate(projectile, x, y))
					return HookResult.Cancel;
			}

			return HookResult.Continue;
		}

		static void WorldFile_SaveWorld(On.Terraria.IO.WorldFile.orig_SaveWorld_bool_bool orig, bool useCloudSaving, bool resetTime)
		{
			if (_hookManager.InvokeWorldSave(resetTime))
				return;

			orig(useCloudSaving, resetTime);
		}

		private static void WorldGen_StartHardmode(On.Terraria.WorldGen.orig_StartHardmode orig)
		{
			if (_hookManager.InvokeWorldStartHardMode())
				return;

			orig();
		}

		static HookResult OnDropMeteor(ref int x, ref int y)
		{
			if (_hookManager.InvokeWorldMeteorDrop(x, y))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}

		private static void Main_checkXMas(On.Terraria.Main.orig_checkXMas orig)
		{
			if (_hookManager.InvokeWorldChristmasCheck(ref Terraria.Main.xMas))
				return;

			orig();
		}

		private static void Main_checkHalloween(On.Terraria.Main.orig_checkHalloween orig)
		{
			if (_hookManager.InvokeWorldHalloweenCheck(ref Main.halloween))
				return;

			orig();
		}

		private static void WorldGen_SpreadGrass(On.Terraria.WorldGen.orig_SpreadGrass orig, int i, int j, int dirt, int grass, bool repeat, byte color)
		{
			if (_hookManager.InvokeWorldGrassSpread(i, j, dirt, grass, repeat, color))
				return;

			orig(i, j, dirt, grass, repeat, color);
		}
	}
}
