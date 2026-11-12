using System.Collections.Generic;
using System.Linq;
using Netcode;
using SorryLab;
using SorryLab.Expansion;
using StardewValley;
using StardewValley.Network;

namespace ConversationSystem;

public static class NpcContent
{
	private static List<string> _cantConversation = new List<string>();

	private static List<string> _willNotGivenQuests = new List<string>();

	private static List<string> _willNotBeMissionTarget = new List<string>();

	private static List<string> _willNotBeMention = new List<string>();

	private static Dictionary<string, List<string>> _npcTags = new Dictionary<string, List<string>>();

	private static Dictionary<string, List<string>> _npcFriends = new Dictionary<string, List<string>>();

	public static void Setup()
	{
		FileUtility.SaveToJson<NpcConfig>(ThisMod.helper, NpcConfig.NewVanilla(), "NpcConfigs", "Vanilla.json");
		foreach (FileInfo<NpcConfig> item in FileUtility.LoadJsonFiles<NpcConfig>(ThisMod.helper, "NpcConfigs"))
		{
			Load(item.data);
		}
		Load(NpcConfig.NewRidgeside());
		Load(NpcConfig.NewSVE());
		Load(NpcConfig.NewES());
	}

	public static bool HasData(NPC npc)
	{
		return _npcTags.ContainsKey(((Character)npc).Name);
	}

	public static void CheckLeoMovedAndHandle()
	{
		if (StardewUtility.IsLeoMoved())
		{
			if (_willNotGivenQuests.Contains("Leo"))
			{
				_willNotGivenQuests.Remove("Leo");
			}
			if (_willNotBeMissionTarget.Contains("Leo"))
			{
				_willNotBeMissionTarget.Remove("Leo");
			}
			if (_willNotBeMention.Contains("Leo"))
			{
				_willNotBeMention.Remove("Leo");
			}
			if (_npcFriends.ContainsKey("Leo"))
			{
				_npcFriends.Remove("Leo");
			}
		}
	}

	public static bool CanConversation(NPC npc)
	{
		if (_cantConversation.Contains(((Character)npc).Name))
		{
			return false;
		}
		return true;
	}

	public static void Load(NpcConfig config)
	{
		foreach (KeyValuePair<string, List<string>> npcTag in config.npcTags)
		{
			if (!_npcTags.ContainsKey(npcTag.Key))
			{
				_npcTags[npcTag.Key] = new List<string>();
			}
			foreach (string item in npcTag.Value)
			{
				if (!_npcTags[npcTag.Key].Contains(item))
				{
					_npcTags[npcTag.Key].Add(item);
				}
			}
		}
		foreach (KeyValuePair<string, List<string>> npcRestrictFriend in config.npcRestrictFriends)
		{
			if (!_npcFriends.ContainsKey(npcRestrictFriend.Key))
			{
				_npcFriends[npcRestrictFriend.Key] = new List<string>();
			}
			foreach (string item2 in npcRestrictFriend.Value)
			{
				if (!_npcFriends[npcRestrictFriend.Key].Contains(item2))
				{
					_npcFriends[npcRestrictFriend.Key].Add(item2);
				}
			}
		}
		foreach (string willNotGivenQuest in config.willNotGivenQuests)
		{
			if (!_willNotGivenQuests.Contains(willNotGivenQuest))
			{
				_willNotGivenQuests.Add(willNotGivenQuest);
			}
		}
		foreach (string item3 in config.willNotBeMissionTarget)
		{
			if (!_willNotBeMissionTarget.Contains(item3))
			{
				_willNotBeMissionTarget.Add(item3);
			}
		}
		foreach (string item4 in config.willNotBeMention)
		{
			if (!_willNotBeMention.Contains(item4))
			{
				_willNotBeMention.Add(item4);
			}
		}
		foreach (string item5 in config.cantConversation)
		{
			if (!_cantConversation.Contains(item5))
			{
				_cantConversation.Add(item5);
			}
		}
	}

	public static string GetTalkAboutDialogue(NPC npc, NPC talkAbout)
	{
		WeightDice<string> val = new WeightDice<string>();
		string key = "DialogueSP." + ((Character)npc).Name + "-" + ((Character)talkAbout).Name;
		if (Trs.ContainsGroup(key))
		{
			val.Add(Trs.GR(key), 3f);
		}
		foreach (string npcTag in GetNpcTags(talkAbout))
		{
			string key2 = "Dialogue." + npcTag;
			if (Trs.ContainsGroup(key2))
			{
				val.Add(Trs.GR(key2), 1f);
			}
		}
		string text = "";
		text = ((val.Count <= 0) ? Trs.GR("Dialogue.Default") : val.Cast());
		text = text.Replace("{npc}", NPCDisplayName(talkAbout, npc));
		return GameTextContent.TextGenderHandle(text, (Character)(object)talkAbout);
	}

