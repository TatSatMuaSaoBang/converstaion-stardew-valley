using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using SorryLab;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Network;

namespace ConversationSystem;

public sealed class Processing
{
	internal class Question
	{
		public NPC npc;

		public string text;

		public Dictionary<string, string> options = new Dictionary<string, string>();

		public Question(NPC npc, string text, Dictionary<string, string> options)
		{
			this.npc = npc;
			this.text = text;
			this.options = options;
		}
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

	private Func<ModSave> saveData;

	private ModConfig config;

	private List<string> acceptLocations = new List<string>
	{
		"Farm", "Town", "JoshHouse", "HaleyHouse", "SamHouse", "Blacksmith", "ManorHouse", "SeedShop", "Saloon", "Trailer",
		"Hospital", "Beach", "ElliottHouse", "Mountain", "ScienceHouse", "Tent", "Forest", "WizardHouse", "AnimalShop", "LeahHouse",
		"BusStop", "Mine", "Sewer", "Desert", "ArchaeologyHouse", "WizardHouseBasement", "AdventureGuild", "Woods", "Railroad", "FishShop",
		"BathHouse_Entry", "CommunityCenter", "Backwoods", "Tunnel", "Trailer_Big"
	};

	private NPC _enterRangeNPC;

	private ExtensionMenu _extensionMenu;

	private float pushButtonTime = 0f;

	private float _lastLeftShoulderTime = 0f;

	private float _lastRightShoulderTime = 0f;

	public Processing(Func<ModSave> getData, ModConfig config)
	{
		saveData = getData;
		this.config = config;
		ConversationUtility.Setup(config, getData);
		_extensionMenu = new ExtensionMenu(() => saveData());
		NpcContent.Setup();
	}

	public void SetupEvent(IModEvents modEvents)
	{
		modEvents.GameLoop.DayStarted += OnDayStarted;
		ModEvent.AddUpdateTickedListener((Action)OnUpdateTicked, 100, 0f);
		modEvents.Input.ButtonPressed += OnButtonPressed;
		EventPatcher.onAfterToolIndexChange = (Action<EventArgsToolIndexChange>)Delegate.Combine(EventPatcher.onAfterToolIndexChange, new Action<EventArgsToolIndexChange>(OnToolIndexChange));
	}

	private void OnDayStarted(object sender, DayStartedEventArgs e)
	{
		ConversationUtility.OnDayStarted();
		NpcContent.CheckLeoMovedAndHandle();
		FamilyGifts.OnDayStarted();
		ExtensionMenu.OnDayStarted();
		Farmer player = Game1.player;
		saveData().GetData(player).quests.OnDayStarted();
		saveData().GetData(player).log.OnDayStarted();
		saveData().GetData(player).npcData.OnDayStarted();
	}

	private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Invalid comparison between Unknown and I4
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Invalid comparison between Unknown and I4
		if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
		{
			return;
		}
		Farmer player = Game1.player;
		if (!player.canMove || player.UsingTool)
		{
			return;
		}
		if (e.Button == config.ActionKey)
		{
			if (Time.currentGameTime - pushButtonTime < 0.5f)
			{
				_extensionMenu.DisplayMenu(player);
			}
			else
			{
				UseTalkingSkill(player);
			}
			pushButtonTime = Time.currentGameTime;
		}
		if ((int)e.Button == 2256)
		{
			_lastLeftShoulderTime = Time.currentGameTime;
			if (Time.currentGameTime - _lastRightShoulderTime < 0.33f)
			{
				_extensionMenu.DisplayMenu(player);
			}
		}
		if ((int)e.Button == 2512)
		{
			_lastRightShoulderTime = Time.currentGameTime;
			if (Time.currentGameTime - _lastLeftShoulderTime < 0.33f)
			{
				UseTalkingSkill(player);
			}
		}
	}

