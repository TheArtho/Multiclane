using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class ConsoleCommandAttribute : Attribute
{
    public string Command { get; }
    public string Description { get; }

    public ConsoleCommandAttribute(string command, string description = "")
    {
        Command = command.ToLower();
        Description = description;
    }
}