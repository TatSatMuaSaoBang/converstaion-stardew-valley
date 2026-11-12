// ============================================================================
// Quest.cs
// Quest system: quest models/data/types, quest mentioning, and inquiring NPCs.
//
// Compacted from: Quest.cs, QuestData.cs, QuestType.cs, MentionQuest.cs, InquiringNPC.cs
// ============================================================================

using Netcode;
using SorryLab.Expansion;
using SorryLab;
using StardewValley.Network;
using StardewValley;
using System.Collections.Generic;
using System;

namespace ConversationSystem;

// ------------------------------------------------------------------
// From Quest.cs
// ------------------------------------------------------------------

public class Quest
{
	public Dictionary<string, string> content { get; set; }

	public string fromNpcName { get; set; }

	public string targetNpcName { get; set; }

	public QuestType questType { get; set; }

	public Quest()
	{
		content = new Dictionary<string, string>();
	}

	public NPC FromNpc()
	{
		NPC result = default(NPC);
		if (StardewUtility.TryGetNPCFromName(fromNpcName, ref result))
		{
			return result;
		}
		return null;
	}

	public NPC TargetNpc()
	{
		NPC result = default(NPC);
		if (StardewUtility.TryGetNPCFromName(targetNpcName, ref result))
		{
			return result;
		}
		return null;
	}

	public virtual string StartQuestDialogue(NPC whoCall)
	{
		if (questType == QuestType.HopeItem)
		{
			Object sObjectFromItemID = StardewUtility.GetSObjectFromItemID(content["item"]);
			return Trs.GR("Quest.HopeItem", (Character)(object)FromNpc()).Replace("{npc}", NpcContent.NPCDisplayName(FromNpc(), whoCall)).Replace("{i}", ((Item)sObjectFromItemID).DisplayName);
		}
		if (questType == QuestType.Praise)
		{
			return Trs.GR("Quest.Praise", (Character)(object)FromNpc()).Replace("{npc}", NpcContent.NPCDisplayName(FromNpc(), whoCall));
		}
		if (questType == QuestType.Quarrel)
		{
			return Trs.GR("Quest.Quarrel", (Character)(object)TargetNpc()).Replace("{npc}", NpcContent.NPCDisplayName(TargetNpc(), whoCall));
		}
		return "Quest.StartQuestDialogue";
	}

	public virtual string QuestGuideText()
	{
		if (questType == QuestType.HopeItem)
		{
			Object sObjectFromItemID = StardewUtility.GetSObjectFromItemID(content["item"]);
			return Trs.GR("Quest.HopeItem.Guide", (Character)(object)FromNpc()).Replace("{npc}", NpcContent.NPCDisplayName(FromNpc())).Replace("{i}", ((Item)sObjectFromItemID).DisplayName);
		}
		if (questType == QuestType.Praise)
		{
			return Trs.GR("Quest.Praise.Guide", (Character)(object)FromNpc()).Replace("{npc}", NpcContent.NPCDisplayName(FromNpc()));
		}
		if (questType == QuestType.Quarrel)
		{
			return Trs.GR("Quest.Quarrel.Guide", (Character)(object)FromNpc()).Replace("{npc}", NpcContent.NPCDisplayName(TargetNpc())).Replace("{npc1}", NpcContent.NPCDisplayName(FromNpc()));
		}
		return "Quest.QuestGuideText";
	}

	public virtual string QuestNpcLocationText()
	{
		NPC val = default(NPC);
		if (questType == QuestType.Quarrel && StardewUtility.TryGetNPCFromName(targetNpcName, ref val))
		{
			if (((Character)val).currentLocation.DisplayName.Contains("_") || ((Character)val).currentLocation.DisplayName.Contains("Custom"))
			{
				return "";
			}
			return Trs.GR("Misk.NPCName.Location", (Character)(object)val).Replace("{npc}", NpcContent.NPCDisplayName(val)).Replace("{location}", ((Character)val).currentLocation.DisplayName);
		}
		if (((Character)FromNpc()).currentLocation.DisplayName.Contains("_") || ((Character)FromNpc()).currentLocation.DisplayName.Contains("Custom"))
		{
			return "";
		}
		return Trs.GR("Misk.NPCName.Location", (Character)(object)FromNpc()).Replace("{npc}", NpcContent.NPCDisplayName(FromNpc())).Replace("{location}", ((Character)FromNpc()).currentLocation.DisplayName);
	}
}

