using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuRoutine : MonoBehaviour
{
    [Header("Sprites")]
    public Texture BackgroundNoLight;
    public Texture BackgroundWithLight;
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

    [Header("Introduction Texts")]
    public Text IntroText1;
    public Text IntroText2;
    public Text IntroText3;

    [Header("Variables")]
    public int ZoomPeriod;

    [Header("AudioSources")]
    public AudioSource MainAudioSource;
    public AudioSource EffectAudioSource;

    private RectTransform _menuTransform;
    private RectTransform _bgTransform;

    private RawImage _bgImage;
    private RawImage _logoImage;

    private Animator _lightAnimator;

    private float _textFadeTime = 1f;
    private float _textTime = 4f;

    private bool _menuFlag;
    private bool _soundFlag;

    public void Awake()
    {
        _bgTransform = Background.GetComponent<RectTransform>();
        _menuTransform = MainMenu.GetComponent<RectTransform>();

        _bgImage = Background.GetComponent<RawImage>();
        _logoImage = Logo.GetComponent<RawImage>();

        _lightAnimator = LightBeam.GetComponent<Animator>();
    }

    public void StartGame()
    {
        CreditsButton.SetActive(false);

        _menuTransform.DOScale(new Vector3(100.5f, 100.5f, 1), ZoomPeriod).SetEase(Ease.InCubic).OnStart(() =>
        {
            _bgTransform.DOScale(new Vector3(1.5f, 1.5f, 1), ZoomPeriod + 10f).SetEase(Ease.InCubic);

            _logoImage.DOColor(Color.clear, (float) ZoomPeriod / 2).SetDelay((float) ZoomPeriod / 2)
                .OnComplete(() =>
                {
                    IntroText1.DOFade(1, _textFadeTime).OnComplete(() =>
                    {
                        IntroText1.DOFade(0, _textFadeTime).SetDelay(_textTime)
                            .OnComplete(() =>
                            {
                                IntroText2.DOFade(1, _textFadeTime).OnComplete(() =>
                                {
                                    IntroText2.DOFade(0, _textFadeTime).SetDelay(_textTime)
                                        .OnComplete(() =>
                                        {
                                            IntroText3.DOFade(1, _textFadeTime + 3f).OnComplete(() =>
                                            {
                                                IntroText3.DOFade(0, _textFadeTime + 3f).SetDelay(_textTime)
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
        _bgImage.texture = BackgroundWithLight;
        LightBeam.SetActive(false);
    }

    public void ShowCredits()
    {
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
}
