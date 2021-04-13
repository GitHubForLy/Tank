using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System;
using System.IO;
using AopCore;

public class SyncAop 
{
    [InitializeOnLoadMethod]
    public static void Weave()
    {
        CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;  
    }

    private static void OnCompilationFinished(string assembly, CompilerMessage[] messages)
    {
        if (messages.Length > 0)
        {
            foreach (var msg in messages)
            {
                if (msg.type == CompilerMessageType.Error)
                {
                    return;
                }
            }
        }
        // Should not run on the editor only assemblies
        if (assembly.Contains("-Editor") || assembly.Contains(".Editor"))
        {
            return;
        }

        // Should not run on own assembly or Unity assemblies
        if (assembly.Contains("com.unity") || Path.GetFileName(assembly).StartsWith("Unity"))
        {
            return; 
        }

        if(assembly.Contains("AopCore")) 
        {
            return;
        }


        List<string> paths = new List<string>();
        var unityengine= AppDomain.CurrentDomain.GetAssemblies().First(m => m.Location.Contains("UnityEngine.CoreModule"));
        if (unityengine != null)
            paths.Add(Path.GetDirectoryName(unityengine.Location));

        WeavePamater weavePamater = new WeavePamater
        {
            SerachPaths = paths.ToArray(),
            AssemblyName = assembly,
            Notify = new WeaveNotify(),
            WeaveDependency = false,
            Symbol=SymbolFile.Mdb
        };

        Debug.Log("begin weave:" + assembly);
        var st= System.Diagnostics.Stopwatch.StartNew();
        st.Start();
        WeaveRunner.Weave(weavePamater);
        st.Stop();
        Debug.Log("finished weave:" + assembly+",     time:"+st.ElapsedMilliseconds);
    }


    class WeaveNotify : INotify
    {
        public void Notify(NotifyLevel level, string messagae)
        {
            switch(level)
            {
                case NotifyLevel.Error:
                    Debug.LogError(messagae);
                    return;
                case NotifyLevel.Message:
                    Debug.Log(messagae);
                    return;
                case NotifyLevel.Warning:
                    Debug.LogWarning(messagae);
                    return;
            }
        }
    }
}
