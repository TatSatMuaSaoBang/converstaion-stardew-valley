// ============================================================================
// TradeAndUtility.cs
// Trading (buy/sell), event logging (EventType/LogEvent/EventLog), and shared conversation utilities.
//
// Compacted from: TradeBuy.cs, TradeSell.cs, ConversationUtility.cs, EventLog.cs, EventType.cs, LogEvent.cs
// ============================================================================

using Microsoft.Xna.Framework;
using Netcode;
using SorryLab.Expansion;
using SorryLab.ShareData;
using SorryLab;
using StardewValley.Characters;
using StardewValley.Network;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ConversationSystem;

// ------------------------------------------------------------------
// From TradeBuy.cs
// ------------------------------------------------------------------

public static class TradeBuy
{
	internal class TradeData
	{
		public NPC npc;

		public Object item;

		public int basicPrice;

		public int beggingPoint;

		public float priceMultiple;

		public int friendshipChange;

		public int maxStack;

		public EventType eventType;

		public int numOfNegotiations;

		public int CurrentPrice => (int)((float)basicPrice * priceMultiple);
	}

	private static float _hagglingChance = 0.5f;

	private static float _hagglingMistakeChance = 0.5f;

	private static Dictionary<Farmer, TradeData> _tradeData = new Dictionary<Farmer, TradeData>();

	private static float GetSkillHagglingChance(Farmer farmer)
	{
		float num = ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		float num2 = (1f - _hagglingChance) * num;
		return _hagglingChance + num2;
	}

	private static float GetSkillHagglingMistakeChance(Farmer farmer)
	{
		float num = 1f - ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		return _hagglingMistakeChance * num;
	}

	private static TradeData GetTradeData(Farmer farmer)
	{
		if (!_tradeData.ContainsKey(farmer))
		{
			_tradeData[farmer] = new TradeData();
		}
		return _tradeData[farmer];
	}

	public static void StartTrade(Farmer farmer, ModSave saveData, NPC npc, Object item)
	{
		TradeData tradeData = GetTradeData(farmer);
		((Item)item).Stack = (tradeData.maxStack = Mathf.RandomRange(1, ConversationSkill.MaxTradeItemStack(farmer) + 1));
		tradeData.item = item;
		tradeData.priceMultiple = 1f;
		tradeData.beggingPoint = ConversationUtility.GetBeggingPrice(item) * ((Item)item).Stack;
		tradeData.basicPrice = ConversationUtility.GetPriceByQuality(item) * ((Item)item).Stack;
		tradeData.numOfNegotiations = 0;
		tradeData.friendshipChange = 0;
		tradeData.npc = npc;
		tradeData.eventType = EventType.None;
		if (!ConversationUtility.IsFamily(farmer, npc))
		{
			DisplayOptions(farmer, saveData);
		}
		else
		{
			FamilyGiveItem(farmer, saveData);
		}
	}

	private static void FamilyGiveItem(Farmer farmer, ModSave saveData)
	{
		TradeData data = GetTradeData(farmer);
		DialogueEvent.AddDialogue(data.npc, Trs.GR("Trade.FamilyGiveItem").Replace("{i}", ((Item)data.item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, data.npc, ((Item)data.item).ItemId, 1, ((Item)data.item).Quality);
		});
		data.eventType = EventType.None;
		ApplyFriendship(farmer, saveData);
	}

