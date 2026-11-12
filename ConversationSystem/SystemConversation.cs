// ============================================================================
// SystemConversation.cs
// Core conversation system: main dialogue flow, fixed menu state, NPC dialogue content, and the question ('ask about') system.
//
// Compacted from: Conversation.cs, FixedMenu.cs, NpcContent.cs, QuestionSystem.cs
// ============================================================================

using Netcode;
using SorryLab.Expansion;
using SorryLab;
using StardewValley.Network;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ConversationSystem;

// ------------------------------------------------------------------
// From Conversation.cs
// ------------------------------------------------------------------

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

// ------------------------------------------------------------------
// From FixedMenu.cs
// ------------------------------------------------------------------

public static class FixedMenu
{
	private static Dictionary<Farmer, NPC> _npc = new Dictionary<Farmer, NPC>();

	private static float _praiseSuccessRate = 0.55f;

	private static int _staminaCost = 25;

	private static NPC GetCurrentTalkNPC(Farmer farmer)
	{
		if (!_npc.ContainsKey(farmer))
		{
			_npc[farmer] = null;
		}
		return _npc[farmer];
	}

	private static void SetCurrentTalkNPC(Farmer farmer, NPC who)
	{
		_npc[farmer] = who;
	}

	private static float GetSkillPraiseSuccessRate(Farmer farmer)
	{
		float num = ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		float num2 = (1f - _praiseSuccessRate) * num;
		return _praiseSuccessRate + num2;
	}

	public static void OpenFixedMenuOptions(Farmer farmer, ModSave saveData, NPC npc, string dialogue)
	{
		SetCurrentTalkNPC(farmer, npc);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		string text = Trs.GR("Misk.StaminaCost").Replace("{s}", _staminaCost.ToString());
		string key = Trs.GR("FixedMenu.Praise").Replace("{npc}", NpcContent.NPCDisplayName(npc)) + text;
		string key2 = Trs.GR("FixedMenu.Insult").Replace("{npc}", NpcContent.NPCDisplayName(npc)) + text;
		string key3 = Trs.GR("FixedMenu.Prev");
		dictionary[key] = delegate
		{
			DoPraise(farmer, saveData);
		};
		dictionary[key2] = delegate
		{
			DoInsult(farmer, saveData);
		};
		dictionary[key3] = delegate
		{
			Conversation.OpenPrev(farmer);
		};
		ConversationUtility.DisplayOptions(farmer, npc, dialogue, dictionary);
	}

	private static bool PerformStamina(Farmer farmer, int stamina)
	{
		if (farmer.Stamina < (float)stamina)
		{
			Game1.playSound("cancel", (int?)null);
			DialogueEvent.AddObjectDialogue("Misk.StaminaInsufficient");
			return false;
		}
		farmer.Stamina -= (float)stamina;
		return true;
	}

