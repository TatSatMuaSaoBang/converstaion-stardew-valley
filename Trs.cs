using System.Collections.Generic;
using SorryLab;
using StardewModdingAPI;
using StardewValley;

public static class Trs
{
	private static GameTextContent gameTextContent;

	public static void Setup(IMod mod)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		gameTextContent = new GameTextContent(mod);
	}

	public static string GR(string key)
	{
		return gameTextContent.GetRandomTextFormGroup(key);
	}

	public static string GR(string key, Character who)
	{
		return gameTextContent.GetRandomTextFormGroup(key, who);
	}

	public static List<string> GRList(string key, int count)
	{
		return gameTextContent.GetTextsFormGroup(key, count);
	}

	public static bool ContainsGroup(string key)
	{
		return gameTextContent.ContainsTextGroup(key);
	}

	public static string Get(string key)
	{
		return gameTextContent.GetText(key);
	}

	public static string Get(string key, Character who)
	{
		return gameTextContent.GetText(key, who);
	}
}
