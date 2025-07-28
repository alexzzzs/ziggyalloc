using System;
using System.Collections.Generic;

namespace ZiggyAlloc
{
    public sealed class DeferScope : IDisposable
    {
        private readonly Stack<Action> _actions = new();

        public void Add(Action action) => _actions.Push(action);

        public void Dispose()
        {
            while (_actions.Count > 0)
                _actions.Pop().Invoke();
        }

        public void Defer(Action action) => Add(action);

        public static DeferScope Start() => new();
    }
}