	private static void DoPraise(Farmer farmer, ModSave saveData)
	{
		if (!PerformStamina(farmer, _staminaCost))
		{
			return;
		}
		NPC who = GetCurrentTalkNPC(farmer);
		if (who == null)
		{
			return;
		}
		ConversationSkill.AddExp(farmer, 12, ConversationUtility.config);
		string text = Trs.GR("FixedMenu.Praise.Message", (Character)(object)GetCurrentTalkNPC(farmer));
		text = text.Replace("{npc}", NpcContent.NPCDisplayName(who));
		DialogueEvent.AddObjectDialogue(text);
		DialogueEvent.AddAction((Action)delegate
		{
			farmer.doEmote(40);
		});
		Quest questMaybeComplete = saveData.GetData(farmer).quests.GetQuestMaybeComplete(farmer, who, QuestType.Praise);
		if (questMaybeComplete != null)
		{
			MentionQuest.OnPraiseQuestComplete(farmer, saveData, who, questMaybeComplete);
		}
		else if (Mathf.RandomValue < GetSkillPraiseSuccessRate(farmer))
		{
			DialogueEvent.AddAction((Action)delegate
			{
				Game1.playSound("give_gift", (int?)null);
			});
			DialogueEvent.AddDialogue(who, Trs.GR("FixedMenu.Praise.Success"));
			if (ConversationSkill.HasCustomProfession(farmer, ConversationSkill.lv10B1) && Mathf.RandomValue < 0.5f)
			{
				DialogueEvent.AddAction((Action)delegate
				{
					ConversationUtility.DropItem(farmer, who, ListExpansion.GetRandomOne<string>(ItemList.giftsLv1), 1);
				});
			}
			DialogueEvent.AddAction((Action)delegate
			{
				((Character)who).doEmote(32, true);
			});
			float num = 20f;
			if (ConversationSkill.HasCustomProfession(farmer, ConversationSkill.lv5B))
			{
				num += num * 1.5f;
			}
			farmer.changeFriendship((int)num, who);
			ConversationUtility.AddEventLog(farmer, saveData, EventType.Praise, who);
		}
		else
		{
			DialogueEvent.AddAction((Action)delegate
			{
				Game1.playSound("cancel", (int?)null);
			});
			DialogueEvent.AddDialogue(who, Trs.GR("FixedMenu.Praise.Fail"));
			DialogueEvent.AddAction((Action)delegate
			{
				((Character)who).doEmote(12, true);
			});
			farmer.changeFriendship(-10, who);
			ConversationUtility.AddEventLog(farmer, saveData, EventType.PraiseFail, who);
		}
	}

	private static void DoInsult(Farmer farmer, ModSave saveData)
	{
		if (!PerformStamina(farmer, _staminaCost))
		{
			return;
		}
		NPC who = GetCurrentTalkNPC(farmer);
		if (who == null)
		{
			return;
		}
		ConversationSkill.AddExp(farmer, 50, ConversationUtility.config);
		Game1.playSound("slimedead", (int?)null);
		string text = Trs.GR("FixedMenu.Insult.Message");
		text = text.Replace("{npc}", NpcContent.NPCDisplayName(who));
		DialogueEvent.AddObjectDialogue(text);
		DialogueEvent.AddAction((Action)delegate
		{
			farmer.doEmote(40);
		});
		Quest questMaybeComplete = saveData.GetData(farmer).quests.GetQuestMaybeComplete(farmer, who, QuestType.Quarrel);
		if (questMaybeComplete != null)
		{
			MentionQuest.OnQuarrelQuestComplete(farmer, saveData, who, questMaybeComplete);
		}
		else
		{
			DialogueEvent.AddAction((Action)delegate
			{
				Game1.playSound("cancel", (int?)null);
			});
			DialogueEvent.AddDialogue(who, Trs.GR("FixedMenu.Insult.Success"));
			DialogueEvent.AddAction((Action)delegate
			{
				((Character)who).doEmote(12, true);
			});
			farmer.changeFriendship(-60, who);
			ConversationUtility.SetTodayConversationRemainingNumber(farmer, who, 0);
		}
		ConversationUtility.AddEventLog(farmer, saveData, EventType.Insult, who);
	}
}

// ------------------------------------------------------------------
// From NpcContent.cs
// ------------------------------------------------------------------

public static class NpcContent
{
	private static List<string> _cantConversation = new List<string>();

	private static List<string> _willNotGivenQuests = new List<string>();

	private static List<string> _willNotBeMissionTarget = new List<string>();

	private static List<string> _willNotBeMention = new List<string>();

	private static Dictionary<string, List<string>> _npcTags = new Dictionary<string, List<string>>();

	private static Dictionary<string, List<string>> _npcFriends = new Dictionary<string, List<string>>();

	public static void Setup()
	{
		FileUtility.SaveToJson<NpcConfig>(ThisMod.helper, NpcConfig.NewVanilla(), "NpcConfigs", "Vanilla.json");
		foreach (FileInfo<NpcConfig> item in FileUtility.LoadJsonFiles<NpcConfig>(ThisMod.helper, "NpcConfigs"))
		{
			Load(item.data);
		}
		Load(NpcConfig.NewRidgeside());
		Load(NpcConfig.NewSVE());
		Load(NpcConfig.NewES());
	}

	public static bool HasData(NPC npc)
	{
		return _npcTags.ContainsKey(((Character)npc).Name);
	}

