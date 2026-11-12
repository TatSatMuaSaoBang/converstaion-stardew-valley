using System.Collections.Generic;

namespace ConversationSystem;

public class NpcConfig
{
	public List<string> cantConversation = new List<string>();

	public List<string> willNotGivenQuests = new List<string>();

	public List<string> willNotBeMissionTarget = new List<string>();

	public List<string> willNotBeMention = new List<string>();

	public Dictionary<string, List<string>> npcTags = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> npcRestrictFriends = new Dictionary<string, List<string>>();

	public static NpcConfig NewVanilla()
	{
		NpcConfig npcConfig = new NpcConfig();
		npcConfig.willNotGivenQuests.AddRange(new List<string> { "Krobus", "Dwarf", "Leo", "Wizard" });
		npcConfig.willNotBeMissionTarget.AddRange(new List<string> { "Krobus", "Dwarf", "Leo", "Wizard" });
		npcConfig.willNotBeMention.AddRange(new List<string> { "Krobus", "Dwarf", "Leo", "Wizard" });
		npcConfig.cantConversation.AddRange(new List<string> { "Leo" });
		npcConfig.npcTags = new Dictionary<string, List<string>>
		{
			{
				"Alex",
				new List<string> { "Strong", "Friendly", "Glib", "Upright", "Town:Pelican" }
			},
			{
				"Elliott",
				new List<string> { "Elegant", "Artist", "Talented", "Personalized", "Town:Pelican" }
			},
			{
				"Harvey",
				new List<string> { "Kind", "Timid", "Cautious", "Nagging", "Town:Pelican" }
			},
			{
				"Sam",
				new List<string> { "Playfulness", "Friendly", "Vitality", "Egocentric", "Town:Pelican" }
			},
			{
				"Sebastian",
				new List<string> { "Talented", "Independent", "Personalized", "Indifferent", "Town:Pelican" }
			},
			{
				"Shane",
				new List<string> { "Unfriendly", "Independent", "Personalized", "Drunkenness", "Town:Pelican" }
			},
			{
				"Abigail",
				new List<string> { "Naughty", "Vitality", "Personalized", "Mystery", "Town:Pelican" }
			},
			{
				"Emily",
				new List<string> { "Mystery", "Friendly", "Vitality", "Gentle", "Town:Pelican" }
			},
			{
				"Haley",
				new List<string> { "Fashion", "Egocentric", "Beauty", "Vitality", "Town:Pelican" }
			},
			{
				"Leah",
				new List<string> { "Talented", "Artist", "Beauty", "Friendly", "Town:Pelican" }
			},
			{
				"Maru",
				new List<string> { "Clever", "Knowledgeable", "Friendly", "Vitality", "Town:Pelican" }
			},
			{
				"Penny",
				new List<string> { "Kind", "Gentle", "Modest", "Trusty", "Town:Pelican" }
			},
			{
				"Caroline",
				new List<string> { "Friendly", "Kind", "Elegant", "Trusty", "Town:Pelican" }
			},
			{
				"Clint",
				new List<string> { "Shy", "Professional", "Upright", "Timid", "Town:Pelican" }
			},
			{
				"Demetrius",
				new List<string> { "Knowledgeable", "Professional", "Stiff", "Egocentric", "Town:Pelican" }
			},
			{
				"Dwarf",
				new List<string> { "Unknown", "Rumors-Dwarf" }
			},
			{
				"Evelyn",
				new List<string> { "Kind", "Friendly", "Gentle", "Trusty", "Town:Pelican" }
			},
			{
				"George",
				new List<string> { "Unfriendly", "Stiff", "Upright", "Lonely", "Town:Pelican" }
			},
			{
				"Gus",
				new List<string> { "Friendly", "Kind", "Generous", "Trusty", "Town:Pelican" }
			},
			{
				"Jas",
				new List<string> { "Cute", "Kind", "Lonely", "Shy", "Kid", "Town:Pelican" }
			},
			{
				"Jodi",
				new List<string> { "Friendly", "Frugal", "Vitality", "Kind", "Town:Pelican" }
			},
			{
				"Kent",
				new List<string> { "Strong", "Serious", "Trusty", "Indifferent", "Town:Pelican" }
			},
			{
				"Krobus",
				new List<string> { "Unknown", "Rumors-Krobus" }
			},
			{
				"Lewis",
				new List<string> { "Serious", "Friendly", "Stiff", "Frugal", "Town:Pelican" }
			},
			{
				"Linus",
				new List<string> { "Unhygienic", "Mystery", "Lonely", "Poverty", "Town:Pelican" }
			},
			{
				"Marnie",
				new List<string> { "Gentle", "Friendly", "Modest", "Kind", "Town:Pelican" }
			},
			{
				"Pam",
				new List<string> { "Poverty", "Friendly", "Drunkenness", "Frugal", "Town:Pelican" }
			},
			{
				"Pierre",
				new List<string> { "Frugal", "Serious", "Stiff", "Serious", "Town:Pelican" }
			},
			{
				"Robin",
				new List<string> { "Friendly", "Talented", "Strong", "Trusty", "Town:Pelican" }
			},
			{
				"Sandy",
				new List<string> { "Mystery", "Generous", "Friendly", "Personalized", "Town:Pelican" }
			},
			{
				"Vincent",
				new List<string> { "Naughty", "Cute", "Vitality", "Timid", "Kid", "Town:Pelican" }
			},
			{
				"Willy",
				new List<string> { "Independent", "Trusty", "Professional", "Generous", "Town:Pelican" }
			},
			{
				"Wizard",
				new List<string> { "Mystery", "Rumors-Wizard", "Town:Pelican" }
			},
			{
				"Leo",
				new List<string> { "Orphans", "child", "Town:Pelican" }
			}
		};
		npcConfig.npcRestrictFriends = new Dictionary<string, List<string>>
		{
			{
				"Krobus",
				new List<string> { "Wizard" }
			},
			{
				"Dwarf",
				new List<string> { "Wizard" }
			},
			{
				"Leo",
				new List<string> { "Willy" }
			},
			{
				"Wizard",
				new List<string> { "Krobus", "Dwarf", "Linus" }
			}
		};
		return npcConfig;
	}

