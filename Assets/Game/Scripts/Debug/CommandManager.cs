using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class CommandManager
{
    private static readonly Dictionary<string, MethodInfo> Commands = new();

    // Scanne les méthodes annotées avec [ConsoleCommand] dans une classe donnée
    public static void RegisterCommands(object target)
    {
        var methods = target.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<ConsoleCommandAttribute>() != null);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
            if (attribute != null)
            {
                Commands[attribute.Command] = method;
            }
        }
    }

    // Exécute une commande avec des paramètres
    public static string ExecuteCommand(object target, string commandLine)
    {
        var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "";

        var command = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (Commands.TryGetValue(command, out var method))
        {
            try
            {
                var parameters = method.GetParameters();
                var parsedArgs = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i < args.Length)
                    {
                        parsedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
                    }
                    else if (parameters[i].HasDefaultValue)
                    {
                        parsedArgs[i] = parameters[i].DefaultValue;
                    }
                    else
                    {
                        return $"Missing argument for parameter '{parameters[i].Name}'."; // Gestion des paramètres manquants
                    }
                }

                // Appelle la méthode et retourne le résultat (si applicable)
                var result = method.Invoke(target, parsedArgs);
                return result as string;
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.InnerException?.Message ?? ex.Message}"; // Gestion des exceptions
            }
        }

        return $"Unknown command: {command}."; // Gestion des commandes inconnues
    }

    // Retourne la liste des commandes disponibles
    public static string GetCommandList()
    {
        return string.Join("\n", Commands.Select(c =>
        {
            var attribute = c.Value.GetCustomAttribute<ConsoleCommandAttribute>();
            var parameters = c.Value.GetParameters()
                .Select(p => $"{p.Name}: {p.ParameterType.Name}") // Affiche le nom et le type du paramètre
                .ToArray();
        
            var paramsString = parameters.Length > 0 ? $" {{ {string.Join(", ", parameters)} }}" : "";
        
            return $"{attribute.Command}{paramsString}: {attribute.Description}";
        }));
    }

}