	public static void CheckLeoMovedAndHandle()
	{
		if (StardewUtility.IsLeoMoved())
		{
			if (_willNotGivenQuests.Contains("Leo"))
			{
				_willNotGivenQuests.Remove("Leo");
			}
			if (_willNotBeMissionTarget.Contains("Leo"))
			{
				_willNotBeMissionTarget.Remove("Leo");
			}
			if (_willNotBeMention.Contains("Leo"))
			{
				_willNotBeMention.Remove("Leo");
			}
			if (_npcFriends.ContainsKey("Leo"))
			{
				_npcFriends.Remove("Leo");
			}
		}
	}

	public static bool CanConversation(NPC npc)
	{
		if (_cantConversation.Contains(((Character)npc).Name))
		{
			return false;
		}
		return true;
	}

	public static void Load(NpcConfig config)
	{
		foreach (KeyValuePair<string, List<string>> npcTag in config.npcTags)
		{
			if (!_npcTags.ContainsKey(npcTag.Key))
			{
				_npcTags[npcTag.Key] = new List<string>();
			}
			foreach (string item in npcTag.Value)
			{
				if (!_npcTags[npcTag.Key].Contains(item))
				{
					_npcTags[npcTag.Key].Add(item);
				}
			}
		}
		foreach (KeyValuePair<string, List<string>> npcRestrictFriend in config.npcRestrictFriends)
		{
			if (!_npcFriends.ContainsKey(npcRestrictFriend.Key))
			{
				_npcFriends[npcRestrictFriend.Key] = new List<string>();
			}
			foreach (string item2 in npcRestrictFriend.Value)
			{
				if (!_npcFriends[npcRestrictFriend.Key].Contains(item2))
				{
					_npcFriends[npcRestrictFriend.Key].Add(item2);
				}
			}
		}
		foreach (string willNotGivenQuest in config.willNotGivenQuests)
		{
			if (!_willNotGivenQuests.Contains(willNotGivenQuest))
			{
				_willNotGivenQuests.Add(willNotGivenQuest);
			}
		}
		foreach (string item3 in config.willNotBeMissionTarget)
		{
			if (!_willNotBeMissionTarget.Contains(item3))
			{
				_willNotBeMissionTarget.Add(item3);
			}
		}
		foreach (string item4 in config.willNotBeMention)
		{
			if (!_willNotBeMention.Contains(item4))
			{
				_willNotBeMention.Add(item4);
			}
		}
		foreach (string item5 in config.cantConversation)
		{
			if (!_cantConversation.Contains(item5))
			{
				_cantConversation.Add(item5);
			}
		}
	}

	public static string GetTalkAboutDialogue(NPC npc, NPC talkAbout)
	{
		WeightDice<string> val = new WeightDice<string>();
		string key = "DialogueSP." + ((Character)npc).Name + "-" + ((Character)talkAbout).Name;
		if (Trs.ContainsGroup(key))
		{
			val.Add(Trs.GR(key), 3f);
		}
		foreach (string npcTag in GetNpcTags(talkAbout))
		{
			string key2 = "Dialogue." + npcTag;
			if (Trs.ContainsGroup(key2))
			{
				val.Add(Trs.GR(key2), 1f);
			}
		}
		string text = "";
		text = ((val.Count <= 0) ? Trs.GR("Dialogue.Default") : val.Cast());
		text = text.Replace("{npc}", NPCDisplayName(talkAbout, npc));
		return GameTextContent.TextGenderHandle(text, (Character)(object)talkAbout);
	}

	private static List<string> GetNpcTags(NPC npc)
	{
		if (_npcTags.ContainsKey(((Character)npc).Name))
		{
			return _npcTags[((Character)npc).Name];
		}
		return new List<string>();
	}

	public static string NPCDisplayName(NPC npc, NPC whoCall = null)
	{
		if (whoCall == null)
		{
			return ((Character)npc).displayName;
		}
		string key = "Address." + ((Character)whoCall).Name + "-" + ((Character)npc).Name;
		if (Trs.ContainsGroup(key))
		{
			return Trs.GR(key);
		}
		return ((Character)npc).displayName;
	}

