using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public class SceneConfigWizard : EditorWindow
{
    private SceneController targetScript;
    private int step = 0;

    private int numScenes = 1;
    private int numChars = 1;

    // Data Holders
    private List<SceneController.CharacterData> masterCharacterList = new List<SceneController.CharacterData>();
    private List<int> expressionCounts = new List<int>();
    private List<SceneController.SceneData> tempSceneList = new List<SceneController.SceneData>();

    Vector2 scrollPos;
    private string[] slotOptions = new string[] { "Trái (Q)", "Giữa (W)", "Phải (E)" };

    [MenuItem("Tools/Scene Config Wizard")]
    public static void ShowWindow()
    {
        GetWindow<SceneConfigWizard>("Scene Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("SCENE CONFIGURATOR", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (targetScript == null)
        {
            targetScript = EditorGUILayout.ObjectField("Scene Controller:", targetScript, typeof(SceneController), true) as SceneController;
            if (targetScript == null)
            {
                EditorGUILayout.HelpBox("Kéo GameObject chứa SceneController vào đây!", MessageType.Info);
                return;
            }
        }

        switch (step)
        {
            case 0: DrawStep_Counts(); break;
            case 1: DrawStep_ConfigCharacters(); break;
            case 2: DrawStep_ConfigScenes(); break;
            case 3: DrawStep_Finish(); break;
        }
    }

    // --- BƯỚC 1: SỐ LƯỢNG ---
    void DrawStep_Counts()
    {
        GUILayout.Label("Bước 1: Khởi tạo số lượng", EditorStyles.label);
        numScenes = EditorGUILayout.IntField("Số Scene:", numScenes);
        numChars = EditorGUILayout.IntField("Số Nhân vật:", numChars);

        GUILayout.Space(10);
        if (GUILayout.Button("Tiếp theo: Tạo Nhân vật"))
        {
            InitializeCharacterList();
            step = 1;
        }
    }

    void InitializeCharacterList()
    {
        if (masterCharacterList.Count == 0)
        {
            for (int i = 0; i < numChars; i++)
            {
                masterCharacterList.Add(new SceneController.CharacterData { characterName = "Char " + (i + 1) });
                expressionCounts.Add(1);
            }
        }
    }

    // --- BƯỚC 2: SETUP NHÂN VẬT ---
    void DrawStep_ConfigCharacters()
    {
        GUILayout.Label("Bước 2: Cấu hình Nhân vật & Biểu cảm", EditorStyles.label);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < masterCharacterList.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            masterCharacterList[i].characterName = EditorGUILayout.TextField("Tên:", masterCharacterList[i].characterName);
            masterCharacterList[i].defaultSprite = (Sprite)EditorGUILayout.ObjectField("Ảnh gốc:", masterCharacterList[i].defaultSprite, typeof(Sprite), false);

            expressionCounts[i] = EditorGUILayout.IntField("Số biểu cảm:", expressionCounts[i]);

            // Resize list biểu cảm
            var expList = masterCharacterList[i].expressions;
            if (expressionCounts[i] != expList.Count)
            {
                while (expList.Count < expressionCounts[i]) expList.Add(null);
                while (expList.Count > expressionCounts[i]) expList.RemoveAt(expList.Count - 1);
            }

            EditorGUI.indentLevel++;
            for (int j = 0; j < expList.Count; j++)
            {
                string label = (j == 0) ? "A" : (j == 1) ? "S" : (j == 2) ? "D" : (j == 3) ? "F" : "Extra";
                expList[j] = (Sprite)EditorGUILayout.ObjectField($"Phím {label}", expList[j], typeof(Sprite), false);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Quay lại")) step = 0;
        if (GUILayout.Button("Tiếp theo: Gán vào Scene"))
        {
            InitializeSceneList();
            step = 2;
        }
        GUILayout.EndHorizontal();
    }

    void InitializeSceneList()
    {
        if (tempSceneList.Count != numScenes)
        {
            tempSceneList.Clear();
            for (int i = 0; i < numScenes; i++)
                tempSceneList.Add(new SceneController.SceneData { sceneName = "Scene " + (i + 1) });
        }
    }

    // --- BƯỚC 3: SETUP SCENE & SLOT ---
    void DrawStep_ConfigScenes()
    {
        GUILayout.Label("Bước 3: Gán Background & Xếp chỗ ngồi", EditorStyles.label);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < tempSceneList.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label($"Cấu hình Scene {i + 1} (Phím {i + 1})", EditorStyles.boldLabel);

            tempSceneList[i].sceneName = EditorGUILayout.TextField("Tên Scene:", tempSceneList[i].sceneName);

            // [NEW] Phần chọn Video hoặc Ảnh
            GUILayout.BeginHorizontal();
            GUILayout.Label("Background:", GUILayout.Width(80));
            GUILayout.BeginVertical();
            tempSceneList[i].backgroundSprite = (Sprite)EditorGUILayout.ObjectField("Ảnh tĩnh:", tempSceneList[i].backgroundSprite, typeof(Sprite), false);
            tempSceneList[i].backgroundVideo = (VideoClip)EditorGUILayout.ObjectField("Video Clip:", tempSceneList[i].backgroundVideo, typeof(VideoClip), false);

            if (tempSceneList[i].backgroundVideo != null)
                EditorGUILayout.HelpBox("Đang dùng Video (Ảnh tĩnh sẽ bị ẩn)", MessageType.Info);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label("Chọn nhân vật xuất hiện:", EditorStyles.label);

            if (tempSceneList[i].charactersInScene == null)
                tempSceneList[i].charactersInScene = new List<SceneController.SceneCharacterInstance>();

            for (int k = 0; k < masterCharacterList.Count; k++)
            {
                var masterChar = masterCharacterList[k];
                var existingConfig = tempSceneList[i].charactersInScene.FirstOrDefault(x => x.data == masterChar);
                bool isSelected = (existingConfig != null);

                GUILayout.BeginHorizontal();

                bool toggle = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                GUILayout.Label(masterChar.characterName, GUILayout.Width(100));

                if (toggle)
                {
                    if (!isSelected)
                    {
                        existingConfig = new SceneController.SceneCharacterInstance
                        {
                            data = masterChar,
                            startActive = true,
                            targetSlotIndex = 1
                        };
                        tempSceneList[i].charactersInScene.Add(existingConfig);
                    }

                    GUILayout.Label("Vị trí:", GUILayout.Width(40));
                    existingConfig.targetSlotIndex = GUILayout.Toolbar(existingConfig.targetSlotIndex, slotOptions, GUILayout.Width(200));

                    GUILayout.Space(10);
                    GUILayout.Label("Hiện sẵn:", GUILayout.Width(60));
                    existingConfig.startActive = EditorGUILayout.Toggle(existingConfig.startActive);
                }
                else
                {
                    if (isSelected) tempSceneList[i].charactersInScene.Remove(existingConfig);
                }

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Quay lại")) step = 1;
        if (GUILayout.Button("HOÀN TẤT & LƯU"))
        {
            SaveToScript();
            step = 3;
        }
        GUILayout.EndHorizontal();
    }

    void DrawStep_Finish()
    {
        EditorGUILayout.HelpBox("Đã lưu cấu hình thành công! Hãy bấm Play để kiểm tra.", MessageType.Info);
        if (GUILayout.Button("Đóng cửa sổ")) Close();
        if (GUILayout.Button("Làm lại từ đầu")) step = 0;
    }

    void SaveToScript()
    {
        Undo.RecordObject(targetScript, "Save Config");
        targetScript.sceneList = new List<SceneController.SceneData>(tempSceneList);
        EditorUtility.SetDirty(targetScript);
    }
}