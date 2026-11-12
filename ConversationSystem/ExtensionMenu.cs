using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using SorryLab;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Network;

namespace ConversationSystem;

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
