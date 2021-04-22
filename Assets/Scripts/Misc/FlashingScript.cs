using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#pragma warning disable 649

public class FlashingScript : MonoBehaviour
{
    [SerializeField] private bool isTilemap;
    
    private float colorHue = 0f;
    private int hueMultiplier = 1;
    private int saturationMultiplier = 1;
    private Material tilemapMaterial;
    private SpriteRenderer spriteRenderer;

    public bool colorBool = false;
    [HideInInspector] public float colorSaturation = 0f;

    private void Start()
    {
        if(!isTilemap)
            spriteRenderer = transform.GetComponent<SpriteRenderer>();
        else
            tilemapMaterial = transform.GetComponent<TilemapRenderer>().material;
    }

    private void Update()
    {
        ColorChange();
    }

    // Visuals for special enemy ships, rapid fire, or for a preview invader in the title screen.
    private void ColorChange()
    {
        if(colorBool)
        {
            // Sliding between 0.3 and 0.5 for saturation.
            if(colorSaturation >= 0.5f && saturationMultiplier == 1)
                saturationMultiplier = -1;
            else if(colorSaturation <= 0.3f && saturationMultiplier == -1)
                saturationMultiplier = 1;
            
            // Sliding between 0 and 1 for hue.
            if(colorHue >= 1 && hueMultiplier == 1)
                hueMultiplier = -1;
            else if(colorHue <= 0 && hueMultiplier == -1)
                hueMultiplier = 1;

            colorSaturation += 0.01f * saturationMultiplier;
            colorHue += 0.02f * hueMultiplier;
            Color flashingColor = Color.HSVToRGB(colorHue, colorSaturation, 1f);

            if(!isTilemap)
                spriteRenderer.color = flashingColor;
            else
                tilemapMaterial.color = flashingColor;
        }
    }

    // Don't flash the colors anymore, return to white.
    public void ResetColor()
    {
        colorBool = false;
        if(!isTilemap)
            spriteRenderer.color = Color.white;
        else
            tilemapMaterial.color = Color.white;
    }
}

#pragma warning restore 649
