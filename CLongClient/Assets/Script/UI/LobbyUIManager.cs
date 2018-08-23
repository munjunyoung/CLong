using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyUIManager : MonoBehaviour
{
    public Button btnMatch;
    public Button btnMathCancel;
    public Button btnShop;
    public Button btnOption;
    public Button btnExit;

    public GameObject matchWaitingUI;
    public Transform pivot;

    public void OnClickMatch(bool b)
    {
        matchWaitingUI.SetActive(b);
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

    private void Awake()
    {
        DOTween.defaultEaseType = Ease.OutBack;
    }

    private IEnumerator MatchTimer()
    {
        yield return new WaitForSecondsRealtime(1.0f);
    }
}