	public static bool IsKid(NPC npc)
	{
		if (npc == null)
		{
			return false;
		}
		if (_npcTags.ContainsKey(((Character)npc).Name))
		{
			return _npcTags[((Character)npc).Name].Contains("Kid");
		}
		return false;
	}

	public static string GetNpcTown(NPC npc)
	{
		foreach (string npcTag in GetNpcTags(npc))
		{
			string[] array = npcTag.Split(':');
			if (array.Length >= 2 && array[0] == "Town")
			{
				return array[1];
			}
		}
		return "Unknown";
	}

	public static List<NPC> GetNpcListFromTown(Farmer farmer, NPC npc, IEnumerable<string> without = null)
	{
		return GetNpcListFromTown(farmer, GetNpcTown(npc), without);
	}

	public static List<NPC> GetNpcListFromTown(Farmer farmer, string town, IEnumerable<string> without = null)
	{
		List<NPC> list = new List<NPC>();
		NPC val = default(NPC);
		Friendship val2 = default(Friendship);
		foreach (KeyValuePair<string, List<string>> npcTag in _npcTags)
		{
			if (StardewUtility.TryGetNPCFromName(npcTag.Key, ref val) && (without == null || !without.Contains(((Character)val).Name)) && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(npcTag.Key, ref val2) && GetNpcTown(val) == town)
			{
				list.Add(val);
			}
		}
		return list;
	}

	public static List<NPC> GetNpcTalkAboutList(Farmer farmer, NPC npc)
	{
		if (_npcFriends.ContainsKey(((Character)npc).Name))
		{
			List<NPC> list = new List<NPC>();
			NPC item = default(NPC);
			Friendship val = default(Friendship);
			foreach (string item2 in _npcFriends[((Character)npc).Name])
			{
				if (StardewUtility.TryGetNPCFromName(item2, ref item) && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(item2, ref val))
				{
					list.Add(item);
				}
			}
			return list;
		}
		List<string> list2 = new List<string> { ((Character)npc).Name };
		list2.AddRange(_willNotBeMention);
		string npcTown = GetNpcTown(npc);
		if (npcTown != "Unknown")
		{
			return GetNpcListFromTown(farmer, npc, list2);
		}
		return StardewUtility.GetAllFriends(farmer, (IEnumerable<string>)list2);
	}

	public static bool IsNpcPossibleGivenQuest(NPC npc)
	{
		if (_willNotGivenQuests.Contains(((Character)npc).Name))
		{
			return false;
		}
		return true;
	}

	public static NPC GetQuestTargetPossibleNPC(Farmer farmer, NPC talkWithWho)
	{
		List<string> list = new List<string> { ((Character)talkWithWho).Name };
		list.AddRange(_willNotBeMissionTarget);
		List<NPC> allFriends = StardewUtility.GetAllFriends(farmer, (IEnumerable<string>)list.ToArray());
		for (int i = 0; i < allFriends.Count; i++)
		{
			if (QuestData.CheckNpcHasQuest(allFriends[i]))
			{
				allFriends.RemoveAt(i);
				i++;
			}
		}
		if (allFriends.Count > 0)
		{
			return ListExpansion.GetRandomOne<NPC>(allFriends);
		}
		return null;
	}

	public static string ToNpcSaidFormat(NPC npc, string dialogue)
	{
		string text = Trs.Get("Misk.NpcSaid").Replace("{npc}", NPCDisplayName(npc));
		string text2 = Trs.Get("Misk.QuoteMarks").Replace("{*}", dialogue);
		return text + text2;
	}
}

// ------------------------------------------------------------------
// From QuestionSystem.cs
// ------------------------------------------------------------------

internal static class QuestionSystem
{
	internal class QuestionData
	{
		public NPC npc;

		public Question question;
	}

	public class Question
	{
		public string text;

		public int correctIndex;

		public List<string> options;
	}

	public enum QuestionType
	{
		CropSeason,
		TreesCrops,
		KegAndWine,
		MaturityTime,
		FishSeasons,
		HighestPrices,
		HighestStamina,
		Ingredients,
		ItemTaste,
		FishingFloor
	}

