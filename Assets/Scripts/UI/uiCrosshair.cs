using UnityEngine;
using UnityEngine.UI;

public class uiCrosshair : MonoBehaviour {

    [SerializeField]
    GameObject
        //ic = inner crosshair
        //oc = outer crosshair
        centerDot,
        ic,
        oc;
    Animation crosshairAnimation;
    RectTransform
        centerDotTransform,
        icTransform,
        icTopTransform,
        icBottomTransform,
        icLeftTransform,
        icRightTransform,
        ocTransform,
        ocTopTransform,
        ocBottomTransform,
        ocLeftTransform,
        ocRightTransform;
    Outline
        centerDotOutline,
        icTopOutline,
        icBottomOutline,
        icLeftOutline,
        icRightOutline,
        ocTopOutline,
        ocBottomOutline,
        ocLeftOutline,
        ocRightOutline;
    Image
        centerDotImage,
        icTopImage,
        icBottomImage,
        icLeftImage,
        icRightImage,
        ocTopImage,
        ocBottomImage,
        ocLeftImage,
        ocRightImage;
    [SerializeField]
    float
        dotSize = 2f,
        dotOpacity = 1f,
        dotOutlineOpacity = 1f,
        icLength = 20f,
        icThickness = 2f,
        icOffset = 0f,
        icOpacity = 1f,
        icOutlineOpacity = 1f;
    [SerializeField]
    bool
        dotEnabled,
        icEnabled,
        ocEnabled,
        rainbowEnabled;
    [SerializeField]
    Color
        dotColor,
        dotOutlineColor,
        icColor,
        icOutlineColor,
        rainbowColor;
    void Awake() {
        //crosshairAnimation = GetComponent<Animation>(); import rainbow anim

        centerDotTransform = centerDot.GetComponent<RectTransform>();
        centerDotOutline = centerDot.GetComponent<Outline>();
        centerDotImage = centerDot.GetComponent<Image>();

        icTransform = ic.GetComponent<RectTransform>();

        Transform icTop = icTransform.Find("Top");
        icTopTransform = icTop.GetComponent<RectTransform>();
        icTopOutline = icTop.GetComponent<Outline>();
        icTopImage = icTop.GetComponent<Image>();

        Transform icBottom = icTransform.Find("Bottom");
        icBottomTransform = icBottom.GetComponent<RectTransform>();
        icBottomOutline = icBottom.GetComponent<Outline>();
        icBottomImage = icBottom.GetComponent<Image>();

        Transform icLeft = icTransform.Find("Left");
        icLeftTransform = icLeft.GetComponent<RectTransform>();
        icLeftOutline = icLeft.GetComponent<Outline>();
        icLeftImage = icLeft.GetComponent<Image>();

        Transform icRight = icTransform.Find("Right");
        icRightTransform = icRight.GetComponent<RectTransform>();
        icRightOutline = icRight.GetComponent<Outline>();
        icRightImage = icRight.GetComponent<Image>();

        //ocTransform = oc.GetComponent<RectTransform>();
        //ocTopTransform = oc.transform.Find("Top").GetComponent<RectTransform>();
        //ocBottomTransform = oc.transform.Find("Bottom").GetComponent<RectTransform>();
        //ocLeftTransform = oc.transform.Find("Left").GetComponent<RectTransform>();
        //ocRightTransform = oc.transform.Find("Right").GetComponent<RectTransform>();
    }
    void Start() {

    }

    void Update() {
        centerDot.SetActive(dotEnabled);
        centerDotTransform.sizeDelta = new(dotSize, dotSize);
        centerDotOutline.effectColor = new(dotOutlineColor.r, dotOutlineColor.g, dotOutlineColor.b, dotOutlineOpacity);

        Color icColorNew;
        if (rainbowEnabled) {
            if (!crosshairAnimation.isPlaying) { crosshairAnimation.Play(); }
            icColorNew = new(rainbowColor.r, rainbowColor.g, rainbowColor.b, icOpacity); ;
            centerDotImage.color = new(rainbowColor.r, rainbowColor.g, rainbowColor.b, dotOpacity);
        }
        else {
            icColorNew = new(icColor.r, icColor.g, icColor.b, icOpacity);
            centerDotImage.color = new(dotColor.r, dotColor.g, dotColor.b, dotOpacity);
        }

        Color icOutlineColorNew = new(icOutlineColor.r, icOutlineColor.g, icOutlineColor.b, icOutlineOpacity);
        icTransform.gameObject.SetActive(icEnabled);

        icTopTransform.sizeDelta = new(icThickness, icLength);
        icTopTransform.localPosition = new(0, icOffset, 0);
        icTopOutline.effectColor = icOutlineColorNew;
        icTopImage.color = icColorNew;

        icBottomTransform.sizeDelta = new(icThickness, icLength);
        icBottomTransform.localPosition = new(0, -icOffset, 0);
        icBottomOutline.effectColor = icOutlineColorNew;
        icBottomImage.color = icColorNew;

        icLeftTransform.sizeDelta = new(icLength, icThickness);
        icLeftTransform.localPosition = new(-icOffset, 0, 0);
        icLeftOutline.effectColor = icOutlineColorNew;
        icLeftImage.color = icColorNew;

        icRightTransform.sizeDelta = new(icLength, icThickness);
        icRightTransform.localPosition = new(icOffset, 0, 0);
        icRightOutline.effectColor = icOutlineColorNew;
        icRightImage.color = icColorNew;



        //oc.SetActive(ocEnabled);
        //ocTopTransform.sizeDelta = new(ocThickness, ocLength);
        //ocTopTransform.localPosition = new(0, ocOffset, 0);
        //ocBottomTransform.sizeDelta = new(ocThickness, ocLength);
        //ocBottomTransform.localPosition = new(0, -ocOffset, 0);
        //ocLeftTransform.sizeDelta = new(ocLength, ocThickness);
        //ocLeftTransform.localPosition = new(-ocOffset, 0, 0);
        //ocRightTransform.sizeDelta = new(ocLength, ocThickness);
        //ocRightTransform.localPosition = new(ocOffset, 0, 0);
    }
}