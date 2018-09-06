using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CLongLib;

public class LobbyUIManager : Singleton<LobbyUIManager>
{
    private enum PanelMove { UP = 0, DOWN, LEFT, RIGHT, RETURN }

    public GameObject effectChangeCharacter;
    public Transform pivot;
    public GameObject noticeUI;
    [Header("Inventory Panel")]
    public GameObject[] mainCharacterList;
    public ToggleGroup mainCharacterToggleGroup;

    [Header("Matching Panel")]
    public RectTransform matchWaitingUI;
    public Text waitTime;

    private const byte _MATCH_REGISTER = 0;
    private const byte _MATCH_CANCEL = 1;

    private Coroutine _waitTimer;

    private byte _curCharacterIdx;
    private byte[] _equippedItemAry = new byte[3] { 0, 0, 0 };//{ 0xFF, 0xFF, 0xFF };

    public void OnClickMatch(bool b)
    {
        //matchWaitingUI.SetActive(b);
        if (b)
        {
            for (int i = 0; i < _equippedItemAry.Length; ++i)
            {
                if (_equippedItemAry[i] == 0xFF)
                {
                    noticeUI.SetActive(true);
                    return;
                }
            }

            if (_waitTimer != null)
                return;

            NetworkManager.Instance.SendPacket(new Queue_Req(_MATCH_REGISTER), NetworkManager.Protocol.TCP);
            _waitTimer = StartCoroutine(MatchTimer(waitTime));
            matchWaitingUI.DOAnchorPosY(-98.55f, 0.25f);
        }
        else
        {
            NetworkManager.Instance.SendPacket(new Queue_Req(_MATCH_CANCEL), NetworkManager.Protocol.TCP);
            StopCoroutine(_waitTimer);
            _waitTimer = null;
            matchWaitingUI.DOAnchorPosY(98.55f, 0.25f);
            waitTime.text = "00 : 00";
        }
    }

    public void OnClickMoveCanvas(int i)
    {
        var p = (PanelMove)i;
        switch (p)
        {
            case PanelMove.UP:
                pivot.DOLocalMoveY(1080, 0.3f);
                break;
            case PanelMove.DOWN:
                pivot.DOLocalMoveY(-1080, 0.3f);
                break;
            case PanelMove.LEFT:
                pivot.DOLocalMoveX(-1920, 0.3f);
                break;
            case PanelMove.RIGHT:
                pivot.DOLocalMoveX(1920, 0.3f);
                break;
            case PanelMove.RETURN:
                pivot.DOLocalMoveX(0, 0.3f);
                pivot.DOLocalMoveY(0, 0.3f);
                break;
        }
    }

    public void OnClickNoticeConfirm()
    {
        noticeUI.SetActive(false);
    }

    public void OnClickOption()
    {

    }

    public void OnClickExit()
    {

    }

    public void OnClickInvenOK()
    {
        NetworkManager.Instance.SendPacket(new PlayerSetting_Req(_curCharacterIdx, _equippedItemAry[0], _equippedItemAry[1], _equippedItemAry[2])
            , NetworkManager.Protocol.TCP);
        this.OnClickMoveCanvas((int)PanelMove.RETURN);
    }


    public void OnClickChangeCharacter(int idx)
    {
        mainCharacterList[_curCharacterIdx].SetActive(false);
        _curCharacterIdx = (byte)idx;
        mainCharacterList[_curCharacterIdx].SetActive(true);
    }

    public void SetEquippedItem(int idx, byte id)
    {
        _equippedItemAry[idx] = id;
    }

    protected override void Init()
    {
        _curCharacterIdx = 0;
        //OnClickChangeCharacter(0);
        DOTween.defaultEaseType = Ease.OutBack;
    }

    private IEnumerator MatchTimer(Text t)
    {
        float elapsedTime = 0;
        int count = 0;
        while (true)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= 1)
            {
                count++;
                waitTime.text = string.Format("{0} : {1}", (count / 60).ToString("D2"), (count % 60).ToString("D2"));
                elapsedTime = 0;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
