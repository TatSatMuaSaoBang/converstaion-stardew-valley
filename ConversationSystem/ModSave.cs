using System.Collections.Generic;
using StardewValley;

namespace ConversationSystem;

public sealed class ModSave
{
	public class Save
	{
		public QuestData quests = new QuestData();

		public EventLog log = new EventLog();

		public NpcData npcData = new NpcData();
	}

	public Dictionary<string, Save> saves = new Dictionary<string, Save>();

	public Save GetData(Farmer farmer)
	{
		if (!saves.ContainsKey(((Character)farmer).Name))
		{
			saves[((Character)farmer).Name] = new Save();
		}
		return saves[((Character)farmer).Name];
	}
}
