using SorryLab;
using StardewValley;

namespace ConversationSystem;

public class LogEvent
{
	public EventType type;

	public string npcName = "";

	public string itemID = "";

	public string content = "";

	public int count = 0;

	public string ToGreetingDialogue(NPC whoCall)
	{
		string text = Trs.GR("Conversation.Greeting.EventLog." + type);
		NPC val = default(NPC);
		if (npcName != "" && StardewUtility.TryGetNPCFromName(npcName, ref val))
		{
			text = text.Replace("{npc}", NpcContent.NPCDisplayName(val, whoCall));
			text = GameTextContent.TextGenderHandle(text, (Character)(object)val);
		}
		if (itemID != "")
		{
			Object sObjectFromItemID = StardewUtility.GetSObjectFromItemID(itemID);
			text = text.Replace("{i}", ((Item)sObjectFromItemID).DisplayName);
		}
		if (content != "")
		{
			text = text.Replace("{c}", content);
		}
		return text;
	}
}
