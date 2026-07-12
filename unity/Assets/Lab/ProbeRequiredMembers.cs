using UnityEngine;

namespace Sharplings.Lab
{
    // PolySharp smoke test: required members (C# 11). Unguarded on purpose —
    // if PolySharp's RequiredMember/SetsRequiredMembers/CompilerFeatureRequired
    // shims are not wired, this fails to compile (CS0246). No Stage 2 needed:
    // required members is within -langversion:preview; PolySharp supplies the
    // attributes Mono's BCL lacks.
    public class ProbeRequiredMembers : MonoBehaviour
    {
        [ContextMenu("Probe")]
        private void Probe()
        {
            var config = new LabConfig { Name = "Boss" };
            Debug.Log($"PASS: required members (C# 11) compiled. Name = {config.Name}");
        }

        private sealed class LabConfig
        {
            public required string Name { get; init; }
        }
    }
}
