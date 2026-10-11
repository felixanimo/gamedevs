
using UnityEngine;
using UnityEngine.UI;

public class PetStats : MonoBehaviour
{
    [Header("Stat Bars")]
    public Image energyFill;
    public Image sleepFill;
    public Image moodFill;

    [Header("Stats")]
    [Range(0, 100)] public float energy = 100f;
    [Range(0, 100)] public float sleep = 100f;
    [Range(0, 100)] public float mood = 100f;

    [Header("Stat Settings")]
    public float changeSpeed = 5f;
    public float decreaseRate = 0.5f;
    public float actionDuration = 3f;

    [Header("Buttons")]
    public Button playingButton;
    public Button eatingButton;
    public Button studyingButton;
    public Button sleepingButton;
    public Button idleButton;

    [Header("Sleeping Settings")]
    public float sleepThreshold = 30f;

    [Header("Animation and Rooms")]
    public Animator leafeonAnimator;
    public GameObject dayRoom;
    public GameObject nightRoom;

    [Header("Background Music")]
    public AudioSource musicSource;
    public AudioClip dayMusic;
    public AudioClip nightMusic;

    [Header("Mood Effects")]
    public GameObject happyEffect;
    public GameObject sadEffect;
    public float moodThreshold = 50f;

    private int action = 0;
    private float timer = 0f;
    private bool isSleeping = false;

    void Start()
    {
        action = 0;
        timer = 0f;
        isSleeping = false;

        SetAnimation("idleanims");
        ShowDayRoom();
        PlayMusic(dayMusic);

        UpdateBars();
        UpdateButtons();
        UpdateMoodEffects();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Natural stat decrease
        energy -= decreaseRate * dt;
        mood -= decreaseRate * dt;

        // Sleep only decreases naturally when awake
        if (!isSleeping)
            sleep -= decreaseRate * dt;

        // Gradual activity effects
        if (action != 0)
        {
            float t = isSleeping
                ? dt
                : Mathf.Min(dt, Mathf.Max(0f, timer));

            float amount = changeSpeed * t;

            switch (action)
            {
                case 1: // Playing
                    energy -= amount;
                    sleep -= amount * 0.4f;
                    mood += amount;
                    break;

                case 2: // Eating
                    energy += amount;
                    sleep -= amount * 0.4f;
                    break;

                case 3: // Studying
                    energy -= amount;
                    sleep -= amount * 0.8f;
                    mood -= amount * 0.6f;
                    break;

                case 4: // Sleeping
                    energy += amount * 0.4f;
                    sleep += amount;
                    break;
            }

            // Non-sleeping actions only last 3 seconds
            if (!isSleeping)
            {
                timer -= t;

                if (timer <= 0f)
                {
                    action = 0;
                    timer = 0f;
                }
            }
        }

        // Keep all stats between 0 and 100
        energy = Mathf.Clamp(energy, 0f, 100f);
        sleep = Mathf.Clamp(sleep, 0f, 100f);
        mood = Mathf.Clamp(mood, 0f, 100f);

        // Automatically wake up at 100% Sleep
        if (isSleeping && sleep >= 100f)
        {
            WakeUp();
            return;
        }

        UpdateBars();
        UpdateButtons();
        UpdateMoodEffects();
    }

    void UpdateBars()
    {
        if (energyFill != null)
            energyFill.fillAmount = energy / 100f;

        if (sleepFill != null)
            sleepFill.fillAmount = sleep / 100f;

        if (moodFill != null)
            moodFill.fillAmount = mood / 100f;
    }

    void UpdateButtons()
    {
        bool lowSleep = sleep <= sleepThreshold;

        // Disable every button while sleeping
        if (isSleeping)
        {
            SetButton(playingButton, false);
            SetButton(eatingButton, false);
            SetButton(studyingButton, false);
            SetButton(sleepingButton, false);
            SetButton(idleButton, false);
            return;
        }

        // At low Sleep, only Sleeping is available
        SetButton(playingButton, !lowSleep);
        SetButton(eatingButton, !lowSleep);
        SetButton(studyingButton, !lowSleep);
        SetButton(idleButton, !lowSleep);
        SetButton(sleepingButton, lowSleep);
    }

    void SetButton(Button btn, bool enabled)
    {
        if (btn != null)
            btn.interactable = enabled;
    }

    void UpdateMoodEffects()
    {
        // Hearts appear at 50% Mood or higher
        if (happyEffect != null)
            happyEffect.SetActive(mood >= moodThreshold);

        // Rain cloud appears below 50% Mood
        if (sadEffect != null)
            sadEffect.SetActive(mood < moodThreshold);
    }

    void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    void ShowDayRoom()
    {
        if (dayRoom != null)
            dayRoom.SetActive(true);

        if (nightRoom != null)
            nightRoom.SetActive(false);
    }

    void ShowNightRoom()
    {
        if (dayRoom != null)
            dayRoom.SetActive(false);

        if (nightRoom != null)
            nightRoom.SetActive(true);
    }

    void SetAnimation(string stateName)
    {
        if (leafeonAnimator == null)
            return;

        // Reset old animation parameters
        foreach (AnimatorControllerParameter p
                 in leafeonAnimator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger)
                leafeonAnimator.ResetTrigger(p.name);

            if (p.name == "Action" &&
                p.type == AnimatorControllerParameterType.Int)
                leafeonAnimator.SetInteger("Action", 0);
        }

        leafeonAnimator.Play(stateName, 0, 0f);
    }

    public void Playing()
    {
        if (isSleeping || sleep <= sleepThreshold)
            return;

        action = 1;
        timer = actionDuration;

        SetAnimation("playinganims");
        ShowDayRoom();
        PlayMusic(dayMusic);
        UpdateButtons();
    }

    public void Eating()
    {
        if (isSleeping || sleep <= sleepThreshold)
            return;

        action = 2;
        timer = actionDuration;

        SetAnimation("eatinganims");
        ShowDayRoom();
        PlayMusic(dayMusic);
        UpdateButtons();
    }

    public void Studying()
    {
        if (isSleeping || sleep <= sleepThreshold)
            return;

        action = 3;
        timer = actionDuration;

        SetAnimation("studyinganims");
        ShowDayRoom();
        PlayMusic(dayMusic);
        UpdateButtons();
    }

    public void Sleeping()
    {
        // Only start when manually pressed at low Sleep
        if (isSleeping || sleep > sleepThreshold)
            return;

        isSleeping = true;
        action = 4;
        timer = 0f;

        SetAnimation("sleepinganims");
        ShowNightRoom();
        PlayMusic(nightMusic);
        UpdateButtons();
    }

    public void Idle()
    {
        if (isSleeping || sleep <= sleepThreshold)
            return;

        action = 0;
        timer = 0f;

        SetAnimation("idleanims");
        ShowDayRoom();
        PlayMusic(dayMusic);
        UpdateButtons();
    }

    void WakeUp()
    {
        isSleeping = false;
        action = 0;
        timer = 0f;
        sleep = 100f;

        SetAnimation("idleanims");
        ShowDayRoom();
        PlayMusic(dayMusic);

        UpdateBars();
        UpdateButtons();
        UpdateMoodEffects();

        Debug.Log("Leafeon is fully rested!");
    }
}
