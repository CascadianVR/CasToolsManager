#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class CasToolsManager : EditorWindow
{
    private static List<ToolItem> toolItems = new List<ToolItem>();

    private Vector2 scrollPos;
    
    [MenuItem("Cascadian/CasToolsManager")]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        CasToolsManager window = (CasToolsManager)GetWindow(typeof(CasToolsManager));
        window.Show();
        window.minSize = new Vector2(800, 400);

        Setup();
    }

    private void OnValidate()
    {
        Setup();
    }

    private static void Setup()
    {
        toolItems = new List<ToolItem>();
        
        dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(Application.dataPath + "/CasToolsManager/ToolItems.json"));

        if (data == null) return;
        
        foreach (var tool in data.tools)
        {
            var toolItem = new ToolItem((string)tool.name, (string)tool.url);
            toolItems.Add(toolItem);
        }
    }
    
    private void OnGUI()
    {
        using (var horizontalScope = new EditorGUILayout.HorizontalScope("box"))
        {
            using (var verticalScope = new EditorGUILayout.VerticalScope("box"))
            {
                using (var horizontalScope2 = new EditorGUILayout.HorizontalScope("box"))
                {
                    var style = new GUIStyle("label");
                    style.richText = true;
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("<size=20><b>CasToolsManager</b></size>", style);
                    GUILayout.FlexibleSpace();
                }
            }
            using (var verticalScope = new EditorGUILayout.VerticalScope("box"))
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos ,  GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false), GUILayout.Width(520));
                foreach (var tool in toolItems)
                {
                    var filePath = Application.dataPath + "/CasToolsManager/" + tool.name + "/" + tool.name + ".cs";

                    if (DoesToolExist(filePath))
                    {
                        GUILayout.BeginHorizontal(new GUIStyle("AC BoldHeader"), GUILayout.Width(500));
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical(GUILayout.Height(60));
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(tool.name + " | Installed");
                        GUILayout.FlexibleSpace();
                        GUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    else if (GUILayout.Button(tool.name, GUILayout.Width(500), GUILayout.Height(60)))
                    {
                        ImportTool(tool);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }

    private void ImportTool(ToolItem tool)
    {
        using (WebClient wc = new WebClient())
        {
            CheckDirectory(Application.dataPath + "/CasToolsManager/" + tool.name);
            
            try {
                wc.DownloadFile(
                    tool.url,
                    Application.dataPath + "/CasToolsManager/" + tool.name + "/" + tool.name + ".cs"
                );
            }
            catch (Exception ex) {
                Debug.LogError(ex.ToString());
            }
        }
        
        AssetDatabase.Refresh();
    }
    
    void CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    bool DoesToolExist(string path)
    {
        return File.Exists(path);
    }
    
    class ToolItem
    {
        public string name;
        public string url;

        public ToolItem(string name, string url)
        {
            this.name = name;
            this.url = url;
        }
    }
}

#endif