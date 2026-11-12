// ============================================================================
// ConversationStyle.cs
// Conversation skill & presentation style: skill levels/professions, skill events, in-game menus/UI, GMCM config options, and ModConfig.
//
// Compacted from: ConversationSkill.cs, ConversationSkillEvents.cs, ConversationProfession.cs, ExtensionMenu.cs, GMCMHandle.cs, ModConfig.cs
// ============================================================================

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Netcode;
using SorryLab.Expansion;
using SorryLab;
using SpaceCore.Interface;
using SpaceCore;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ConversationSystem;

// ------------------------------------------------------------------
// From ConversationSkill.cs
// ------------------------------------------------------------------

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

// ------------------------------------------------------------------
// From ConversationSkillEvents.cs
// ------------------------------------------------------------------

public static class ConversationSkillEvents
{
	public static IModHelper entryHelper;

	public static void Setup(IModEvents modEvents, IModHelper helper)
	{
		entryHelper = helper;
		modEvents.GameLoop.GameLaunched += GameLaunched;
		modEvents.GameLoop.DayStarted += DayStarted;
		modEvents.Display.MenuChanged += MenuChanged;
	}

	private static void GameLaunched(object sender, GameLaunchedEventArgs e)
	{
		Skills.RegisterSkill((Skill)(object)new ConversationSkill(entryHelper));
	}

	private unsafe static void DayStarted(object sender, DayStartedEventArgs e)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!Game1.IsMasterGame)
		{
			return;
		}
		Enumerator enumerator = Game1.getOnlineFarmers().GetEnumerator();
		try
		{
			while (((Enumerator)(ref enumerator)).MoveNext())
			{
				Farmer current = ((Enumerator)(ref enumerator)).Current;
				Farmer player = Game1.GetPlayer(current.UniqueMultiplayerID, false);
				if (player.isActive() && !SkillExtensions.HasCustomProfession(player, (Profession)(object)ConversationSkill.lv5A))
				{
				}
			}
		}
		finally
		{
			((IDisposable)(*(Enumerator*)(&enumerator))/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static void MenuChanged(object sender, MenuChangedEventArgs e)
	{
		IClickableMenu newMenu = e.NewMenu;
		SkillLevelUpMenu val = (SkillLevelUpMenu)(object)((newMenu is SkillLevelUpMenu) ? newMenu : null);
		if (val != null)
		{
			string value = entryHelper.Reflection.GetField<string>((object)val, "currentSkill", true).GetValue();
			if (!(value != ConversationSkill.SkillID))
			{
				Game1.playSound("newRecord", (int?)null);
				int value2 = entryHelper.Reflection.GetField<int>((object)val, "currentLevel", true).GetValue();
				int num = 0;
				((IClickableMenu)val).height = num + 256 + val.getExtraInfoForLevel(value, value2).Count * 64 * 3 / 4;
			}
		}
	}

	private static void SaveLoaded(object sender, SaveLoadedEventArgs e)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		int customSkillLevel = SkillExtensions.GetCustomSkillLevel(Game1.player, ConversationSkill.SkillID);
		if (customSkillLevel != 0)
		{
			if (customSkillLevel >= 5 && !SkillExtensions.HasCustomProfession(Game1.player, (Profession)(object)ConversationSkill.lv5A) && !SkillExtensions.HasCustomProfession(Game1.player, (Profession)(object)ConversationSkill.lv5B))
			{
				Game1.endOfNightMenus.Push((IClickableMenu)new SkillLevelUpMenu(ConversationSkill.SkillID, 5));
			}
			if (customSkillLevel >= 10 && !SkillExtensions.HasCustomProfession(Game1.player, (Profession)(object)ConversationSkill.lv10A1) && !SkillExtensions.HasCustomProfession(Game1.player, (Profession)(object)ConversationSkill.lv10A2) && !SkillExtensions.HasCustomProfession(Game1.player, (Profession)(object)ConversationSkill.lv10B1) && !SkillExtensions.HasCustomProfession(Game1.player, (Profession)(object)ConversationSkill.lv10B2))
			{
				Game1.endOfNightMenus.Push((IClickableMenu)new SkillLevelUpMenu(ConversationSkill.SkillID, 10));
			}
		}
	}
}

// ------------------------------------------------------------------
// From ConversationProfession.cs
// ------------------------------------------------------------------

public class ConversationProfession : Profession
{
	private string id;

	public ConversationProfession(IModHelper helper, Skill skill, string id)
		: base(skill, id)
	{
		((Profession)this).Icon = helper.ModContent.Load<Texture2D>("assets/" + id + ".png");
		this.id = id;
	}

	public override string GetName()
	{
		return Trs.GR("Skill." + id + ".Name");
	}

	public override string GetDescription()
	{
		return Trs.GR("Skill." + id + ".Desc");
	}
}

// ------------------------------------------------------------------
// From ExtensionMenu.cs
// ------------------------------------------------------------------

public class ExtensionMenu
{
	public Func<ModSave> saveData;

