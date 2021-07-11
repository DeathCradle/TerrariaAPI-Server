using Microsoft.Xna.Framework;
using OTAPI;
using Terraria;

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

			Hooks.NPC.Spawn += OnSpawn;
			Hooks.NPC.DropLoot += OnDropLoot;
			Hooks.NPC.BossBag += OnBossBagItem;
			Hooks.NPC.Killed += OnKilled;
		}

		static void OnKilled(object sender, Hooks.NPC.KilledEventArgs e)
		{
			_hookManager.InvokeNpcKilled(e.npc);
		}

		static void OnSetDefaultsById(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int Type, NPCSpawnParams spawnparams)
		{
			if (_hookManager.InvokeNpcSetDefaultsInt(ref Type, self))
				return;

			orig(self, Type, spawnparams);
		}

		static void OnSetDefaultsFromNetId(On.Terraria.NPC.orig_SetDefaultsFromNetId orig, NPC self, int id, NPCSpawnParams spawnparams)
		{
			if (_hookManager.InvokeNpcNetDefaults(ref id, self))
				return;

			orig(self, id, spawnparams);
		}

		static double OnStrike(On.Terraria.NPC.orig_StrikeNPC orig, NPC self, int Damage, float knockBack, int hitDirection, bool crit, bool noEffect, bool fromNet, Entity entity)
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

		static void OnTransform(On.Terraria.NPC.orig_Transform orig, NPC self, int newType)
		{
			if (_hookManager.InvokeNpcTransformation(self.whoAmI))
				return;

			orig(self, newType);
		}

		static void OnSpawn(object sender, Hooks.NPC.SpawnEventArgs e)
		{
			var index = e.index;
			if (_hookManager.InvokeNpcSpawn(ref index))
			{
				e.Result = HookResult.Cancel;
				e.index = index;
			}
		}

		static void OnDropLoot(object sender, Hooks.NPC.DropLootEventArgs e)
		{
			if (e.Event == HookEvent.Before)
			{
				var Width = e.Width;
				var Height = e.Height;
				var Type = e.Type;
				var Stack = e.Stack;
				var noBroadcast = e.noBroadcast;
				var pfix = e.pfix;
				var noGrabDelay = e.noGrabDelay;
				var reverseLookup = e.reverseLookup;

				var position = new Vector2(e.X, e.Y);
				if (_hookManager.InvokeNpcLootDrop
				(
					ref position,
					ref Width,
					ref Height,
					ref Type,
					ref Stack,
					ref noBroadcast,
					ref pfix,
					e.npc.type,
					e.npc.whoAmI,
					ref noGrabDelay,
					ref reverseLookup
				))
				{
					e.X = (int)position.X;
					e.Y = (int)position.Y;
					e.Result = HookResult.Cancel;
				}
				e.X = (int)position.X;
				e.Y = (int)position.Y;

				e.Width = Width;
				e.Height = Height;
				e.Type = Type;
				e.Stack = Stack;
				e.noBroadcast = noBroadcast;
				e.pfix = pfix;
				e.noGrabDelay = noGrabDelay;
				e.reverseLookup = reverseLookup;
			}

		}

		static void OnBossBagItem(object sender, Hooks.NPC.BossBagEventArgs e)
		{
			var Width = e.Width;
			var Height = e.Height;
			var Type = e.Type;
			var Stack = e.Stack;
			var noBroadcast = e.noBroadcast;
			var pfix = e.pfix;
			var noGrabDelay = e.noGrabDelay;
			var reverseLookup = e.reverseLookup;

			var positon = new Vector2(e.X, e.Y);
			if (_hookManager.InvokeDropBossBag
			(
				ref positon,
				ref Width,
				ref Height,
				ref Type,
				ref Stack,
				ref noBroadcast,
				ref pfix,
				e.npc.type,
				e.npc.whoAmI,
				ref noGrabDelay,
				ref reverseLookup
			))
			{
				e.Result = HookResult.Cancel;
			}

			e.Width = Width;
			e.Height = Height;
			e.Type = Type;
			e.Stack = Stack;
			e.noBroadcast = noBroadcast;
			e.pfix = pfix;
			e.noGrabDelay = noGrabDelay;
			e.reverseLookup = reverseLookup;
		}

		static void OnAI(On.Terraria.NPC.orig_AI orig, NPC self)
		{
			if (_hookManager.InvokeNpcAIUpdate(self))
				return;

			orig(self);
		}
	}
}
