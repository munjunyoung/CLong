using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CLongLib;

public class LobbyUIManager : MonoBehaviour
{
    public GameObject effectChangeCharacter;
    public GameObject[] mainCharacterList;
    public Transform pivot;

    [Header("Matching Panel")]
    public RectTransform matchWaitingUI;
    public Text waitTime;

    private const byte _MATCH_REGISTER = 0;
    private const byte _MATCH_CANCEL = 1;

    private Coroutine _waitTimer;

    private GameObject _curCharacter;

    public void OnClickMatch(bool b)
    {
        //matchWaitingUI.SetActive(b);
        if (b)
        {
            if (_waitTimer != null)
                return;

            //NetworkManager.Instance.SendPacket(new Queue_Req(_MATCH_REGISTER), NetworkManager.Protocol.TCP);
            _waitTimer = StartCoroutine(MatchTimer(waitTime));
            matchWaitingUI.DOAnchorPosY(-98.55f, 0.25f);
        }
        else
        {
            //NetworkManager.Instance.SendPacket(new Queue_Req(_MATCH_CANCEL), NetworkManager.Protocol.TCP);
            StopCoroutine(_waitTimer);
            _waitTimer = null;
            matchWaitingUI.DOAnchorPosY(98.55f, 0.25f);
            waitTime.text = "00 : 00";
        }
    }

    public void OnClickMovePanel(int value)
    {
        pivot.DOLocalMoveY(value, 0.3f);
    }

    public void OnClickOption()
    {

    }

    public void OnClickExit()
    {

    }

    public void OnClickChangeCharacter(int idx)
    {
        _curCharacter.SetActive(false);
        _curCharacter = mainCharacterList[idx];
        _curCharacter.SetActive(true);
    }

    private void Awake()
    {
        _curCharacter = mainCharacterList[0];
        OnClickChangeCharacter(0);
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
