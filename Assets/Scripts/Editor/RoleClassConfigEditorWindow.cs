using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RoleClassConfigEditorWindow : EditorWindow
{
    private RoleClassConfigList configList = new RoleClassConfigList();
    private Vector2 scrollPos;

    // 你可以改成自己的默认路径
    private string jsonFilePath = "Assets/Resources/Config/RoleClassConfig.json";

    [MenuItem("Tools/MMORPG/Role Class Config Editor")]
    public static void OpenWindow()
    {
        RoleClassConfigEditorWindow window = GetWindow<RoleClassConfigEditorWindow>("RoleClassConfig Editor");
        window.minSize = new Vector2(900, 600);
        window.Show();
    }

    private void OnEnable()
    {
        if (configList == null)
        {
            configList = new RoleClassConfigList();
        }
    }

    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (configList.list == null)
        {
            configList.list = new List<RoleClassConfig>();
        }

        for (int i = 0; i < configList.list.Count; i++)
        {
            DrawRoleConfigItem(configList.list[i], i);
            EditorGUILayout.Space(10);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Role Class Config Editor", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("JSON Path", GUILayout.Width(70));
        jsonFilePath = EditorGUILayout.TextField(jsonFilePath);

        if (GUILayout.Button("选择路径", GUILayout.Width(100)))
        {
            string selectedPath = EditorUtility.SaveFilePanel(
                "选择JSON保存路径",
                Application.dataPath,
                "RoleClassConfig.json",
                "json"
            );

            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    jsonFilePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("路径无效", "请选择项目 Assets 目录下的路径。", "确定");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("新增职业", GUILayout.Height(30)))
        {
            AddNewRoleConfig();
        }

        if (GUILayout.Button("从JSON读取", GUILayout.Height(30)))
        {
            LoadFromJson();
        }

        if (GUILayout.Button("导出JSON", GUILayout.Height(30)))
        {
            SaveToJson();
        }

        if (GUILayout.Button("清空全部", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认清空", "确定要清空当前所有职业配置吗？", "确定", "取消"))
            {
                configList = new RoleClassConfigList();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawRoleConfigItem(RoleClassConfig config, int index)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"职业配置 #{index + 1}", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("删除", GUILayout.Width(80)))
        {
            configList.list.RemoveAt(index);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("基础信息", EditorStyles.boldLabel);
        config.id = EditorGUILayout.IntField("ID", config.id);
        config.className = EditorGUILayout.TextField("Class Name", config.className);
        config.displayName = EditorGUILayout.TextField("Display Name", config.displayName);
        config.description = EditorGUILayout.TextField("Description", config.description);
        config.roleType = EditorGUILayout.TextField("Role Type", config.roleType);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("等级与经验", EditorStyles.boldLabel);
        config.baseLevel = EditorGUILayout.IntField("Base Level", config.baseLevel);
        config.baseExp = EditorGUILayout.IntField("Base Exp", config.baseExp);
        config.baseExpToLevel = EditorGUILayout.IntField("Base Exp To Level", config.baseExpToLevel);
        config.expGrowthRate = EditorGUILayout.FloatField("Exp Growth Rate", config.expGrowthRate);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("基础属性", EditorStyles.boldLabel);
        config.maxHp = EditorGUILayout.IntField("Max HP", config.maxHp);
        config.maxMp = EditorGUILayout.IntField("Max MP", config.maxMp);
        config.attack = EditorGUILayout.IntField("Attack", config.attack);
        config.defense = EditorGUILayout.IntField("Defense", config.defense);
        config.speed = EditorGUILayout.IntField("Speed", config.speed);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("战斗属性", EditorStyles.boldLabel);
        config.critRate = EditorGUILayout.FloatField("Crit Rate", config.critRate);
        config.critDamage = EditorGUILayout.FloatField("Crit Damage", config.critDamage);
        config.hitRate = EditorGUILayout.FloatField("Hit Rate", config.hitRate);
        config.dodgeRate = EditorGUILayout.FloatField("Dodge Rate", config.dodgeRate);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("成长属性", EditorStyles.boldLabel);
        config.hpGrowth = EditorGUILayout.FloatField("HP Growth", config.hpGrowth);
        config.mpGrowth = EditorGUILayout.FloatField("MP Growth", config.mpGrowth);
        config.attackGrowth = EditorGUILayout.FloatField("Attack Growth", config.attackGrowth);
        config.defenseGrowth = EditorGUILayout.FloatField("Defense Growth", config.defenseGrowth);
        config.speedGrowth = EditorGUILayout.FloatField("Speed Growth", config.speedGrowth);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("资源与技能", EditorStyles.boldLabel);
        config.defaultPortraitId = EditorGUILayout.TextField("Default Portrait Id", config.defaultPortraitId);

        DrawSkillIdList(config);

        EditorGUILayout.EndVertical();
    }

    private void DrawSkillIdList(RoleClassConfig config)
    {
        if (config.starterSkillIds == null)
        {
            config.starterSkillIds = new List<int>();
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Starter Skill IDs", EditorStyles.boldLabel);

        int removeIndex = -1;

        for (int i = 0; i < config.starterSkillIds.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            config.starterSkillIds[i] = EditorGUILayout.IntField($"Skill {i + 1}", config.starterSkillIds[i]);

            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
        {
            config.starterSkillIds.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("添加技能ID"))
        {
            config.starterSkillIds.Add(0);
        }

        EditorGUILayout.EndVertical();
    }

    private void AddNewRoleConfig()
    {
        if (configList.list == null)
        {
            configList.list = new List<RoleClassConfig>();
        }

        RoleClassConfig newConfig = new RoleClassConfig
        {
            id = configList.list.Count + 1,
            className = "new_class",
            displayName = "新职业",
            description = "请输入职业描述",
            roleType = "frontline",

            baseLevel = 1,
            baseExp = 0,
            baseExpToLevel = 100,
            expGrowthRate = 1.2f,

            maxHp = 100,
            maxMp = 50,
            attack = 10,
            defense = 10,
            speed = 10,

            critRate = 0.05f,
            critDamage = 1.5f,
            hitRate = 0.95f,
            dodgeRate = 0.05f,

            hpGrowth = 10f,
            mpGrowth = 5f,
            attackGrowth = 2f,
            defenseGrowth = 2f,
            speedGrowth = 0.2f,

            defaultPortraitId = "default_portrait",
            starterSkillIds = new List<int>()
        };

        configList.list.Add(newConfig);
    }

    private void SaveToJson()
    {
        if (configList == null)
        {
            configList = new RoleClassConfigList();
        }

        string json = JsonUtility.ToJson(configList, true);

        string fullPath = GetAbsolutePath(jsonFilePath);
        if (string.IsNullOrEmpty(fullPath))
        {
            EditorUtility.DisplayDialog("保存失败", "JSON 路径无效。", "确定");
            return;
        }

        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(fullPath, json);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("导出成功", $"JSON 已保存到：\n{jsonFilePath}", "确定");
    }

    private void LoadFromJson()
    {
        string fullPath = GetAbsolutePath(jsonFilePath);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("读取失败", $"文件不存在：\n{jsonFilePath}", "确定");
            return;
        }

        string json = File.ReadAllText(fullPath);
        RoleClassConfigList loaded = JsonUtility.FromJson<RoleClassConfigList>(json);

        if (loaded == null || loaded.list == null)
        {
            EditorUtility.DisplayDialog("读取失败", "JSON 解析失败，或 list 为空。", "确定");
            return;
        }

        configList = loaded;
        EditorUtility.DisplayDialog("读取成功", "已从 JSON 读取配置。", "确定");
    }

    private string GetAbsolutePath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return null;

        if (!assetPath.StartsWith("Assets"))
            return null;

        return Path.Combine(Directory.GetCurrentDirectory(), assetPath);
    }
}