// ------------------------------------------------------------------
// From QuestData.cs
// ------------------------------------------------------------------

public class QuestData
{
	private class QuestTypeDice
	{
		public QuestType type;

		public QuestTypeDice(QuestType type)
		{
			this.type = type;
		}
	}

	private static QuestData _instance;

	public List<Quest> todayQuests = new List<Quest>();

	public List<Quest> yesterdayQuests = new List<Quest>();

	public int completeQuestCount = 0;

	private static Dictionary<string, List<Quest>> _todayGivenQuest = new Dictionary<string, List<Quest>>();

	public int Count => todayQuests.Count;

	public QuestData()
	{
		_instance = this;
	}

	public void OnDayStarted()
	{
		yesterdayQuests.Clear();
		yesterdayQuests.AddRange(todayQuests);
		todayQuests.Clear();
		_todayGivenQuest.Clear();
	}

	public void SetQuestComplete(Farmer farmer, ModSave saveData, Quest quest)
	{
		saveData.GetData(farmer).npcData.AddHelpPoints(quest.TargetNpc(), 1);
		todayQuests.Remove(quest);
		completeQuestCount++;
		for (int i = 0; i < completeQuestCount; i++)
		{
			if (Mathf.RandomValue < 0.05f)
			{
				completeQuestCount = 0;
				DialogueEvent.AddAction((Action)delegate
				{
					ConversationUtility.DropItem(farmer, quest.TargetNpc(), "PrizeTicket", 1);
				});
			}
		}
	}

	public void SetQuestEnd(Quest quest)
	{
		todayQuests.Remove(quest);
	}

	public Quest GetQuestMaybeComplete(Farmer farmer, NPC who, QuestType type)
	{
		if (type == QuestType.HopeItem)
		{
			foreach (Quest todayQuest in todayQuests)
			{
				if (todayQuest.questType == type && todayQuest.targetNpcName == ((Character)who).Name && farmer.ActiveObject != null && ((Item)farmer.ActiveObject).ItemId == todayQuest.content["item"])
				{
					return todayQuest;
				}
			}
		}
		if (type == QuestType.Praise)
		{
			foreach (Quest todayQuest2 in todayQuests)
			{
				if (todayQuest2.questType == type && todayQuest2.targetNpcName == ((Character)who).Name)
				{
					return todayQuest2;
				}
			}
		}
		if (type == QuestType.Quarrel)
		{
			foreach (Quest todayQuest3 in todayQuests)
			{
				if (todayQuest3.questType == type && todayQuest3.targetNpcName == ((Character)who).Name)
				{
					return todayQuest3;
				}
			}
		}
		return null;
	}

	public Quest NewRandomQuest(Farmer farmer, ModSave saveData, NPC from, ModConfig config)
	{
		List<string> list = new List<string> { ((Character)from).Name };
		foreach (Quest todayQuest in todayQuests)
		{
			list.Add(todayQuest.fromNpcName);
		}
		NPC questTargetPossibleNPC = NpcContent.GetQuestTargetPossibleNPC(farmer, from);
		if (questTargetPossibleNPC != null)
		{
			WeightDice<QuestTypeDice> val = new WeightDice<QuestTypeDice>();
			if (config.questEnable_FindingItems)
			{
				val.Add(new QuestTypeDice(QuestType.HopeItem), 1f);
			}
			if (config.questEnable_PraiseSomeone)
			{
				val.Add(new QuestTypeDice(QuestType.Praise), 1f);
			}
			if (config.questEnable_InsultSomeone)
			{
				val.Add(new QuestTypeDice(QuestType.Quarrel), 1f);
			}
			if (config.questEnable_QuizGame)
			{
				val.Add(new QuestTypeDice(QuestType.QuizContest), 1f);
			}
			if (config.questEnable_Loan && !saveData.GetData(farmer).npcData.IsTodayClearArrears(from) && !ConversationUtility.IsFamily(farmer, from) && ((Character)from).Name != "Linus" && ((Character)from).Name != "Leo")
			{
				val.Add(new QuestTypeDice(QuestType.Loan), 1f);
			}
			QuestType type = val.Cast().type;
			Quest quest = new Quest
			{
				fromNpcName = ((Character)questTargetPossibleNPC).Name,
				targetNpcName = ((Character)questTargetPossibleNPC).Name,
				questType = type
			};
			if (type == QuestType.HopeItem)
			{
				quest.content["item"] = ListExpansion.GetRandomOne<string>(ItemList.easyItems);
			}
			if (type == QuestType.Praise && ConversationUtility.GetTodayConversationRemainingNumber(farmer, quest.TargetNpc()) == 0)
			{
				ConversationUtility.ChangeTodayConversationRemainingNumber(farmer, quest.TargetNpc(), 1);
			}
			if (type == QuestType.Quarrel)
			{
				quest.fromNpcName = ((Character)from).Name;
				quest.targetNpcName = ((Character)questTargetPossibleNPC).Name;
				if (ConversationUtility.GetTodayConversationRemainingNumber(farmer, quest.TargetNpc()) == 0)
				{
					ConversationUtility.ChangeTodayConversationRemainingNumber(farmer, quest.TargetNpc(), 1);
				}
			}
			if (!_todayGivenQuest.ContainsKey(((Character)from).Name))
			{
				_todayGivenQuest[((Character)from).Name] = new List<Quest>();
			}
			_todayGivenQuest[((Character)from).Name].Add(quest);
			if (quest.questType == QuestType.HopeItem || quest.questType == QuestType.Praise || quest.questType == QuestType.Quarrel)
			{
				todayQuests.Add(quest);
			}
			return quest;
		}
		return null;
	}

