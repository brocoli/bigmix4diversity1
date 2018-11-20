using System.Collections;
using System.Collections.Generic;
using Assets.Pieces;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuRoutine : MonoBehaviour
{
    [Header("Debug")]
    [InspectorButton("WinGameDebug", ButtonWidth = 200)]
    public bool WinGameDebugButton;
    [InspectorButton("LoseGameDebug", ButtonWidth = 200)]
    public bool LoseGameDebugButton;

    [Header("Sprites")]
    public Sprite BackgroundNoLight;
    public Sprite BackgroundWithLight;
    public Sprite SoundOn;
    public Sprite SoundOff;

    [Header("GameObjects")]
    public GameObject Background;
    public GameObject Foreground;
    public GameObject LightBeam;
    public GameObject FadeToWhite;
    public GameObject Logo;
    public GameObject Spawner;
    public GameObject GameOver;
    public GameObject MusicIcon;
    public GameObject SFXIcon;

    [Header("Menus")]
    public GameObject MainMenu;
    public GameObject CreditsMenu;
    public GameObject ConfigMenu;
    public GameObject QuitMenu;

    [Header("Buttons")]
    public GameObject ConfigButton;
    public GameObject CreditsButton;
    public GameObject SkipIntroButton;

    [Header("Introduction Texts")]
    public Text IntroText1;
    public Text IntroText2;
    public Text IntroText3;

    [Header("Variables")]
    public float ZoomPeriod = 15f;
    public float TextFadeTime = 1f;
    public float TextTime = 4f;
    public float ExtraDelay = 3f;

    [Header("AudioSources")]
    public AudioSource MainAudioSource;
    public AudioSource EffectAudioSource;

    private RectTransform _menuTransform;
    private Transform _bgTransform;
    private Transform _fgTransform;

    private SpriteRenderer _bgImage;
    private SpriteRenderer _fgImage;
    private RawImage _logoImage;
    private Animator _lightAnimator;

    private RawImage _gameOverImage;

    private bool _menuFlag;
    private bool _musicFlag;
    private bool _sfxFlag;

    private Tween[] _introTweens;

    public void Awake()
    {
        _bgTransform = Background.transform;
        _fgTransform = Foreground.transform;
        _menuTransform = MainMenu.GetComponent<RectTransform>();

        _bgImage = Background.GetComponent<SpriteRenderer>();
        _fgImage = Foreground.GetComponent<SpriteRenderer>();
        _logoImage = Logo.GetComponent<RawImage>();

        _lightAnimator = LightBeam.GetComponent<Animator>();

        _gameOverImage = GameOver.GetComponent<RawImage>();

        _introTweens = new Tween[10];
    }

    private void WinGameDebug()
    {
        if (EditorApplication.isPlaying)
            StartCoroutine(WinGame());
        else
            Debug.LogError("This button should only be used in play mode");
    }

    private void LoseGameDebug()
    {
        if (EditorApplication.isPlaying)
            StartCoroutine(LoseGame());
        else
            Debug.LogError("This button should only be used in play mode");
    }

    public void StartGame()
    {
        EffectAudioSource.PlayOneShot(EffectAudioSource.clip);

        _introTweens[0] = _menuTransform.DOScale(new Vector3(100.5f, 100.5f, 1), ZoomPeriod).SetEase(Ease.InCubic).OnStart(() =>
        {
            _introTweens[1] = _bgTransform.DOScale(new Vector3(4f, 4f, 1), ZoomPeriod + 10f).SetEase(Ease.InCubic);
            _introTweens[2] = _fgTransform.DOScale(new Vector3(4f, 4f, 1), ZoomPeriod + 10f).SetEase(Ease.InCubic);

            _introTweens[3] = _logoImage.DOColor(Color.clear, (float) ZoomPeriod / 2).SetDelay((float) ZoomPeriod / 2)
                .OnComplete(() =>
                {
                    SkipIntroButton.SetActive(true);
                    MainMenu.GetComponent<CanvasGroup>().interactable = false;
                    MainMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

                    _introTweens[4] = IntroText1.DOFade(1, TextFadeTime).OnComplete(() =>
                    {
                        _introTweens[5] = IntroText1.DOFade(0, TextFadeTime).SetDelay(TextTime)
                            .OnComplete(() =>
                            {
                                _introTweens[6] = IntroText2.DOFade(1, TextFadeTime).OnComplete(() =>
                                {
                                    _introTweens[7] = IntroText2.DOFade(0, TextFadeTime).SetDelay(TextTime)
                                        .OnComplete(() =>
                                        {
                                            _introTweens[8] = IntroText3.DOFade(1, TextFadeTime + ExtraDelay).OnComplete(() =>
                                            {
                                                _introTweens[9] = IntroText3.DOFade(0, TextFadeTime + ExtraDelay).SetDelay(TextTime)
                                                    .OnComplete(() =>
                                                    {
                                                        SkipIntroButton.SetActive(false);
                                                        MainMenu.SetActive(false);
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
        _fgImage.sprite = BackgroundWithLight;
        LightBeam.SetActive(false);
        Spawner.SetActive(true);
    }

    public IEnumerator WinGame()
    {
        LightBeam.SetActive(true);
        yield return new WaitForSeconds(_lightAnimator.GetCurrentAnimatorClipInfo(0).Length + 1);

        var fadeImage = FadeToWhite.GetComponent<Image>();
        fadeImage.color = new UnityEngine.Color(1.0f, 1.0f, 1.0f, 0.0f);

        FadeToWhite.SetActive(true);
        fadeImage.DOFade(1f, 1f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(1f + float.Epsilon);

        //yield return ResetAllThings();

        ShowCredits();

        yield return new WaitForSeconds(6f + float.Epsilon);

        //fadeImage.DOFade(0f, 1.5f);
        //yield return new WaitForSeconds(1.5f + float.Epsilon);

        HideCredits();

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(0);
    }

    public IEnumerator LoseGame()
    {
        var fadeImage = FadeToWhite.GetComponent<Image>();
        fadeImage.color = Color.clear;

        FadeToWhite.SetActive(true);
        fadeImage.DOFade(1f, 0.5f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(0.5f + float.Epsilon);

        _gameOverImage.DOFade(1f, 0.5f);
        yield return new WaitForSeconds(1.5f + float.Epsilon);
        
        yield return ResetAllThings();

        _gameOverImage.DOFade(0f, 0.5f);
        fadeImage.DOFade(0f, 0.5f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator ResetAllThings()
    {
        _bgImage.sprite = BackgroundNoLight;
        _fgImage.sprite = BackgroundNoLight;

        _bgTransform.transform.localScale = new Vector3(3.4f, 3.4f, 1f);
        _fgTransform.transform.localScale = new Vector3(3.4f, 3.4f, 1f);
        _menuTransform.transform.localScale = new Vector3(1f, 1f, 1f);
        LightBeam.SetActive(false);
        _logoImage.color = Color.white;

        var yReferences = GameObject.FindWithTag("YReferences");
        foreach (Transform t in yReferences.transform)
        {
            var pos = t.position;
            pos.y = -15f;
            t.position = pos;
        }
        Spawner.GetComponent<PieceRandomizer>().MaxReferenceY = -15f;

        yield return new WaitForSeconds(0.5f);

        var pieces = GameObject.FindGameObjectsWithTag("Pieces");
        foreach (var pieceObject in pieces)
        {
            GameObject.Destroy(pieceObject);
        }

        var spawnerPos = Spawner.transform.position;
        spawnerPos.y = 12.21f;
        Spawner.transform.position = spawnerPos;
        Spawner.SetActive(false);

        var cameraPos = Camera.main.transform.position;
        cameraPos.y = 0;
        Camera.main.transform.position = cameraPos;
    }

    public void OpenConfig()
    {
        EffectAudioSource.PlayOneShot(EffectAudioSource.clip);

        if (MainMenu.GetComponent<CanvasGroup>().alpha > 0 && !_menuFlag)
        {
            Time.timeScale = 0;
            ConfigButton.SetActive(false);

            _menuFlag = true;
            MainMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).SetUpdate(true)
                .OnComplete(() =>
                {
                    _menuFlag = false;
                    ConfigMenu.SetActive(true);
                    ConfigMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetUpdate(true);
                });
        }
        else if (ConfigMenu.GetComponent<CanvasGroup>().alpha > 0 && !_menuFlag)
        {
            Time.timeScale = 1;
            ConfigButton.SetActive(true);

            _menuFlag = true;
            ConfigMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).SetUpdate(true)
                .OnComplete(() =>
                {
                    _menuFlag = false;
                    MainMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetUpdate(true);
                    ConfigMenu.SetActive(false);
                });
        }
        else if (!_menuFlag)
        {
            Time.timeScale = 0;
            ConfigButton.SetActive(false);

            _menuFlag = false;
            ConfigMenu.SetActive(true);
            ConfigMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        }
        else
        {
            Time.timeScale = 1;
            ConfigButton.SetActive(true);

            _menuFlag = false;
            ConfigMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
            ConfigMenu.SetActive(false);
        }
    }

    public void ShowCredits()
    {
        EffectAudioSource.PlayOneShot(EffectAudioSource.clip);

        if (ConfigMenu.GetComponent<CanvasGroup>().alpha > 0 && !_menuFlag)
        {
            CreditsButton.SetActive(true);

            _menuFlag = true;
            ConfigMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).SetUpdate(true)
                .OnComplete(() =>
                {
                    _menuFlag = false;
                    ConfigMenu.SetActive(false);
                    CreditsMenu.SetActive(true);
                    CreditsMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetUpdate(true);
                });
        }
        else if (CreditsMenu.GetComponent<CanvasGroup>().alpha > 0 && !_menuFlag)
        {
            CreditsButton.SetActive(false);

            _menuFlag = true;
            CreditsMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).SetUpdate(true)
                .OnComplete(() =>
                {
                    _menuFlag = false;
                    ConfigMenu.SetActive(true);
                    CreditsMenu.SetActive(false);
                    ConfigMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetUpdate(true);
                });
        }
        else
        {
            CreditsMenu.SetActive(true);
            CreditsMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetUpdate(true);
        }
    }

    public void HideCredits()
    {
        CreditsMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).SetUpdate(true);
        CreditsMenu.SetActive(false);
    }


    public void MuteMusic()
    {
        if (!_musicFlag)
        {
            MusicIcon.GetComponent<Image>().sprite = SoundOff;
            MainAudioSource.mute = true;
            _musicFlag = true;
        }
        else
        {
            MusicIcon.GetComponent<Image>().sprite = SoundOn;
            MainAudioSource.mute = false;
            _musicFlag = false;
        }
    }

    public void MuteSFX()
    {
        if (!_sfxFlag)
        {
            SFXIcon.GetComponent<Image>().sprite = SoundOff;
            EffectAudioSource.mute = true;
            _sfxFlag = true;
        }
        else
        {
            SFXIcon.GetComponent<Image>().sprite = SoundOn;
            EffectAudioSource.mute = false;
            _sfxFlag = false;
        }
    }

    public void CloseGame()
    {
        EffectAudioSource.PlayOneShot(EffectAudioSource.clip);

        if (ConfigMenu.GetComponent<CanvasGroup>().alpha > 0 && !_menuFlag)
        {
            _menuFlag = true;
            ConfigMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).SetUpdate(true)
                .OnComplete(() =>
                {
                    _menuFlag = false;
                    ConfigMenu.SetActive(false);
                    QuitMenu.SetActive(true);
                    QuitMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetUpdate(true);
                });
        }
    }

    public void CloseGameConfirmation(bool doYouWantToQuit)
    {
        EffectAudioSource.PlayOneShot(EffectAudioSource.clip);

        if (doYouWantToQuit)
        {
            Debug.Log("Application.Quit()");
            Application.Quit();
        }
        else
        {
            QuitMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).SetUpdate(true)
                .OnComplete(() =>
                {
                    _menuFlag = false;
                    ConfigMenu.SetActive(true);
                    QuitMenu.SetActive(false);
                    ConfigMenu.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetUpdate(true);
                });
        }
    }

    public void SkipIntro()
    {
        foreach (var introTween in _introTweens)
        {
            introTween.Complete(false);
            introTween.Kill();
        }

        SkipIntroButton.SetActive(false);
        MainMenu.SetActive(false);
        LightBeam.SetActive(true);
        StartCoroutine(DelayChange());
    }
}
