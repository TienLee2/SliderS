using System.Collections;
using System.Collections.Generic;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public Image[] characterSlots; // 0:Left(Q), 1:Center(W), 2:Right(E)

    [Header("Transition Settings")]
    public TransitionType transitionType = TransitionType.Fade;
    public float transitionDuration = 2.0f;
    public bool randomTransition = false;

    [Header("Input Settings")]
    public float doubleTapThreshold = 0.3f;
    public Color selectedColor = Color.white;
    public Color unselectedColor = Color.gray;

    // --- DỮ LIỆU ---
    [HideInInspector] // Ẩn đi để dùng Wizard config cho gọn
    public List<SceneData> sceneList = new List<SceneData>();

    // --- RUNTIME STATE ---
    private int currentSceneIndex = 0;
    private bool[] isSelected;
    private Dictionary<KeyCode, float> lastKeyPressTimes = new Dictionary<KeyCode, float>();
    private Dictionary<int, CharacterData> slotMap = new Dictionary<int, CharacterData>();
    private bool isTransitioning = false;

    // --- CẤU TRÚC DỮ LIỆU ---
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public Sprite defaultSprite;
        public List<Sprite> expressions = new List<Sprite>();
    }

    [System.Serializable]
    public class SceneCharacterInstance
    {
        public CharacterData data;
        public bool startActive = true;
        public int targetSlotIndex = 1; // 0=Q, 1=W, 2=E
    }

    [System.Serializable]
    public class SceneData
    {
        public string sceneName;
        public Sprite backgroundSprite;
        public List<SceneCharacterInstance> charactersInScene = new List<SceneCharacterInstance>();
    }

    void Start()
    {
        isSelected = new bool[characterSlots.Length];
        lastKeyPressTimes[KeyCode.Q] = 0;
        lastKeyPressTimes[KeyCode.W] = 0;
        lastKeyPressTimes[KeyCode.E] = 0;
        lastKeyPressTimes[KeyCode.Tab] = 0;

        // Load scene đầu tiên ngay lập tức (không effect)
        LoadSceneDataImmediately(0);
        SelectOnly(0);
    }

    void Update()
    {
        if (isTransitioning) return;

        HandleSceneInput();
        HandleSelectionAndToggleInput();
        HandleExpressionInput();
        UpdateSelectionVisuals();
    }

    // --- LOGIC CHUYỂN CẢNH (TRANSITION) ---
    void RequestLoadScene(int index)
    {
        if (index < 0 || index >= sceneList.Count) return;
        if (index == currentSceneIndex) return;

        StartCoroutine(TransitionRoutine(index));
    }

    IEnumerator TransitionRoutine(int index)
    {
        isTransitioning = true;

        TransitionType typeToUse = randomTransition ? GetRandomTransition() : transitionType;
        TransitionAnimator.Start(typeToUse, duration: transitionDuration);

        yield return new WaitForSeconds(transitionDuration);

        LoadSceneDataImmediately(index);

        yield return new WaitForSeconds(transitionDuration);

        isTransitioning = false;
    }

    TransitionType GetRandomTransition()
    {
        TransitionType[] niceTypes = {
            TransitionType.Fade, TransitionType.Wipe, TransitionType.Dissolve,
            TransitionType.Pixelate, TransitionType.Mosaic, TransitionType.Burn
        };
        return niceTypes[Random.Range(0, niceTypes.Length)];
    }

    // --- LOGIC LOAD DỮ LIỆU ---
    void LoadSceneDataImmediately(int index)
    {
        currentSceneIndex = index;

        // Đổi background
        if (backgroundImage != null)
            backgroundImage.sprite = sceneList[currentSceneIndex].backgroundSprite;

        // Reset Slot 
        slotMap.Clear();
        foreach (var slot in characterSlots)
        {
            slot.gameObject.SetActive(false);
            slot.sprite = null;
        }

        // Setup nhân vật mới 
        var currentSceneChars = sceneList[currentSceneIndex].charactersInScene;
        foreach (var charInst in currentSceneChars)
        {
            int slotIdx = charInst.targetSlotIndex;
            if (slotIdx >= 0 && slotIdx < characterSlots.Length)
            {
                // Gán ảnh
                characterSlots[slotIdx].sprite = charInst.data.defaultSprite;
                characterSlots[slotIdx].preserveAspect = true; // Giữ tỉ lệ ảnh gốc

                // Set trạng thái bật/tắt
                characterSlots[slotIdx].gameObject.SetActive(charInst.startActive);

                // Lưu vào map để điều khiển biểu cảm sau này
                if (!slotMap.ContainsKey(slotIdx))
                {
                    slotMap.Add(slotIdx, charInst.data);
                }
            }
        }
        Debug.Log($"Loaded Scene: {sceneList[index].sceneName}");
    }

    // --- XỬ LÝ INPUT ---
    void HandleSceneInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) RequestLoadScene(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) RequestLoadScene(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) RequestLoadScene(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) RequestLoadScene(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) RequestLoadScene(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) RequestLoadScene(5);
    }

    void HandleSelectionAndToggleInput()
    {
        CheckInputForKey(KeyCode.Q, 0);
        CheckInputForKey(KeyCode.W, 1);
        CheckInputForKey(KeyCode.E, 2);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (IsDoubleTap(KeyCode.Tab)) ToggleAllVisibility(); else SelectAll();
            lastKeyPressTimes[KeyCode.Tab] = Time.time;
        }
    }

    void HandleExpressionInput()
    {
        int expIndex = -1;
        if (Input.GetKeyDown(KeyCode.A)) expIndex = 0;
        if (Input.GetKeyDown(KeyCode.S)) expIndex = 1;
        if (Input.GetKeyDown(KeyCode.D)) expIndex = 2;
        if (Input.GetKeyDown(KeyCode.F)) expIndex = 3;

        if (expIndex != -1)
        {
            for (int i = 0; i < characterSlots.Length; i++)
            {
                if (isSelected[i] && characterSlots[i].gameObject.activeSelf)
                    SetExpressionForSlot(i, expIndex);
            }
        }
    }

    // --- HELPER ---
    void CheckInputForKey(KeyCode key, int slotIndex)
    {
        if (Input.GetKeyDown(key))
        {
            if (IsDoubleTap(key))
            {
                // Double Tap: Tắt/Mở nếu có nhân vật
                if (characterSlots[slotIndex].sprite != null)
                {
                    bool isActive = characterSlots[slotIndex].gameObject.activeSelf;
                    characterSlots[slotIndex].gameObject.SetActive(!isActive);
                }
            }
            else
            {
                // Single Tap: Chọn nhân vật
                SelectOnly(slotIndex);
                // Nếu đang tắt mà bấm chọn -> Tự động bật lên
                if (!characterSlots[slotIndex].gameObject.activeSelf && characterSlots[slotIndex].sprite != null)
                    characterSlots[slotIndex].gameObject.SetActive(true);
            }
            lastKeyPressTimes[key] = Time.time;
        }
    }

    void SetExpressionForSlot(int slotIndex, int expIndex)
    {
        if (slotMap.ContainsKey(slotIndex))
        {
            CharacterData charData = slotMap[slotIndex];
            if (expIndex < charData.expressions.Count && charData.expressions[expIndex] != null)
            {
                characterSlots[slotIndex].sprite = charData.expressions[expIndex];
            }
        }
    }

    bool IsDoubleTap(KeyCode key) => (Time.time - lastKeyPressTimes[key]) <= doubleTapThreshold;

    void SelectOnly(int targetIndex)
    {
        for (int i = 0; i < isSelected.Length; i++) isSelected[i] = (i == targetIndex);
    }

    void SelectAll()
    {
        for (int i = 0; i < isSelected.Length; i++) isSelected[i] = true;
    }

    void ToggleAllVisibility()
    {
        bool anyActive = false;
        foreach (var slot in characterSlots) if (slot.gameObject.activeSelf) anyActive = true;
        bool targetState = !anyActive;
        foreach (var slot in characterSlots)
        {
            if (slot.sprite != null) slot.gameObject.SetActive(targetState);
        }
    }

    void UpdateSelectionVisuals()
    {
        for (int i = 0; i < characterSlots.Length; i++)
            characterSlots[i].color = isSelected[i] ? selectedColor : unselectedColor;
    }
}