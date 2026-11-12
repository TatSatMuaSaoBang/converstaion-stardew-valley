using System;
using System.Collections.Generic;
using SorryLab;
using SorryLab.Expansion;
using StardewValley;

namespace ConversationSystem;

public static class Conversation
{
	internal class ConversationData
	{
		public Dictionary<string, Action> options;

		public string dialogue;

		public NPC npc;
	}

	internal class Option
	{
		public string text;

		public Action action;

		public Option(string optionText, Action action)
		{
			text = optionText;
			this.action = action;
		}
	}

	private static int _everyDayTalkLimit = 2;

	private static float _showMultiItemChance = 0.15f;

	private static Dictionary<Farmer, ConversationData> _conversationData = new Dictionary<Farmer, ConversationData>();

	public static int SkillEeryDayTalkLimit(Farmer farmer, NPC who)
	{
		int num = _everyDayTalkLimit;
		if (ConversationSkill.HasCustomProfession(farmer, ConversationSkill.lv10B2))
		{
			num++;
		}
		if (ConversationSkill.GetLevel(farmer) >= 8 && ConversationUtility.IsFamily(farmer, who))
		{
			num++;
		}
		return num;
	}

	private static ConversationData GetConversationData(Farmer farmer)
	{
		if (!_conversationData.ContainsKey(farmer))
		{
			_conversationData[farmer] = new ConversationData();
		}
		return _conversationData[farmer];
	}

	public static void StartTalking(NPC npc, ModSave saveData, Farmer farmer, ModConfig config)
	{
		ConversationData data = GetConversationData(farmer);
		data.npc = npc;
		if (farmer.isRidingHorse())
		{
			return;
		}
		if (!NpcContent.CanConversation(data.npc))
		{
			DialogueEvent.AddObjectDialogue(Trs.GR("Conversation.CantConversation").Replace("{npc}", NpcContent.NPCDisplayName(data.npc)));
			return;
		}
		if (!farmer.canUnderstandDwarves && ((Character)data.npc).Name.Equals("Dwarf"))
		{
			DialogueEvent.AddObjectDialogue(Trs.GR("Conversation.DontKnowDwarvish", (Character)(object)data.npc).Replace("{npc}", NpcContent.NPCDisplayName(data.npc)));
			return;
		}
		((Character)data.npc).faceTowardFarmerForPeriod(2000, 4, false, farmer);
		if (ConversationUtility.IsFirstTalkToday(data.npc, farmer))
		{
			float num = ConversationSkill.StaminaHealPerLevel * (float)ConversationSkill.GetLevel(farmer);
			float stamina = farmer.Stamina;
			farmer.Stamina = Math.Min(farmer.MaxStamina, farmer.Stamina + num);
			if (farmer.Stamina - stamina > 0f)
			{
				StardewUtility.UIShowStaminaChange((int)(farmer.Stamina - stamina));
				Game1.playSound("coin", (int?)null);
			}
			Greeting(farmer, saveData);
		}
		ConversationUtility.ChangeTodayConversationRemainingNumber(farmer, data.npc, -1);
		DialogueEvent.AddAction((Action)delegate
		{
			data.options = GetOptions(farmer, saveData, config, 3);
			string text = Trs.GR("Conversation.WhatsUp");
			data.dialogue = NpcContent.ToNpcSaidFormat(data.npc, text);
			DialogueEvent.AddDialogue(data.npc, text);
			ConversationUtility.DisplayOptions(farmer, data.npc, data.dialogue, data.options);
		});
	}

	public static void OpenPrev(Farmer farmer)
	{
		ConversationData conversationData = GetConversationData(farmer);
		if (conversationData.npc != null && conversationData.options != null)
		{
			ConversationUtility.DisplayOptions(farmer, conversationData.npc, conversationData.dialogue, conversationData.options);
		}
	}