	private static void DisplayOptions(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		tradeData.numOfNegotiations++;
		string newValue = ((((Item)tradeData.item).Stack == 1) ? ((Item)tradeData.item).DisplayName : $"{((Item)tradeData.item).DisplayName}({((Item)tradeData.item).Stack})");
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		string key = Trs.GR("Trade.Buy.Agree").Replace("{p}", tradeData.CurrentPrice.ToString()).Replace("{i}", newValue);
		string key2 = Trs.GR("Trade.Buy.Haggling").Replace("{p}", tradeData.CurrentPrice.ToString()).Replace("{i}", newValue);
		string key3 = Trs.GR("Trade.Buy.Begging").Replace("{i}", newValue) + (ConversationUtility.config.DisplayFriendShip ? (" " + Trs.GR("Trade.Buy.FriendShipCost").Replace("{fs}", tradeData.beggingPoint.ToString())) : "");
		string key4 = Trs.GR("Trade.Buy.Exit");
		string key5 = Trs.GR("Trade.IncreaseStack");
		string key6 = Trs.GR("Trade.DecreaseStack");
		if (tradeData.basicPrice != 0)
		{
			dictionary[key] = delegate
			{
				BuyAgree(farmer, saveData);
			};
			if (tradeData.priceMultiple > 0.6f)
			{
				dictionary[key2] = delegate
				{
					StarHaggling(farmer, saveData);
				};
			}
			if (tradeData.numOfNegotiations == 1)
			{
				dictionary[key3] = delegate
				{
					Begging(farmer, saveData);
				};
				if (((Item)tradeData.item).Stack < tradeData.maxStack)
				{
					dictionary[key5] = delegate
					{
						IncreaseStack(farmer, saveData);
					};
				}
				if (((Item)tradeData.item).Stack > 1)
				{
					dictionary[key6] = delegate
					{
						DecreaseStack(farmer, saveData);
					};
				}
			}
		}
		else
		{
			dictionary[key3] = delegate
			{
				Begging(farmer, saveData);
			};
		}
		dictionary[key4] = delegate
		{
			Exit(farmer);
		};
		string text = Trs.GR("Trade.Buy.Message").Replace("{i}", newValue).Replace("{npc}", NpcContent.NPCDisplayName(tradeData.npc))
			.Replace("{p}", tradeData.CurrentPrice.ToString());
		if (ConversationUtility.config.DisplayFriendShip && tradeData.numOfNegotiations == 1)
		{
			text += Trs.GR("Trade.FriendShipInfo").Replace("{npc}", NpcContent.NPCDisplayName(tradeData.npc)).Replace("{fs}", farmer.getFriendshipLevelForNPC(((Character)tradeData.npc).Name).ToString());
		}
		ConversationUtility.DisplayOptions(farmer, tradeData.npc, text, dictionary);
	}

	private static void IncreaseStack(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		((Item)tradeData.item).Stack = ((Item)tradeData.item).Stack + 1;
		tradeData.basicPrice = ConversationUtility.GetPriceByQuality(tradeData.item) * ((Item)tradeData.item).Stack;
		tradeData.beggingPoint = ConversationUtility.GetBeggingPrice(tradeData.item) * ((Item)tradeData.item).Stack;
		tradeData.numOfNegotiations--;
		DisplayOptions(farmer, saveData);
	}

	private static void DecreaseStack(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		((Item)tradeData.item).Stack = ((Item)tradeData.item).Stack - 1;
		tradeData.basicPrice = ConversationUtility.GetPriceByQuality(tradeData.item) * ((Item)tradeData.item).Stack;
		tradeData.beggingPoint = ConversationUtility.GetBeggingPrice(tradeData.item) * ((Item)tradeData.item).Stack;
		tradeData.numOfNegotiations--;
		DisplayOptions(farmer, saveData);
	}

	private static void BuyAgree(Farmer farmer, ModSave saveData)
	{
		TradeData data = GetTradeData(farmer);
		ConversationSkill.AddExp(farmer, 12, ConversationUtility.config);
		if (farmer.Money >= data.CurrentPrice)
		{
			Farmer obj = farmer;
			obj.Money -= data.CurrentPrice;
			Game1.playSound("moneyDial", (int?)null);
			if (data.friendshipChange < 0)
			{
				DialogueEvent.AddDialogue(data.npc, Trs.GR("Trade.Buy.Success.BadReaction").Replace("{i}", ((Item)data.item).DisplayName));
			}
			else
			{
				DialogueEvent.AddDialogue(data.npc, Trs.GR("Trade.Buy.Success.GoodReaction").Replace("{i}", ((Item)data.item).DisplayName));
			}
			DialogueEvent.AddAction((Action)delegate
			{
				ConversationUtility.DropItem(farmer, data.npc, ((Item)data.item).ItemId, ((Item)data.item).Stack, ((Item)data.item).Quality);
			});
			data.eventType = EventType.BuyItem;
		}
		else
		{
			Game1.playSound("cancel", (int?)null);
			DialogueEvent.AddDialogue(data.npc, Trs.GR("Trade.Buy.NoMoney").Replace("{i}", ((Item)data.item).DisplayName));
		}
		ApplyFriendship(farmer, saveData);
	}

