using UnityEngine;

public class MaterialModifier : MonoBehaviour
{
    [Tooltip("Tint for material color.")]
    public Color color = Color.white;
    public Texture texture;
    public Texture normals;
    [Range(0,1)]
    public float shininess;

    MaterialPropertyBlock matBlock;

    public Renderer blockTarget = null;


    [Header("Tiling Material Only")]
    [Tooltip("Adjusts how large a repitition of the UV is in units.")]
    public Vector2 tileSize = new Vector2(1,1);

    [Tooltip("Changes UV samples to be based on world space instead of local.")]
    public bool worldSpace = false;

    private void OnValidate()
    {
        blockTarget ??= GetComponent<Renderer>();

        matBlock = new MaterialPropertyBlock();
        matBlock.SetColor("_Color", color);
        if(texture != null)
            matBlock.SetTexture("_MainTex", texture);    
        matBlock.SetFloat("_Glossiness", shininess);
        matBlock.SetVector("_TileSize", tileSize);
        matBlock.SetFloat("_UseWorldSpace", worldSpace ? 1f : 0f);
        blockTarget.SetPropertyBlock(matBlock);
    }
}
