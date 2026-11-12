using ConversationSystem;
using SorryLab;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

internal sealed class ThisMod : Mod
{
	public ModConfig config;

	private static ModSave saveData;

	private Processing processing;

	public const string GMCMUniqueId = "spacechase0.GenericModConfigMenu";

	public static float _lastSaveTime;

	private KeybindList debugInfoKey { get; set; } = KeybindList.Parse("LeftControl + F3");

	public static ThisMod instance { get; private set; }

	public static IModHelper helper => ((Mod)instance).Helper;

	public override void Entry(IModHelper helper)
	{
		instance = this;
		Trs.Setup((IMod)(object)this);
		config = ((Mod)this).Helper.ReadConfig<ModConfig>();
		helper.Events.Input.ButtonPressed += OnButtonPressed;
		helper.Events.GameLoop.Saved += OnSaved;
		helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
		processing = new Processing(() => GetSave(), config);
		processing.SetupEvent(helper.Events);
		ConversationSkillEvents.Setup(helper.Events, helper);
		helper.Events.GameLoop.GameLaunched += delegate
		{
			if (((Mod)this).Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
			{
				IModInfo val = ((Mod)this).Helper.ModRegistry.Get("spacechase0.GenericModConfigMenu");
				IGenericModConfigMenuApi api = ((Mod)this).Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
				GMCMHandle.Handle(api, this);
			}
		};
	}

	private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
	{
		if (Context.IsWorldReady && !debugInfoKey.JustPressed())
		{
		}
	}

	private void OnSaved(object sender = null, SavedEventArgs e = null)
	{
		if (Time.time - _lastSaveTime < 5f)
		{
			_lastSaveTime = Time.time;
			((Mod)this).Helper.Data.WriteJsonFile<ModSave>(SaveFileName(), saveData);
		}
	}

	private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
	{
		ModSave modSave = ((Mod)this).Helper.Data.ReadJsonFile<ModSave>(SaveFileName());
		if (modSave != null && saveData == null)
		{
			saveData = modSave;
		}
		else
		{
			GetSave();
		}
	}

	private ModSave GetSave()
	{
		if (saveData == null)
		{
			saveData = new ModSave();
		}
		return saveData;
	}

	private string SaveFileName()
	{
		return "Saves/" + Game1.GetSaveGameName(true) + ".json";
	}
}