	private static void Begging(Farmer farmer, ModSave saveData)
	{
		TradeData data = GetTradeData(farmer);
		if (farmer.getFriendshipLevelForNPC(((Character)data.npc).Name) >= data.beggingPoint)
		{
			ConversationSkill.AddExp(farmer, 16, ConversationUtility.config);
			Game1.playSound("give_gift", (int?)null);
			if (data.beggingPoint > 0)
			{
				DialogueEvent.AddDialogue(data.npc, Trs.GR("Trade.Buy.Begging.Success.BadReaction").Replace("{i}", ((Item)data.item).DisplayName));
			}
			else
			{
				DialogueEvent.AddDialogue(data.npc, Trs.GR("Trade.Buy.Begging.Success.GoodReaction").Replace("{i}", ((Item)data.item).DisplayName));
			}
			DialogueEvent.AddAction((Action)delegate
			{
				ConversationUtility.DropItem(farmer, data.npc, ((Item)data.item).ItemId, ((Item)data.item).Stack, ((Item)data.item).Quality);
			});
			if (data.beggingPoint > 0)
			{
				data.friendshipChange -= data.beggingPoint;
				data.eventType = EventType.BeggingItem;
			}
			ApplyFriendship(farmer, saveData);
		}
		else
		{
			Game1.playSound("cancel", (int?)null);
			string text = Trs.GR("Trade.Buy.Begging.Fail");
			text = text.Replace("{i}", data.item.displayName);
			DialogueEvent.AddDialogue(data.npc, text);
		}
	}

	private static void StarHaggling(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		bool flag = Mathf.RandomValue < GetSkillHagglingChance(farmer);
		ConversationSkill.AddExp(farmer, 10, ConversationUtility.config);
		if (flag)
		{
			tradeData.priceMultiple *= 0.8f;
			Game1.playSound("give_gift", (int?)null);
			DialogueEvent.AddDialogue(tradeData.npc, Trs.GR("Trade.Buy.Haggling.Success").Replace("{i}", ((Item)tradeData.item).DisplayName));
			DisplayOptions(farmer, saveData);
		}
		else if (Mathf.RandomValue < GetSkillHagglingMistakeChance(farmer) || tradeData.numOfNegotiations >= 3)
		{
			Game1.playSound("slimedead", (int?)null);
			DialogueEvent.AddDialogue(tradeData.npc, Trs.GR("Trade.Buy.Haggling.Mistake").Replace("{i}", ((Item)tradeData.item).DisplayName));
			tradeData.friendshipChange -= 15;
			ApplyFriendship(farmer, saveData);
		}
		else
		{
			Game1.playSound("cancel", (int?)null);
			DialogueEvent.AddDialogue(tradeData.npc, Trs.GR("Trade.Buy.Haggling.Fail").Replace("{i}", ((Item)tradeData.item).DisplayName));
			tradeData.friendshipChange -= 5;
			DisplayOptions(farmer, saveData);
		}
	}

	private static void Exit(Farmer farmer)
	{
	}