	public static int TodayQuestCount()
	{
		return _instance.Count;
	}

	public static bool IsTodayGivenQuest(NPC npc)
	{
		if (_todayGivenQuest.TryGetValue(((Character)npc).Name, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	public static bool CheckNpcHasQuest(NPC npc)
	{
		foreach (Quest todayQuest in _instance.todayQuests)
		{
			if (todayQuest.fromNpcName == ((Character)npc).Name)
			{
				return true;
			}
		}
		return false;
	}

	public bool HaveItemQuest()
	{
		foreach (Quest todayQuest in todayQuests)
		{
			if (todayQuest.questType == QuestType.HopeItem)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHopeItemNpc(NPC npc)
	{
		foreach (Quest todayQuest in todayQuests)
		{
			if (todayQuest.questType == QuestType.HopeItem && todayQuest.fromNpcName == ((Character)npc).Name)
			{
				return true;
			}
		}
		return false;
	}

	public string GetAnyQuestItem(Farmer farmer)
	{
		List<string> list = new List<string>();
		foreach (Quest todayQuest in todayQuests)
		{
			if (todayQuest.questType == QuestType.HopeItem)
			{
				string text = todayQuest.content["item"];
				if (!StardewUtility.HasItemInInventory(farmer, text))
				{
					list.Add(text);
				}
			}
		}
		if (list.Count > 0)
		{
			return ListExpansion.GetRandomOne<string>(list);
		}
		return "168";
	}
}

// ------------------------------------------------------------------
// From QuestType.cs
// ------------------------------------------------------------------

public enum QuestType
{
	Praise,
	HopeItem,
	Quarrel,
	Loan,
	QuizContest
}

// ------------------------------------------------------------------
// From MentionQuest.cs
// ------------------------------------------------------------------

public static class MentionQuest
{
	private static float _askQuestItemSussesChance = 0.5f;

	private static float GetSkillAskQuestItemSussesChance(Farmer farmer)
	{
		float num = ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		float num2 = (1f - _askQuestItemSussesChance) * num;
		return _askQuestItemSussesChance + num2;
	}

	public static void StartMentionQuest(Farmer farmer, ModSave saveData, NPC npc, ModConfig config)
	{
		Quest quest = saveData.GetData(farmer).quests.NewRandomQuest(farmer, saveData, npc, config);
		if (quest != null)
		{
			if (quest.questType == QuestType.Loan)
			{
				AskLoan(farmer, saveData, npc);
				return;
			}
			if (quest.questType == QuestType.QuizContest)
			{
				AskQuizContest(farmer, npc);
				return;
			}
			DialogueEvent.AddDialogue(npc, quest.StartQuestDialogue(npc));
			DialogueEvent.AddGlobalMessage(quest.QuestGuideText());
		}
		else
		{
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.Unknown"));
		}
	}

	public static void OnQuarrelQuestComplete(Farmer farmer, ModSave saveData, NPC npc, Quest quest)
	{
		NPC fromNpc = default(NPC);
		if (StardewUtility.TryGetNPCFromName(quest.fromNpcName, ref fromNpc))
		{
			DialogueEvent.AddAction((Action)delegate
			{
				Game1.playSound("give_gift", (int?)null);
			});
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.Quarrel.Apology").Replace("{npc1}", NpcContent.NPCDisplayName(fromNpc, npc)));
			Object reward = ConversationUtility.Reward(1, npc, farmer);
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.Quarrel.Reward").Replace("{i}", ((Item)reward).DisplayName));
			DialogueEvent.AddAction((Action)delegate
			{
				ConversationUtility.DropItem(farmer, npc, ((Item)reward).ItemId, 1, ((Item)reward).Quality);
				if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).ContainsKey(((Character)fromNpc).Name))
				{
					farmer.changeFriendship(40, fromNpc);
				}
				((Character)npc).doEmote(28, true);
			});
			DialogueEvent.AddGlobalMessage(Trs.GR("Quest.Quarrel.Complete", (Character)(object)fromNpc).Replace("{npc}", NpcContent.NPCDisplayName(fromNpc)));
		}
		saveData.GetData(farmer).quests.SetQuestComplete(farmer, saveData, quest);
	}

	public static void OnPraiseQuestComplete(Farmer farmer, ModSave saveData, NPC npc, Quest quest)
	{
		string text = Trs.GR("Quest.Praise.Reaction").Replace("{npc}", NpcContent.NPCDisplayName(npc));
		DialogueEvent.AddAction((Action)delegate
		{
			Game1.playSound("give_gift", (int?)null);
		});
		DialogueEvent.AddDialogue(npc, text);
		Object reward = ConversationUtility.Reward(1, npc, farmer);
		text = Trs.GR("Quest.Praise.Reward").Replace("{i}", ((Item)reward).DisplayName);
		DialogueEvent.AddDialogue(npc, text);
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, npc, ((Item)reward).ItemId, 1, ((Item)reward).Quality);
			farmer.changeFriendship(40, npc);
			((Character)npc).doEmote(32, true);
		});
		text = Trs.GR("Quest.Praise.Complete", (Character)(object)npc).Replace("{npc}", NpcContent.NPCDisplayName(npc));
		DialogueEvent.AddGlobalMessage(text);
		saveData.GetData(farmer).quests.SetQuestComplete(farmer, saveData, quest);
	}

