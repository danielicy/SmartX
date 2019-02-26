using System;

namespace DBTools.ScriptElements
{
    public interface IScriptElement
    {
        string Filename { get; }
        int OrderNumber { get; }
        DateTime Date { get; }

        void Execute();
    }
}
