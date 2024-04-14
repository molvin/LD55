using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RuneIcons
{
    private static Dictionary<string, Sprite> sprites;

    public static void Init()
    {
        sprites = new();
        Sprite[] resources = Resources.LoadAll<Sprite>("CardIcons");

        foreach(Sprite tex in resources)
        {
            sprites.Add(tex.name, tex);
        }
    }

    public static Sprite Get(string name)
    {
        if(sprites == null)
        {
            Init();
        }
        return sprites.GetValueOrDefault(name, null);
    }
}
