using System;
using SpaceCore;
using SpaceCore.Interface;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;

namespace ConversationSystem;

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