	public static NpcConfig NewRidgeside()
	{
		NpcConfig npcConfig = new NpcConfig();
		npcConfig.cantConversation.AddRange(new List<string> { "Torts", "Kiwi" });
		npcConfig.willNotGivenQuests.AddRange(new List<string> { "Torts", "Kiwi", "Jio", "Daia", "Bliss" });
		npcConfig.willNotBeMissionTarget.AddRange(new List<string> { "Torts", "Kiwi", "Jio", "Daia", "Bliss" });
		npcConfig.willNotBeMention.AddRange(new List<string> { "Torts", "Kiwi", "Jio", "Daia", "Bliss" });
		npcConfig.npcTags = new Dictionary<string, List<string>>
		{
			{
				"Ian",
				new List<string> { "Friendly", "Gentle", "Trusty", "Frugal", "Town:Ridgeside" }
			},
			{
				"Jeric",
				new List<string> { "Personalized", "Professional", "Vitality", "Glib", "Town:Ridgeside" }
			},
			{
				"June",
				new List<string> { "Friendly", "Gentle", "Trusty", "Talented", "Town:Ridgeside" }
			},
			{
				"Kenneth",
				new List<string> { "Professional", "Kind", "Upright", "Lonely", "Town:Ridgeside" }
			},
			{
				"Philip",
				new List<string> { "Friendly", "Kind", "Gentle", "Vitality", "Town:Ridgeside" }
			},
			{
				"Sean",
				new List<string> { "Vitality", "Naughty", "Playfulness", "Friendly", "Town:Ridgeside" }
			},
			{
				"Shiro",
				new List<string> { "Cautious", "Modest", "Lonely", "Gentle", "Town:Ridgeside" }
			},
			{
				"Alissa",
				new List<string> { "Gentle", "Modest", "Friendly", "Talented", "Town:Ridgeside" }
			},
			{
				"Blair",
				new List<string> { "Kind", "Friendly", "Gentle", "Vitality", "Town:Ridgeside" }
			},
			{
				"Corine",
				new List<string> { "Friendly", "Gentle", "Kind", "Vitality", "Town:Ridgeside" }
			},
			{
				"Flor",
				new List<string> { "Talented", "Timid", "Modest", "Shy", "Town:Ridgeside" }
			},
			{
				"Kiarra",
				new List<string> { "Talented", "Vitality", "Fashion", "Gentle", "Town:Ridgeside" }
			},
			{
				"Maddie",
				new List<string> { "Serious", "Unfriendly", "Indifferent", "Trusty", "Town:Ridgeside" }
			},
			{
				"Ysabelle",
				new List<string> { "Talented", "Elegant", "Friendly", "Beauty", "Town:Ridgeside" }
			},
			{
				"Anton",
				new List<string> { "Egocentric", "Personalized", "Independent", "Modest", "Town:Ridgeside" }
			},
			{
				"Bryle",
				new List<string> { "Strong", "Glib", "Vitality", "Personalized", "Town:Ridgeside" }
			},
			{
				"Daia",
				new List<string> { "Personalized", "Glib", "Mystery", "Fashion", "Town:Ridgeside" }
			},
			{
				"Jio",
				new List<string> { "Personalized", "Indifferent", "Mystery", "Unfriendly", "Town:Ridgeside" }
			},
			{
				"Faye",
				new List<string> { "Fashion", "Personalized", "Professional", "Talented", "Town:Ridgeside" }
			},
			{
				"Irene",
				new List<string> { "Gentle", "Kind", "Trusty", "Vitality", "Town:Ridgeside" }
			},
			{
				"Paula",
				new List<string> { "Modest", "Gentle", "Drunkenness", "Trusty", "Town:Ridgeside" }
			},
			{
				"Zayne",
				new List<string> { "Gentle", "Modest", "Clever", "Friendly", "Town:Ridgeside" }
			},
			{
				"Bert",
				new List<string> { "Friendly", "Strong", "Upright", "Trusty", "Town:Ridgeside" }
			},
			{
				"Carmen",
				new List<string> { "Friendly", "Professional", "Gentle", "Vitality", "Town:Ridgeside" }
			},
			{
				"Ezekiel",
				new List<string> { "Lonely", "Serious", "Unfriendly", "Indifferent", "Town:Ridgeside" }
			},
			{
				"Freddie",
				new List<string> { "Trusty", "Upright", "Mystery", "Gentle", "Town:Ridgeside" }
			},
			{
				"Kimpoi",
				new List<string> { "Gentle", "Upright", "Trusty", "Shy", "Town:Ridgeside" }
			},
			{
				"Lenny",
				new List<string> { "Nagging", "Egocentric", "Trusty", "Vitality", "Town:Ridgeside" }
			},
			{
				"Lola",
				new List<string> { "Trusty", "Personalized", "Mystery", "Friendly", "Town:Ridgeside" }
			},
			{
				"Lorenzo",
				new List<string> { "Cautious", "Modest", "Stiff", "Serious", "Town:Ridgeside" }
			},
			{
				"Maive",
				new List<string> { "Serious", "Stiff", "Cautious", "Generous", "Town:Ridgeside" }
			},
			{
				"Malaya",
				new List<string> { "Kind", "Elegant", "Friendly", "Gentle", "Town:Ridgeside" }
			},
			{
				"Naomi",
				new List<string> { "Talented", "Gentle", "Vitality", "Professional", "Town:Ridgeside" }
			},
			{
				"Olga",
				new List<string> { "Stiff", "Kind", "Cautious", "Trusty", "Town:Ridgeside" }
			},
			{
				"Pika",
				new List<string> { "Generous", "Gentle", "Elegant", "Vitality", "Town:Ridgeside" }
			},
			{
				"Richard",
				new List<string> { "Gentle", "Knowledgeable", "Kind", "Elegant", "Town:Ridgeside" }
			},
			{
				"Shanice",
				new List<string> { "Friendly", "Kind", "Knowledgeable", "Gentle", "Town:Ridgeside" }
			},
			{
				"Sonny",
				new List<string> { "Modest", "Kind", "Gentle", "Elegant", "Town:Ridgeside" }
			},
			{
				"Ariah",
				new List<string> { "Kid", "Serious", "Shy", "Indifferent", "Town:Ridgeside" }
			},
			{
				"Bliss",
				new List<string> { "Kid", "Egocentric", "Mystery", "Lonely", "Town:Ridgeside" }
			},
			{
				"Keahi",
				new List<string> { "Kid", "Naughty", "Vitality", "Playfulness", "Town:Ridgeside" }
			},
			{
				"Louie",
				new List<string> { "Kid", "Egocentric", "Upright", "Personalized", "Town:Ridgeside" }
			},
			{
				"Pipo",
				new List<string> { "Kid", "Kind", "Friendly", "Gentle", "Town:Ridgeside" }
			},
			{
				"Trinnie",
				new List<string> { "Kid", "Cute", "Naughty", "Vitality", "Town:Ridgeside" }
			},
			{
				"Undreya",
				new List<string> { "Kid", "Mystery", "Naughty", "Gentle", "Town:Ridgeside" }
			},
			{
				"Yuuma",
				new List<string> { "Kid", "Serious", "Modest", "Lonely", "Town:Ridgeside" }
			},
			{
				"Aguar",
				new List<string> { "Knowledgeable", "Mystery", "Personalized", "Indifferent", "Town:Ridgeside" }
			}
		};
		npcConfig.npcRestrictFriends = new Dictionary<string, List<string>>();
		return npcConfig;
	}

