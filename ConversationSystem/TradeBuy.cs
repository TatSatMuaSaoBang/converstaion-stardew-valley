using System;
using System.Collections.Generic;
using SorryLab;
using StardewValley;

namespace ConversationSystem;

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
