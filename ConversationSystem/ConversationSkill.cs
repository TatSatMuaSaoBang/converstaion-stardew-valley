using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SorryLab;
using SorryLab.Expansion;
using SpaceCore;
using StardewModdingAPI;
using StardewValley;

namespace ConversationSystem;

public class ConversationSkill : Skill
{
	public static ConversationProfession lv5A;

	public static ConversationProfession lv5B;

	public static ConversationProfession lv10A1;

	public static ConversationProfession lv10A2;

	public static ConversationProfession lv10B1;

	public static ConversationProfession lv10B2;

	private const float Bonus_Per_Level = 5f;

	private const float StaminaHeal_Per_Level = 2f;

	private const string Skill_ID = "yunhikari.Conversation";

	private static ConversationSkill _instance;

	public static int[] experienceCurve { get; private set; }

	public static string SkillID => "yunhikari.Conversation";

	public static float BonusPerLevel => 5f;

	public static float StaminaHealPerLevel => 2f;

	public ConversationSkill(IModHelper helper)
		: base("yunhikari.Conversation")
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		lv5A = new ConversationProfession(helper, (Skill)(object)this, "5A");
		lv5B = new ConversationProfession(helper, (Skill)(object)this, "5B");
		((Skill)this).Professions.Add((Profession)(object)lv5A);
		((Skill)this).Professions.Add((Profession)(object)lv5B);
		((Skill)this).ProfessionsForLevels.Add(new ProfessionPair(5, (Profession)(object)lv5A, (Profession)(object)lv5B, (Profession)null));
		lv10A1 = new ConversationProfession(helper, (Skill)(object)this, "10A1");
		lv10A2 = new ConversationProfession(helper, (Skill)(object)this, "10A2");
		((Skill)this).Professions.Add((Profession)(object)lv10A1);
		((Skill)this).Professions.Add((Profession)(object)lv10A2);
		((Skill)this).ProfessionsForLevels.Add(new ProfessionPair(10, (Profession)(object)lv10A1, (Profession)(object)lv10A2, (Profession)(object)lv5A));
		lv10B1 = new ConversationProfession(helper, (Skill)(object)this, "10B1");
		lv10B2 = new ConversationProfession(helper, (Skill)(object)this, "10B2");
		((Skill)this).Professions.Add((Profession)(object)lv10B1);
		((Skill)this).Professions.Add((Profession)(object)lv10B2);
		((Skill)this).ProfessionsForLevels.Add(new ProfessionPair(10, (Profession)(object)lv10B1, (Profession)(object)lv10B2, (Profession)(object)lv5B));
		((Skill)this).Icon = helper.ModContent.Load<Texture2D>("assets/iconA.png");
		((Skill)this).SkillsPageIcon = helper.ModContent.Load<Texture2D>("assets/iconB.png");
		int[] obj = new int[10] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 14000 };
		experienceCurve = obj;
		((Skill)this).ExperienceCurve = obj;
		((Skill)this).ExperienceBarColor = new Color(196, 76, 255);
		_instance = this;
	}

	public static void ResetProfession(Farmer farmer)
	{
		foreach (ConversationProfession profession in ((Skill)_instance).Professions)
		{
			((NetHashSet<int>)(object)farmer.professions).Remove(((Profession)profession).GetVanillaId());
		}
		int level = GetLevel(farmer);
		int experienceFor = Skills.GetExperienceFor(farmer, "yunhikari.Conversation");
		Skills.AddExperience(farmer, "yunhikari.Conversation", -experienceFor);
		Skills.AddExperience(farmer, "yunhikari.Conversation", experienceFor);
	}

	public override string GetName()
	{
		return Trs.GR("Skill.Name");
	}

	public override List<string> GetExtraLevelUpInfo(int level)
	{
		List<string> list = new List<string>
		{
			Trs.GR("Skill.Perk.Negotiate").Replace("{n}", $"{5f}"),
			Trs.GR("Skill.Perk.StaminaHeal").Replace("{n}", $"{2f}")
		};
		if (level == 4)
		{
			list.Add(Trs.GR("Skill.Perk.TradeBuy").Replace("{n}", "2"));
			list.Add(Trs.GR("Skill.Perk.TradeSell").Replace("{n}", "2"));
		}
		if (level == 8)
		{
			list.Add(Trs.GR("Skill.Perk.Spouse"));
			list.Add(Trs.GR("Skill.Perk.TradeBuy").Replace("{n}", "3"));
			list.Add(Trs.GR("Skill.Perk.TradeSell").Replace("{n}", "3"));
		}
		if (level == 9)
		{
			list.Add(Trs.GR("Skill.Perk.TradeBuy").Replace("{n}", "4"));
			list.Add(Trs.GR("Skill.Perk.TradeSell").Replace("{n}", "4"));
		}
		return list;
	}

	public override string GetSkillPageHoverText(int level)
	{
		string value = Trs.GR("Skill.Perk.Negotiate").Replace("{n}", $"{5f * (float)level}");
		string value2 = Trs.GR("Skill.Perk.StaminaHeal").Replace("{n}", $"{2f * (float)level}");
		string value3 = Trs.GR("Skill.Perk.TradeBuy").Replace("{n}", MaxTradeItemStack(Game1.player).ToString());
		string value4 = Trs.GR("Skill.Perk.TradeSell").Replace("{n}", MaxTradeItemStack(Game1.player).ToString());
		string value5 = ((level >= 8) ? ("\n" + Trs.GR("Skill.Perk.Spouse")) : "");
		return $"{value}\n{value2}\n{value3}\n{value4}{value5}";
	}

	public static int MaxTradeItemStack(Farmer farmer)
	{
		int level = GetLevel(farmer);
		if (level >= 9)
		{
			return 4;
		}
		if (level >= 8)
		{
			return 3;
		}
		if (level >= 4)
		{
			return 2;
		}
		return 1;
	}

	public static void AddExp(Farmer who, int exp, ModConfig config = null)
	{
		int addExp = exp;
		if (config != null)
		{
			addExp += (int)((float)exp * (config.ExpMultiple - 1f));
		}
		ShowExp(who, addExp);
		DialogueEvent.AddAction_Secondary((Action)delegate
		{
			int skillLevel = Skills.GetSkillLevel(Game1.GetPlayer(who.UniqueMultiplayerID, false), "yunhikari.Conversation");
			Skills.AddExperience(Game1.GetPlayer(who.UniqueMultiplayerID, false), "yunhikari.Conversation", addExp);
			int skillLevel2 = Skills.GetSkillLevel(Game1.GetPlayer(who.UniqueMultiplayerID, false), "yunhikari.Conversation");
			if (skillLevel != skillLevel2)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:NewIdeas"));
			}
		});
	}

	public static void ShowExp(Farmer who, int addExp = 0)
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		if (who != Game1.player || GetLevel(who) >= 10)
		{
			return;
		}
		int num = GetCurrentExp(who) - GetPrevLevelRequireExp(who);
		int num2 = GetNextLevelRequireExp(who) - GetPrevLevelRequireExp(who);
		float progress = FloatExpansion.Round((float)(num + addExp) / (float)num2 * 100f, 1);
		if (addExp > 0)
		{
			bool isLevelUP = num + addExp >= num2;
			SpriteRenderer.DrawSparklingText($"Exp+{addExp}", (Character)(object)who, 1.5f, Color.LimeGreen, Color.Azure).onDisplayEnd = delegate
			{
				if (isLevelUP)
				{
					SpriteRenderer.DrawSparklingText("Level Up!", (Character)(object)who, 1.5f);
				}
				else
				{
					SpriteRenderer.DrawSparklingText($"({progress}%)", (Character)(object)who, 1.5f);
				}
			};
		}
		else
		{
			SpriteRenderer.DrawSparklingText($"({progress}%)", (Character)(object)who, 1.5f);
		}
	}

	public static int GetLevel(Farmer who)
	{
		Farmer player = Game1.GetPlayer(who.UniqueMultiplayerID, false);
		return Skills.GetSkillLevel(player, "yunhikari.Conversation") + Skills.GetSkillBuffLevel(player, "yunhikari.Conversation", (string)null);
	}

	public static int GetCurrentExp(Farmer who)
	{
		return Skills.GetExperienceFor(who, "yunhikari.Conversation");
	}

	public static int GetNextLevelRequireExp(Farmer who)
	{
		int currentExp = GetCurrentExp(who);
		for (int i = 0; i < experienceCurve.Length; i++)
		{
			int num = experienceCurve[i];
			if (num > currentExp)
			{
				return num;
			}
		}
		return 0;
	}

	public static int GetPrevLevelRequireExp(Farmer who)
	{
		int currentExp = GetCurrentExp(who);
		for (int i = 0; i < experienceCurve.Length; i++)
		{
			int num = experienceCurve[i];
			if (num >= currentExp)
			{
				return (i != 0) ? experienceCurve[i - 1] : 0;
			}
		}
		return experienceCurve[experienceCurve.Length - 1];
	}

	public static bool HasCustomProfession(Farmer who, ConversationProfession profession)
	{
		Farmer player = Game1.GetPlayer(who.UniqueMultiplayerID, false);
		return SkillExtensions.HasCustomProfession(player, (Profession)(object)profession);
	}
}
