using System;
using System.Collections.Generic;
using System.Linq;
using SorryLab;
using SorryLab.Expansion;
using StardewValley;

namespace ConversationSystem;

internal static class QuestionSystem
{
	internal class QuestionData
	{
		public NPC npc;

		public Question question;
	}

	public class Question
	{
		public string text;

		public int correctIndex;

		public List<string> options;
	}

	public enum QuestionType
	{
		CropSeason,
		TreesCrops,
		KegAndWine,
		MaturityTime,
		FishSeasons,
		HighestPrices,
		HighestStamina,
		Ingredients,
		ItemTaste,
		FishingFloor
	}

	private static List<QuestionType> _typesHaveBeenUsed = new List<QuestionType>();

	private static int _optionCount = 3;

	private static Dictionary<Farmer, QuestionData> _questionData = new Dictionary<Farmer, QuestionData>();

	private static Dictionary<QuestionType, Func<Farmer, Question>> questions = new Dictionary<QuestionType, Func<Farmer, Question>>
	{
		{
			QuestionType.CropSeason,
			GetQuest_CropSeason
		},
		{
			QuestionType.TreesCrops,
			GetQuest_TreesCrops
		},
		{
			QuestionType.KegAndWine,
			GetQuest_KegAndWine
		},
		{
			QuestionType.MaturityTime,
			GetQuest_MaturityTime
		},
		{
			QuestionType.FishSeasons,
			GetQuest_FishSeasons
		},
		{
			QuestionType.HighestPrices,
			GetQuest_HighestPrices
		},
		{
			QuestionType.HighestStamina,
			GetQuest_HighestStamina
		},
		{
			QuestionType.Ingredients,
			GetQuest_Ingredients
		},
		{
			QuestionType.ItemTaste,
			GetQuest_ItemTaste
		},
		{
			QuestionType.FishingFloor,
			GetQuest_FishingFloor
		}
	};

	private static QuestionData GetQuestionData(Farmer farmer)
	{
		if (!_questionData.ContainsKey(farmer))
		{
			_questionData[farmer] = new QuestionData();
		}
		return _questionData[farmer];
	}

	public static Question PlayQuestion(Farmer farmer, NPC npc, string preMsg = "", Action<bool> callback = null)
	{
		QuestionData questionData = GetQuestionData(farmer);
		questionData.npc = npc;
		int count = StardewUtility.GetAllFriends(farmer, (IEnumerable<string>)null).Count;
		List<QuestionType> list = questions.Keys.ToList();
		if (_typesHaveBeenUsed.Count >= questions.Count)
		{
			_typesHaveBeenUsed.Clear();
		}
		for (int i = 0; i < list.Count; i++)
		{
			QuestionType item = list[i];
			if (_typesHaveBeenUsed.Contains(item))
			{
				list.RemoveAt(i);
				i--;
			}
		}
		QuestionType randomOne = ListExpansion.GetRandomOne<QuestionType>(list);
		questionData.question = questions[randomOne](farmer);
		string text = questionData.question.text;
		_typesHaveBeenUsed.Add(randomOne);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>();
		for (int j = 0; j < questionData.question.options.Count; j++)
		{
			int answer = j;
			dictionary[questionData.question.options[j]] = delegate
			{
				Answer(farmer, answer, callback);
			};
		}
		if (npc != null)
		{
			if (!string.IsNullOrEmpty(preMsg))
			{
				DialogueEvent.AddDialogue(npc, preMsg + "\n" + text);
			}
			else
			{
				DialogueEvent.AddDialogue(npc, text);
			}
			text = NpcContent.ToNpcSaidFormat(npc, text);
		}
		ConversationUtility.DisplayOptions(farmer, npc, text, dictionary);
		return questionData.question;
	}

	private static void Answer(Farmer farmer, int index, Action<bool> callback)
	{
		QuestionData questionData = GetQuestionData(farmer);
		bool flag = questionData.question.correctIndex == index;
		if (flag)
		{
			Game1.playSound("give_gift", (int?)null);
		}
		else
		{
			Game1.playSound("slimedead", (int?)null);
		}
		callback?.Invoke(flag);
	}

	private static Question NewQuestion()
	{
		return new Question
		{
			correctIndex = Mathf.RandomRange(0, _optionCount),
			options = new List<string>()
		};
	}