	private static List<QuestionType> _typesHaveBeenUsed = new List<QuestionType>();

	private static int _optionCount = 3;

	private static Dictionary<Farmer, QuestionData> _questionData = new Dictionary<Farmer, QuestionData>();

	private static Dictionary<QuestionType, Func<Farmer, Question>> questions = new Dictionary<QuestionType, Func<Farmer, Question>>
	{
		{
			QuestionType.CropSeason,
			GetQuest_CropSeason
		},
		{
			QuestionType.TreesCrops,
			GetQuest_TreesCrops
		},
		{
			QuestionType.KegAndWine,
			GetQuest_KegAndWine
		},
		{
			QuestionType.MaturityTime,
			GetQuest_MaturityTime
		},
		{
			QuestionType.FishSeasons,
			GetQuest_FishSeasons
		},
		{
			QuestionType.HighestPrices,
			GetQuest_HighestPrices
		},
		{
			QuestionType.HighestStamina,
			GetQuest_HighestStamina
		},
		{
			QuestionType.Ingredients,
			GetQuest_Ingredients
		},
		{
			QuestionType.ItemTaste,
			GetQuest_ItemTaste
		},
		{
			QuestionType.FishingFloor,
			GetQuest_FishingFloor
		}
	};

	private static QuestionData GetQuestionData(Farmer farmer)
	{
		if (!_questionData.ContainsKey(farmer))
		{
			_questionData[farmer] = new QuestionData();
		}
		return _questionData[farmer];
	}

	public static Question PlayQuestion(Farmer farmer, NPC npc, string preMsg = "", Action<bool> callback = null)
	{
		QuestionData questionData = GetQuestionData(farmer);
		questionData.npc = npc;
		int count = StardewUtility.GetAllFriends(farmer, (IEnumerable<string>)null).Count;
		List<QuestionType> list = questions.Keys.ToList();
		if (_typesHaveBeenUsed.Count >= questions.Count)
		{
			_typesHaveBeenUsed.Clear();
		}
		for (int i = 0; i < list.Count; i++)
		{
			QuestionType item = list[i];
			if (_typesHaveBeenUsed.Contains(item))
			{
				list.RemoveAt(i);
				i--;
			}
		}
		QuestionType randomOne = ListExpansion.GetRandomOne<QuestionType>(list);
		questionData.question = questions[randomOne](farmer);
		string text = questionData.question.text;
		_typesHaveBeenUsed.Add(randomOne);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		for (int j = 0; j < questionData.question.options.Count; j++)
		{
			int answer = j;
			dictionary[questionData.question.options[j]] = delegate
			{
				Answer(farmer, answer, callback);
			};
		}
		if (npc != null)
		{
			if (!string.IsNullOrEmpty(preMsg))
			{
				DialogueEvent.AddDialogue(npc, preMsg + "\n" + text);
			}
			else
			{
				DialogueEvent.AddDialogue(npc, text);
			}
			text = NpcContent.ToNpcSaidFormat(npc, text);
		}
		ConversationUtility.DisplayOptions(farmer, npc, text, dictionary);
		return questionData.question;
	}

	private static void Answer(Farmer farmer, int index, Action<bool> callback)
	{
		QuestionData questionData = GetQuestionData(farmer);
		bool flag = questionData.question.correctIndex == index;
		if (flag)
		{
			Game1.playSound("give_gift", (int?)null);
		}
		else
		{
			Game1.playSound("slimedead", (int?)null);
		}
		callback?.Invoke(flag);
	}

	private static Question NewQuestion()
	{
		return new Question
		{
			correctIndex = Mathf.RandomRange(0, _optionCount),
			options = new List<string>()
		};
	}