	private NPC GetNearNpc(Farmer farmer)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		NPC val = null;
		float num = float.MaxValue;
		foreach (NPC character in Game1.currentLocation.characters)
		{
			if (!((Character)character).IsVillager || character is Child || !character.CanReceiveGifts())
			{
				continue;
			}
			float num2 = Vector2.Distance(((Character)farmer).Position, ((Character)character).Position);
			bool flag = num2 < 175f;
			if (character != null && !((Character)character).IsMonster && (!farmer.isRidingHorse() || !(character is Horse)) && flag)
			{
				if (val == null)
				{
					val = character;
					num = num2;
				}
				else if (num2 < num)
				{
					val = character;
					num = num2;
				}
			}
		}
		return val;
	}

	private void UseTalkingSkill(Farmer farmer)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		NPC nearNpc = GetNearNpc(farmer);
		if (nearNpc == null)
		{
			farmer.doEmote(40);
			Game1.playSound("slime", (int?)null);
			return;
		}
		((Character)farmer).faceGeneralDirection(((Character)nearNpc).getStandingPosition(), 0, false, true);
		if (!((Character)nearNpc).IsVillager || nearNpc is Child)
		{
			farmer.doEmote(8);
			((Character)nearNpc).doEmote(40, true);
			nearNpc.checkAction(farmer, ((Character)farmer).currentLocation);
			return;
		}
		Friendship val = default(Friendship);
		if (!((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(((Character)nearNpc).Name, ref val))
		{
			DialogueEvent.AddObjectDialogue(Trs.GR("Misk.MustKnow").Replace("{npc}", NpcContent.NPCDisplayName(nearNpc)));
			return;
		}
		if (val.IsDivorced())
		{
			DialogueEvent.AddObjectDialogue(Trs.GR("Misk.Divorced").Replace("{npc}", NpcContent.NPCDisplayName(nearNpc)));
			return;
		}
		Quest questMaybeComplete = saveData().GetData(farmer).quests.GetQuestMaybeComplete(farmer, nearNpc, QuestType.HopeItem);
		if (questMaybeComplete != null)
		{
			MentionQuest.OnHopeItemQuestTrigger(farmer, saveData(), nearNpc, questMaybeComplete);
		}
		else if (ConversationUtility.GetTodayConversationRemainingNumber(farmer, nearNpc) > 0)
		{
			((Character)nearNpc).faceDirection(-3);
			farmer.doEmote(8);
			((Character)nearNpc).doEmote(40, true);
			Conversation.StartTalking(nearNpc, saveData(), farmer, config);
		}
		else
		{
			DialogueEvent.AddObjectDialogue(Trs.GR("Misk.StopDisturbing").Replace("{npc}", NpcContent.NPCDisplayName(nearNpc)));
			farmer.doEmote(28);
		}
	}

	private void OnToolIndexChange(EventArgsToolIndexChange e)
	{
		Farmer player = Game1.player;
		if (player.ActiveObject == null)
		{
			return;
		}
		NPC nearNpc = GetNearNpc(player);
		if (nearNpc != null)
		{
			Quest questMaybeComplete = saveData().GetData(player).quests.GetQuestMaybeComplete(player, nearNpc, QuestType.HopeItem);
			if (questMaybeComplete != null)
			{
				MentionQuest.OnHopeItemQuestTrigger(player, saveData(), nearNpc, questMaybeComplete);
			}
		}
	}

	private void UpdateCheckNpc(Farmer farmer)
	{
		if (saveData() == null)
		{
			return;
		}
		List<Quest> todayQuests = saveData().GetData(farmer).quests.todayQuests;
		bool flag = false;
		for (int i = 0; i < todayQuests.Count; i++)
		{
			Quest quest = todayQuests[i];
			if (quest.questType == QuestType.HopeItem)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			NPC nearNpc = GetNearNpc(farmer);
			if (nearNpc != null && _enterRangeNPC != nearNpc)
			{
				_enterRangeNPC = nearNpc;
				OnNpcEnter(farmer, _enterRangeNPC);
			}
			if (nearNpc == null && _enterRangeNPC != null)
			{
				_enterRangeNPC = null;
			}
		}
	}

	private void OnNpcEnter(Farmer farmer, NPC npc)
	{
		Quest questMaybeComplete = saveData().GetData(farmer).quests.GetQuestMaybeComplete(farmer, npc, QuestType.HopeItem);
		if (questMaybeComplete != null)
		{
			MentionQuest.OnHopeItemQuestTrigger(farmer, saveData(), npc, questMaybeComplete);
		}
	}

	private void OnUpdateTicked()
	{
		Farmer player = Game1.player;
		if (Game1.activeClickableMenu == null && player.CanMove && !player.UsingTool)
		{
			UpdateCheckNpc(player);
		}
	}
}
