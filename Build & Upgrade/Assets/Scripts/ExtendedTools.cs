using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ExtendedTools
{
    public class ExtendedList<T> : List<T>
    {
        public int Limit = 0;

        public event EventHandler OnAdd;
        public event EventHandler OnRemove;

        public new void Add(T item)
        {
            if (Limit != 0)
            {
                if (base.Count < Limit)
                {
                    if (null != OnAdd)
                    {
                        OnAdd(this, null);
                    }
                    base.Add(item);
                }
            }
            else
            {
                if (null != OnAdd)
                {
                    OnAdd(this, null);
                }
                base.Add(item);
            }
        }

        public new void Remove(T item)
        {
            if (null != OnRemove)
            {
                OnRemove(this, null);
            }
            base.Remove(item);
        }
    }

    public static List<Transform> GetClosestObjects(Transform _Sender, List<Transform> _Objects)
    {
        int MaxAmount = _Objects.Count;
        List<Transform> ClosestObjects = new List<Transform>();
        for (int i = 0; i < MaxAmount; i++)
        {
            Transform ClosestObject = GetClosestObject(_Sender, _Objects);
            _Objects.Remove(ClosestObject);
            ClosestObjects.Add(ClosestObject);
        }

        return ClosestObjects;
    }

    public static Transform GetClosestObject(Transform _Sender, List<Transform> _Objects)
    {
        Transform ClosestObject = null;
        float ClosestDistanceSqr = Mathf.Infinity;
        Vector3 CurrentPosition = _Sender.position;
        foreach (Transform _Object in _Objects)
        {
            Vector3 DirectionToTarget = _Object.position - CurrentPosition;
            float dSqrToTarget = DirectionToTarget.sqrMagnitude;
            if (dSqrToTarget < ClosestDistanceSqr)
            {
                ClosestDistanceSqr = dSqrToTarget;
                ClosestObject = _Object;
            }
        }

        return ClosestObject;
    }

    public static void RemoveAt<T>(ref T[] arr, int index)
    {
        for (int a = index; a < arr.Length - 1; a++)
        {
            arr[a] = arr[a + 1];
        }
        Array.Resize(ref arr, arr.Length - 1);
    }

    public static bool IsWin()
    {
#if UNITY_EDITOR
        return Application.platform == RuntimePlatform.WindowsEditor;
#else
        return Application.platform == RuntimePlatform.WindowsPlayer;
#endif
    }

#if UNITY_EDITOR
    public static Texture2D PreviewObject(GameObject gameObject)
    {
        var texture = AssetPreview.GetAssetPreview(gameObject);
        if (texture != null)
        {
            texture.name = gameObject.name;
            return texture;
        }
        return null;
    }

    public static void SetTextureImporterFormat(Texture2D texture, bool isReadable)
    {
        if (null == texture) return;

        string assetPath = AssetDatabase.GetAssetPath(texture);
        var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (tImporter != null)
        {
            tImporter.textureType = TextureImporterType.Default;

            tImporter.isReadable = isReadable;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
    }
#endif

    public static T[] FindObjectsOfTypeIncludingDisabled<T>()
    {
        var ActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var RootObjects = ActiveScene.GetRootGameObjects();
        var MatchObjects = new List<T>();

        foreach (var ro in RootObjects)
        {
            var Matches = ro.GetComponentsInChildren<T>(true);
            MatchObjects.AddRange(Matches);
        }

        return MatchObjects.ToArray();
    }

    public static Texture2D createReadabeTexture2D(Texture2D texture2d)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture2d.width, texture2d.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        Graphics.Blit(texture2d, renderTexture);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D readableTextur2D = new Texture2D(texture2d.width, texture2d.height);

        readableTextur2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTextur2D.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return readableTextur2D;
    }

    public static bool TextureToPNG(Texture2D texture, string _Directory)// = $"{Application.dataPath}/ObjectPreviewOutput/", string FileName = $"{texture.name}")
    {
        //var dirPath = string.Empty;
        byte[] bytes = texture.EncodeToPNG();
        if (texture == null)
        {
            return false;
        }
        /*if (string.IsNullOrEmpty(FileName))
        {
            FileName = texture.name;
        }*/
        /*if (string.IsNullOrEmpty(_Directory))
        {
            dirPath = Application.dataPath + "/ObjectPreviewOutput/";
        }
        else
        {
            dirPath = _Directory;
        }*/
        if (!Directory.Exists(_Directory))
        {
            Directory.CreateDirectory(_Directory);
        }
        //File.WriteAllBytes(dirPath + FileName + ".png", bytes);
        //Debug.Log(dirPath + FileName + ".png");
        File.WriteAllBytes(_Directory + texture.name + ".png", bytes);
        Debug.Log(_Directory + texture.name + ".png");
        return true;
    }
    public static bool TextureToJPG(Texture2D texture, string _Directory)// = $"{Application.dataPath}/ObjectPreviewOutput/", string FileName = $"{texture.name}")
    {
        byte[] bytes = texture.EncodeToJPG();
        if (texture == null)
        {
            return false;
        }
        if (!Directory.Exists(_Directory))
        {
            Directory.CreateDirectory(_Directory);
        }
        File.WriteAllBytes(_Directory + texture.name + ".jpg", bytes);
        Debug.Log(_Directory + texture.name + ".jpg");
        return true;
    }
}