	public static void AskQuestItem(Farmer farmer, ModSave saveData, NPC npc, string itemID)
	{
		bool flag = Mathf.RandomValue < GetSkillAskQuestItemSussesChance(farmer);
		Object sObjectFromItemID = StardewUtility.GetSObjectFromItemID(itemID);
		if (flag)
		{
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.HopeItem.Ask.Success").Replace("{i}", ((Item)sObjectFromItemID).DisplayName));
			TradeBuy.StartTrade(farmer, saveData, npc, sObjectFromItemID);
			return;
		}
		string text = Trs.GR("Quest.HopeItem.Ask.Fail").Replace("{i}", ((Item)sObjectFromItemID).DisplayName);
		if (text.Contains("{npc}"))
		{
			NPC randomOne = ListExpansion.GetRandomOne<NPC>(NpcContent.GetNpcTalkAboutList(farmer, npc));
			if (randomOne != null)
			{
				text = text.Replace("{npc}", NpcContent.NPCDisplayName(randomOne, npc));
				text = GameTextContent.TextGenderHandle(text, (Character)(object)randomOne);
				DialogueEvent.AddDialogue(npc, text);
			}
			else
			{
				AskQuestItem(farmer, saveData, npc, itemID);
			}
		}
		else
		{
			DialogueEvent.AddDialogue(npc, text);
		}
	}

	private static void AskQuizContest(Farmer farmer, NPC npc)
	{
		string text = Trs.GR("Quest.QuizContest.Start");
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		dictionary[Trs.GR("Answer.Agree")] = delegate
		{
			QuizContestProcess(farmer, npc, agree: true);
		};
		dictionary[Trs.GR("Answer.Reject")] = delegate
		{
			QuizContestProcess(farmer, npc, agree: false);
		};
		if (npc != null)
		{
			DialogueEvent.AddDialogue(npc, text);
			text = NpcContent.ToNpcSaidFormat(npc, text);
		}
		ConversationUtility.DisplayOptions(farmer, npc, text, dictionary);
	}

	private static void QuizContestProcess(Farmer farmer, NPC npc, bool agree)
	{
		if (agree)
		{
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.QuizContest.AgreeReaction"));
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.QuizContest.Rule"));
			DialogueEvent.AddAction((Action)delegate
			{
				QuizContestNextQuestion(farmer, npc, 1);
			});
		}
		else
		{
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.QuizContest.RejectReaction"));
		}
	}