	private static void ApplyFriendship(Farmer farmer, ModSave saveData)
	{
		TradeData data = GetTradeData(farmer);
		if (ConversationSkill.HasCustomProfession(farmer, ConversationSkill.lv10A1))
		{
			data.friendshipChange = (int)((float)data.friendshipChange * 0.5f);
		}
		if (data.friendshipChange < 0)
		{
			if (data.friendshipChange <= -40)
			{
				DialogueEvent.AddObjectDialogue(Trs.GR("Trade.VeryVeryBadReaction").Replace("{npc}", NpcContent.NPCDisplayName(data.npc)).Replace("{i}", ((Item)data.item).DisplayName));
			}
			else if (data.friendshipChange <= -15)
			{
				DialogueEvent.AddObjectDialogue(Trs.GR("Trade.VeryBadReaction").Replace("{npc}", NpcContent.NPCDisplayName(data.npc)).Replace("{i}", ((Item)data.item).DisplayName));
			}
			else
			{
				DialogueEvent.AddObjectDialogue(Trs.GR("Trade.BadReaction").Replace("{npc}", NpcContent.NPCDisplayName(data.npc)).Replace("{i}", ((Item)data.item).DisplayName));
			}
			DialogueEvent.AddAction((Action)delegate
			{
				((Character)data.npc).doEmote(12, true);
			});
		}
		farmer.changeFriendship(data.friendshipChange, data.npc);
		if (data.eventType == EventType.BuyItem && data.friendshipChange < 0)
		{
			data.eventType = EventType.BuyItem_BadTrade;
		}
		if (data.eventType != EventType.None)
		{
			ConversationUtility.AddEventLog(farmer, saveData, data.eventType, data.npc, ((Item)data.item).ItemId);
		}
	}
}

// ------------------------------------------------------------------
// From TradeSell.cs
// ------------------------------------------------------------------

public static class TradeSell
{
	internal class TradeData
	{
		public NPC npc;

		public Object item;

		public int basicPrice;

		public float priceMultiple;

		public int friendshipChange;

		public int stack;

		public int currentMaxStack;

		public string reason;

		public EventType eventType;

		public int numOfNegotiations;

		public string itemDisplayNameWithStack => (stack == 1) ? ((Item)item).DisplayName : $"{((Item)item).DisplayName}({stack})";

		public int GetCurrentPrice(Farmer farmer)
		{
			float num = (float)basicPrice * priceMultiple;
			if (ConversationSkill.HasCustomProfession(farmer, ConversationSkill.lv10A2))
			{
				num *= 1.2f;
			}
			return (int)num;
		}
	}

	private static float _gougingChance = 0.5f;

	private static float _gougingMistakeChance = 0.5f;

	private static Dictionary<Farmer, TradeData> _tradeData = new Dictionary<Farmer, TradeData>();

	private static float GetSkillGougingChance(Farmer farmer)
	{
		float num = ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		float num2 = (1f - _gougingChance) * num;
		return _gougingChance + num2;
	}

	private static float GetSkillGougingMistakeChance(Farmer farmer)
	{
		float num = 1f - ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		return _gougingMistakeChance * num;
	}

	private static TradeData GetTradeData(Farmer farmer)
	{
		if (!_tradeData.ContainsKey(farmer))
		{
			_tradeData[farmer] = new TradeData();
		}
		return _tradeData[farmer];
	}

	public static void StartTrade(Farmer farmer, ModSave saveData, NPC npc, Object item)
	{
		TradeData tradeData = GetTradeData(farmer);
		tradeData.currentMaxStack = (tradeData.stack = Mathf.Min(((Item)item).Stack, ConversationSkill.MaxTradeItemStack(farmer) + 1));
		tradeData.item = item;
		tradeData.priceMultiple = 1f;
		tradeData.basicPrice = (int)((float)(ConversationUtility.GetPriceByQuality(item) * tradeData.stack) * ConversationUtility.config.SellPriceMultiple);
		tradeData.numOfNegotiations = 0;
		tradeData.friendshipChange = 0;
		tradeData.npc = npc;
		tradeData.eventType = EventType.None;
		DialogueEvent.AddDialogue(npc, Trs.GR("Trade.Sell.Beginning").Replace("{i}", ((Item)tradeData.item).DisplayName));
		tradeData.reason = Trs.GR("Trade.Sell.Reason");
		if (tradeData.reason.Contains("{npc}"))
		{
			NPC randomOne = ListExpansion.GetRandomOne<NPC>(NpcContent.GetNpcTalkAboutList(farmer, npc));
			if (randomOne != null)
			{
				tradeData.reason = tradeData.reason.Replace("{npc}", NpcContent.NPCDisplayName(randomOne, npc));
				tradeData.reason = GameTextContent.TextGenderHandle(tradeData.reason, (Character)(object)randomOne);
				string text = tradeData.reason.Replace("{i}", ((Item)tradeData.item).DisplayName).Replace("{p}", tradeData.GetCurrentPrice(farmer).ToString());
				DialogueEvent.AddDialogue(npc, text);
				DisplayOptions(farmer, saveData);
			}
			else
			{
				StartTrade(farmer, saveData, npc, item);
			}
		}
		else
		{
			string text2 = tradeData.reason.Replace("{i}", ((Item)tradeData.item).DisplayName).Replace("{p}", tradeData.GetCurrentPrice(farmer).ToString());
			DialogueEvent.AddDialogue(npc, text2);
			DisplayOptions(farmer, saveData);
		}
	}

