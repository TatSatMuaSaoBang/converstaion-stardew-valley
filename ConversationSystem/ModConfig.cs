using StardewModdingAPI;

namespace ConversationSystem;

public sealed class ModConfig
{
	public bool DisplayFriendShip = false;

	public float ExpMultiple = 1f;

	public float SellPriceMultiple = 1.2f;

	public float SellPriceMultipleExpensive = 3f;

	public SButton ActionKey = (SButton)81;

	public bool ArrearsOption = true;

	public bool BirthdayOption = true;

	public bool ResetSkillOption = false;

	public bool questEnable_FindingItems = true;

	public bool questEnable_PraiseSomeone = true;

	public bool questEnable_InsultSomeone = true;

	public bool questEnable_QuizGame = true;

	public bool questEnable_Loan = true;

	public int GetEnableQuestCount()
	{
		int num = 0;
		if (questEnable_FindingItems)
		{
			num++;
		}
		if (questEnable_PraiseSomeone)
		{
			num++;
		}
		if (questEnable_InsultSomeone)
		{
			num++;
		}
		if (questEnable_QuizGame)
		{
			num++;
		}
		if (questEnable_Loan)
		{
			num++;
		}
		return num;
	}
}