	private static Dictionary<Farmer, bool> _todayResetProfession = new Dictionary<Farmer, bool>();

	public ExtensionMenu(Func<ModSave> save)
	{
		saveData = save;
	}

	public static void OnDayStarted()
	{
		_todayResetProfession.Clear();
	}

	private static bool GetTodayResetProfession(Farmer farmer)
	{
		if (!_todayResetProfession.ContainsKey(farmer))
		{
			_todayResetProfession[farmer] = false;
		}
		return _todayResetProfession[farmer];
	}

	private static void SetTodayResetProfession(Farmer farmer, bool value)
	{
		_todayResetProfession[farmer] = value;
	}

	public void DisplayMenu(Farmer farmer)
	{
		Game1.playSound("coin", (int?)null);
		string msg = Trs.GR("ExtensionMenu.Text");
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		if (saveData().GetData(farmer).quests.Count > 0)
		{
			dictionary[Trs.GR("ExtensionMenu.Option.Quest")] = delegate
			{
				DisplayQuests(farmer);
			};
		}
		if (IsAnyNpcBirthday(farmer, out var _) && ConversationUtility.config.BirthdayOption)
		{
			dictionary[Trs.GR("ExtensionMenu.Option.Birthday")] = delegate
			{
				DisplayBirthdayNPCs(farmer);
			};
		}
		if (IsAnyNpcArrears(farmer) && ConversationUtility.config.ArrearsOption)
		{
			dictionary[Trs.GR("ExtensionMenu.Option.Arrears")] = delegate
			{
				DisplayArrears(farmer);
			};
		}
		if (CanResetProfession(farmer) && ConversationUtility.config.ResetSkillOption)
		{
			dictionary[Trs.GR("ExtensionMenu.Option.ResetProfession")] = delegate
			{
				ResetProfession(farmer);
			};
		}
		dictionary[Trs.GR("ExtensionMenu.Option.Tips")] = delegate
		{
			DisplayTips(farmer);
		};
		ConversationUtility.DisplayOptions(farmer, null, msg, dictionary);
	}

	private bool CanResetProfession(Farmer farmer)
	{
		if (GetTodayResetProfession(farmer))
		{
			return false;
		}
		if (ConversationSkill.GetLevel(farmer) < 5)
		{
			return false;
		}
		return true;
	}

	private void ResetProfession(Farmer farmer)
	{
		SetTodayResetProfession(farmer, value: true);
		ConversationSkill.ResetProfession(farmer);
		DialogueEvent.AddGlobalMessage(Trs.GR("ExtensionMenu.Option.ResetProfession.Response"));
	}

	private bool IsAnyNpcBirthday(Farmer farmer, out List<NPC> NPCs)
	{
		NPCs = new List<NPC>();
		Friendship val = default(Friendship);
		foreach (NPC item in StardewUtility.GetAllNPC((IEnumerable<string>)null))
		{
			if (item.CanReceiveGifts() && !(item is Child) && ((Character)item).IsVillager && item.isBirthday() && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(((Character)item).Name, ref val) && val.GiftsToday == 0)
			{
				NPCs.Add(item);
			}
		}
		return NPCs.Count > 0;
	}

	private bool IsAnyNpcArrears(Farmer farmer)
	{
		return saveData().GetData(farmer).npcData.GetAllNpcArrears().Count > 0;
	}

	private void DisplayTips(Farmer farmer)
	{
		DialogueEvent.AddObjectDialogue(Trs.GR("ExtensionMenu.Option.Tips.Text"));
		ConversationSkill.ShowExp(farmer);
	}

	private void DisplayArrears(Farmer farmer)
	{
		Dictionary<string, int> allNpcArrears = saveData().GetData(farmer).npcData.GetAllNpcArrears();
		List<string> list = allNpcArrears.Keys.ToList();
		string text = "";
		NPC val = default(NPC);
		for (int i = 0; i < list.Count; i++)
		{
			string text2 = list[i];
			if (StardewUtility.TryGetNPCFromName(text2, ref val))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += "\n";
				}
				text += Trs.GR("ExtensionMenu.Option.Arrears.Message").Replace("{npc}", ((Character)val).displayName).Replace("{value}", allNpcArrears[text2].ToString());
			}
		}
		DialogueEvent.AddGlobalMessage(text);
	}

	private void DisplayQuests(Farmer farmer)
	{
		string text = "";
		foreach (Quest todayQuest in saveData().GetData(farmer).quests.todayQuests)
		{
			if (text != "")
			{
				text += "\n\n";
			}
			text = text + ">" + todayQuest.QuestGuideText() + todayQuest.QuestNpcLocationText();
		}
		DialogueEvent.AddGlobalMessage(text);
	}

	private void DisplayBirthdayNPCs(Farmer farmer)
	{
		string text = "";
		IsAnyNpcBirthday(farmer, out var NPCs);
		foreach (NPC item in NPCs)
		{
			if (text != "")
			{
				text += "\n\n";
			}
			string text2 = Trs.GR("ExtensionMenu.Option.Birthday.Text", (Character)(object)item).Replace("{npc}", NpcContent.NPCDisplayName(item));
			if (((Character)item).currentLocation != null)
			{
				string text3 = Trs.GR("Misk.NPC.Location", (Character)(object)item).Replace("{npc}", NpcContent.NPCDisplayName(item));
				text3 = text3.Replace("{location}", ((Character)item).currentLocation.DisplayName);
				text2 += text3;
			}
			text = text + ">" + text2;
		}
		DialogueEvent.AddGlobalMessage(text);
	}
}

