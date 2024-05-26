using System;
using System.Text;
using Godot;

namespace StDBCraft.Scripts.Utils;

using static GD;

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error
}

public class Logger
{
    private readonly string _name;

    public Logger(Type target)
    {
        _name = target.Name;
    }

    private void Log(LogLevel level, string message)
    {
        Print($"[{level.ToString()}] [{_name}]: {message}");
    }

    public void Trace(params object[] what)
    {
        Log(LogLevel.Trace, AppendPrintParams(what));
    }

    public void Debug(params object[] what)
    {
        Log(LogLevel.Debug, AppendPrintParams(what));
    }

    public void Info(params object[] what)
    {
        Log(LogLevel.Info, AppendPrintParams(what));
    }

    public void Warning(params object[] what)
    {
        Log(LogLevel.Warning, AppendPrintParams(what));
    }

    public void Error(params object[] what)
    {
        Log(LogLevel.Error, AppendPrintParams(what));
    }

    private static string AppendPrintParams(object[] parameters)
    {
        if (parameters == null)
            return "null";
        var stringBuilder = new StringBuilder();
        for (var index = 0; index < parameters.Length; ++index)
            stringBuilder.Append(parameters[index]?.ToString() ?? "null").Append(' ');
        return stringBuilder.ToString();
    }
}