	private static Question GetQuest_CropSeason(Farmer farmer)
	{
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Expected O, but got Unknown
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Expected O, but got Unknown
		Question question = NewQuestion();
		Dictionary<Season, List<string>> dictionary = new Dictionary<Season, List<string>>
		{
			{
				(Season)0,
				new List<string>
				{
					"597", "Carrot", "190", "248", "188", "250", "24", "192", "252", "400",
					"591", "271"
				}
			},
			{
				(Season)1,
				new List<string>
				{
					"258", "304", "260", "254", "376", "264", "266", "268", "593", "SummerSquash",
					"256"
				}
			},
			{
				(Season)2,
				new List<string>
				{
					"300", "274", "284", "278", "Broccoli", "282", "272", "595", "398", "276",
					"280"
				}
			},
			{
				(Season)3,
				new List<string> { "Powdermelon" }
			}
		};
		List<Season> list = new List<Season>
		{
			(Season)0,
			(Season)1,
			(Season)2
		};
		Season randomOneAndRemove = ListExpansion.GetRandomOneAndRemove<Season>(list);
		list.Add((Season)3);
		string newValue = Trs.GR($"Season.{randomOneAndRemove}");
		question.text = Trs.GR("Question.Type.CropSeason").Replace("{season}", newValue);
		List<string> list2 = dictionary[randomOneAndRemove];
		List<string> list3 = new List<string>();
		for (int i = 0; i < list.Count; i++)
		{
			list3.AddRange(dictionary[list[i]]);
		}
		for (int j = 0; j < _optionCount; j++)
		{
			if (j == question.correctIndex)
			{
				Object val = new Object(ListExpansion.GetRandomOneAndRemove<string>(list2), 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(list3), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_TreesCrops(Farmer farmer)
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		Question question = NewQuestion();
		List<string> list = new List<string> { "613", "634", "91", "638", "834", "635", "636", "637" };
		List<string> list2 = new List<string>
		{
			"454", "410", "258", "90", "282", "414", "260", "254", "832", "Powdermelon",
			"252", "296", "396", "268", "400", "406"
		};
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				Object val = new Object(ListExpansion.GetRandomOneAndRemove<string>(list), 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(list2), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		question.text = Trs.GR("Question.Type.TreesCrops");
		return question;
	}

	private static Question GetQuest_KegAndWine(Farmer farmer)
	{
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Expected O, but got Unknown
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Expected O, but got Unknown
		Question question = NewQuestion();
		List<string> list = new List<string>
		{
			"613", "634", "91", "638", "834", "635", "636", "637", "454", "410",
			"258", "90", "282", "414", "260", "254", "832", "Powdermelon", "252", "296",
			"396", "268", "400", "406"
		};
		List<string> list2 = new List<string>
		{
			"300", "274", "284", "278", "Broccoli", "Carrot", "190", "270", "272", "259",
			"248", "188", "304", "250", "24", "192", "276", "264", "266", "SummerSquash",
			"830", "815", "256", "271", "262", "280"
		};
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				Object val = new Object(ListExpansion.GetRandomOneAndRemove<string>(list), 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(list2), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		question.text = Trs.GR("Question.Type.KegAndWine");
		return question;
	}

	private static Question GetQuest_MaturityTime(Farmer farmer)
	{
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Expected O, but got Unknown
		Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>
		{
			{
				3,
				new List<string> { "Carrot" }
			},
			{
				4,
				new List<string> { "248", "24", "262", "278" }
			},
			{
				5,
				new List<string> { "260", "272" }
			},
			{
				6,
				new List<string> { "250", "192", "591", "264", "SummerSquash", "284" }
			},
			{
				7,
				new List<string> { "413", "376", "300", "282", "Powdermelon", "771" }
			},
			{
				8,
				new List<string> { "400", "593", "421", "274", "Broccoli" }
			},
			{
				9,
				new List<string> { "266" }
			},
			{
				10,
				new List<string> { "433", "188", "398", "280" }
			},
			{
				11,
				new List<string> { "304", "256" }
			},
			{
				12,
				new List<string> { "190", "254", "595", "90" }
			},
			{
				13,
				new List<string> { "252", "258", "268", "]" }
			},
			{
				14,
				new List<string> { "270", "832" }
			},
			{
				24,
				new List<string> { "417" }
			},
			{
				28,
				new List<string> { "454" }
			}
		};
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		List<string> list = new List<string>();
		List<int> list2 = dictionary.Keys.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			List<string> list3 = dictionary[list2[i]];
			list.AddRange(list3);
			for (int j = 0; j < list3.Count; j++)
			{
				dictionary2[list3[j]] = list2[i];
			}
		}
		string randomOneAndRemove = ListExpansion.GetRandomOneAndRemove<string>(list);
		int item = dictionary2[randomOneAndRemove];
		list2.Remove(item);
		Object val = new Object(randomOneAndRemove, 1, false, -1, 0);
		Question question = NewQuestion();
		question.text = Trs.GR("Question.Type.MaturityTime").Replace("{crop}", ((Item)val).DisplayName);
		for (int k = 0; k < _optionCount; k++)
		{
			if (k == question.correctIndex)
			{
				question.options.Add(item.ToString());
			}
			else
			{
				question.options.Add(ListExpansion.GetRandomOneAndRemove<int>(list2).ToString());
			}
		}
		return question;
	}

	private static Question GetQuest_FishSeasons(Farmer farmer)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Expected O, but got Unknown
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Expected O, but got Unknown
		Question question = NewQuestion();
		Season randomOne = ListExpansion.GetRandomOne<Season>(new List<Season>
		{
			(Season)0,
			(Season)1,
			(Season)2,
			(Season)3
		});
		string newValue = Trs.GR($"Season.{randomOne}");
		question.text = Trs.GR("Question.Type.FishSeasons").Replace("{season}", newValue);
		Dictionary<Season, List<string>> dictionary = new Dictionary<Season, List<string>>
		{
			{
				(Season)0,
				new List<string> { "129", "131", "147", "148", "267", "708" }
			},
			{
				(Season)1,
				new List<string> { "128", "130", "146", "149", "155", "267", "701", "708" }
			},
			{
				(Season)2,
				new List<string> { "129", "131", "148", "154", "155", "701", "705" }
			},
			{
				(Season)3,
				new List<string> { "130", "131", "146", "147", "151", "154", "705", "708" }
			}
		};
		List<string> list = new List<string>();
		foreach (Season key in dictionary.Keys)
		{
			foreach (string item in dictionary[key])
			{
				if (!dictionary[randomOne].Contains(item) && !list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				Object val = new Object(ListExpansion.GetRandomOneAndRemove<string>(list), 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(dictionary[randomOne]), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_HighestPrices(Farmer farmer)
	{
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Expected O, but got Unknown
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Expected O, but got Unknown
		Question question = NewQuestion();
		List<(string, int)> list = new List<(string, int)>
		{
			("304", 25),
			("24", 35),
			("188", 40),
			("258", 50),
			("256", 60),
			("282", 75),
			("192", 80),
			("264", 90),
			("284", 100),
			("250", 110),
			("400", 120),
			("300", 150),
			("274", 160),
			("190", 175),
			("252", 220),
			("254", 250),
			("266", 260),
			("276", 320),
			("454", 550),
			("268", 750)
		};
		List<(string, int)> list2 = new List<(string, int)>();
		for (int i = 0; i < _optionCount; i++)
		{
			list2.Add(ListExpansion.GetRandomOneAndRemove<(string, int)>(list));
		}
		list2 = list2.OrderByDescending(((string, int) item) => item.Item2).ToList();
		(string, int) tuple = list2[0];
		list2.RemoveAt(0);
		question.text = Trs.GR("Question.Type.HighestPrices");
		for (int num = 0; num < _optionCount; num++)
		{
			if (num == question.correctIndex)
			{
				Object val = new Object(tuple.Item1, 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<(string, int)>(list2).Item1, 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_HighestStamina(Farmer farmer)
	{
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Expected O, but got Unknown
		Question question = NewQuestion();
		List<(string, int)> list = new List<(string, int)>
		{
			("253", 8),
			("194", 50),
			("227", 75),
			("199", 85),
			("209", 90),
			("195", 100),
			("196", 113),
			("226", 115),
			("207", 125),
			("215", 130),
			("205", 135),
			("197", 138),
			("206", 150),
			("200", 165),
			("239", 170),
			("218", 175),
			("201", 200),
			("222", 215),
			("230", 240),
			("221", 250)
		};
		List<(string, int)> list2 = new List<(string, int)>();
		for (int i = 0; i < _optionCount; i++)
		{
			list2.Add(ListExpansion.GetRandomOneAndRemove<(string, int)>(list));
		}
		list2 = list2.OrderByDescending(((string, int) item) => item.Item2).ToList();
		(string, int) tuple = list2[0];
		list2.RemoveAt(0);
		question.text = Trs.GR("Question.Type.HighestStamina");
		for (int num = 0; num < _optionCount; num++)
		{
			if (num == question.correctIndex)
			{
				Object val = new Object(tuple.Item1, 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<(string, int)>(list2).Item1, 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_Ingredients(Farmer farmer)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "303", "304" },
			{ "346", "262" },
			{ "459", "340" },
			{ "424", "184" },
			{ "426", "436" },
			{ "419", "423" },
			{ "395", "433" },
			{ "614", "815" },
			{ "428", "440" },
			{ "306", "176" },
			{ "307", "442" },
			{ "308", "305" },
			{ "807", "107" },
			{ "432", "430" },
			{ "247", "270" }
		};
		List<string> list = dictionary.Keys.ToList();
		string randomOneAndRemove = ListExpansion.GetRandomOneAndRemove<string>(list);
		Object val = new Object(randomOneAndRemove, 1, false, -1, 0);
		Question question = NewQuestion();
		question.text = Trs.GR("Question.Type.Ingredients").Replace("{obj}", ((Item)val).DisplayName);
		for (int i = 0; i < _optionCount; i++)
		{
			val = ((i != question.correctIndex) ? new Object(dictionary[ListExpansion.GetRandomOneAndRemove<string>(list)], 1, false, -1, 0) : new Object(dictionary[randomOneAndRemove], 1, false, -1, 0));
			question.options.Add(((Item)val).DisplayName);
		}
		return question;
	}

	private static Question GetQuest_ItemTaste(Farmer farmer)
	{
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Expected O, but got Unknown
		QuestionData questionData = GetQuestionData(farmer);
		List<string> list = new List<string>
		{
			"Alex", "Elliott", "Harvey", "Sam", "Sebastian", "Shane", "Abigail", "Emily", "Haley", "Leah",
			"Maru", "Penny", "Caroline", "Clint", "Demetrius", "Evelyn", "George", "Gus", "Jas", "Jodi",
			"Lewis", "Linus", "Marnie", "Pam", "Pierre", "Robin", "Vincent", "Willy"
		};
		if (questionData.npc != null)
		{
			list.Remove(((Character)questionData.npc).Name);
		}
		NPC val = default(NPC);
		StardewUtility.TryGetNPCFromName(ListExpansion.GetRandomOne<string>(list), ref val);
		Question question = NewQuestion();
		List<string> itemIDListFromTaste = StardewUtility.GetItemIDListFromTaste(val, (GiftTaste)(!(Mathf.RandomValue > 0.5f)));
		List<string> itemIDListFromTaste2 = StardewUtility.GetItemIDListFromTaste(val, (GiftTaste)((Mathf.RandomValue > 0.5f) ? 3 : 4));
		question.text = Trs.GR("Question.Type.ItemTaste").Replace("{npc}", ((Character)val).displayName);
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(itemIDListFromTaste), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
			else
			{
				Object val3 = new Object(ListExpansion.GetRandomOneAndRemove<string>(itemIDListFromTaste2), 1, false, -1, 0);
				question.options.Add(((Item)val3).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_FishingFloor(Farmer farmer)
	{
		Question question = NewQuestion();
		question.text = Trs.GR("Question.Type.FishingFloor");
		List<string> list = new List<string> { "20", "60", "100" };
		List<string> list2 = new List<string>
		{
			"5", "10", "15", "25", "30", "35", "40", "45", "50", "55",
			"65", "70", "75", "80", "85", "90", "95"
		};
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				question.options.Add(ListExpansion.GetRandomOneAndRemove<string>(list));
			}
			else
			{
				question.options.Add(ListExpansion.GetRandomOneAndRemove<string>(list2));
			}
		}
		return question;
	}
}
