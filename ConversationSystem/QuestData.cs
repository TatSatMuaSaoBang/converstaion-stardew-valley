using System;
using System.Collections.Generic;
using SorryLab;
using SorryLab.Expansion;
using StardewValley;

namespace ConversationSystem;

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
