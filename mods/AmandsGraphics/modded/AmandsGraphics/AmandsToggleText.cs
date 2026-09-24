using TMPro;
using UnityEngine;

namespace AmandsGraphics;

public sealed class AmandsToggleText : MonoBehaviour
{
    public TMP_Text TMPText { get; private set; }

    public string text = "";
    public Color color = new(0.84f, 0.88f, 0.95f, 0.69f);
    public int fontSize = 26;
    public float outlineWidth = 0.01f;
    public FontStyles fontStyles = FontStyles.SmallCaps;
    public TextAlignmentOptions textAlignmentOptions = TextAlignmentOptions.Right;
    public float time = 2f;
    public float lifeTime = 0f;
    public float OpacitySpeed = 0.08f;

    private float _opacity = 1f;
    private float _startOpacity = 0f;
    private bool _updateOpacity = false;
    private bool _updateStartOpacity = false;

    public void Start()
    {
        TMPText = gameObject.AddComponent<TextMeshProUGUI>();

        if (TMPText != null)
        {
            TMPText.text = text;
            TMPText.color = color;
            TMPText.fontSize = fontSize;
            TMPText.outlineWidth = outlineWidth;
            TMPText.fontStyle = fontStyles;
            TMPText.alignment = textAlignmentOptions;
            TMPText.alpha = 0f;
            _updateStartOpacity = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void UpdateText(string Text)
    {
        text = Text;
        if (TMPText != null)
        {
            TMPText.text = Text;
        }

        lifeTime = 0f;

        if (_updateOpacity && TMPText != null)
        {
            _opacity = 1f;
            TMPText.alpha = _opacity;
            _updateOpacity = false;
        }
    }
    public void Update()
    {
        lifeTime += Time.deltaTime;

        if (lifeTime > time)
        {
            _updateOpacity = true;
        }

        if (_updateOpacity && TMPText != null)
        {
            _opacity -= Math.Max(0.01f, OpacitySpeed);
            TMPText.alpha = _opacity;
            if (_opacity < 0)
            {
                _updateOpacity = false;
                _updateStartOpacity = false;
                Destroy(gameObject);
            }
        }
        else if (_updateStartOpacity && _startOpacity < 1f && TMPText != null)
        {
            _startOpacity += OpacitySpeed * 2f;
            TMPText.alpha = _startOpacity;
        }
    }
}
