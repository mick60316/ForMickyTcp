using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class DrawSystem : MonoBehaviour
{
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    public RawImage sourceImage;
    public Text penWidthText;
    public Slider penWidthSlider;
    private Texture2D texture;
    private int penWidth = 10;
    private Color32 penColor;
    private Color32[] colorBuffer;
    private Color32[] defaultColor = new Color32[]
    {
        new Color32(255,0,0,255),
        new Color32(0,255,0,255),
        new Color32(0,0,255,255),
};



    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
        texture = TextureToTexture2D(sourceImage.texture);
        colorBuffer = new Color32[penWidth * penWidth];
        for (int i = 0; i < colorBuffer.Length; i++)
        {
            colorBuffer[i] = new Color32(255, 0, 0, 255);
        }
        penColor = defaultColor[0];
    }

    void Update()
    {
        //Check if the left Mouse button is clicked
        if (Input.GetKey(KeyCode.Mouse0))
        {
            //Set up the new Pointer Event
            m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.tag == "drawable")
                {
                    Vector2 texturePos = result.screenPosition - new Vector2(sourceImage.GetComponent<RectTransform>().position.x, sourceImage.GetComponent<RectTransform>().position.y);

                    texturePos.y = 512 + texturePos.y;
                    //Debug.Log("Hit " + texturePos + " "+ sourceImage.GetComponent<RectTransform>().position);
                    texture.SetPixels32((int)texturePos.x - penWidth / 2, (int)texturePos.y + penWidth / 2, penWidth, penWidth, colorBuffer);
                    texture.Apply();
                    sourceImage.texture = texture;
                }
            }
        }

    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;
    }

    public void setPenColor(Color32 color)
    {
        penColor = color;
        for (int i = 0; i < colorBuffer.Length; i++)
        {
            colorBuffer[i] = penColor;
        }
    }
    public void setColorIndex(int index)
    {
        setPenColor(defaultColor[index]);
    }
    public void setPenWidth(int width)
    {
        penWidth = width;
        colorBuffer = new Color32[width * width];
        setPenColor(penColor);
    }
    public void penWidthChange()
    {
        penWidth = (int)(penWidthSlider.value * 10) + 10;
        setPenWidth(penWidth);
        penWidthText.text = "Pen Width : " + penWidth;
    }

}
