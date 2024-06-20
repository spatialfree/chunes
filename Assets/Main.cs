using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;
using System.IO;
using B83.Win32;
using TMPro;
using NaughtyAttributes;
using Random = UnityEngine.Random;
using UnityEngine.Android;
using SystemVolume;
using UnityEngine.Networking;
using UnityEngine.Audio;

public class Main : MonoBehaviour
{
  public string folderPath;
  public string coverPath;

  [Header("References")]
  public AudioSource musicSource;

  public List<Album> albums = new List<Album>();
  public List<PlayQueue> queue;
  public int queueIndex;
  public string debug;
  UnityWebRequest w;
  public float volume = 0;
  public Font font;

  [Serializable]
  public class PlayQueue
  {
    public Track track;
    public float start, stop;

    public PlayQueue(Track track, float start, float stop)
    {
      this.track = track;
      this.start = start;
      this.stop = stop;
    }
  }

  void Start()
  {
#if UNITY_EDITOR
    folderPath = "C:/Users/cadet/Music";
    musicSource.volume = 0.1f;
#endif
    Refresh();

    volumeController = new SystemVolumeController();

    inputField.text = folderPath;


    style.font = font;
  }
  GUIStyle style = new GUIStyle();

  List<string> explorer = new List<string>();
  public TMP_InputField inputField;

  public void GOTO()
  {
    explorer = new List<string>(Directory.GetDirectories(inputField.text));
  }




  SystemVolumeController volumeController;

  void Update()
  {
    volume = volumeController.Volume;
    if (volume > 0)
    {
      if (queue.Count > 0)
      {
        if (musicSource.clip == null)
        {
          // w = UnityWebRequestMultimedia.GetAudioClip(queue[queueIndex].track, AudioType.OGGVORBIS);
          // DownloadHandlerAudioClip.GetContent(w);
          musicSource.clip = queue[queueIndex].track.clip;
          musicSource.time = queue[queueIndex].start;
          musicSource.Play();
        }

        if (musicSource.time >= queue[queueIndex].stop)
        {
          // auto loop... no
          if (queueIndex < queue.Count - 1)
          {
            queueIndex++;
            musicSource.clip = null;
          }
          else
          {
            musicSource.Pause();
          }
        }

        // debug = Path.GetFileName(queue[queueIndex].track);
      }
    }
    else
    {
      musicSource.Pause();
    }


    // volume = 
    // if (AutoPress(KeyCode.Comma))
    // {
    //   musicSource.volume -= 0.1f;
    // }

    Vector3 cursorPos = Input.mousePosition;
    cursorPos.y = Screen.height - cursorPos.y;
    tapDelta += Time.deltaTime;
    if (Input.GetMouseButtonDown(0))
    {
      for (int i = 0; i < albums.Count; i++)
      {
        bool inBounds = cursorPos.x > albums[i].pos.x && cursorPos.x < albums[i].pos.x + albums[i].size.x * pixelWidth &&
                        cursorPos.y > albums[i].pos.y && cursorPos.y < albums[i].pos.y + albums[i].size.y * pixelWidth;
        if (inBounds)
        {
          grabOffset = (Vector3)albums[i].pos - cursorPos;
          selectedIndex = i;
          break;
        }
      }

      if (tapDelta > 0.5f)
      {
        tapDelta = 0;
      }
      else
      {
        if (selectedIndex > -1)
        {
          albums[selectedIndex].flipped = !albums[selectedIndex].flipped;
          PlayQueue newQueue = new PlayQueue(albums[selectedIndex].tracks[0], 0, albums[selectedIndex].tracks[0].length);
          queue.Add(newQueue);
        }
      }
    }

    if (Input.GetMouseButton(0))
    {
      // if move fast enough then move up to 0
      // otherwise keep index

      if (selectedIndex > -1)
      {
        albums[selectedIndex].pos = cursorPos + grabOffset;

        if ((cursorPos - oldCursorPos).magnitude / Time.deltaTime > 600)
        {
          Album tempAlbum = albums[selectedIndex];
          albums.RemoveAt(selectedIndex);
          albums.Insert(0, tempAlbum);
          selectedIndex = 0;
        }
      }
    }

    if (Input.GetMouseButtonUp(0))
    {
      selectedIndex = -1;
    }

    oldCursorPos = cursorPos;
  }

  Vector3 grabOffset;
  Vector3 oldCursorPos;
  float tapDelta;
  List<Drawn> selectedList;
  int selectedIndex = -1;
  public int pixelWidth = 32; // pixels per unit


  public interface Interact
  {
    void Tap();

    void Hold(Vector2 toPos);

    void Lift();
  }

  [Serializable]
  public class Drawn : Interact
  {
    public Vector2 pos;
    public Vector2 size;
    public Texture2D tex;
    public bool interactable;