	private static void QuizContestNextQuestion(Farmer farmer, NPC npc, int progress)
	{
		QuestionSystem.PlayQuestion(farmer, npc, Trs.GR($"Quest.QuizContest.Q{progress}"), delegate(bool b)
		{
			if (b)
			{
				if (progress >= 3)
				{
					QuizContestSuccess(farmer, npc);
				}
				else
				{
					DialogueEvent.AddDialogue(npc, Trs.GR("Quest.QuizContest.CorrectAnswer"));
					QuizContestNextQuestion(farmer, npc, progress + 1);
				}
			}
			else
			{
				QuizContestFail(farmer, npc, progress);
			}
		});
	}

	private static void QuizContestSuccess(Farmer farmer, NPC npc)
	{
		Object item = ConversationUtility.Reward(3, npc, farmer);
		((Item)item).Quality = ((Mathf.RandomValue > 0.5f) ? 1 : ((Mathf.RandomValue > 0.5f) ? 2 : 4));
		DialogueEvent.AddDialogue(npc, Trs.GR("Quest.QuizContest.ChallengeSuccess"));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, npc, ((Item)item).ItemId, 1, ((Item)item).Quality);
		});
	}

	private static void QuizContestFail(Farmer farmer, NPC npc, int progress)
	{
		DialogueEvent.AddDialogue(npc, Trs.GR("Quest.QuizContest.ChallengeFailed"));
		if (progress > 1)
		{
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.QuizContest.ConsolationPrize"));
			Object item = ConversationUtility.Reward(1, npc, farmer);
			if (progress == 3)
			{
				((Item)item).Quality = 1;
			}
			DialogueEvent.AddAction((Action)delegate
			{
				ConversationUtility.DropItem(farmer, npc, ((Item)item).ItemId, 1, ((Item)item).Quality);
			});
		}
	}

	private static void AskLoan(Farmer farmer, ModSave saveData, NPC npc)
	{
		string text = Trs.GR("Quest.Loan");
		DialogueEvent.AddDialogue(npc, text);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		int num = ((Mathf.RandomValue < 0.5f) ? 100 : ((Mathf.RandomValue < 0.5f) ? 150 : 200));
		for (int i = 1; i <= 3; i++)
		{
			int p = num * i;
			dictionary[Trs.GR("Quest.Loan.Promise").Replace("{p}", p.ToString())] = delegate
			{
				AnswerLoan(farmer, saveData, npc, p);
			};
		}
		dictionary[Trs.GR("Quest.Loan.Reject")] = delegate
		{
			AnswerLoan(farmer, saveData, npc, 0);
		};
		ConversationUtility.DisplayOptions(farmer, npc, NpcContent.ToNpcSaidFormat(npc, text), dictionary);
	}

	private static void AnswerLoan(Farmer farmer, ModSave saveData, NPC npc, int price)
	{
		if (price == 0)
		{
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.Loan.Reject.Reaction"));
			return;
		}
		if (farmer.Money < price)
		{
			DialogueEvent.AddDialogue(npc, Trs.GR("Quest.Loan.Reject.NotEnoughMoney"));
			return;
		}
		saveData.GetData(farmer).npcData.AddArrears(npc, price);
		farmer.Money -= price;
		Game1.playSound("moneyDial", (int?)null);
		farmer.changeFriendship(price / 20, npc);
		DialogueEvent.AddDialogue(npc, Trs.GR("Quest.Loan.Promise.Reaction"));
		DialogueEvent.AddGlobalMessage(Trs.GR("Quest.Loan.Complete").Replace("{npc}", NpcContent.NPCDisplayName(npc)));
	}

	public static void LoanFollowUp(Farmer farmer, ModSave saveData, NPC npc)
	{
		int price = saveData.GetData(farmer).npcData.GetArrears(npc);
		DialogueEvent.AddDialogue(npc, Trs.GR("Quest.Loan.Promise.FollowUp").Replace("{p}", price.ToString()));
		DialogueEvent.AddAction((Action)delegate
		{
			Farmer obj = farmer;
			obj.Money += price;
			saveData.GetData(farmer).npcData.ClearArrears(npc);
		});
		int itemLevel = 1;
		if (price >= 250)
		{
			itemLevel = 2;
		}
		if (price >= 400)
		{
			itemLevel = 3;
		}
		Object item = ConversationUtility.Reward(itemLevel, npc, farmer);
		DialogueEvent.AddDialogue(npc, Trs.GR("Quest.Loan.Reward").Replace("{i}", ((Item)item).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, npc, ((Item)item).ItemId, 1, ((Item)item).Quality);
		});
		DialogueEvent.AddDialogue(npc, Trs.GR("Conversation.Greeting.Then"));
	}

	public static void OnHopeItemQuestTrigger(Farmer farmer, ModSave saveData, NPC npc, Quest quest)
	{
		Object sObjectFromItemID = StardewUtility.GetSObjectFromItemID(quest.content["item"]);
		string text = Trs.GR("Quest.HopeItem.Reaction").Replace("{i}", ((Item)sObjectFromItemID).DisplayName);
		DialogueEvent.AddDialogue(npc, text);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		dictionary[Trs.GR("Answer.Agree")] = delegate
		{
			OnHopeItemQuestAgree_QuestComplete(farmer, saveData, npc, quest);
		};
		dictionary[Trs.GR("Answer.Reject")] = delegate
		{
			OnHopeItemQuestReject_QuestComplete(farmer, saveData, npc, quest);
		};
		text = Trs.GR("Quest.HopeItem.Options").Replace("{i}", ((Item)sObjectFromItemID).DisplayName).Replace("{npc}", NpcContent.NPCDisplayName(npc));
		ConversationUtility.DisplayOptions(farmer, npc, text, dictionary);
	}

	private static void OnHopeItemQuestAgree_QuestComplete(Farmer farmer, ModSave saveData, NPC npc, Quest quest)
	{
		Game1.playSound("give_gift", (int?)null);
		Object reward = ConversationUtility.Reward(2, npc, farmer);
		DialogueEvent.AddDialogue(npc, Trs.GR("Quest.HopeItem.Reward").Replace("{i}", ((Item)reward).DisplayName));
		DialogueEvent.AddAction((Action)delegate
		{
			ConversationUtility.DropItem(farmer, npc, ((Item)reward).ItemId, 1, ((Item)reward).Quality);
			farmer.changeFriendship(20, npc);
			farmer.reduceActiveItemByOne();
			((Character)npc).doEmote(32, true);
		});
		DialogueEvent.AddGlobalMessage(Trs.GR("Quest.HopeItem.Agree.Complete", (Character)(object)npc).Replace("{npc}", NpcContent.NPCDisplayName(npc)));
		ConversationUtility.AddEventLog(farmer, saveData, EventType.QuestHopeItem, npc, ((Item)farmer.ActiveObject).ItemId);
		saveData.GetData(farmer).quests.SetQuestComplete(farmer, saveData, quest);
	}

	private static void OnHopeItemQuestReject_QuestComplete(Farmer farmer, ModSave saveData, NPC npc, Quest quest)
	{
		Game1.playSound("cancel", (int?)null);
		Object sObjectFromItemID = StardewUtility.GetSObjectFromItemID(quest.content["item"]);
		DialogueEvent.AddDialogue(npc, Trs.GR("Quest.HopeItem.Reject").Replace("{i}", ((Item)sObjectFromItemID).DisplayName));
		DialogueEvent.AddGlobalMessage(Trs.GR("Quest.HopeItem.Reject.Complete", (Character)(object)npc).Replace("{npc}", NpcContent.NPCDisplayName(npc)));
		ConversationUtility.AddEventLog(farmer, saveData, EventType.QuestHopeItemReject, npc, ((Item)farmer.ActiveObject).ItemId);
		saveData.GetData(farmer).quests.SetQuestEnd(quest);
	}
}

