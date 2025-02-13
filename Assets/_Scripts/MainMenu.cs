﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject SongChooser;
    public LoadSongInfos SongInfos;
    public GameObject PanelAreYouSure;
    public GameObject PanelChooseTutorial;
    public GameObject PanelPictureTutorial;
    public GameObject PanelAnimationTutorial;
    public GameObject LevelChooser;
    public GameObject LevelButtonTemplate;
    public GameObject Title;
    public GameObject NoSongsFound;
    public AudioSource SongPreview;

    private SongSettings Songsettings;
    private SceneHandling SceneHandling;

    AudioClip PreviewAudioClip = null;
    bool PlayNewPreview = false;

    private void Awake()
    {
        Songsettings = GameObject.FindGameObjectWithTag("SongSettings").GetComponent<SongSettings>();
        SceneHandling = GameObject.FindGameObjectWithTag("SceneHandling").GetComponent<SceneHandling>();
    }

    public void ShowSongs()
    {
        if (SongInfos.AllSongs.Count == 0)
        {
            Title.gameObject.SetActive(false);
            NoSongsFound.gameObject.SetActive(true);
            return;
        }

        Songsettings.CurrentSong = SongInfos.AllSongs[SongInfos.CurrentSong];

        Title.gameObject.SetActive(false);
        PanelAreYouSure.gameObject.SetActive(false);
        PanelPictureTutorial.gameObject.SetActive(false);
        PanelAnimationTutorial.gameObject.SetActive(false);
        LevelChooser.gameObject.SetActive(false);
        SongChooser.gameObject.SetActive(true);
        var song = SongInfos.GetCurrentSong();

        SongInfos.SongName.text = song.Name;
        SongInfos.Artist.text = song.AuthorName;
        SongInfos.BPM.text = song.BPM;
        SongInfos.Levels.text = song.Difficulties.Count.ToString();

        byte[] byteArray = File.ReadAllBytes(song.CoverImagePath);
        Texture2D sampleTexture = new Texture2D(2, 2);
        bool isLoaded = sampleTexture.LoadImage(byteArray);

        if (isLoaded)
        {
            SongInfos.Cover.texture = sampleTexture;
        }

        StartCoroutine(PreviewSong(Songsettings.CurrentSong.AudioFilePath));
    }

    public IEnumerator PreviewSong(string audioFilePath)
    {
        SongPreview.Stop();
        PreviewAudioClip = null;
        PlayNewPreview = true;

        yield return null;

        var downloadHandler = new DownloadHandlerAudioClip(Songsettings.CurrentSong.AudioFilePath, AudioType.OGGVORBIS);
        downloadHandler.compressed = false;
        downloadHandler.streamAudio = true;
        var uwr = new UnityWebRequest(
                Songsettings.CurrentSong.AudioFilePath,
                UnityWebRequest.kHttpVerbGET,
                downloadHandler,
                null);

        var request = uwr.SendWebRequest();
        while(!request.isDone)
            yield return null;

        PreviewAudioClip = DownloadHandlerAudioClip.GetContent(uwr);
    }

    private void FixedUpdate()
    {
        if (PreviewAudioClip != null && PlayNewPreview)
        {
            PlayNewPreview = false;
            SongPreview.Stop();
            SongPreview.clip = PreviewAudioClip;
            SongPreview.time = 40f;
            SongPreview.Play();
        }
    }

    public void NextSong()
    {
        var song = SongInfos.NextSong();

        SongInfos.SongName.text = song.Name;
        SongInfos.Artist.text = song.AuthorName;
        SongInfos.BPM.text = song.BPM;
        SongInfos.Levels.text = song.Difficulties.Count.ToString();

        byte[] byteArray = File.ReadAllBytes(song.CoverImagePath);
        Texture2D sampleTexture = new Texture2D(2, 2);
        bool isLoaded = sampleTexture.LoadImage(byteArray);

        if (isLoaded)
        {
            SongInfos.Cover.texture = sampleTexture;
        }

        StartCoroutine(PreviewSong(Songsettings.CurrentSong.AudioFilePath));
    }

    public void PreviousSong()
    {
        var song = SongInfos.PreviousSong();

        SongInfos.SongName.text = song.Name;
        SongInfos.Artist.text = song.AuthorName;
        SongInfos.BPM.text = song.BPM;
        SongInfos.Levels.text = song.Difficulties.Count.ToString();

        byte[] byteArray = File.ReadAllBytes(song.CoverImagePath);
        Texture2D sampleTexture = new Texture2D(2, 2);
        bool isLoaded = sampleTexture.LoadImage(byteArray);

        if (isLoaded)
        {
            SongInfos.Cover.texture = sampleTexture;
        }

        StartCoroutine(PreviewSong(Songsettings.CurrentSong.AudioFilePath));
    }

    public void LoadSong()
    {
        SongPreview.Stop();
        var song = SongInfos.GetCurrentSong();
        if(song.Difficulties.Count > 1)
        {
            foreach (var gameObj in LevelChooser.GetComponentsInChildren<Button>(true))
            {
                if (gameObj.gameObject.name == "ButtonTemplate")
                    continue;

                Destroy(gameObj.gameObject);
            }

            SongChooser.gameObject.SetActive(false);
            PanelPictureTutorial.gameObject.SetActive(false);
            PanelAnimationTutorial.gameObject.SetActive(false);
            PanelAreYouSure.gameObject.SetActive(false);
            LevelChooser.gameObject.SetActive(true);

            var buttonsCreated = new List<GameObject>();

            foreach (var difficulty in song.Difficulties)
            {
                var button = GameObject.Instantiate(LevelButtonTemplate, LevelChooser.transform);

                button.GetComponentInChildren<Text>().text = difficulty;
                button.GetComponentInChildren<Button>().onClick.AddListener(() => StartSceneWithDifficulty(difficulty));
                button.SetActive(true);
                buttonsCreated.Add(button);
            }

            switch (buttonsCreated.Count)
            {
                case 2:
                    buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-287, buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(287, buttonsCreated[1].GetComponent<RectTransform>().localPosition.y);
                    break;
                case 3:
                    buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-287, buttonsCreated[0].GetComponent<RectTransform>().position.y);
                    buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(0, buttonsCreated[1].GetComponent<RectTransform>().position.y);
                    buttonsCreated[2].GetComponent<RectTransform>().localPosition = new Vector3(287, buttonsCreated[2].GetComponent<RectTransform>().position.y);
                    break;
                case 4:
                    buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-430, buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(-144, buttonsCreated[1].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[2].GetComponent<RectTransform>().localPosition = new Vector3(144, buttonsCreated[2].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[3].GetComponent<RectTransform>().localPosition = new Vector3(430, buttonsCreated[3].GetComponent<RectTransform>().localPosition.y);
                    break;
                case 5:
                    buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-520, buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(-264, buttonsCreated[1].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[2].GetComponent<RectTransform>().localPosition = new Vector3(0, buttonsCreated[2].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[3].GetComponent<RectTransform>().localPosition = new Vector3(260, buttonsCreated[3].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[4].GetComponent<RectTransform>().localPosition = new Vector3(520, buttonsCreated[4].GetComponent<RectTransform>().localPosition.y);          
                    break;
                case 6:
                    buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-600, buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(-360, buttonsCreated[1].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[2].GetComponent<RectTransform>().localPosition = new Vector3(-120, buttonsCreated[2].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[3].GetComponent<RectTransform>().localPosition = new Vector3(120, buttonsCreated[3].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[4].GetComponent<RectTransform>().localPosition = new Vector3(360, buttonsCreated[4].GetComponent<RectTransform>().localPosition.y);
                    buttonsCreated[5].GetComponent<RectTransform>().localPosition = new Vector3(600, buttonsCreated[5].GetComponent<RectTransform>().localPosition.y);
                    break;
                default:
                    break;
            }
            Debug.Log(buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
        }
        else
        {
            StartSceneWithDifficulty(song.Difficulties[0]);
        }
    }

    private void StartSceneWithDifficulty(string difficulty)
    {
        SongInfos.GetCurrentSong().SelectedDifficulty = difficulty;
        StartCoroutine(LoadSongScene());
    }

    private IEnumerator LoadSongScene()
    {
        yield return SceneHandling.LoadScene("OpenSaber", LoadSceneMode.Additive);
        yield return SceneHandling.UnloadScene("Menu");
    }

    public void AreYouSure()
    {
        NoSongsFound.gameObject.SetActive(false);
        Title.gameObject.SetActive(false);
        SongChooser.gameObject.SetActive(false);
        LevelChooser.gameObject.SetActive(false);
        PanelAreYouSure.gameObject.SetActive(true);
    }

    public void No()
    {
        PanelAreYouSure.gameObject.SetActive(false);
        Title.gameObject.SetActive(true);
    }

    public void Yes()
    {
        Application.Quit();
    }

    public void ShowTutorial()
    {
        NoSongsFound.gameObject.SetActive(false);
        Title.gameObject.SetActive(false);
        SongChooser.gameObject.SetActive(false);
        LevelChooser.gameObject.SetActive(false);
        PanelAreYouSure.gameObject.SetActive(false);
        PanelChooseTutorial.gameObject.SetActive(true);
       // PanelPictureTutorial.gameObject.SetActive(true);
    }

    public void ShowPictureTutorial()
    {
        NoSongsFound.gameObject.SetActive(false);
        Title.gameObject.SetActive(false);
        SongChooser.gameObject.SetActive(false);
        LevelChooser.gameObject.SetActive(false);
        PanelAreYouSure.gameObject.SetActive(false);
        PanelChooseTutorial.gameObject.SetActive(false);
        PanelPictureTutorial.gameObject.SetActive(true);
    }

    public void ShowAnimationTutorial()
    {
        NoSongsFound.gameObject.SetActive(false);
        Title.gameObject.SetActive(false);
        SongChooser.gameObject.SetActive(false);
        LevelChooser.gameObject.SetActive(false);
        PanelAreYouSure.gameObject.SetActive(false);
        PanelChooseTutorial.gameObject.SetActive(false);
        PanelPictureTutorial.gameObject.SetActive(false);
        PanelAnimationTutorial.gameObject.SetActive(true);

    }

    public void confirm()
    {
        PanelPictureTutorial.gameObject.SetActive(false);
        PanelAnimationTutorial.gameObject.SetActive(false);
        Title.gameObject.SetActive(true);
    }

    private IEnumerator LoadTutorialScene()
    {
        yield return SceneHandling.LoadScene("OpenSaber", LoadSceneMode.Additive);
        yield return SceneHandling.LoadScene("TutorialScene", LoadSceneMode.Additive);
        yield return SceneHandling.UnloadScene("Menu");
    }
}