	private static Dictionary<string, Action> GetOptions(Farmer farmer, ModSave saveData, ModConfig config, int count = 1)
	{
		ConversationData data = GetConversationData(farmer);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		WeightDice<Option> val = new WeightDice<Option>();
		int friendshipHeartLevelForNPC = farmer.getFriendshipHeartLevelForNPC(((Character)data.npc).Name);
		if (farmer.ActiveObject != null && ((Item)farmer.ActiveObject).canBeGivenAsGift() && !ConversationUtility.IsGivenItemToday(data.npc, ((Item)farmer.ActiveObject).ItemId) && ((Item)farmer.ActiveObject).ItemId != "458" && ((Item)farmer.ActiveObject).ItemId != "277" && !ItemList.IsBirdieQuestItem(farmer.ActiveObject) && !ConversationUtility.IsFamily(farmer, data.npc))
		{
			if (ConversationSkill.HasCustomProfession(farmer, ConversationSkill.lv5A))
			{
				dictionary[Trs.GR("Conversation.ShowItem").Replace("{i}", ((Item)farmer.ActiveObject).DisplayName)] = delegate
				{
					TradeSell.StartTrade(farmer, saveData, data.npc, farmer.ActiveObject);
				};
				count++;
			}
			else
			{
				val.Add(new Option(Trs.GR("Conversation.ShowItem").Replace("{i}", ((Item)farmer.ActiveObject).DisplayName), delegate
				{
					TradeSell.StartTrade(farmer, saveData, data.npc, farmer.ActiveObject);
				}), 1.25f);
			}
		}
		if (NpcContent.IsNpcPossibleGivenQuest(data.npc) && !QuestData.IsTodayGivenQuest(data.npc) && config.GetEnableQuestCount() > 0 && saveData.GetData(farmer).quests.Count < 3)
		{
			float num = 0.8f;
			for (int num2 = 0; num2 < QuestData.TodayQuestCount(); num2++)
			{
				num *= 0.6f;
			}
			val.Add(new Option(Trs.GR("Conversation.AskQuest"), delegate
			{
				MentionQuest.StartMentionQuest(farmer, saveData, data.npc, config);
			}), num);
		}
		List<NPC> npcTalkAboutList = NpcContent.GetNpcTalkAboutList(farmer, data.npc);
		if (npcTalkAboutList.Count > 0)
		{
			for (int num3 = 0; num3 < 5; num3++)
			{
				if (npcTalkAboutList.Count == 0)
				{
					break;
				}
				NPC friend = ListExpansion.GetRandomOneAndRemove<NPC>(npcTalkAboutList);
				val.Add(new Option(Trs.GR("Conversation.AskNPC").Replace("{npc}", NpcContent.NPCDisplayName(friend)), delegate
				{
					InquiringNPC.StartInquiring(farmer, saveData, data.npc, friend);
				}), 0.8f);
			}
		}
		val.Add(new Option(Trs.GR("Conversation.AskItem"), delegate
		{
			AskItemTrade(farmer, saveData);
		}), 1.25f);
		string questItemID = saveData.GetData(farmer).quests.GetAnyQuestItem(farmer);
		if (saveData.GetData(farmer).quests.HaveItemQuest() && questItemID != "168" && !saveData.GetData(farmer).quests.IsHopeItemNpc(data.npc) && !ConversationUtility.IsGivenItemToday(data.npc, questItemID))
		{
			string optionText = Trs.GR("Quest.HopeItem.Ask").Replace("{i}", ((Item)StardewUtility.GetSObjectFromItemID(questItemID)).DisplayName);
			val.Add(new Option(optionText, delegate
			{
				MentionQuest.AskQuestItem(farmer, saveData, data.npc, questItemID);
			}), 1f);
		}
		while (val.Count > 0)
		{
			Option option = val.CastAndRemove();
			dictionary[option.text] = option.action;
			if (dictionary.Count == count)
			{
				break;
			}
		}
		dictionary[Trs.GR("Conversation.Other")] = delegate
		{
			FixedMenu.OpenFixedMenuOptions(farmer, saveData, data.npc, data.dialogue);
		};
		return dictionary;
	}