	private static void DisplayOptions(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		tradeData.numOfNegotiations++;
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		string key = Trs.GR("Trade.Sell.Agree").Replace("{price}", tradeData.GetCurrentPrice(farmer).ToString()).Replace("{i}", tradeData.itemDisplayNameWithStack)
			.Replace("{p}", tradeData.GetCurrentPrice(farmer).ToString());
		string key2 = Trs.GR("Trade.Sell.Gouging").Replace("{price}", tradeData.GetCurrentPrice(farmer).ToString()).Replace("{i}", tradeData.itemDisplayNameWithStack)
			.Replace("{p}", tradeData.GetCurrentPrice(farmer).ToString());
		string key3 = Trs.GR("Trade.Sell.Reject").Replace("{i}", tradeData.itemDisplayNameWithStack);
		string key4 = Trs.GR("Trade.IncreaseStack");
		string key5 = Trs.GR("Trade.DecreaseStack");
		string dialogue = tradeData.reason.Replace("{i}", tradeData.itemDisplayNameWithStack).Replace("{p}", tradeData.GetCurrentPrice(farmer).ToString());
		dictionary[key] = delegate
		{
			SellAgree(farmer, saveData);
		};
		if (tradeData.priceMultiple < 1.5f)
		{
			dictionary[key2] = delegate
			{
				StartGouging(farmer, saveData);
			};
		}
		if (tradeData.numOfNegotiations == 1)
		{
			if (tradeData.stack < tradeData.currentMaxStack)
			{
				dictionary[key4] = delegate
				{
					IncreaseStack(farmer, saveData);
				};
			}
			if (tradeData.stack > 1)
			{
				dictionary[key5] = delegate
				{
					DecreaseStack(farmer, saveData);
				};
			}
		}
		dictionary[key3] = delegate
		{
			Reject(farmer);
		};
		string text = NpcContent.ToNpcSaidFormat(tradeData.npc, dialogue);
		if (ConversationUtility.config.DisplayFriendShip && tradeData.numOfNegotiations == 1)
		{
			text += Trs.GR("Trade.FriendShipInfo").Replace("{npc}", NpcContent.NPCDisplayName(tradeData.npc)).Replace("{fs}", farmer.getFriendshipLevelForNPC(((Character)tradeData.npc).Name).ToString());
		}
		ConversationUtility.DisplayOptions(farmer, tradeData.npc, text, dictionary);
	}

	private static void IncreaseStack(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		tradeData.stack++;
		tradeData.basicPrice = (int)((float)(ConversationUtility.GetPriceByQuality(tradeData.item) * tradeData.stack) * ConversationUtility.config.SellPriceMultiple);
		tradeData.numOfNegotiations--;
		DisplayOptions(farmer, saveData);
	}

	private static void DecreaseStack(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		tradeData.stack--;
		tradeData.basicPrice = (int)((float)(ConversationUtility.GetPriceByQuality(tradeData.item) * tradeData.stack) * ConversationUtility.config.SellPriceMultiple);
		tradeData.numOfNegotiations--;
		DisplayOptions(farmer, saveData);
	}

