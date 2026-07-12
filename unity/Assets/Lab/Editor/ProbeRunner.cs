using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sharplings.Lab.Editor
{
    // Headless entry point for the Lab probes.
    //
    // EN: Every probe in `Assets/Lab/` is a MonoBehaviour whose private
    //     `Probe()` is normally invoked by hand via its `[ContextMenu]`
    //     entry. This runner invokes them all in one headless pass:
    //
    //         Unity -batchmode -nographics -projectPath unity \
    //           -executeMethod Sharplings.Lab.Editor.ProbeRunner.RunAll \
    //           -quit -logFile <file>
    //
    //     then read the PASS / SKIP / FAIL lines from the log. This file
    //     deliberately uses only C# 9: it lives in its own editor-only
    //     assembly (`Sharplings.Lab.Editor.asmdef`), which has no
    //     `csc.rsp`, so it compiles under Unity's stock language version
    //     regardless of what stage the Lab itself is at.
    // JA: `Assets/Lab/` の各 probe は MonoBehaviour で、private の
    //     `Probe()` を `[ContextMenu]` から手動で呼ぶのが通常の使い方
    //     です。この runner はそれらを headless で一括実行します:
    //
    //         Unity -batchmode -nographics -projectPath unity \
    //           -executeMethod Sharplings.Lab.Editor.ProbeRunner.RunAll \
    //           -quit -logFile <file>
    //
    //     実行後、log の PASS / SKIP / FAIL 行を読んでください。この
    //     ファイルは意図的に C# 9 の範囲だけで書いています。専用の
    //     editor 専用 assembly（`Sharplings.Lab.Editor.asmdef`）に置かれ、
    //     そこには `csc.rsp` が無いため、Lab 本体がどの stage にあっても
    //     Unity の stock な言語バージョンでコンパイルされます。
    public static class ProbeRunner
    {
        public static void RunAll()
        {
            var host = new GameObject("Sharplings.Lab.ProbeRunner");
            try
            {
                var probeTypes = typeof(ProbeExtensionMembers).Assembly
                    .GetTypes()
                    .Where(type => !type.IsAbstract && typeof(MonoBehaviour).IsAssignableFrom(type))
                    .OrderBy(type => type.Name)
                    .ToArray();

                Debug.Log($"ProbeRunner: running {probeTypes.Length} probe(s).");

                foreach (var type in probeTypes)
                {
                    var probe = type.GetMethod("Probe", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (probe == null)
                    {
                        Debug.Log($"NO-PROBE: {type.Name} has no private Probe() method; skipped.");
                        continue;
                    }

                    var component = host.AddComponent(type);
                    try
                    {
                        probe.Invoke(component, null);
                    }
                    catch (Exception exception)
                    {
                        Debug.Log($"FAIL: {type.Name}.Probe() threw: {exception.InnerException ?? exception}");
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }
    }
}