	private static void Greeting(Farmer farmer, ModSave saveData)
	{
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		ConversationData conversationData = GetConversationData(farmer);
		if (saveData.GetData(farmer).npcData.GetArrears(conversationData.npc) > 0)
		{
			MentionQuest.LoanFollowUp(farmer, saveData, conversationData.npc);
			return;
		}
		if (saveData.GetData(farmer).npcData.GetHelpPoints(conversationData.npc) > 0)
		{
			GivenLittleGift(farmer, saveData);
			return;
		}
		if (ConversationUtility.IsFamily(farmer, conversationData.npc) && !FamilyGifts.IsWhoGivenGiftToday(farmer, conversationData.npc))
		{
			FamilyGifts.PlayGiftReaction(farmer, conversationData.npc);
			return;
		}
		if (Mathf.RandomValue < 0.5f && saveData.GetData(farmer).log.HasEvent(((Character)conversationData.npc).Name))
		{
			LogEvent anyEventAndRemove = saveData.GetData(farmer).log.GetAnyEventAndRemove(((Character)conversationData.npc).Name);
			DialogueEvent.AddDialogue(conversationData.npc, anyEventAndRemove.ToGreetingDialogue(conversationData.npc));
			return;
		}
		WeightDice<string> val = new WeightDice<string>();
		val.Add("Normal", 1f);
		val.Add("Weather", 1f);
		string text = val.Cast();
		string text2 = Trs.GR("Conversation.Greeting");
		if (text == "Weather")
		{
			Weather todayWeather = StardewUtility.GetTodayWeather(((Character)conversationData.npc).currentLocation);
			text2 = Trs.GR($"Conversation.Greeting.Weather.{todayWeather}");
		}
		DialogueEvent.AddDialogue(conversationData.npc, text2);
	}

	private static void GivenLittleGift(Farmer farmer, ModSave saveData)
	{
		ConversationData data = GetConversationData(farmer);
		saveData.GetData(farmer).npcData.CostHelpPoint(data.npc, 1);
		Object item = ConversationUtility.Reward(1, data.npc, farmer);
		DialogueEvent.AddDialogue(data.npc, Trs.GR("Conversation.Greeting.LittleGift").Replace("{i}", ((Item)item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, data.npc, ((Item)item).ItemId, 1, ((Item)item).Quality);
		});
		DialogueEvent.AddDialogue(data.npc, Trs.GR("Conversation.Greeting.Then"));
	}

	private static void AskItemTrade(Farmer farmer, ModSave saveData)
	{
		ConversationData conversationData = GetConversationData(farmer);
		if (Mathf.RandomValue < _showMultiItemChance)
		{
			ShowMultiItemTrade(farmer, saveData, 3);
		}
		else
		{
			ShowSingleItemTrade(farmer, saveData, ConversationUtility.Reward(1, conversationData.npc, farmer));
		}
	}

	private static void ShowMultiItemTrade(Farmer farmer, ModSave saveData, int count)
	{
		ConversationData conversationData = GetConversationData(farmer);
		string text = Trs.GR("Conversation.AskItem.MutiItem");
		DialogueEvent.AddDialogue(conversationData.npc, text);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		for (int i = 0; i < count; i++)
		{
			Object item = ConversationUtility.Reward(1, conversationData.npc, farmer);
			dictionary[((Item)item).DisplayName] = delegate
			{
				ShowSingleItemTrade(farmer, saveData, item);
			};
		}
		text = NpcContent.ToNpcSaidFormat(conversationData.npc, text);
		ConversationUtility.DisplayOptions(farmer, conversationData.npc, text, dictionary);
	}

	private static void ShowSingleItemTrade(Farmer farmer, ModSave saveData, Object item)
	{
		ConversationData conversationData = GetConversationData(farmer);
		string text = Trs.GR("Conversation.AskItem.Reaction").Replace("{i}", ((Item)item).DisplayName);
		if (text.Contains("{npc}"))
		{
			NPC anyFriend = StardewUtility.GetAnyFriend(farmer, (IEnumerable<string>)new string[1] { ((Character)conversationData.npc).Name });
			if (anyFriend == null)
			{
				ShowSingleItemTrade(farmer, saveData, item);
				return;
			}
			text = text.Replace("{npc}", NpcContent.NPCDisplayName(anyFriend, conversationData.npc));
		}
		DialogueEvent.AddDialogue(conversationData.npc, text);
		TradeBuy.StartTrade(farmer, saveData, conversationData.npc, item);
	}
}