	private static void SellAgree(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		if (tradeData.item == farmer.ActiveObject)
		{
			Game1.playSound("moneyDial", (int?)null);
			farmer.Money += tradeData.GetCurrentPrice(farmer);
			for (int i = 0; i < tradeData.stack; i++)
			{
				farmer.reduceActiveItemByOne();
			}
			ConversationSkill.AddExp(farmer, 12, ConversationUtility.config);
			if (tradeData.friendshipChange < 0)
			{
				DialogueEvent.AddDialogue(tradeData.npc, Trs.GR("Trade.Sell.Success.BadReaction").Replace("{i}", tradeData.itemDisplayNameWithStack));
			}
			else
			{
				DialogueEvent.AddDialogue(tradeData.npc, Trs.GR("Trade.Sell.Success.GoodReaction").Replace("{i}", tradeData.itemDisplayNameWithStack));
			}
			if (((Character)tradeData.npc).Name == "Linus")
			{
				DialogueEvent.AddObjectDialogue_Secondary(Trs.GR("Trade.Sell.Linus"));
			}
			else if (NpcContent.IsKid(tradeData.npc))
			{
				DialogueEvent.AddObjectDialogue_Secondary(Trs.GR("Trade.Sell.Kid"));
			}
			tradeData.eventType = EventType.SellItem;
			ApplyFriendship(farmer, saveData);
		}
	}

	private static void StartGouging(Farmer farmer, ModSave saveData)
	{
		TradeData tradeData = GetTradeData(farmer);
		bool flag = Mathf.RandomValue < GetSkillGougingChance(farmer);
		ConversationSkill.AddExp(farmer, 10, ConversationUtility.config);
		if (flag)
		{
			tradeData.priceMultiple *= 1.15f;
			Game1.playSound("give_gift", (int?)null);
			string text = Trs.GR("Trade.Sell.Gouging.Success").Replace("{i}", tradeData.itemDisplayNameWithStack).Replace("{p}", tradeData.GetCurrentPrice(farmer).ToString());
			DialogueEvent.AddDialogue(tradeData.npc, text);
			DisplayOptions(farmer, saveData);
		}
		else if (Mathf.RandomValue < GetSkillGougingMistakeChance(farmer) || tradeData.numOfNegotiations >= 3)
		{
			Game1.playSound("slimedead", (int?)null);
			DialogueEvent.AddDialogue(tradeData.npc, Trs.GR("Trade.Sell.Gouging.Mistake").Replace("{i}", tradeData.itemDisplayNameWithStack));
			tradeData.friendshipChange -= 15;
			ApplyFriendship(farmer, saveData);
		}
		else
		{
			Game1.playSound("cancel", (int?)null);
			string text2 = Trs.GR("Trade.Sell.Gouging.Fail").Replace("{i}", tradeData.itemDisplayNameWithStack).Replace("{p}", tradeData.GetCurrentPrice(farmer).ToString());
			DialogueEvent.AddDialogue(tradeData.npc, text2);
			tradeData.friendshipChange -= 5;
			DisplayOptions(farmer, saveData);
		}
	}

	private static void Reject(Farmer farmer)
	{
		TradeData tradeData = GetTradeData(farmer);
		DialogueEvent.AddDialogue(tradeData.npc, Trs.GR("Trade.Sell.Reject.Reaction").Replace("{npc}", NpcContent.NPCDisplayName(tradeData.npc)).Replace("{i}", tradeData.itemDisplayNameWithStack));
	}

	private static void ApplyFriendship(Farmer farmer, ModSave saveData)
	{
		TradeData data = GetTradeData(farmer);
		if (ConversationSkill.HasCustomProfession(farmer, ConversationSkill.lv10A1))
		{
			data.friendshipChange = (int)((float)data.friendshipChange * 0.5f);
		}
		if (data.friendshipChange < 0)
		{
			if (data.friendshipChange <= -15)
			{
				DialogueEvent.AddObjectDialogue(Trs.GR("Trade.VeryBadReaction").Replace("{npc}", NpcContent.NPCDisplayName(data.npc)).Replace("{i}", data.itemDisplayNameWithStack));
			}
			else
			{
				DialogueEvent.AddObjectDialogue(Trs.GR("Trade.BadReaction").Replace("{npc}", NpcContent.NPCDisplayName(data.npc)).Replace("{i}", data.itemDisplayNameWithStack));
			}
			DialogueEvent.AddAction((Action)delegate
			{
				((Character)data.npc).doEmote(12, true);
			});
		}
		farmer.changeFriendship(data.friendshipChange, data.npc);
		if (data.eventType == EventType.SellItem && data.friendshipChange < 0)
		{
			data.eventType = EventType.SellItem_BadTrade;
		}
		if (data.eventType != EventType.None)
		{
			ConversationUtility.AddEventLog(farmer, saveData, data.eventType, data.npc, ((Item)data.item).ItemId);
		}
	}
}

