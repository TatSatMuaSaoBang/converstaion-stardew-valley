using System;
using System.Collections.Generic;
using SorryLab;
using SorryLab.Expansion;
using StardewValley;

namespace ConversationSystem;

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
