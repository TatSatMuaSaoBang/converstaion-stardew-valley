using System;
using SorryLab;
using StardewModdingAPI;

namespace ConversationSystem;

internal static class GMCMHandle
{
	public static void Handle(IGenericModConfigMenuApi api, ThisMod mod)
	{
		api.Register(((Mod)mod).ModManifest, (Action)delegate
		{
			mod.config = new ModConfig();
		}, (Action)delegate
		{
			((Mod)mod).Helper.WriteConfig<ModConfig>(mod.config);
		}, false);
		api.AddNumberOption(((Mod)mod).ModManifest, (Func<float>)(() => mod.config.ExpMultiple), (Action<float>)delegate(float value)
		{
			mod.config.ExpMultiple = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ExpMultiple.Name")), (Func<string>)(() => Trs.Get("GMCM.ExpMultiple.Desc")), (float?)1f, (float?)10f, (float?)0.5f, (Func<float, string>)null, (string)null);
		api.AddNumberOption(((Mod)mod).ModManifest, (Func<float>)(() => mod.config.SellPriceMultiple), (Action<float>)delegate(float value)
		{
			mod.config.SellPriceMultiple = value;
		}, (Func<string>)(() => Trs.Get("GMCM.SellPrice.Name")), (Func<string>)(() => Trs.Get("GMCM.SellPrice.Desc")), (float?)1.2f, (float?)2f, (float?)0.05f, (Func<float, string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.DisplayFriendShip), (Action<bool>)delegate(bool value)
		{
			mod.config.DisplayFriendShip = value;
		}, (Func<string>)(() => Trs.Get("GMCM.DisplayFriendShip.Name")), (Func<string>)(() => Trs.Get("GMCM.DisplayFriendShip.Desc")), (string)null);
		api.AddKeybind(((Mod)mod).ModManifest, (Func<SButton>)(() => mod.config.ActionKey), (Action<SButton>)delegate(SButton value)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			mod.config.ActionKey = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ActionKey.Name")), (Func<string>)(() => Trs.Get("GMCM.ActionKey.Desc")), (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.BirthdayOption), (Action<bool>)delegate(bool value)
		{
			mod.config.BirthdayOption = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ShowBitrhday.Name")), (Func<string>)(() => Trs.Get("GMCM.ShowBitrhday.Desc")), (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.ArrearsOption), (Action<bool>)delegate(bool value)
		{
			mod.config.ArrearsOption = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ShowArrears.Name")), (Func<string>)(() => Trs.Get("GMCM.ShowArrears.Desc")), (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.ResetSkillOption), (Action<bool>)delegate(bool value)
		{
			mod.config.ResetSkillOption = value;
		}, (Func<string>)(() => Trs.Get("GMCM.ShowResetSkill.Name")), (Func<string>)(() => Trs.Get("GMCM.ShowResetSkill.Desc")), (string)null);
		api.AddSectionTitle(((Mod)mod).ModManifest, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.Section")), (Func<string>)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_FindingItems), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_FindingItems = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.FindingItems")), (Func<string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_PraiseSomeone), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_PraiseSomeone = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.PraiseSomeone")), (Func<string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_InsultSomeone), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_InsultSomeone = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.InsultSomeone")), (Func<string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_QuizGame), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_QuizGame = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.QuizGame")), (Func<string>)null, (string)null);
		api.AddBoolOption(((Mod)mod).ModManifest, (Func<bool>)(() => mod.config.questEnable_Loan), (Action<bool>)delegate(bool value)
		{
			mod.config.questEnable_Loan = value;
		}, (Func<string>)(() => Trs.Get("GMCM.QuestEnable.Loan")), (Func<string>)null, (string)null);
	}
}