	private static List<string> GetNpcTags(NPC npc)
	{
		if (_npcTags.ContainsKey(((Character)npc).Name))
		{
			return _npcTags[((Character)npc).Name];
		}
		return new List<string>();
	}

	public static string NPCDisplayName(NPC npc, NPC whoCall = null)
	{
		if (whoCall == null)
		{
			return ((Character)npc).displayName;
		}
		string key = "Address." + ((Character)whoCall).Name + "-" + ((Character)npc).Name;
		if (Trs.ContainsGroup(key))
		{
			return Trs.GR(key);
		}
		return ((Character)npc).displayName;
	}

	public static bool IsKid(NPC npc)
	{
		if (npc == null)
		{
			return false;
		}
		if (_npcTags.ContainsKey(((Character)npc).Name))
		{
			return _npcTags[((Character)npc).Name].Contains("Kid");
		}
		return false;
	}

	public static string GetNpcTown(NPC npc)
	{
		foreach (string npcTag in GetNpcTags(npc))
		{
			string[] array = npcTag.Split(':');
			if (array.Length >= 2 && array[0] == "Town")
			{
				return array[1];
			}
		}
		return "Unknown";
	}

	public static List<NPC> GetNpcListFromTown(Farmer farmer, NPC npc, IEnumerable<string> without = null)
	{
		return GetNpcListFromTown(farmer, GetNpcTown(npc), without);
	}

	public static List<NPC> GetNpcListFromTown(Farmer farmer, string town, IEnumerable<string> without = null)
	{
		List<NPC> list = new List<NPC>();
		NPC val = default(NPC);
		Friendship val2 = default(Friendship);
		foreach (KeyValuePair<string, List<string>> npcTag in _npcTags)
		{
			if (StardewUtility.TryGetNPCFromName(npcTag.Key, ref val) && (without == null || !without.Contains(((Character)val).Name)) && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(npcTag.Key, ref val2) && GetNpcTown(val) == town)
			{
				list.Add(val);
			}
		}
		return list;
	}

	public static List<NPC> GetNpcTalkAboutList(Farmer farmer, NPC npc)
	{
		if (_npcFriends.ContainsKey(((Character)npc).Name))
		{
			List<NPC> list = new List<NPC>();
			NPC item = default(NPC);
			Friendship val = default(Friendship);
			foreach (string item2 in _npcFriends[((Character)npc).Name])
			{
				if (StardewUtility.TryGetNPCFromName(item2, ref item) && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)farmer.friendshipData).TryGetValue(item2, ref val))
				{
					list.Add(item);
				}
			}
			return list;
		}
		List<string> list2 = new List<string> { ((Character)npc).Name };
		list2.AddRange(_willNotBeMention);
		string npcTown = GetNpcTown(npc);
		if (npcTown != "Unknown")
		{
			return GetNpcListFromTown(farmer, npc, list2);
		}
		return StardewUtility.GetAllFriends(farmer, (IEnumerable<string>)list2);
	}

	public static bool IsNpcPossibleGivenQuest(NPC npc)
	{
		if (_willNotGivenQuests.Contains(((Character)npc).Name))
		{
			return false;
		}
		return true;
	}

	public static NPC GetQuestTargetPossibleNPC(Farmer farmer, NPC talkWithWho)
	{
		List<string> list = new List<string> { ((Character)talkWithWho).Name };
		list.AddRange(_willNotBeMissionTarget);
		List<NPC> allFriends = StardewUtility.GetAllFriends(farmer, (IEnumerable<string>)list.ToArray());
		for (int i = 0; i < allFriends.Count; i++)
		{
			if (QuestData.CheckNpcHasQuest(allFriends[i]))
			{
				allFriends.RemoveAt(i);
				i++;
			}
		}
		if (allFriends.Count > 0)
		{
			return ListExpansion.GetRandomOne<NPC>(allFriends);
		}
		return null;
	}

	public static string ToNpcSaidFormat(NPC npc, string dialogue)
	{
		string text = Trs.Get("Misk.NpcSaid").Replace("{npc}", NPCDisplayName(npc));
		string text2 = Trs.Get("Misk.QuoteMarks").Replace("{*}", dialogue);
		return text + text2;
	}
}
