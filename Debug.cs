using System;
using StardewModdingAPI;

public static class Debug
{
	public static Action<string, LogLevel> onLogInvoke;

	static Debug()
	{
		onLogInvoke = delegate(string s, LogLevel l)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			((Mod)ThisMod.instance).Monitor.Log(s, l);
		};
	}

	public static void Log(object message, LogLevel logLevel = (LogLevel)1)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (message == null)
		{
			message = "Null";
		}
		onLogInvoke?.Invoke(message.ToString(), logLevel);
	}

	public static void LogWarring(object message)
	{
		if (message == null)
		{
			message = "Null";
		}
		onLogInvoke?.Invoke(message.ToString(), (LogLevel)3);
	}

	public static void LogError(object message)
	{
		if (message == null)
		{
			message = "Null";
		}
		onLogInvoke?.Invoke(message.ToString(), (LogLevel)4);
	}
}
