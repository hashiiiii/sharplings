using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sharplings.Lab
{
    // Runtime counterpart of Assets/Lab/Editor/ProbeRunner.cs. A built player
    // has no -executeMethod entry point, so this MonoBehaviour runs every Lab
    // probe on Start and quits. PlayerBuilder puts it on the bootstrap scene.
    // Uses only reflection + Debug.Log, so it stays valid C# 9 even though the
    // Lab assembly compiles at -langversion:preview.
    public sealed class ProbeRuntimeRunner : MonoBehaviour
    {
        private void Start()
        {
            var probeTypes = typeof(ProbeRuntimeRunner).Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract
                    && type != typeof(ProbeRuntimeRunner)
                    && typeof(MonoBehaviour).IsAssignableFrom(type))
                .OrderBy(type => type.Name)
                .ToArray();

            Debug.Log($"ProbeRuntimeRunner: running {probeTypes.Length} probe(s).");

            foreach (var type in probeTypes)
            {
                var probe = type.GetMethod("Probe",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (probe == null)
                {
                    Debug.Log($"NO-PROBE: {type.Name} has no private Probe(); skipped.");
                    continue;
                }

                var component = gameObject.AddComponent(type);
                try
                {
                    probe.Invoke(component, null);
                }
                catch (Exception exception)
                {
                    Debug.Log($"FAIL: {type.Name}.Probe() threw: {exception.InnerException ?? exception}");
                }
            }

            Application.Quit(0);
        }
    }
}
