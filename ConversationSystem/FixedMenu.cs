using System;
using System.Collections.Generic;
using SorryLab;
using SorryLab.Expansion;
using StardewValley;

namespace ConversationSystem;

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
