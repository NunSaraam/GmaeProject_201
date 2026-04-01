using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance {get; private set;}

    [Header("Dialog References")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;

    [SerializeField] private Image portraitImage;

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;


    [Header("Dialog Settings")]
    [SerializeField] private float typingSpeed = .5f;
    [SerializeField] private bool useTypewriterEffect = true;

    private bool isTyping = false;
    private Coroutine typingCoroutine;                  //코루틴 선언

    private DialogSO currentDialog;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dialogDatabase != null)
        {
            dialogDatabase.Init();              //초기화
        }
        else
        {
            Debug.LogError("Dialog Database is not assinged to Dialog Manager");
        }

        if (NextButton)
        {
            NextButton.onClick.AddListener(NextDialog);
        }
        else
        {
            Debug.LogError("Next Button is not assinged");
        }
    }

    private void Start()
    {
        CloseDialog();
        StartDialog(1);
    }

    // ID로 대화 시작
    public void StartDialog(int dialogID)
    {
        DialogSO dialog = dialogDatabase.GetDialogByld(dialogID);

        if (dialog != null)
        {
            StartDialog(dialog);
        }
        else
        {
            Debug.LogError($"Dialog with ID {dialogID} not found!");
        }
    }

    // DialogSO로 대화 시작
    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDialog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }

    public void ShowDialog()
    {
        if (currentDialog == null) return;

        characterNameText.text = currentDialog.characterName;            // 캐릭터 이름 설정

        if (useTypewriterEffect)
        {
            StartTypingEffect(currentDialog.text);

        }
        else
        {
            dialogText.text = currentDialog.text;
        }

        // 초상화 설정 (새로 추가된 부분)
        if (currentDialog.portrait != null)
        {
            portraitImage.sprite = currentDialog.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(currentDialog.portaitPath))
        {
            //Resources 폴더에서 이미지 로드 (Assets/Resources/Chracters/Narrator.png)
            Sprite portrait = Resources.Load<Sprite>(currentDialog.portaitPath);
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Portrait not found at path : {currentDialog.portaitPath}");
                portraitImage.gameObject.SetActive(false);
            }
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    public void CloseDialog()                                            // 대화 종료
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
        StopTypingEffect();
    }

    public void NextDialog()
    {
        if (isTyping)
        {
            StopTypingEffect();
            dialogText.text = currentDialog.text;
            isTyping = false;
            return;
        }

        if (currentDialog != null && currentDialog.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogByld(currentDialog.nextId);

            if (nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }

    // 텍스트 타이핑 효과 코루틴
    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";

        foreach (char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    // 타이핑 효과 함수 시작
    private void StartTypingEffect(string text)
    {
        isTyping = true;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }
}
