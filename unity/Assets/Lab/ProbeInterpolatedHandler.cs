using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sharplings.Lab
{
    // PolySharp-backed probe: interpolated string handlers (C# 10). PolySharp
    // supplies InterpolatedStringHandler/InterpolatedStringHandlerArgument
    // attributes. This minimal handler just concatenates.
    public class ProbeInterpolatedHandler : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
            int count = 3;
            LogTo($"count is {count}");
        }

        private static void LogTo(LabHandler handler) =>
            Debug.Log($"PASS: interpolated string handler (C# 10) ran. text = '{handler.Result}'");

        [InterpolatedStringHandler]
        private ref struct LabHandler
        {
            private string _result;
            public string Result => _result;

            public LabHandler(int literalLength, int formattedCount) => _result = "";
            public void AppendLiteral(string s) => _result += s;
            public void AppendFormatted<T>(T value) => _result += value?.ToString();
        }
    }
}
