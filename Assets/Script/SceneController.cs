using System.Collections;
using System.Collections.Generic;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SceneController : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public RawImage backgroundVideoDisplay;
    public VideoPlayer videoPlayer;

    public Image[] characterSlots; // 0:Left(Q), 1:Center(W), 2:Right(E)

    [Header("Transition Settings")]
    public TransitionType transitionType = TransitionType.Fade;
    public float transitionDuration = 2.0f;
    public bool randomTransition = false;

    [Header("Input Settings")]
    public float doubleTapThreshold = 0.3f;
    public Color selectedColor = Color.white;
    public Color unselectedColor = Color.gray;

    public List<SceneData> sceneList = new List<SceneData>();

    private int currentSceneIndex = 0;
    private bool[] isSelected;
    private Dictionary<KeyCode, float> lastKeyPressTimes = new Dictionary<KeyCode, float>();
    private Dictionary<int, CharacterData> slotMap = new Dictionary<int, CharacterData>();
    private bool isTransitioning = false;

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
        public VideoClip backgroundVideo;
        public List<SceneCharacterInstance> charactersInScene = new List<SceneCharacterInstance>();
    }

    void Start()
    {
        isSelected = new bool[characterSlots.Length];
        lastKeyPressTimes[KeyCode.Q] = 0;
        lastKeyPressTimes[KeyCode.W] = 0;
        lastKeyPressTimes[KeyCode.E] = 0;
        lastKeyPressTimes[KeyCode.Tab] = 0;

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false; // Loop
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
        }

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
        SceneData data = sceneList[currentSceneIndex];

        // 1. XỬ LÝ BACKGROUND (VIDEO vs ẢNH)
        if (data.backgroundVideo != null)
        {
            if (backgroundImage != null) backgroundImage.gameObject.SetActive(false); // Tắt khung ảnh tĩnh

            if (videoPlayer != null && backgroundVideoDisplay != null)
            {
                backgroundVideoDisplay.gameObject.SetActive(true);
                videoPlayer.clip = data.backgroundVideo;

                videoPlayer.prepareCompleted += (source) =>
                {
                    backgroundVideoDisplay.texture = source.texture;
                    source.Play();
                };
                videoPlayer.Prepare();
            }
        }
        else
        {
            // --- Chế độ Ảnh tĩnh ---
            if (videoPlayer != null) videoPlayer.Stop();
            if (backgroundVideoDisplay != null) backgroundVideoDisplay.gameObject.SetActive(false);

            if (backgroundImage != null)
            {
                backgroundImage.gameObject.SetActive(true);
                backgroundImage.sprite = data.backgroundSprite;
            }
        }

        slotMap.Clear();
        foreach (var slot in characterSlots)
        {
            slot.gameObject.SetActive(false);
            slot.sprite = null;
        }

        foreach (var charInst in data.charactersInScene)
        {
            int slotIdx = charInst.targetSlotIndex;
            if (slotIdx >= 0 && slotIdx < characterSlots.Length)
            {
                characterSlots[slotIdx].sprite = charInst.data.defaultSprite;
                characterSlots[slotIdx].preserveAspect = true; // Giữ tỉ lệ ảnh gốc

                // Set trạng thái bật/tắt
                characterSlots[slotIdx].gameObject.SetActive(charInst.startActive);

                if (!slotMap.ContainsKey(slotIdx))
                {
                    slotMap.Add(slotIdx, charInst.data);
                }
            }
        }
        Debug.Log($"Loaded Scene: {data.sceneName}");
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
        if (Input.GetKeyDown(KeyCode.G)) expIndex = 4;
        if (Input.GetKeyDown(KeyCode.H)) expIndex = 5;
        if (Input.GetKeyDown(KeyCode.J)) expIndex = 6;
        if (Input.GetKeyDown(KeyCode.K)) expIndex = 7;
        if (Input.GetKeyDown(KeyCode.L)) expIndex = 8;
        if (Input.GetKeyDown(KeyCode.Z)) expIndex = 9;
        if (Input.GetKeyDown(KeyCode.X)) expIndex = 10;
        if (Input.GetKeyDown(KeyCode.C)) expIndex = 11;
        if (Input.GetKeyDown(KeyCode.V)) expIndex = 12;
        if (Input.GetKeyDown(KeyCode.B)) expIndex = 13;
        if (Input.GetKeyDown(KeyCode.N)) expIndex = 14;
        if (Input.GetKeyDown(KeyCode.M)) expIndex = 15;


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