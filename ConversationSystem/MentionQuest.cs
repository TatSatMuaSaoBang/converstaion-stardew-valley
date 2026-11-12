using System;
using System.Collections.Generic;
using Netcode;
using SorryLab;
using SorryLab.Expansion;
using StardewValley;
using StardewValley.Network;

namespace ConversationSystem;

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
