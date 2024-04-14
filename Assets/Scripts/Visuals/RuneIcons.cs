using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RuneIcons
{
    private static Dictionary<string, Texture2D> textures;

    public static void Init()
    {
        textures = new();
        Texture2D[] resources = Resources.LoadAll<Texture2D>("RuneIcons");

        foreach(Texture2D tex in resources)
        {
            textures.Add(tex.name, tex);
        }
    }

    public static Texture2D Get(string name)
    {
        if(textures == null)
        {
            Init();
        }
        return textures.GetValueOrDefault(name, null);
    }
}
