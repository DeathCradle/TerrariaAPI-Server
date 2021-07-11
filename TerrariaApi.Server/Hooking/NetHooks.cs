using System;
using Microsoft.Xna.Framework;
using OTAPI;
using Terraria;
using Terraria.Localization;

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

			Hooks.NetMessage.SendData += OnSendData;
			Hooks.NetMessage.SendBytes += OnSendBytes;
			Hooks.MessageBuffer.GetData += OnReceiveData;
			Hooks.MessageBuffer.NameCollision += OnNameCollision;
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

		static void OnSendData(object sender, Hooks.NetMessage.SendDataEventArgs e)
		{
			if (e.Event == HookEvent.Before)
			{
				var msgType = e.msgType;
				var remoteClient = e.remoteClient;
				var ignoreClient = e.ignoreClient;
				var text = e.text;
				var number = e.number;
				var number2 = e.number2;
				var number3 = e.number3;
				var number4 = e.number4;
				var number5 = e.number5;
				var number6 = e.number6;
				var number7 = e.number7;
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
					e.Result = HookResult.Cancel;
				}

				e.msgType = msgType;
				e.remoteClient = remoteClient;
				e.ignoreClient = ignoreClient;
				e.text = text;
				e.number = number;
				e.number2 = number2;
				e.number3 = number3;
				e.number4 = number4;
				e.number5 = number5;
				e.number6 = number6;
				e.number7 = number7;
			}
		}

		static void OnReceiveData(object sender, Hooks.MessageBuffer.GetDataEventArgs e)
		{
			if (!Enum.IsDefined(typeof(PacketTypes), (int)e.packetId))
			{
				e.Result = HookResult.Cancel;
			}
			else
			{
				var msgId = e.packetId;
				var readOffset = e.readOffset;
				var length = e.length;

				if (_hookManager.InvokeNetGetData(ref msgId, e.instance, ref readOffset, ref length))
				{
					e.Result = HookResult.Cancel;
				}

				e.packetId = msgId;
				e.readOffset = readOffset;
				e.length = length;
			}
		}

		static void OnGreetPlayer(On.Terraria.NetMessage.orig_greetPlayer orig, int plr)
		{
			if (_hookManager.InvokeNetGreetPlayer(plr))
				return;

			orig(plr);
		}

		static void OnSendBytes(object sender, Hooks.NetMessage.SendBytesEventArgs e)
		{
			if (_hookManager.InvokeNetSendBytes(Netplay.Clients[e.remoteClient], e.data, e.offset, e.size))
			{
				e.Result = HookResult.Cancel;
			}
		}

		static void OnNameCollision(object sender, Hooks.MessageBuffer.NameCollisionEventArgs e)
		{
			if (_hookManager.InvokeNetNameCollision(e.player.whoAmI, e.player.name))
			{
				e.Result = HookResult.Cancel;
			}
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
