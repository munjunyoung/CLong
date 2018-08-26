using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Capture : MonoBehaviour
{
    public Camera _cameraWithRendTexture;
    int i = 0;
    // Use this for initialization
    void Start ()
    {
        _cameraWithRendTexture = GameObject.Find("capturecam").GetComponent<Camera>();
       
	}

    int size = 512;
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKeyUp(KeyCode.F10))
        {
            
            RenderTexture.active = _cameraWithRendTexture.targetTexture;
            Texture2D newTexture = new Texture2D(_cameraWithRendTexture.targetTexture.width, _cameraWithRendTexture.targetTexture.height, TextureFormat.ARGB32, false);
            newTexture.ReadPixels(new Rect(0, 0, _cameraWithRendTexture.targetTexture.width, _cameraWithRendTexture.targetTexture.height), 0, 0, false);
            newTexture.Apply();
            Color[] colorArrayCropped = newTexture.GetPixels(0, 0, size, size);
            Texture2D croppedTex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            croppedTex.SetPixels(colorArrayCropped);
            croppedTex = FillInClear(croppedTex, _cameraWithRendTexture.backgroundColor);
            croppedTex.Apply();
            byte[] bytes = croppedTex.EncodeToPNG();
            var goName = transform.GetChild(0).gameObject.name;
            File.WriteAllBytes(goName + ".png", bytes);
            Debug.Log(bytes.Length);
        }
	}

    public Texture2D FillInClear(Texture2D tex2D, Color whatToFillWith)
    {

        for (int i = 0; i < tex2D.width; i++)
        {
            for (int j = 0; j < tex2D.height; j++)
            {
                if (tex2D.GetPixel(i, j) == Color.clear)
                    tex2D.SetPixel(i, j, whatToFillWith);
            }
        }
        return tex2D;
    }   
}
