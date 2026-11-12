using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using SorryLab;
using SorryLab.Expansion;
using SorryLab.ShareData;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Network;

namespace ConversationSystem;

public static class ConversationUtility
{
	private static ModConfig _config;

	private static Dictionary<Farmer, Dictionary<string, int>> _todayConversationRemainingNumber = new Dictionary<Farmer, Dictionary<string, int>>();

	public static ModConfig config => _config;

	private static Dictionary<string, int> GetTodayConversationRemainingNumber(Farmer farmer)
	{
		if (!_todayConversationRemainingNumber.ContainsKey(farmer))
		{
			_todayConversationRemainingNumber[farmer] = new Dictionary<string, int>();
		}
		return _todayConversationRemainingNumber[farmer];
	}

	public static void Setup(ModConfig config, Func<ModSave> save)
	{
		_config = config;
	}

	public static bool IsFirstTalkToday(NPC npc, Farmer farmer)
	{
		return GetTodayConversationRemainingNumber(farmer, npc) == Conversation.SkillEeryDayTalkLimit(farmer, npc);
	}

	public static int GetTodayConversationRemainingNumber(Farmer farmer, NPC npc)
	{
		if (GetTodayConversationRemainingNumber(farmer).TryGetValue(((Character)npc).Name, out var value))
		{
			return value;
		}
		return 0;
	}

	public static bool ChangeTodayConversationRemainingNumber(Farmer farmer, NPC npc, int change)
	{
		if (GetTodayConversationRemainingNumber(farmer).ContainsKey(((Character)npc).Name))
		{
			GetTodayConversationRemainingNumber(farmer)[((Character)npc).Name] += change;
			return true;
		}
		return false;
	}

	public static bool SetTodayConversationRemainingNumber(Farmer farmer, NPC npc, int value)
	{
		if (GetTodayConversationRemainingNumber(farmer).ContainsKey(((Character)npc).Name))
		{
			GetTodayConversationRemainingNumber(farmer)[((Character)npc).Name] = value;
			return true;
		}
		return false;
	}

	public static bool IsFamily(Farmer farmer, NPC who)
	{
		Friendship val = default(Friendship);
		if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(((Character)who).Name, ref val) && (val.IsMarried() || val.IsRoommate()))
		{
			return true;
		}
		return false;
	}

	public static void OnDayStarted()
	{
		Farmer player = Game1.player;
		foreach (NPC item in StardewUtility.GetAllNPC((IEnumerable<string>)null))
		{
			if (item.CanReceiveGifts() && !(item is Child) && ((Character)item).IsVillager)
			{
				GetTodayConversationRemainingNumber(player)[((Character)item).Name] = Conversation.SkillEeryDayTalkLimit(player, item);
			}
		}
	}

	public static void DisplayOptions(Farmer farmer, NPC npc, string msg, Dictionary<string, Action> actionOptions)
	{
		msg = msg.Replace("$h", "").Replace("$k", "").Replace("$s", "")
			.Replace("$l", "")
			.Replace("$a", "")
			.Replace("@", ((Character)farmer).displayName);
		List<string> list = actionOptions.Keys.ToList();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		List<string> optionList = new List<string>();
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			dictionary[i.ToString()] = text;
			optionList.Add(text);
		}
		DialogueEvent.AddQuestion(npc, msg, dictionary, (Action<NPC, string>)delegate(NPC who, string index)
		{
			for (int j = 0; j < optionList.Count; j++)
			{
				if (j.ToString() == index)
				{
					actionOptions[optionList[j]]();
				}
			}
		});
	}

	public static Object DropItem(Farmer farmer, NPC npc, string itemID, int count, int quality = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(((Character)farmer).Position, ((Character)npc).Position);
		TodayGivenItem.AddTodayItem(npc, itemID);
		if (((Character)npc).currentLocation == ((Character)farmer).currentLocation && num < 250f)
		{
			return StardewUtility.DropItem((Character)(object)npc, itemID, count, quality);
		}
		return StardewUtility.DropItem((Character)(object)farmer, itemID, count, quality);
	}

	public static bool IsGivenItemToday(NPC npc, string itemID)
	{
		return TodayGivenItem.IsGivenItemToday(npc, itemID);
	}

	public static Object UpgradeRewardQuality(Object item, NPC npc, Farmer farmer)
	{
		float num = (float)farmer.getFriendshipHeartLevelForNPC(((Character)npc).Name) / 20f;
		num += (float)ConversationSkill.GetLevel(farmer) * 0.01f;
		Friendship val = default(Friendship);
		if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(((Character)npc).Name, ref val))
		{
			for (int i = 0; i < 3; i++)
			{
				if (Mathf.RandomValue < num)
				{
					((Item)item).Quality = ((Item)item).Quality + 1;
				}
				if (((Item)item).Quality == 3)
				{
					((Item)item).Quality = 4;
					break;
				}
			}
		}
		return item;
	}

	public static Object Reward(int itemLevel, NPC npc, Farmer farmer)
	{
		float num = (float)farmer.getFriendshipHeartLevelForNPC(((Character)npc).Name) / 30f;
		num += (float)ConversationSkill.GetLevel(farmer) * 0.01f;
		Object item = null;
		for (int i = 0; i < 3; i++)
		{
			if (Mathf.RandomValue < num)
			{
				itemLevel++;
			}
			if (itemLevel == 3)
			{
				break;
			}
		}
		itemLevel = Mathf.Clamp(itemLevel, 1, 3);
		if (itemLevel <= 1)
		{
			item = StardewUtility.GetSObjectFromItemID(ListExpansion.GetRandomOne<string>(ItemList.giftsLv1));
		}
		if (itemLevel == 2)
		{
			item = StardewUtility.GetSObjectFromItemID(ListExpansion.GetRandomOne<string>(ItemList.giftsLv2));
		}
		if (itemLevel >= 3)
		{
			item = StardewUtility.GetSObjectFromItemID(ListExpansion.GetRandomOne<string>(ItemList.giftsLv3));
		}
		return UpgradeRewardQuality(item, npc, farmer);
	}

	public static void AddEventLog(Farmer farmer, ModSave saveData, EventType logType, NPC npc, string itemID = "", string content = "")
	{
		saveData.GetData(farmer).log.AddEvent(logType, npc, itemID, content);
	}

	public static int GetPriceByQuality(Object obj)
	{
		int num = obj.Price;
		if (((Item)obj).Quality == 1)
		{
			num = (int)((float)num * 1.25f);
		}
		if (((Item)obj).Quality == 2)
		{
			num = (int)((float)num * 1.5f);
		}
		if (((Item)obj).Quality == 4)
		{
			num = (int)((float)num * 2f);
		}
		return num;
	}

	public static int GetBeggingPrice(Object obj)
	{
		int num = GetPriceByQuality(obj) / 8;
		if (num < 4)
		{
			num = 0;
		}
		return num;
	}
}
