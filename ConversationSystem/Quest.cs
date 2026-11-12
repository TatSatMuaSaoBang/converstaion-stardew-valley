using System.Collections.Generic;
using SorryLab;
using StardewValley;

namespace ConversationSystem;

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
