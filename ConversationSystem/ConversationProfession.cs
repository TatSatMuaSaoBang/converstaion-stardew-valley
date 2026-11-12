using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewModdingAPI;

namespace ConversationSystem;

public class ConversationProfession : Profession
{
	private string id;

	public ConversationProfession(IModHelper helper, Skill skill, string id)
		: base(skill, id)
	{
		((Profession)this).Icon = helper.ModContent.Load<Texture2D>("assets/" + id + ".png");
		this.id = id;
	}

	public override string GetName()
	{
		return Trs.GR("Skill." + id + ".Name");
	}

	public override string GetDescription()
	{
		return Trs.GR("Skill." + id + ".Desc");
	}
}
