using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sharplings.Lab
{
    // PolySharp-backed probe: CallerArgumentExpression (C# 10). PolySharp
    // supplies CallerArgumentExpressionAttribute, which Mono's BCL lacks.
    public class ProbeCallerArgumentExpression : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
            int a = 2, b = 3;
            Describe(a + b);
        }

        private static void Describe(int value,
            [CallerArgumentExpression("value")] string expression = "")
        {
            Debug.Log($"PASS: CallerArgumentExpression (C# 10) ran. expr = '{expression}'");
        }
    }
}