// ------------------------------------------------------------------
// From InquiringNPC.cs
// ------------------------------------------------------------------

public static class InquiringNPC
{
	internal class InquiringData
	{
		public NPC npc;

		public NPC talkWho;
	}

	internal class Taste
	{
		public GiftTaste taste;

		public Taste(GiftTaste t)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			taste = t;
		}
	}

	private static float _talkTasteChance = 0.3f;

	private static float _askCommentChance = 0.7f;

	private static float _spreadCommentChance = 0.6f;

	private static float _tasteItemTradeChance = 0.4f;

	private static Dictionary<Farmer, InquiringData> _inquiringData = new Dictionary<Farmer, InquiringData>();

	private static float GetSkillTasteItemTradeChance(Farmer farmer)
	{
		float num = ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		float num2 = (1f - _tasteItemTradeChance) * num;
		return _tasteItemTradeChance + num2;
	}

	private static float GetSkillPositiveSpreadChance(Farmer farmer)
	{
		float num = ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		float num2 = (1f - _spreadCommentChance) * num;
		return _spreadCommentChance + num2;
	}

	private static float GetSkillNegativeSpreadChance(Farmer farmer)
	{
		float num = 1f - ConversationSkill.BonusPerLevel * (float)ConversationSkill.GetLevel(farmer) / 100f;
		return _spreadCommentChance * num;
	}

	private static InquiringData GetTradeData(Farmer farmer)
	{
		if (!_inquiringData.ContainsKey(farmer))
		{
			_inquiringData[farmer] = new InquiringData();
		}
		return _inquiringData[farmer];
	}

	public static void StartInquiring(Farmer farmer, ModSave saveData, NPC npc, NPC talkWho)
	{
		InquiringData tradeData = GetTradeData(farmer);
		tradeData.npc = npc;
		tradeData.talkWho = talkWho;
		if (Mathf.RandomValue < _talkTasteChance)
		{
			TalkAboutTaste(farmer, saveData);
		}
		else
		{
			TalkAboutInformation(farmer, saveData);
		}
	}

	private static void TalkAboutInformation(Farmer farmer, ModSave saveData)
	{
		InquiringData tradeData = GetTradeData(farmer);
		DialogueEvent.AddDialogue(tradeData.npc, NpcContent.GetTalkAboutDialogue(tradeData.npc, tradeData.talkWho));
		if (Mathf.RandomValue < _askCommentChance)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			DisplayCommentOptions(farmer, saveData);
		}
	}

	private static void DisplayCommentOptions(Farmer farmer, ModSave saveData)
	{
		InquiringData tradeData = GetTradeData(farmer);
		string text = Trs.GR("Inquiring.Comment.Ask", (Character)(object)tradeData.talkWho).Replace("{npc}", NpcContent.NPCDisplayName(tradeData.talkWho, tradeData.npc));
		DialogueEvent.AddDialogue(tradeData.npc, text);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		List<KeyValuePair<string, Action>> list = new List<KeyValuePair<string, Action>>();
		string option_Tactful = Trs.GR("Inquiring.Comment.Tactful", (Character)(object)tradeData.talkWho).Replace("{npc}", NpcContent.NPCDisplayName(tradeData.talkWho));
		list.Add(new KeyValuePair<string, Action>(option_Tactful, delegate
		{
			DoNpcCommentTactful(option_Tactful);
		}));
		bool flag = Mathf.RandomValue < 0.5f;
		List<string> list2 = Trs.GRList("Inquiring.Comment.Praise", flag ? 1 : 2);
		for (int num = 0; num < list2.Count; num++)
		{
			string option_Praise = list2[num];
			option_Praise = GameTextContent.TextGenderHandle(option_Praise.Replace("{npc}", NpcContent.NPCDisplayName(tradeData.talkWho)), (Character)(object)tradeData.talkWho);
			list.Add(new KeyValuePair<string, Action>(option_Praise, delegate
			{
				DoNpcCommentPraise(farmer, saveData, option_Praise);
			}));
		}
		List<string> list3 = Trs.GRList("Inquiring.Comment.Insult", (!flag) ? 1 : 2);
		for (int num2 = 0; num2 < list3.Count; num2++)
		{
			string option_Insult = list3[num2];
			option_Insult = GameTextContent.TextGenderHandle(option_Insult.Replace("{npc}", NpcContent.NPCDisplayName(tradeData.talkWho)), (Character)(object)tradeData.talkWho);
			list.Add(new KeyValuePair<string, Action>(option_Insult, delegate
			{
				DoNpcCommentInsult(farmer, saveData, option_Insult);
			}));
		}
		while (list.Count > 0)
		{
			KeyValuePair<string, Action> randomOneAndRemove = ListExpansion.GetRandomOneAndRemove<KeyValuePair<string, Action>>(list);
			dictionary[randomOneAndRemove.Key] = randomOneAndRemove.Value;
		}
		string msg = NpcContent.ToNpcSaidFormat(tradeData.npc, text);
		ConversationUtility.DisplayOptions(farmer, tradeData.npc, msg, dictionary);
	}

	private static void DoNpcCommentTactful(string dialogue)
	{
	}

	private static void DoNpcCommentPraise(Farmer farmer, ModSave saveData, string dialogue)
	{
		InquiringData tradeData = GetTradeData(farmer);
		Game1.playSound("give_gift", (int?)null);
		string text = Trs.GR("Inquiring.Comment.Praise.Reaction", (Character)(object)tradeData.talkWho).Replace("{npc}", NpcContent.NPCDisplayName(tradeData.talkWho, tradeData.npc));
		DialogueEvent.AddDialogue(tradeData.npc, text);
		ConversationSkill.AddExp(farmer, 10, ConversationUtility.config);
		if (Mathf.RandomValue < GetSkillPositiveSpreadChance(farmer))
		{
			DialogueEvent.AddGlobalMessage(Trs.GR("Inquiring.Comment.Praise.FollowUp").Replace("{npc}", NpcContent.NPCDisplayName(tradeData.npc)).Replace("{npc1}", NpcContent.NPCDisplayName(tradeData.talkWho)));
			DialogueEvent.AddAction((Action)delegate
			{
				Game1.playSound("give_gift", (int?)null);
			});
			float num = 10f;
			if (ConversationSkill.HasCustomProfession(farmer, ConversationSkill.lv5B))
			{
				num += num * 1.5f;
			}
			farmer.changeFriendship((int)num, tradeData.talkWho);
			ConversationUtility.AddEventLog(farmer, saveData, EventType.CommentPraise, tradeData.talkWho, "", dialogue);
		}
	}

	private static void DoNpcCommentInsult(Farmer farmer, ModSave saveData, string dialogue)
	{
		InquiringData tradeData = GetTradeData(farmer);
		Game1.playSound("slimedead", (int?)null);
		ConversationSkill.AddExp(farmer, 24, ConversationUtility.config);
		string text = Trs.GR("Inquiring.Comment.Insult.Reaction", (Character)(object)tradeData.talkWho).Replace("{npc}", NpcContent.NPCDisplayName(tradeData.talkWho, tradeData.npc));
		DialogueEvent.AddDialogue(tradeData.npc, text);
		if (Mathf.RandomValue < GetSkillNegativeSpreadChance(farmer))
		{
			DialogueEvent.AddGlobalMessage(Trs.GR("Inquiring.Comment.Insult.FollowUp").Replace("{npc}", NpcContent.NPCDisplayName(tradeData.npc)).Replace("{npc1}", NpcContent.NPCDisplayName(tradeData.talkWho)));
			DialogueEvent.AddAction((Action)delegate
			{
				Game1.playSound("cancel", (int?)null);
			});
			farmer.changeFriendship(-10, tradeData.talkWho);
			ConversationUtility.AddEventLog(farmer, saveData, EventType.CommentInsult, tradeData.talkWho, "", dialogue);
		}
	}

	private static void TalkAboutTaste(Farmer farmer, ModSave saveData)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		InquiringData tradeData = GetTradeData(farmer);
		WeightDice<Taste> val = new WeightDice<Taste>();
		val.Add(new Taste((GiftTaste)0), 1f);
		val.Add(new Taste((GiftTaste)1), 2f);
		val.Add(new Taste((GiftTaste)2), 3f);
		val.Add(new Taste((GiftTaste)3), 2f);
		val.Add(new Taste((GiftTaste)4), 2f);
		List<string> list = new List<string>();
		GiftTaste val2 = (GiftTaste)2;
		while (val.Count > 0)
		{
			val2 = val.CastAndRemove().taste;
			list.AddRange(StardewUtility.GetItemIDListFromTaste(tradeData.talkWho, val2));
			if (list.Count > 0)
			{
				break;
			}
		}
		if (list.Count > 0)
		{
			string randomOne = ListExpansion.GetRandomOne<string>(list);
			Object sObjectFromItemID = StardewUtility.GetSObjectFromItemID(randomOne);
			string text = Trs.GR($"Inquiring.Taste.{val2}").Replace("{i}", ((Item)sObjectFromItemID).DisplayName).Replace("{npc}", NpcContent.NPCDisplayName(tradeData.talkWho, tradeData.npc));
			DialogueEvent.AddDialogue(tradeData.npc, GameTextContent.TextGenderHandle(text, (Character)(object)tradeData.talkWho));
			if (Mathf.RandomValue < GetSkillTasteItemTradeChance(farmer))
			{
				sObjectFromItemID = ConversationUtility.UpgradeRewardQuality(sObjectFromItemID, tradeData.npc, farmer);
				DialogueEvent.AddDialogue(tradeData.npc, Trs.GR("Inquiring.HoldItem").Replace("{i}", sObjectFromItemID.displayName));
				TradeBuy.StartTrade(farmer, saveData, tradeData.npc, sObjectFromItemID);
			}
		}
	}
}
