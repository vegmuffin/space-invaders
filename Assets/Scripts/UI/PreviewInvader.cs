using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

public class PreviewInvader : MonoBehaviour
{
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;
    [SerializeField] private bool isFlashing = false;
    private bool whichSprite = false;
    private float timer = 0f;
    private Image image;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        if(!isFlashing)
            image = transform.GetComponent<Image>();
        else
            spriteRenderer = transform.GetComponent<SpriteRenderer>();
    }

    // Lets make the title screen feel more alive!
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= 0.5f)
        {
            timer = 0f;
            switch(whichSprite)
            {
                case false:
                    if(!isFlashing)
                        image.sprite = sprite2;
                    else
                        spriteRenderer.sprite = sprite2;
                    break;
                case true:
                    if(!isFlashing)
                        image.sprite = sprite1;
                    else
                        spriteRenderer.sprite = sprite1;
                    break;
            }
            whichSprite = !whichSprite;
        }
    }
}

#pragma warning restore 649