    public void DoubleTap()
    {

    }

    public void Tap()
    {
      throw new NotImplementedException();
    }

    public void Hold(Vector2 toPos)
    {
      throw new NotImplementedException();
    }

    public void Lift()
    {
      throw new NotImplementedException();
    }
  }

  [Serializable]
  public class Album : Drawn
  {
    public string directory;
    public List<Track> tracks;
    public float longestTrack;
    public bool flipped;

    public Album(Vector2 pos, Vector2 size, Texture2D tex, string directory, List<Track> tracks, float longestTrack)
    {
      this.pos = pos;
      this.size = size;
      this.tex = tex;
      this.directory = directory;
      this.tracks = tracks;
      this.longestTrack = longestTrack;

      this.interactable = true;
    }
  }

  [Serializable]
  public class Track
  {
    public string path;
    public float length;
    public AudioClip clip;

    public Track(string path, float length, AudioClip clip)
    {
      this.path = path;
      this.length = length;
      this.clip = clip;
    }
  }

  public Texture2D flippedTexture;
  public Drawn slide, knob;
  public Color trackBG;
  void OnGUI()
  {
    if (Event.current.type.Equals(EventType.Repaint))
    {
      for (int i = albums.Count - 1; i >= 0; i--)
      {
        if (albums[i].flipped)
        {
          // Graphics.DrawTexture(new Rect(albums[i].pos.x, albums[i].pos.y, albums[i].size.x * pixelWidth, albums[i].size.y * pixelWidth), flippedTexture);
          GUIDrawRect(new Rect(albums[i].pos.x, albums[i].pos.y, albums[i].size.x * pixelWidth, albums[i].size.y * pixelWidth), Color.black);
          GUI.Label(new Rect(albums[i].pos.x + 10, albums[i].pos.y + 10, (albums[i].size.x * pixelWidth) - 10, 20), Path.GetFileName(albums[i].directory));

          for (int j = 0; j < albums[i].tracks.Count; j++)
          {
            float l = albums[i].tracks[j].length / albums[i].longestTrack;
            GUIDrawRect(new Rect(albums[i].pos.x, albums[i].pos.y + 30 + (20 * j), (albums[i].size.x * pixelWidth) * l, 20), trackBG);
            GUI.Label(new Rect(albums[i].pos.x + 10, albums[i].pos.y + 30 + (20 * j), (albums[i].size.x * pixelWidth) - 10, 20), Path.GetFileNameWithoutExtension(albums[i].tracks[j].path), style);
          }
        }
        else
        {
          Graphics.DrawTexture(new Rect(albums[i].pos.x, albums[i].pos.y, albums[i].size.x * pixelWidth, albums[i].size.y * pixelWidth), albums[i].tex);
        }
      }

      Graphics.DrawTexture(new Rect(slide.pos.x, slide.pos.y, Screen.width, knob.size.y * pixelWidth), slide.tex);
      Graphics.DrawTexture(new Rect((Screen.width - (knob.size.x * pixelWidth)) * volume, 0, knob.size.x * pixelWidth, knob.size.y * pixelWidth), knob.tex);
    }

    for (int i = 0; i < explorer.Count; i++)
    {
      GUI.Label(new Rect(10, 256 + (i * 20), 720, 20), explorer[i]);
    }
  }


