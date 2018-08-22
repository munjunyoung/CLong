using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKExample : MonoBehaviour
{
    public Transform lookTarget;
    [Range(0.0f, 1.0f)]
    public float lookWeight;
    [Range(0.0f, 1.0f)]
    public float bodyWeight;
    [Range(0.0f, 1.0f)]
    public float headWeight;
    [Range(0.0f, 1.0f)]
    public float eyesWeight;
    [Range(0.0f, 1.0f)]
    public float clampWeight;

    public Animator animator;

    void Start()
    {
        //animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            animator.SetBool("forward", true);
        if (Input.GetKeyUp(KeyCode.W))
            animator.SetBool("forward", false);

        if (Input.GetKeyDown(KeyCode.D))
            animator.SetBool("right", true);
        if (Input.GetKeyUp(KeyCode.D))
            animator.SetBool("right", false);

        if (Input.GetKeyDown(KeyCode.A))
            animator.SetBool("left", true);
        if (Input.GetKeyUp(KeyCode.A))
            animator.SetBool("left", false);
    }

    void OnAnimatorIK(int layerIndex)
    {
        animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
        animator.SetLookAtPosition(lookTarget.position);
    }
}
