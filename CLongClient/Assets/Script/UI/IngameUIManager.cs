using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CLongLib;
using UnityEngine.SceneManagement;

public class IngameUIManager : Singleton<IngameUIManager>
{
    [Header("Round UI")]
    public Text curRound;
    public Text redTeamScore;
    public Text blueTeamScore;
    public GameObject winFrame;
    public GameObject lossFrame;
    public GameObject lobbyBtn;
    public Text roundTimerText;
    public Text roundResultText;

    public float timerValue = 3f;

    [Header("HP UI")]
    public Slider hpSlider;
    public RectTransform recoverHpRect;
    public RectTransform dmgHpRect;
    public Animator damagedAnim;
    
    private int _curValue = 100;
    private int _maxValue = 100;

    [Header("Aim UI")]
    public RectTransform aimRect;
    [SerializeField]
    private float _curSize
    {
        get { return aimRect.sizeDelta.x; }
        set
        {
            value = Mathf.Clamp(value, _defaultAimSize, _defaultAimSize * 3f);
            aimRect.sizeDelta = Vector2.one * value;
        }
    }
    [SerializeField]
    private float _defaultAimSize = 20;
    [SerializeField, Range(0, 1)]
    private float _aimIntensity = 0;
    [SerializeField, Range(0, 1)]
    private float _aimRecover = 0;
    
    [Header("ItemValue UI")]
    public Text ItemValueText;
    public Image ItemImage;
    private int currentValue;
    private int maxValue;

    public void OnClickLobbyBtn()
    {
        StartCoroutine(ChangeToLobby());
    }

    public void InitRound()
    {
        winFrame.SetActive(false);
        lossFrame.SetActive(false);
        lobbyBtn.SetActive(false);
        roundTimerText.gameObject.SetActive(false);
        roundResultText.gameObject.SetActive(false);
    }

    /// <summary>
    /// called when equip or change weapon
    /// </summary>
    /// <param name="initValue">aim Size</param>
    /// <param name="intense">aim intensity</param>
    /// <param name="recovery">aim recovery</param>
    public void SetBoundInfo(float initValue, float intense, float recovery)
    {
        _defaultAimSize = initValue;
        _aimIntensity = intense;
        _aimRecover = recovery;
    }

    public void SetHpValue(int value)
    {
        if (value < _curValue)
        {
            // Damaged
            var ratio = (float)value / (float)_maxValue;
            dmgHpRect.anchorMin = new Vector2(ratio, 0);
            dmgHpRect.anchorMax = new Vector2(_curValue * 0.01f, 1);
            dmgHpRect.gameObject.SetActive(true);
            recoverHpRect.gameObject.SetActive(false);
            damagedAnim.SetTrigger("Damaged");
            dmgHpRect.DOAnchorMax(new Vector2(dmgHpRect.anchorMin.x, 1), 0.5f);
        }
        else
        {
            var ratio = (float)value / (float)_maxValue;
            recoverHpRect.anchorMin = new Vector2(_curValue * 0.01f, 0);
            recoverHpRect.anchorMax = new Vector2(ratio, 1);
            recoverHpRect.gameObject.SetActive(true);
            dmgHpRect.gameObject.SetActive(false);

            recoverHpRect.DOAnchorMin(new Vector2(recoverHpRect.anchorMax.x, 0), 0.2f);
        }

        _curValue = value;
        hpSlider.value = _curValue;
        //hpSlider.DOValue(value, 0.5f, true);
    }

    public void Zoom(bool b)
    {

    }

    public void ReboundAimPoint()
    {
        _curSize *= (1+_aimIntensity);
    }

    protected override void Init()
    {
        NetworkManager.Instance.RecvHandler += ProcessPacket;
        InitRound();
    }

    private void ProcessPacket(IPacket p, NetworkManager.Protocol pt)
    {
        if (pt == NetworkManager.Protocol.UDP)
            return;

        if (p is Player_Sync)
        {
            var s = (Player_Sync)p;
            SetHpValue(s.hp);
        }
        else if (p is Player_Recover)
        {
            var s = (Player_Recover)p;
            SetHpValue(s.amount);
        }
        else if (p is Round_Stat)
        {
            var s = (Round_Stat)p;
            curRound.text = "Round " + s.curRound;
            redTeamScore.text = s.pointRed.ToString();
            blueTeamScore.text = s.pointBlue.ToString();
        }
        else if (p is Round_Result)
        {
            var s = (Round_Result)p;
            switch(s.winTeam)
            {
                case 0:
                    roundResultText.text = "<color=blue>BLUE</color> Team Win !";
                    break;
                case 1:
                    roundResultText.text = "<color=red>RED</color> Team Win !";
                    break;
            }
            roundResultText.gameObject.SetActive(true);
        }
        else if (p is Player_Reset)
        {
            var s = (Player_Reset)p;
            SetHpValue(s.hp);
            InitRound();
        }
        else if(p is Player_Init)
        {
            var s = (Player_Init)p;
            if(s.assign)
                SetHpValue(s.hp);
        }
        else if (p is Round_Timer)
        {
            var s = (Round_Timer)p;
            switch(s.countDown)
            {
                case 0:
                    roundTimerText.gameObject.SetActive(true);
                    StartCoroutine(TimerStart());
                    break;
                case 1:
                    //StartCoroutine()
                    break;
            }
        }
        else if(p is Match_End)
        {
            var s = (Match_End)p;
            
            StartCoroutine(EndMatch(s.win));
        }
    }

    IEnumerator TimerStart()
    {
        while (timerValue > 0)
        {
            roundTimerText.text = timerValue.ToString();
            timerValue--;
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerable TimerStartEnd()
    {
        roundTimerText.text = "START !";
        yield return new WaitForSeconds(1f);
        roundTimerText.gameObject.SetActive(false);
    }

    private IEnumerator EndMatch(bool isWin)
    {
        yield return new WaitForSecondsRealtime(1.0f);
        winFrame.SetActive(isWin);
        lossFrame.SetActive(!isWin);
        lobbyBtn.SetActive(true);
        roundTimerText.gameObject.SetActive(false);
        roundResultText.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
    }

    bool cursor = false;

    private void Update()
    {
        //if (Input.GetKey(KeyCode.F11))
        //    ReboundAimPoint();

        if (Input.GetKeyDown(KeyCode.F12))
            Cursor.lockState = (cursor = !cursor) ? CursorLockMode.Locked : CursorLockMode.None;

        _curSize -= Time.deltaTime * _aimRecover * 30;
    }

    private IEnumerator ChangeToLobby()
    {
        string sName = "02Lobby";
        Scene cs = SceneManager.GetActiveScene();
        var asyncOp = SceneManager.LoadSceneAsync(sName, LoadSceneMode.Additive);
        while (!asyncOp.isDone)
            yield return null;

        var scene = SceneManager.GetSceneByName(sName);
        SceneManager.MoveGameObjectToScene(GlobalManager.Instance.gameObject, scene);

        SceneManager.UnloadSceneAsync(cs);
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.RecvHandler -= ProcessPacket;
    }

    public void SetItemText(int cv, int mv)
    {
        currentValue = cv;
        maxValue = mv;
        ItemValueText.text= cv + " / " + mv;
    }
}