  public void Refresh()
  {
    albums.Clear();
    List<string> folders = new List<string>(Directory.GetDirectories(folderPath));
    // tracks = new List<string>(Directory.GetFiles(folderPath, "*.ogg", SearchOption.AllDirectories));
    // trackIndex = 0;

    for (int i = 0; i < folders.Count; i++)
    {
      // Auto Replace jpg with png
      // string[] jpg = Directory.GetFiles(folders[i], "*.jpg");
      // if (jpg.Length > 0)
      // {
      //   w = UnityWebRequestTexture.GetTexture(jpg[0]);
      //   yield return w.SendWebRequest();
      //   Texture2D notJPG = new Texture2D(700, 700, TextureFormat.ARGB32, 0, false);
      //   notJPG.wrapMode = TextureWrapMode.Clamp;
      //   notJPG = DownloadHandlerTexture.GetContent(w);
      //   notJPG.Apply();

      //   byte[] bytes = notJPG.EncodeToPNG();
      //   File.WriteAllBytes(folders[i] + "/cover.png", bytes);

      //   File.Delete(jpg[0]);
      // }

      // w = UnityWebRequestTexture.GetTexture(folders[i] + "/cover.png");
      // yield return w.SendWebRequest();
      Texture2D tex = new Texture2D(700, 700, TextureFormat.ARGB32, 0, false);
      tex.wrapMode = TextureWrapMode.Clamp;
      tex.LoadImage(File.ReadAllBytes(folders[i] + "/cover.png"));
      // tex = DownloadHandlerTexture.GetContent(w);
      tex.Apply();

      List<string> trackPaths = new List<string>(Directory.GetFiles(folders[i], "*.ogg"));
      List<Track> newTracks = new List<Track>();
      float longestTrack = 0;
      for (int j = 0; j < trackPaths.Count; j++)
      {
        // w = UnityWebRequestMultimedia.GetAudioClip(trackPaths[j], AudioType.OGGVORBIS);
        // yield return w.SendWebRequest();
        // AudioClip newAudioClip = DownloadHandlerAudioClip.GetContent(w);

        // WWW www = new WWW(trackPaths[j]);
        // yield return www.isDone;
        // AudioClip newAudioClip = www.GetAudioClip();

        // byte[] byt = File.ReadAllBytes(trackPaths[j]);
        // float[] _flo = new float[byt.Length / 4];
        // for (int k = 0; k < _flo.Length; k++)
        // {
        //   if (BitConverter.IsLittleEndian)
        //     Array.Reverse(byt, k * 4, 4);

        //   _flo[k] = BitConverter.ToSingle(byt, k * 4) / 0x80000000;
        // }
        // AudioClip newAudioClip = AudioClip.Create("clippy", _flo.Length, 2, 44100, false);
        // newAudioClip.SetData(_flo, 0);

        byte[] byt = File.ReadAllBytes(trackPaths[j]);
        TanjentOGG.TanjentOGG t = new TanjentOGG.TanjentOGG();
        t.DecodeToFloats(byt);
        // Console.WriteLine(t.Channels + " " + t.SampleRate + " " + t.DecodedFloats.Length);
        // float[] fS = new float[byt.Length];
        // t.DecodeToFloatSamples(byt, fS);
        AudioClip newAudioClip = AudioClip.Create("clippy", t.DecodedFloats.Length / 2, t.Channels, t.SampleRate, false);
        newAudioClip.SetData(t.DecodedFloats, 0);

        if (newAudioClip.length > longestTrack)
        {
          longestTrack = newAudioClip.length;
        }

        newTracks.Add(new Track(trackPaths[j], newAudioClip.length, newAudioClip));
      }

      Album newAlbum = new Album(
        new Vector2(Random.Range(0, Screen.width),
        Random.Range(0, Screen.height)),
        new Vector2(4, 4),
        tex,
        folders[i],
        newTracks,
        longestTrack
      );

      albums.Add(newAlbum);
    }

    debug = "";
  }

  [Button]
  public void ImportCover()
  {
    if (!IsNullOrEmpty(coverPath))
    {
      StartCoroutine(ImportCoverArt());
    }
  }

  IEnumerator ImportCoverArt()
  {
    // if (directory == folderPath) // make folder if not in folder
    // {
    //   directory = folderPath + "/" + Path.GetFileNameWithoutExtension(tracks[trackIndex]);
    //   Directory.CreateDirectory(directory);

    //   // move song into folder
    //   File.Move(tracks[trackIndex], directory + "/" + Path.GetFileName(tracks[trackIndex]));
    // }

    w = UnityWebRequestTexture.GetTexture(coverPath);
    yield return w.SendWebRequest();
    Texture2D texture = new Texture2D(700, 700, TextureFormat.ARGB32, 0, false);
    texture = DownloadHandlerTexture.GetContent(w);
    texture.Apply();

    // replace this with a Graphics.DrawTexture
    // playingAlbum.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
    byte[] bytes = texture.EncodeToPNG();
    File.WriteAllBytes(albums[0].directory + "/cover.png", bytes);

    Refresh();
  }

  // can always restore from the recycling bin and bandcamp
  [Button]
  public void DeleteAlbum()
  {
    Directory.Delete(albums[0].directory, true);
    albums.RemoveAt(0);
  }

  float delay;
  bool AutoPress(KeyCode keyCode)
  {
    if (Input.GetKey(keyCode) && Time.time > delay)
    {
      delay = Time.time + 0.3f;
      return true;
    }
    else
    {
      return false;
    }

  }

  public bool IsNullOrEmpty(string s)
  {
    return (s == null || s == String.Empty) ? true : false;
  }

  private static Texture2D _staticRectTexture;
  private static GUIStyle _staticRectStyle;

  // Note that this function is only meant to be called from OnGUI() functions.
  public static void GUIDrawRect(Rect position, Color color)
  {
    if (_staticRectTexture == null)
    {
      _staticRectTexture = new Texture2D(1, 1);
    }

    if (_staticRectStyle == null)
    {
      _staticRectStyle = new GUIStyle();
    }

    _staticRectTexture.SetPixel(0, 0, color);
    _staticRectTexture.Apply();

    _staticRectStyle.normal.background = _staticRectTexture;

    GUI.Box(position, GUIContent.none, _staticRectStyle);
  }
}