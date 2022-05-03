using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    // code referenced from CodeMonkey: https://www.youtube.com/watch?v=Ap8bGol7qBk
    // THIS CODE IS UNUSED

    [SerializeField]
    private Sprite[] frameArray;

    private int currentFrame;
    private float timer;
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private float framerate = .1f;

    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= framerate)
        {
            timer -= framerate;
            currentFrame = (currentFrame + 1) % frameArray.Length;
            spriteRenderer.sprite = frameArray[currentFrame];
        }
    }
}
