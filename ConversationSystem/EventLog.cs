using System.Collections.Generic;
using SorryLab.Expansion;
using StardewValley;

namespace ConversationSystem;

public class EventLog
{
	public List<LogEvent> todayLogs = new List<LogEvent>();

	public List<LogEvent> yesterdayLogs = new List<LogEvent>();

	public LogEvent AddEvent(EventType type, NPC who, string itemID = "", string _content = "")
	{
		foreach (LogEvent todayLog in todayLogs)
		{
			if (todayLog.type == type && todayLog.npcName == ((Character)who).Name && todayLog.itemID == itemID && todayLog.content == _content)
			{
				todayLog.count++;
				return todayLog;
			}
		}
		LogEvent logEvent = new LogEvent
		{
			type = type,
			npcName = ((Character)who).Name,
			itemID = itemID,
			content = _content,
			count = 1
		};
		todayLogs.Add(logEvent);
		return logEvent;
	}

	public bool HasEvent(params string[] withoutNPCNames)
	{
		List<LogEvent> list = new List<LogEvent>();
		List<string> list2 = new List<string>(withoutNPCNames);
		foreach (LogEvent todayLog in todayLogs)
		{
			if (!list2.Contains(todayLog.npcName))
			{
				list.Add(todayLog);
			}
		}
		return list.Count > 0;
	}

	public LogEvent GetAnyEventAndRemove(params string[] withoutNPCNames)
	{
		List<LogEvent> list = new List<LogEvent>();
		List<string> list2 = new List<string>(withoutNPCNames);
		foreach (LogEvent todayLog in todayLogs)
		{
			if (!list2.Contains(todayLog.npcName))
			{
				list.Add(todayLog);
			}
		}
		if (list.Count > 0)
		{
			LogEvent randomOne = ListExpansion.GetRandomOne<LogEvent>(list);
			todayLogs.Remove(randomOne);
			return randomOne;
		}
		return null;
	}

	public void OnDayStarted()
	{
		yesterdayLogs.Clear();
		yesterdayLogs.AddRange(todayLogs);
		todayLogs.Clear();
	}
}
