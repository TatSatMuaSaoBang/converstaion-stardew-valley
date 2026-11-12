using System;
using System.Collections.Generic;
using ConversationSystem;
using Netcode;
using SorryLab;
using StardewValley;
using StardewValley.Network;

public static class FamilyGifts
{
	private static Dictionary<Farmer, List<NPC>> _getGiftToday = new Dictionary<Farmer, List<NPC>>();

	private static List<NPC> GetGiftToday(Farmer farmer)
	{
		if (!_getGiftToday.ContainsKey(farmer))
		{
			_getGiftToday[farmer] = new List<NPC>();
		}
		return _getGiftToday[farmer];
	}

	public static void OnDayStarted()
	{
		_getGiftToday.Clear();
	}

	public static bool IsWhoGivenGiftToday(Farmer farmer, NPC who)
	{
		return GetGiftToday(farmer).Contains(who);
	}

	public static void PlayGiftReaction(Farmer farmer, NPC who)
	{
		GetGiftToday(farmer).Add(who);
		switch (Game1.Date.DayOfWeek)
		{
		case DayOfWeek.Sunday:
			Sunday(farmer, who);
			break;
		case DayOfWeek.Monday:
			Monday(farmer, who);
			break;
		case DayOfWeek.Tuesday:
			Tuesday(farmer, who);
			break;
		case DayOfWeek.Wednesday:
			Wednesday(farmer, who);
			break;
		case DayOfWeek.Thursday:
			Thursday(farmer, who);
			break;
		case DayOfWeek.Friday:
			Friday(farmer, who);
			break;
		case DayOfWeek.Saturday:
			Saturday(farmer, who);
			break;
		}
		DialogueEvent.AddAction((Action)delegate
		{
			Friendship val = default(Friendship);
			if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(((Character)who).Name, ref val))
			{
				if (val.IsMarried())
				{
					((Character)who).doEmote(20, true);
				}
				else
				{
					((Character)who).doEmote(32, true);
				}
			}
		});
	}

	public static void Sunday(Farmer farmer, NPC who)
	{
		int money = 750;
		money += Mathf.RandomRange(0, 750);
		DialogueEvent.AddDialogue(who, Trs.GR("Conversation.Greeting.Family.Sunday").Replace("{p}", money.ToString()));
		DialogueEvent.AddGlobalMessage(Trs.GR("Conversation.Greeting.Family.Sunday.Global").Replace("{p}", money.ToString()).Replace("{n}", ((Character)who).displayName));
		DialogueEvent.AddAction((Action)delegate
		{
			Game1.playSound("moneyDial", (int?)null);
			Farmer obj = farmer;
			obj.Money += money;
		});
	}

	public static void Monday(Farmer farmer, NPC who)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		Object item = new Object("466", 20, false, -1, 0);
		DialogueEvent.AddDialogue(who, Trs.GR("Conversation.Greeting.Family.Monday").Replace("{i}", ((Item)item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, who, ((Item)item).ItemId, ((Item)item).Stack, ((Item)item).Quality);
		});
	}

	public static void Tuesday(Farmer farmer, NPC who)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		Object item = new Object("GoldenMysteryBox", 3, false, -1, 0);
		DialogueEvent.AddDialogue(who, Trs.GR("Conversation.Greeting.Family.Tuesday").Replace("{i}", ((Item)item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, who, ((Item)item).ItemId, ((Item)item).Stack, ((Item)item).Quality);
		});
	}

	public static void Wednesday(Farmer farmer, NPC who)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		Object item = new Object("787", 2, false, -1, 0);
		DialogueEvent.AddDialogue(who, Trs.GR("Conversation.Greeting.Family.Wednesday").Replace("{i}", ((Item)item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, who, ((Item)item).ItemId, ((Item)item).Stack, ((Item)item).Quality);
		});
	}

	public static void Thursday(Farmer farmer, NPC who)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		Object item = new Object("773", 3, false, -1, 0);
		DialogueEvent.AddDialogue(who, Trs.GR("Conversation.Greeting.Family.Thursday").Replace("{i}", ((Item)item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, who, ((Item)item).ItemId, ((Item)item).Stack, ((Item)item).Quality);
		});
	}

	public static void Friday(Farmer farmer, NPC who)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		Object item = new Object("253", 2, false, -1, 0);
		DialogueEvent.AddDialogue(who, Trs.GR("Conversation.Greeting.Family.Friday").Replace("{i}", ((Item)item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, who, ((Item)item).ItemId, ((Item)item).Stack, ((Item)item).Quality);
		});
	}

	public static void Saturday(Farmer farmer, NPC who)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		Object item = new Object("PrizeTicket", 1, false, -1, 0);
		DialogueEvent.AddDialogue(who, Trs.GR("Conversation.Greeting.Family.Saturday").Replace("{i}", ((Item)item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, who, ((Item)item).ItemId, ((Item)item).Stack, ((Item)item).Quality);
		});
	}
}
