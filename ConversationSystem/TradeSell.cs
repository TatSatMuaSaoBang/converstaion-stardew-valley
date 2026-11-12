using System;
using System.Collections.Generic;
using SorryLab;
using SorryLab.Expansion;
using StardewValley;

namespace ConversationSystem;

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
