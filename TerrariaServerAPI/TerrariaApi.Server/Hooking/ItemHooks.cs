using System;
using ModFramework;
using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class ItemHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Item hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.Item.SetDefaults_int_bool += OnSetDefaults;
			On.Terraria.Item.netDefaults += OnNetDefaults;

			Hooks.Chest.QuickStack = OnQuickStack;
		}

		private static void OnNetDefaults(On.Terraria.Item.orig_netDefaults orig, Item self, int type)
		{
			if (_hookManager.InvokeItemNetDefaults(ref type, self))
				return;

			orig(self, type);
		}

		private static void OnSetDefaults(On.Terraria.Item.orig_SetDefaults_int_bool orig, Item self, int Type, bool noMatCheck)
		{
			if (_hookManager.InvokeItemSetDefaultsInt(ref Type, self))
				return;

			orig(self, Type, noMatCheck);
		}

		static HookResult OnQuickStack(int playerId, Item item, int chestIndex)
		{
			if (_hookManager.InvokeItemForceIntoChest(Main.chest[chestIndex], item, Main.player[playerId]))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}
	}
}
