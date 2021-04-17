using System;
using Microsoft.Xna.Framework;
using OTAPI;
using Terraria;
using ModFramework;

namespace TerrariaApi.Server.Hooking
{
	internal static class NpcHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Npc hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.NPC.SetDefaults += OnSetDefaultsById;
			On.Terraria.NPC.SetDefaultsFromNetId += OnSetDefaultsFromNetId;
			On.Terraria.NPC.StrikeNPC += OnStrike;
			On.Terraria.NPC.Transform += OnTransform;
			On.Terraria.NPC.AI += OnAI;

			Hooks.NPC.Spawn = OnSpawn;
			Hooks.NPC.DropLoot = OnDropLoot;
			Hooks.NPC.BossBag = OnBossBagItem;
			Hooks.NPC.Killed = OnKilled;
		}

		private static void OnKilled(NPC npc)
		{
			_hookManager.InvokeNpcKilled(npc);
		}

		private static void OnSetDefaultsById(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int Type, NPCSpawnParams spawnparams)
		{
			if (_hookManager.InvokeNpcSetDefaultsInt(ref Type, self))
				return;

			orig(self, Type, spawnparams);
		}

		private static void OnSetDefaultsFromNetId(On.Terraria.NPC.orig_SetDefaultsFromNetId orig, NPC self, int id, NPCSpawnParams spawnparams)
		{
			if (_hookManager.InvokeNpcNetDefaults(ref id, self))
				return;

			orig(self, id, spawnparams);
		}

		private static double OnStrike(On.Terraria.NPC.orig_StrikeNPC orig, NPC self, int Damage, float knockBack, int hitDirection, bool crit, bool noEffect, bool fromNet, Entity entity)
		{
			if (entity is Player player)
			{
				if (_hookManager.InvokeNpcStrike(self, ref Damage, ref knockBack, ref hitDirection, ref crit, ref noEffect, ref fromNet, player))
				{
					return 0;
				}
			}

			return orig(self, Damage, knockBack, hitDirection, crit, noEffect, fromNet, entity);
		}

		private static void OnTransform(On.Terraria.NPC.orig_Transform orig, NPC self, int newType)
		{
			if (_hookManager.InvokeNpcTransformation(self.whoAmI))
				return;

			orig(self, newType);
		}

		static HookResult OnSpawn(ref int index)
		{
			if (_hookManager.InvokeNpcSpawn(ref index))
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		static HookResult OnDropLoot(
			HookEvent @event, Terraria.NPC instance, ref int itemIndex,
			ref int X, ref int Y, ref int Width, ref int Height, ref int Type,
			ref int Stack, ref bool noBroadcast, ref int pfix, ref bool noGrabDelay, ref bool reverseLookup)
		{
			if (@event == HookEvent.Before)
			{
				var position = new Vector2(X, Y);
				if (_hookManager.InvokeNpcLootDrop
				(
					ref position,
					ref Width,
					ref Height,
					ref Type,
					ref Stack,
					ref noBroadcast,
					ref pfix,
					instance.type,
					instance.whoAmI,
					ref noGrabDelay,
					ref reverseLookup
				))
				{
					X = (int)position.X;
					Y = (int)position.Y;
					return HookResult.Cancel;
				}
				X = (int)position.X;
				Y = (int)position.Y;
			}

			return HookResult.Continue;
		}

		static HookResult OnBossBagItem(
				NPC npc,
				ref int X,
				ref int Y,
				ref int Width,
				ref int Height,
				ref int Type,
				ref int Stack,
				ref bool noBroadcast,
				ref int pfix,
				ref bool noGrabDelay,
				ref bool reverseLookup
			)
		{
			var positon = new Vector2(X, Y);
			if (_hookManager.InvokeDropBossBag
			(
				ref positon,
				ref Width,
				ref Height,
				ref Type,
				ref Stack,
				ref noBroadcast,
				ref pfix,
				npc.type,
				npc.whoAmI,
				ref noGrabDelay,
				ref reverseLookup
			))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}

		private static void OnAI(On.Terraria.NPC.orig_AI orig, NPC self)
		{
			if (_hookManager.InvokeNpcAIUpdate(self))
				return;

			orig(self);
		}
	}
}
