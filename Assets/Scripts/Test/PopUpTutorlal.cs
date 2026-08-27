using UnityEngine;
using UnityEngine.UI;

public class PopUpTutorlal : MonoBehaviour
{
    [SerializeField] private Image tutorialImage;
    [SerializeField] private float readDelay = 1.5f;

    [SerializeField] private KeyCode firstKey = KeyCode.Q;
    [SerializeField] private KeyCode repeatKey = KeyCode.T;

    bool isOpen = false;
    bool used = false;
    bool needDelay = false;
    float openTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        tutorialImage.enabled = false;
    }


    // Update is called once per frame
    void Update()
    {
        bool pressedFirst = Input.GetKeyDown(firstKey);
        bool pressedRepeat = Input.GetKeyDown(repeatKey);

        if (!pressedFirst && !pressedRepeat) return;

        if (isOpen)
        {
            if (!needDelay || Time.time >= openTime + readDelay)
            {
                tutorialImage.enabled = false;
                isOpen = false;
            }
            return;
            
        }   
        if (pressedRepeat || (pressedFirst && !used))
        {
            tutorialImage.enabled = true;
            isOpen = true;
            openTime = Time.time;
            used = true;
            needDelay = pressedFirst;
        }   
    }
}