// ------------------------------------------------------------------
// From ConversationUtility.cs
// ------------------------------------------------------------------

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

// ------------------------------------------------------------------
// From EventLog.cs
// ------------------------------------------------------------------

public class EventLog
{
	public List<LogEvent> todayLogs = new List<LogEvent>();

	public List<LogEvent> yesterdayLogs = new List<LogEvent>();

	public LogEvent AddEvent(EventType type, NPC who, string itemID = "", string _content = "")
	{
		foreach (LogEvent todayLog in todayLogs)
		{
			if (todayLog.type == type && todayLog.npcName == ((Character)who).Name && todayLog.itemID == itemID && todayLog.content == _content)
			{
				todayLog.count++;
				return todayLog;
			}
		}
		LogEvent logEvent = new LogEvent
		{
			type = type,
			npcName = ((Character)who).Name,
			itemID = itemID,
			content = _content,
			count = 1
		};
		todayLogs.Add(logEvent);
		return logEvent;
	}

	public bool HasEvent(params string[] withoutNPCNames)
	{
		List<LogEvent> list = new List<LogEvent>();
		List<string> list2 = new List<string>(withoutNPCNames);
		foreach (LogEvent todayLog in todayLogs)
		{
			if (!list2.Contains(todayLog.npcName))
			{
				list.Add(todayLog);
			}
		}
		return list.Count > 0;
	}

	public LogEvent GetAnyEventAndRemove(params string[] withoutNPCNames)
	{
		List<LogEvent> list = new List<LogEvent>();
		List<string> list2 = new List<string>(withoutNPCNames);
		foreach (LogEvent todayLog in todayLogs)
		{
			if (!list2.Contains(todayLog.npcName))
			{
				list.Add(todayLog);
			}
		}
		if (list.Count > 0)
		{
			LogEvent randomOne = ListExpansion.GetRandomOne<LogEvent>(list);
			todayLogs.Remove(randomOne);
			return randomOne;
		}
		return null;
	}

	public void OnDayStarted()
	{
		yesterdayLogs.Clear();
		yesterdayLogs.AddRange(todayLogs);
		todayLogs.Clear();
	}
}

// ------------------------------------------------------------------
// From EventType.cs
// ------------------------------------------------------------------

public enum EventType
{
	None,
	BuyItem,
	BuyItem_BadTrade,
	BeggingItem,
	SellItem,
	SellItem_BadTrade,
	Praise,
	PraiseFail,
	Insult,
	QuestHopeItem,
	QuestHopeItemReject,
	CommentInsult,
	CommentPraise
}

// ------------------------------------------------------------------
// From LogEvent.cs
// ------------------------------------------------------------------

public class LogEvent
{
	public EventType type;

	public string npcName = "";

	public string itemID = "";

	public string content = "";

	public int count = 0;

	public string ToGreetingDialogue(NPC whoCall)
	{
		string text = Trs.GR("Conversation.Greeting.EventLog." + type);
		NPC val = default(NPC);
		if (npcName != "" && StardewUtility.TryGetNPCFromName(npcName, ref val))
		{
			text = text.Replace("{npc}", NpcContent.NPCDisplayName(val, whoCall));
			text = GameTextContent.TextGenderHandle(text, (Character)(object)val);
		}
		if (itemID != "")
		{
			Object sObjectFromItemID = StardewUtility.GetSObjectFromItemID(itemID);
			text = text.Replace("{i}", ((Item)sObjectFromItemID).DisplayName);
		}
		if (content != "")
		{
			text = text.Replace("{c}", content);
		}
		return text;
	}
}