	private static Question GetQuest_CropSeason(Farmer farmer)
	{
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Expected O, but got Unknown
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Expected O, but got Unknown
		Question question = NewQuestion();
		Dictionary<Season, List<string>> dictionary = new Dictionary<Season, List<string>>
		{
			{
				(Season)0,
				new List<string>
				{
					"597", "Carrot", "190", "248", "188", "250", "24", "192", "252", "400",
					"591", "271"
				}
			},
			{
				(Season)1,
				new List<string>
				{
					"258", "304", "260", "254", "376", "264", "266", "268", "593", "SummerSquash",
					"256"
				}
			},
			{
				(Season)2,
				new List<string>
				{
					"300", "274", "284", "278", "Broccoli", "282", "272", "595", "398", "276",
					"280"
				}
			},
			{
				(Season)3,
				new List<string> { "Powdermelon" }
			}
		};
		List<Season> list = new List<Season>
		{
			(Season)0,
			(Season)1,
			(Season)2
		};
		Season randomOneAndRemove = ListExpansion.GetRandomOneAndRemove<Season>(list);
		list.Add((Season)3);
		string newValue = Trs.GR($"Season.{randomOneAndRemove}");
		question.text = Trs.GR("Question.Type.CropSeason").Replace("{season}", newValue);
		List<string> list2 = dictionary[randomOneAndRemove];
		List<string> list3 = new List<string>();
		for (int i = 0; i < list.Count; i++)
		{
			list3.AddRange(dictionary[list[i]]);
		}
		for (int j = 0; j < _optionCount; j++)
		{
			if (j == question.correctIndex)
			{
				Object val = new Object(ListExpansion.GetRandomOneAndRemove<string>(list2), 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(list3), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_TreesCrops(Farmer farmer)
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		Question question = NewQuestion();
		List<string> list = new List<string> { "613", "634", "91", "638", "834", "635", "636", "637" };
		List<string> list2 = new List<string>
		{
			"454", "410", "258", "90", "282", "414", "260", "254", "832", "Powdermelon",
			"252", "296", "396", "268", "400", "406"
		};
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				Object val = new Object(ListExpansion.GetRandomOneAndRemove<string>(list), 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(list2), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		question.text = Trs.GR("Question.Type.TreesCrops");
		return question;
	}

	private static Question GetQuest_KegAndWine(Farmer farmer)
	{
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Expected O, but got Unknown
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Expected O, but got Unknown
		Question question = NewQuestion();
		List<string> list = new List<string>
		{
			"613", "634", "91", "638", "834", "635", "636", "637", "454", "410",
			"258", "90", "282", "414", "260", "254", "832", "Powdermelon", "252", "296",
			"396", "268", "400", "406"
		};
		List<string> list2 = new List<string>
		{
			"300", "274", "284", "278", "Broccoli", "Carrot", "190", "270", "272", "259",
			"248", "188", "304", "250", "24", "192", "276", "264", "266", "SummerSquash",
			"830", "815", "256", "271", "262", "280"
		};
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				Object val = new Object(ListExpansion.GetRandomOneAndRemove<string>(list), 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(list2), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		question.text = Trs.GR("Question.Type.KegAndWine");
		return question;
	}

	private static Question GetQuest_MaturityTime(Farmer farmer)
	{
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Expected O, but got Unknown
		Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>
		{
			{
				3,
				new List<string> { "Carrot" }
			},
			{
				4,
				new List<string> { "248", "24", "262", "278" }
			},
			{
				5,
				new List<string> { "260", "272" }
			},
			{
				6,
				new List<string> { "250", "192", "591", "264", "SummerSquash", "284" }
			},
			{
				7,
				new List<string> { "413", "376", "300", "282", "Powdermelon", "771" }
			},
			{
				8,
				new List<string> { "400", "593", "421", "274", "Broccoli" }
			},
			{
				9,
				new List<string> { "266" }
			},
			{
				10,
				new List<string> { "433", "188", "398", "280" }
			},
			{
				11,
				new List<string> { "304", "256" }
			},
			{
				12,
				new List<string> { "190", "254", "595", "90" }
			},
			{
				13,
				new List<string> { "252", "258", "268", "]" }
			},
			{
				14,
				new List<string> { "270", "832" }
			},
			{
				24,
				new List<string> { "417" }
			},
			{
				28,
				new List<string> { "454" }
			}
		};
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		List<string> list = new List<string>();
		List<int> list2 = dictionary.Keys.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			List<string> list3 = dictionary[list2[i]];
			list.AddRange(list3);
			for (int j = 0; j < list3.Count; j++)
			{
				dictionary2[list3[j]] = list2[i];
			}
		}
		string randomOneAndRemove = ListExpansion.GetRandomOneAndRemove<string>(list);
		int item = dictionary2[randomOneAndRemove];
		list2.Remove(item);
		Object val = new Object(randomOneAndRemove, 1, false, -1, 0);
		Question question = NewQuestion();
		question.text = Trs.GR("Question.Type.MaturityTime").Replace("{crop}", ((Item)val).DisplayName);
		for (int k = 0; k < _optionCount; k++)
		{
			if (k == question.correctIndex)
			{
				question.options.Add(item.ToString());
			}
			else
			{
				question.options.Add(ListExpansion.GetRandomOneAndRemove<int>(list2).ToString());
			}
		}
		return question;
	}

	private static Question GetQuest_FishSeasons(Farmer farmer)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Expected O, but got Unknown
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Expected O, but got Unknown
		Question question = NewQuestion();
		Season randomOne = ListExpansion.GetRandomOne<Season>(new List<Season>
		{
			(Season)0,
			(Season)1,
			(Season)2,
			(Season)3
		});
		string newValue = Trs.GR($"Season.{randomOne}");
		question.text = Trs.GR("Question.Type.FishSeasons").Replace("{season}", newValue);
		Dictionary<Season, List<string>> dictionary = new Dictionary<Season, List<string>>
		{
			{
				(Season)0,
				new List<string> { "129", "131", "147", "148", "267", "708" }
			},
			{
				(Season)1,
				new List<string> { "128", "130", "146", "149", "155", "267", "701", "708" }
			},
			{
				(Season)2,
				new List<string> { "129", "131", "148", "154", "155", "701", "705" }
			},
			{
				(Season)3,
				new List<string> { "130", "131", "146", "147", "151", "154", "705", "708" }
			}
		};
		List<string> list = new List<string>();
		foreach (Season key in dictionary.Keys)
		{
			foreach (string item in dictionary[key])
			{
				if (!dictionary[randomOne].Contains(item) && !list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				Object val = new Object(ListExpansion.GetRandomOneAndRemove<string>(list), 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(dictionary[randomOne]), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_HighestPrices(Farmer farmer)
	{
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Expected O, but got Unknown
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Expected O, but got Unknown
		Question question = NewQuestion();
		List<(string, int)> list = new List<(string, int)>
		{
			("304", 25),
			("24", 35),
			("188", 40),
			("258", 50),
			("256", 60),
			("282", 75),
			("192", 80),
			("264", 90),
			("284", 100),
			("250", 110),
			("400", 120),
			("300", 150),
			("274", 160),
			("190", 175),
			("252", 220),
			("254", 250),
			("266", 260),
			("276", 320),
			("454", 550),
			("268", 750)
		};
		List<(string, int)> list2 = new List<(string, int)>();
		for (int i = 0; i < _optionCount; i++)
		{
			list2.Add(ListExpansion.GetRandomOneAndRemove<(string, int)>(list));
		}
		list2 = list2.OrderByDescending(((string, int) item) => item.Item2).ToList();
		(string, int) tuple = list2[0];
		list2.RemoveAt(0);
		question.text = Trs.GR("Question.Type.HighestPrices");
		for (int num = 0; num < _optionCount; num++)
		{
			if (num == question.correctIndex)
			{
				Object val = new Object(tuple.Item1, 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<(string, int)>(list2).Item1, 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_HighestStamina(Farmer farmer)
	{
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Expected O, but got Unknown
		Question question = NewQuestion();
		List<(string, int)> list = new List<(string, int)>
		{
			("253", 8),
			("194", 50),
			("227", 75),
			("199", 85),
			("209", 90),
			("195", 100),
			("196", 113),
			("226", 115),
			("207", 125),
			("215", 130),
			("205", 135),
			("197", 138),
			("206", 150),
			("200", 165),
			("239", 170),
			("218", 175),
			("201", 200),
			("222", 215),
			("230", 240),
			("221", 250)
		};
		List<(string, int)> list2 = new List<(string, int)>();
		for (int i = 0; i < _optionCount; i++)
		{
			list2.Add(ListExpansion.GetRandomOneAndRemove<(string, int)>(list));
		}
		list2 = list2.OrderByDescending(((string, int) item) => item.Item2).ToList();
		(string, int) tuple = list2[0];
		list2.RemoveAt(0);
		question.text = Trs.GR("Question.Type.HighestStamina");
		for (int num = 0; num < _optionCount; num++)
		{
			if (num == question.correctIndex)
			{
				Object val = new Object(tuple.Item1, 1, false, -1, 0);
				question.options.Add(((Item)val).DisplayName);
			}
			else
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<(string, int)>(list2).Item1, 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_Ingredients(Farmer farmer)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "303", "304" },
			{ "346", "262" },
			{ "459", "340" },
			{ "424", "184" },
			{ "426", "436" },
			{ "419", "423" },
			{ "395", "433" },
			{ "614", "815" },
			{ "428", "440" },
			{ "306", "176" },
			{ "307", "442" },
			{ "308", "305" },
			{ "807", "107" },
			{ "432", "430" },
			{ "247", "270" }
		};
		List<string> list = dictionary.Keys.ToList();
		string randomOneAndRemove = ListExpansion.GetRandomOneAndRemove<string>(list);
		Object val = new Object(randomOneAndRemove, 1, false, -1, 0);
		Question question = NewQuestion();
		question.text = Trs.GR("Question.Type.Ingredients").Replace("{obj}", ((Item)val).DisplayName);
		for (int i = 0; i < _optionCount; i++)
		{
			val = ((i != question.correctIndex) ? new Object(dictionary[ListExpansion.GetRandomOneAndRemove<string>(list)], 1, false, -1, 0) : new Object(dictionary[randomOneAndRemove], 1, false, -1, 0));
			question.options.Add(((Item)val).DisplayName);
		}
		return question;
	}

	private static Question GetQuest_ItemTaste(Farmer farmer)
	{
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Expected O, but got Unknown
		QuestionData questionData = GetQuestionData(farmer);
		List<string> list = new List<string>
		{
			"Alex", "Elliott", "Harvey", "Sam", "Sebastian", "Shane", "Abigail", "Emily", "Haley", "Leah",
			"Maru", "Penny", "Caroline", "Clint", "Demetrius", "Evelyn", "George", "Gus", "Jas", "Jodi",
			"Lewis", "Linus", "Marnie", "Pam", "Pierre", "Robin", "Vincent", "Willy"
		};
		if (questionData.npc != null)
		{
			list.Remove(((Character)questionData.npc).Name);
		}
		NPC val = default(NPC);
		StardewUtility.TryGetNPCFromName(ListExpansion.GetRandomOne<string>(list), ref val);
		Question question = NewQuestion();
		List<string> itemIDListFromTaste = StardewUtility.GetItemIDListFromTaste(val, (GiftTaste)(!(Mathf.RandomValue > 0.5f)));
		List<string> itemIDListFromTaste2 = StardewUtility.GetItemIDListFromTaste(val, (GiftTaste)((Mathf.RandomValue > 0.5f) ? 3 : 4));
		question.text = Trs.GR("Question.Type.ItemTaste").Replace("{npc}", ((Character)val).displayName);
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				Object val2 = new Object(ListExpansion.GetRandomOneAndRemove<string>(itemIDListFromTaste), 1, false, -1, 0);
				question.options.Add(((Item)val2).DisplayName);
			}
			else
			{
				Object val3 = new Object(ListExpansion.GetRandomOneAndRemove<string>(itemIDListFromTaste2), 1, false, -1, 0);
				question.options.Add(((Item)val3).DisplayName);
			}
		}
		return question;
	}

	private static Question GetQuest_FishingFloor(Farmer farmer)
	{
		Question question = NewQuestion();
		question.text = Trs.GR("Question.Type.FishingFloor");
		List<string> list = new List<string> { "20", "60", "100" };
		List<string> list2 = new List<string>
		{
			"5", "10", "15", "25", "30", "35", "40", "45", "50", "55",
			"65", "70", "75", "80", "85", "90", "95"
		};
		for (int i = 0; i < _optionCount; i++)
		{
			if (i == question.correctIndex)
			{
				question.options.Add(ListExpansion.GetRandomOneAndRemove<string>(list));
			}
			else
			{
				question.options.Add(ListExpansion.GetRandomOneAndRemove<string>(list2));
			}
		}
		return question;
	}
}
