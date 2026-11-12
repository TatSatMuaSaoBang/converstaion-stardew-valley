using System.Collections.Generic;
using SorryLab;
using StardewValley;

namespace ConversationSystem;

public class NpcData
{
	public class Element
	{
		public string npcName { get; set; }

		public int helpPoints { get; set; }

		public int arrears { get; set; }
	}

	public List<Element> todayElements = new List<Element>();

	public List<Element> elements = new List<Element>();

	private List<string> _todayClearArrears = new List<string>();

	public void OnDayStarted()
	{
		foreach (Element todayElement in todayElements)
		{
			GetElement(todayElement.npcName).helpPoints += todayElement.helpPoints;
			GetElement(todayElement.npcName).arrears += todayElement.arrears;
		}
		todayElements.Clear();
		_todayClearArrears.Clear();
	}

	public int GetHelpPoints(NPC npc)
	{
		return GetElement(((Character)npc).Name).helpPoints;
	}

	public void AddHelpPoints(NPC npc, int point)
	{
		GetTodayElement(((Character)npc).Name).helpPoints += point;
	}

	public void AddArrears(NPC npc, int arrears)
	{
		GetTodayElement(((Character)npc).Name).arrears += arrears;
	}

	public int GetArrears(NPC npc)
	{
		return GetElement(((Character)npc).Name).arrears;
	}

	public Dictionary<string, int> GetAllNpcArrears()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 0; i < elements.Count; i++)
		{
			Element element = elements[i];
			if (element.arrears > 0)
			{
				dictionary.Add(element.npcName, element.arrears);
			}
		}
		return dictionary;
	}

	public bool IsTodayClearArrears(NPC npc)
	{
		return _todayClearArrears.Contains(((Character)npc).Name);
	}

	public void ClearArrears(NPC npc)
	{
		GetElement(((Character)npc).Name).arrears = 0;
		if (!_todayClearArrears.Contains(((Character)npc).Name))
		{
			_todayClearArrears.Add(((Character)npc).Name);
		}
	}

	public void CostHelpPoint(NPC npc, int point)
	{
		Element element = GetElement(((Character)npc).Name);
		element.helpPoints = Mathf.Max(0, element.helpPoints - point);
	}

	private Element GetTodayElement(string npcName)
	{
		foreach (Element todayElement in todayElements)
		{
			if (todayElement.npcName == npcName)
			{
				return todayElement;
			}
		}
		Element element = new Element
		{
			npcName = npcName
		};
		todayElements.Add(element);
		return element;
	}

	private Element GetElement(string npcName)
	{
		foreach (Element element2 in elements)
		{
			if (element2.npcName == npcName)
			{
				return element2;
			}
		}
		Element element = new Element
		{
			npcName = npcName
		};
		elements.Add(element);
		return element;
	}
}
