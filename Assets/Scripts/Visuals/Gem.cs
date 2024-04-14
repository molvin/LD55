using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : Draggable
{
    [System.Serializable]
    public class GemColor
    {
        public string Name;
        public Color Color;
    }
    public List<GemColor> Colors;
    public MeshRenderer Renderer;

    public Artifact Artifact;

    public void Init(Artifact artifact)
    {
        var col = Colors.Find(c => c.Name == artifact.Name);
        Color color = col == null ? Color.black : col.Color;
        Renderer.material = Instantiate(Renderer.material);
        Renderer.material.color = color;

        Artifact = artifact;
    }

}
