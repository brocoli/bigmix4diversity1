using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuRoutine : MonoBehaviour
{
    [Header("Sprites")]
    public Texture BackgroundNoLight;
    public Sprite BackgroundWithLight;
    public Sprite SoundOn;
    public Sprite SoundOff;

    [Header("GameObjects")]
    public GameObject MainMenu;
    public GameObject CreditsMenu;
    public GameObject Background;
    public GameObject LightBeam;
    public GameObject Logo;
    public GameObject SoundButton;
    public GameObject CreditsButton;
    public GameObject Spawner;

    [Header("Introduction Texts")]
    public Text IntroText1;
    public Text IntroText2;
    public Text IntroText3;

    [Header("Variables")]
    public int ZoomPeriod;
    public float TextFadeTime = 1f;
    public float TextTime = 4f;
    public float ExtraDelay = 3f;

    [Header("AudioSources")]
    public AudioSource MainAudioSource;
    public AudioSource EffectAudioSource;

    private RectTransform _menuTransform;
    private Transform _bgTransform;

    private SpriteRenderer _bgImage;
    private RawImage _logoImage;

    private Animator _lightAnimator;

    private bool _menuFlag;
    private bool _soundFlag;

    public void Awake()
    {
        _bgTransform = Background.GetComponent<RectTransform>();
        _menuTransform = MainMenu.GetComponent<RectTransform>();

        _bgImage = Background.GetComponent<SpriteRenderer>();
        _logoImage = Logo.GetComponent<RawImage>();

        _lightAnimator = LightBeam.GetComponent<Animator>();
    }

    public void StartGame()
    {
        EffectAudioSource.PlayOneShot(EffectAudioSource.clip);

        CreditsButton.SetActive(false);

        _menuTransform.DOScale(new Vector3(100.5f, 100.5f, 1), ZoomPeriod).SetEase(Ease.InCubic).OnStart(() =>
        {
            _bgTransform.DOScale(new Vector3(4f, 4f, 1), ZoomPeriod + 10f).SetEase(Ease.InCubic);

            _logoImage.DOColor(Color.clear, (float) ZoomPeriod / 2).SetDelay((float) ZoomPeriod / 2)
                .OnComplete(() =>
                {
                    IntroText1.DOFade(1, TextFadeTime).OnComplete(() =>
                    {
                        IntroText1.DOFade(0, TextFadeTime).SetDelay(TextTime)
                            .OnComplete(() =>
                            {
                                IntroText2.DOFade(1, TextFadeTime).OnComplete(() =>
                                {
                                    IntroText2.DOFade(0, TextFadeTime).SetDelay(TextTime)
                                        .OnComplete(() =>
                                        {
                                            IntroText3.DOFade(1, TextFadeTime + ExtraDelay).OnComplete(() =>
                                            {
                                                IntroText3.DOFade(0, TextFadeTime + ExtraDelay).SetDelay(TextTime)
                                                    .OnComplete(() =>
                                                    {
                                                        LightBeam.SetActive(true);
                                                        StartCoroutine(DelayChange());
                                                    });
                                            });
                                        });
                                });
                            });
                    });
                });
        });
    }

    public IEnumerator DelayChange()
    {
        yield return new WaitForSeconds(_lightAnimator.GetCurrentAnimatorClipInfo(0).Length + 3);
        _bgImage.sprite = BackgroundWithLight;
        LightBeam.SetActive(false);
        Spawner.SetActive(true);
    }

    public void ShowCredits()
    {
        EffectAudioSource.PlayOneShot(EffectAudioSource.clip);

        if (MainMenu.GetComponent<CanvasGroup>().alpha > 0 && !_menuFlag)
        {
            _menuFlag = true;
            MainMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f)
                .OnComplete(() =>
                {
                    _menuFlag = false;
                    CreditsMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
                });
        }
        else if (CreditsMenu.GetComponent<CanvasGroup>().alpha > 0 && !_menuFlag)
        {
            _menuFlag = true;
            CreditsMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f)
                .OnComplete(() =>
                {
                    _menuFlag = false;
                    MainMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
                });
        }
    }

    public void ChangeSound()
    {
        if (!_soundFlag)
        {
            SoundButton.GetComponent<Image>().sprite = SoundOff;
            MainAudioSource.mute = true;
            EffectAudioSource.mute = true;
            _soundFlag = true;
        }
        else
        {
            SoundButton.GetComponent<Image>().sprite = SoundOn;
            MainAudioSource.mute = false;
            EffectAudioSource.mute = false;
            _soundFlag = false;
        }
    }

    public void CloseGame()
    {
        EffectAudioSource.PlayOneShot(EffectAudioSource.clip);

        if (CreditsMenu.GetComponent<CanvasGroup>().alpha > 0)
        {
            CreditsMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f)
                .OnComplete(() =>
                {
                    MainMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
                });
        }
        else
        {
            Application.Quit();
        }
    }
}
