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
            try
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

                    try
                    {
                        var component = gameObject.AddComponent(type);
                        probe.Invoke(component, null);
                    }
                    catch (Exception exception)
                    {
                        Debug.Log($"FAIL: {type.Name}.Probe() threw: {exception.InnerException ?? exception}");
                    }
                }
            }
            catch (Exception exception)
            {
                // Log a scan-time failure (e.g. ReflectionTypeLoadException under
                // managed stripping) instead of letting it propagate unhandled;
                // the finally below still guarantees the player exits either way.
                Debug.Log($"FAIL: ProbeRuntimeRunner scan threw: {exception}");
            }
            finally
            {
                // Always exit the headless player, even if the scan or an
                // AddComponent throws — otherwise a batchmode player hangs.
                Application.Quit(0);
            }
        }
    }
}
