using UnityEngine;
using UnityEngine.UI;

public class ButtonMashing : MonoBehaviour
{
    [Header("QTE Reference")]
    public KeyCode mashKey = KeyCode.Space;
    public float duration = 5f;
    public float decreaseSpeed = 0.2f;
    public float increaseStep = 0.0f;

    [Header("UI")]
    public Image progressBar;
    public Text logText;

    [Header("Hand")]
    public Transform hand;
    public Vector3 targetPos;

    Vector3 startPos;
    float timer;
    float progress;
    bool isRunning;
    bool finished;

    private void Start()
    {
        if (hand != null)
            startPos = hand.localPosition;
    }

    private void Update()
    {
        if(!isRunning && !finished && Input.GetKeyDown(mashKey))
        {
            StartQTE();
        }

        if(isRunning)
        {
            timer -= Time.deltaTime;

            if (Input.GetKeyDown(mashKey))
            {
                progress += increaseStep;
            }

            progress -= decreaseSpeed * Time.deltaTime;
            progress = Mathf.Clamp01(progress);

            UpdateUI();
            UpdateHand();

            if (timer <= 0)
                EndQTE();
        }
    }

    void StartQTE()
    {
        isRunning = true;
        finished = false;
        timer = duration;
        progress = 0f;

        if (progressBar != null)
            progressBar.fillAmount = 0f;

        if (hand != null)
            hand.localPosition = startPos;
    }

    void UpdateUI()
    {
        isRunning = false;
        finished = true;

        if (progress >= 0.9f)
            logText.text = "Successful!";
        else
            logText.text = "Failed!";
    }

    void UpdateHand()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, progress, 10f * Time.deltaTime);
        }

    }
    void EndQTE()
    {
        if (hand != null)
        {
            hand.localPosition = Vector3.Lerp(startPos, targetPos, progress);
        }
    }
}
