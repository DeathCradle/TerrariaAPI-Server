using OTAPI;
using System;
using Terraria;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using ModFramework;

namespace TerrariaApi.Server.Hooking
{
	internal class NetHooks
	{
		private static HookManager _hookManager;

		public static readonly object syncRoot = new object();

		/// <summary>
		/// Attaches any of the OTAPI Net hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.NetMessage.greetPlayer += OnGreetPlayer;
			On.Terraria.Netplay.OnConnectionAccepted += OnConnectionAccepted;
			On.Terraria.Chat.ChatHelper.BroadcastChatMessage += OnBroadcastChatMessage;

			Hooks.NetMessage.SendData = OnSendData;
			Hooks.NetMessage.SendBytes = OnSendBytes;
			Hooks.MessageBuffer.GetData = OnReceiveData;
			Hooks.MessageBuffer.NameCollision = OnNameCollision;
		}

		static void OnBroadcastChatMessage(On.Terraria.Chat.ChatHelper.orig_BroadcastChatMessage orig, NetworkText text, Color color, int excludedPlayer)
		{
			float r = color.R, g = color.G, b = color.B;

			var cancel = _hookManager.InvokeServerBroadcast(ref text, ref r, ref g, ref b);

			if (!cancel)
			{
				color.R = (byte)r;
				color.G = (byte)g;
				color.B = (byte)b;

				orig(text, color, excludedPlayer);
			}
		}

		static HookResult OnSendData(
			HookEvent @event,
			int bufferId,
			ref int msgType,
			ref int remoteClient,
			ref int ignoreClient,
			ref Terraria.Localization.NetworkText text,
			ref int number,
			ref float number2,
			ref float number3,
			ref float number4,
			ref int number5,
			ref int number6,
			ref int number7
		)
		{
			if (@event == HookEvent.Before)
			{
				if (_hookManager.InvokeNetSendData
				(
					ref msgType,
					ref remoteClient,
					ref ignoreClient,
					ref text,
					ref number,
					ref number2,
					ref number3,
					ref number4,
					ref number5,
					ref number6,
					ref number7
				))
				{
					return HookResult.Cancel;
				}
			}
			return HookResult.Continue;
		}

		static HookResult OnReceiveData(
			MessageBuffer instance,
			ref byte packetId,
			ref int readOffset,
			ref int start,
			ref int length,
			ref int messageType,
			ref int maxPackets
		)
		{
			if (!Enum.IsDefined(typeof(PacketTypes), (int)packetId))
			{
				return HookResult.Cancel;
			}
			if (_hookManager.InvokeNetGetData(ref packetId, instance, ref readOffset, ref length))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}

		static void OnGreetPlayer(On.Terraria.NetMessage.orig_greetPlayer orig, int plr)
		{
			if (_hookManager.InvokeNetGreetPlayer(plr))
				return;

			orig(plr);
		}

		static HookResult OnSendBytes(
			ref Terraria.Net.Sockets.ISocket socket,
			ref int remoteClient,
			ref byte[] data,
			ref int offset,
			ref int size,
			ref global::Terraria.Net.Sockets.SocketSendCallback callback,
			ref object state
		)
		{
			if (_hookManager.InvokeNetSendBytes(Netplay.Clients[remoteClient], data, offset, size))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}

		static HookResult OnNameCollision(Player player)
		{
			if (_hookManager.InvokeNetNameCollision(player.whoAmI, player.name))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}

		static void OnConnectionAccepted(On.Terraria.Netplay.orig_OnConnectionAccepted orig, Terraria.Net.Sockets.ISocket client)
		{
			int slot = FindNextOpenClientSlot();
			if (slot != -1)
			{
				Netplay.Clients[slot].Reset();
				Netplay.Clients[slot].Socket = client;
			}
			if (FindNextOpenClientSlot() == -1)
			{
				Netplay.StopListening();
			}
		}

		static int FindNextOpenClientSlot()
		{
			lock (syncRoot)
			{
				for (int i = 0; i < Main.maxNetPlayers; i++)
				{
					if (!Netplay.Clients[i].IsConnected())
					{
						return i;
					}
				}
			}
			return -1;
		}
	}
}