	public static NpcConfig NewES()
	{
		NpcConfig npcConfig = new NpcConfig();
		npcConfig.cantConversation.AddRange(new List<string> { "Emperor", "Empress", "Lady Sheba", "Leximonster", "Mister Ginger", "Fievel", "LumaJunimo" });
		npcConfig.willNotGivenQuests.AddRange(new List<string> { "Hector", "Emperor", "Empress", "Lady Sheba", "Leximonster", "Mister Ginger", "Fievel", "LumaJunimo" });
		npcConfig.willNotBeMissionTarget.AddRange(new List<string> { "Hector", "Emperor", "Empress", "Lady Sheba", "Leximonster", "Mister Ginger", "Fievel", "LumaJunimo" });
		npcConfig.willNotBeMention.AddRange(new List<string> { "Hector", "Emperor", "Empress", "Lady Sheba", "Leximonster", "Mister Ginger", "Fievel", "LumaJunimo" });
		npcConfig.npcTags = new Dictionary<string, List<string>>
		{
			{
				"Aideen",
				new List<string> { "Elegant", "Artist", "Independent", "Unfriendly", "Town:EastScarp" }
			},
			{
				"Barron",
				new List<string> { "Knowledgeable", "Clever", "Professional", "Trusty", "Town:EastScarp" }
			},
			{
				"Hector",
				new List<string> { "Mystery", "Town:EastScarp" }
			},
			{
				"Jasper",
				new List<string> { "Friendly", "Knowledgeable", "Personalized", "Shy", "Town:EastScarp" }
			},
			{
				"Juliet",
				new List<string> { "Egocentric", "Naughty", "Personalized", "Playfulness", "Town:EastScarp" }
			},
			{
				"Kataryna",
				new List<string> { "Knowledgeable", "Personalized", "Serious", "Unfriendly", "Town:EastScarp" }
			},
			{
				"Mateo",
				new List<string> { "Mystery", "Kind", "Gentle", "Trusty", "Town:EastScarp" }
			},
			{
				"Sen",
				new List<string> { "Mystery", "Upright", "Modest", "Gentle", "Town:EastScarp" }
			},
			{
				"Sterling",
				new List<string> { "Glib", "Playfulness", "Personalized", "Drunkenness", "Town:EastScarp" }
			},
			{
				"Tristan",
				new List<string> { "Glib", "Default", "___", "___", "Town:EastScarp" }
			},
			{
				"Beatrice",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Eloise",
				new List<string> { "Default", "Kid", "___", "___", "Town:EastScarp" }
			},
			{
				"Jacob",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Jessie",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Josephine",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Lavril",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Oliver",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Rodney",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Rosa",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Vivienne",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"Eyvinder",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			},
			{
				"ToriLK",
				new List<string> { "Default", "___", "___", "___", "Town:EastScarp" }
			}
		};
		npcConfig.npcRestrictFriends = new Dictionary<string, List<string>>();
		return npcConfig;
	}

	public static NpcConfig NewSVE()
	{
		NpcConfig npcConfig = new NpcConfig();
		npcConfig.cantConversation.AddRange(new List<string> { "Apples" });
		npcConfig.willNotGivenQuests.AddRange(new List<string> { "Apples", "Magnus", "Morgan" });
		npcConfig.willNotBeMissionTarget.AddRange(new List<string> { "Apples", "Magnus", "Morgan" });
		npcConfig.willNotBeMention.AddRange(new List<string> { "Apples", "Magnus", "Morgan" });
		npcConfig.npcTags = new Dictionary<string, List<string>>
		{
			{
				"Claire",
				new List<string> { "Elegant", "Independent", "Lonely", "Indifferent", "Town:Pelican" }
			},
			{
				"Lance",
				new List<string> { "Trusty", "Vitality", "Strong", "Professional", "Town:Pelican" }
			},
			{
				"Magnus",
				new List<string> { "Mystery", "Rumors-Wizard" }
			},
			{
				"Olivia",
				new List<string> { "Fashion", "Beauty", "Serious", "Generous", "Town:Pelican" }
			},
			{
				"Scarlett",
				new List<string> { "Clever", "Friendly", "Gentle", "Vitality", "Town:Pelican" }
			},
			{
				"Sophia",
				new List<string> { "Lonely", "Professional", "Shy", "Beauty", "Town:Pelican" }
			},
			{
				"Andy",
				new List<string> { "Unhygienic", "Unfriendly", "Egocentric", "Frugal", "Town:Pelican" }
			},
			{
				"GuntherSilvian",
				new List<string> { "Professional", "Knowledgeable", "Elegant", "Cautious", "Town:Pelican" }
			},
			{
				"Martin",
				new List<string> { "Kind", "Timid", "Upright", "Shy", "Town:Pelican" }
			},
			{
				"MarlonFay",
				new List<string> { "Independent", "Mystery", "Personalized", "Professional", "Town:Pelican" }
			},
			{
				"MorrisTod",
				new List<string> { "Professional", "Serious", "Egocentric", "Stiff", "Town:Pelican" }
			},
			{
				"Morgan",
				new List<string> { "Cute", "Mystery", "Kid" }
			},
			{
				"Susan",
				new List<string> { "Strong", "Friendly", "Glib", "Upright", "Town:Pelican" }
			}
		};
		npcConfig.npcRestrictFriends = new Dictionary<string, List<string>>
		{
			{
				"Wizard",
				new List<string> { "Morgan" }
			},
			{
				"Magnus",
				new List<string> { "Morgan" }
			},
			{
				"Morgan",
				new List<string> { "Krobus", "Wizard", "Dwarf", "Magnus" }
			},
			{
				"Krobus",
				new List<string> { "Magnus", "Morgan" }
			},
			{
				"Dwarf",
				new List<string> { "Magnus", "Morgan" }
			}
		};
		return npcConfig;
	}
}
