using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gem : Draggable
{
    [System.Serializable]
    public class GemColor
    {
        public string Name;
        public GameObject Prefab;
        public Material Material;
    }
    public List<GemColor> Colors;
    public MeshRenderer Renderer;
    public MeshFilter Filter;

    public TextMeshProUGUI Description;
    public GameObject TextBackground;

    public Artifact Artifact;

    private void Start()
    {
        ToggleText(false);
    }

    public void Init(Artifact artifact)
    {
        var col = Colors.Find(c => c.Name == artifact.Name);
        //Filter.mesh = col.Mesh;
        //Renderer.material = Instantiate(col.Material);
        Destroy(transform.GetChild(0).gameObject);
        var go = Instantiate(col.Prefab, transform);
        go.transform.localScale = Vector3.one * 10;
        go.GetComponent<MeshRenderer>().material = col.Material;
        Collider = go.GetComponentInChildren<MeshCollider>();
        Collider.convex = true;
        Artifact = artifact;

        Description.text = $"{artifact.Name}: {artifact.Text}";
    }

    public void ToggleText(bool on)
    {
        Description.enabled = on;
        TextBackground.SetActive(on);
    }

}