// ------------------------------------------------------------------
// From GMCMHandle.cs
// ------------------------------------------------------------------

internal static class GMCMHandle
{
	public static void Handle(IGenericModConfigMenuApi api, ThisMod mod)
	{
		api.Register(((Mod)mod).ModManifest, (Action)delegate
		{
			mod.config = new ModConfig();
		}, (Action)delegate
		{
			((Mod)mod).Helper.WriteConfig<ModConfig>(mod.config);
		}, false);
		api.AddNumberOption(((Mod)mod).ModManifest, (Func<float>)(() => mod.config.ExpMultiple), (Action<float>)delegate(float value)
		{
			mod.config.ExpMultiple = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ExpMultiple.Name")), (Func<string>)(() => Trs.Get("GMCM.ExpMultiple.Desc")), (float?)1f, (float?)10f, (float?)0.5f, (Func<float, string>)null, (string)null);
		api.AddNumberOption(((Mod)mod).ModManifest, (Func<float>)(() => mod.config.SellPriceMultiple), (Action<float>)delegate(float value)
		{
			mod.config.SellPriceMultiple = value;
		}, (Func<string>)(() => Trs.Get("GMCM.SellPrice.Name")), (Func<string>)(() => Trs.Get("GMCM.SellPrice.Desc")), (float?)1.2f, (float?)2f, (float?)0.05f, (Func<float, string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.DisplayFriendShip), (Action<bool>)delegate(bool value)
		{
			mod.config.DisplayFriendShip = value;
		}, (Func<string>)(() => Trs.Get("GMCM.DisplayFriendShip.Name")), (Func<string>)(() => Trs.Get("GMCM.DisplayFriendShip.Desc")), (string)null);
		api.AddKeybind(((Mod)mod).ModManifest, (Func<SButton>)(() => mod.config.ActionKey), (Action<SButton>)delegate(SButton value)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			mod.config.ActionKey = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ActionKey.Name")), (Func<string>)(() => Trs.Get("GMCM.ActionKey.Desc")), (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.BirthdayOption), (Action<bool>)delegate(bool value)
		{
			mod.config.BirthdayOption = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ShowBitrhday.Name")), (Func<string>)(() => Trs.Get("GMCM.ShowBitrhday.Desc")), (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.ArrearsOption), (Action<bool>)delegate(bool value)
		{
			mod.config.ArrearsOption = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ShowArrears.Name")), (Func<string>)(() => Trs.Get("GMCM.ShowArrears.Desc")), (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.ResetSkillOption), (Action<bool>)delegate(bool value)
		{
			mod.config.ResetSkillOption = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ShowResetSkill.Name")), (Func<string>)(() => Trs.Get("GMCM.ShowResetSkill.Desc")), (string)null);
		api.AddSectionTitle(((Mod)mod).ModManifest, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.Section")), (Func<string>)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_FindingItems), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_FindingItems = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.FindingItems")), (Func<string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_PraiseSomeone), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_PraiseSomeone = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.PraiseSomeone")), (Func<string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_InsultSomeone), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_InsultSomeone = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.InsultSomeone")), (Func<string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_QuizGame), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_QuizGame = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.QuizGame")), (Func<string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_Loan), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_Loan = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.Loan")), (Func<string>)null, (string)null);
	}
}

// ------------------------------------------------------------------
// From ModConfig.cs
// ------------------------------------------------------------------

public sealed class ModConfig
{
	public bool DisplayFriendShip = false;

	public float ExpMultiple = 1f;

	public float SellPriceMultiple = 1.2f;

	public float SellPriceMultipleExpensive = 3f;

	public SButton ActionKey = (SButton)81;

	public bool ArrearsOption = true;

	public bool BirthdayOption = true;

	public bool ResetSkillOption = false;

	public bool questEnable_FindingItems = true;

	public bool questEnable_PraiseSomeone = true;

	public bool questEnable_InsultSomeone = true;

	public bool questEnable_QuizGame = true;

	public bool questEnable_Loan = true;

	public int GetEnableQuestCount()
	{
		int num = 0;
		if (questEnable_FindingItems)
		{
			num++;
		}
		if (questEnable_PraiseSomeone)
		{
			num++;
		}
		if (questEnable_InsultSomeone)
		{
			num++;
		}
		if (questEnable_QuizGame)
		{
			num++;
		}
		if (questEnable_Loan)
		{
			num++;
		}
		return num;
	}
}
