using System;
using System.Windows.Media;

namespace Shokouki.Presenters
{
    public interface IScopeView
    {
        double ScopeHeight { get; }
        double ScopeWidth { get; }
        void DrawLine(PointCollection pointCollection);
        void Invoke(Action action);
        void InvokeAsync(Action action);
        void Clear();
        void DrawLine(PointCollection crestPoints, Color red);
    }
}