﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using HarmonyLib;

using TMPro;

using UnityEngine;

namespace SecondaryLanguageSubtitle.Behaviours;

public class SecondaryUIAdv : MonoBehaviour
{
    private static Func<UIAdv, string, string> _prepTextTags;
    private static readonly Dictionary<string, string> _secondaryColorCache = new();

    private EngineMain _engine;
    private UIAdv _uiAdv;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _advText;
    private TextFader _fader;
    
    static SecondaryUIAdv()
    {
        BuildExpression();
    }
    
    internal void Init()
    {
        _engine = EngineMain.engine;
        _uiAdv = GetComponent<UIAdv>();
        _nameText = Instantiate(_uiAdv.nametext.gameObject, transform).GetComponent<TextMeshProUGUI>();
        _nameText.gameObject.name += " Secondary";
        _advText = Instantiate(_uiAdv.advtext.gameObject, transform).GetComponent<TextMeshProUGUI>();
        _advText.gameObject.name += " Secondary";
        _advText.color = GetLighterColor(_uiAdv.advtext.color);
    }
    
    public void ShowText(string textIn, string displayNameIn, bool updateOnly)
    {
        if (SecondaryLanguage.SecLang == EngineMain.Language.none)
        {
            _nameText.text = string.Empty;
            _advText.text = string.Empty;
            return;
        }

        ReinitLangFont();
        var text = Utils.SplitLangStr(textIn, SecondaryLanguage.SecLang);
        var displayName = Utils.SplitLangStr(displayNameIn, SecondaryLanguage.SecLang);
        _nameText.text = displayName;
        text = _prepTextTags(_uiAdv, text);
        
        
        if (!updateOnly && !_engine.skipping)
        {
            _advText.text = string.Empty;
            _fader = new TextFader(_advText, text, GetFadeColorHex());
            _fader.Begin(_engine.uiflow.adv.activeInHierarchy ? 0f : 0.2f);
        }
        else
        {
            _advText.text = text;
        }
    }

    public void ChangeFont(int id)
    {
        if (id < 0 || id >= UIAdv.fontname.Length)
            id = 0;
        var fontAsset = Resources.Load<TMP_FontAsset>("font/" + UIAdv.fontname[id]);
        _nameText.font = fontAsset;
        _advText.font = fontAsset;
        // AdvTextShadow_Update();
    }
    
    public void AdvTextShadow_Update()
    {
        var text = $"font/{UIAdv.fontname[_engine.prefs.font.value]}{(_engine.prefs.fontShadow.value ? 0 : 1)}";
        var material = Resources.Load<Material>(text);
        if (material != null) _advText.fontMaterial = material;
    }

    public void ReinitUI()
    {
        ReapplyText();
        ChangeFont(_engine.prefs.font.value);
        ReinitLangFont();
    }

    public void ReapplyText()
    {
        if (_engine.uiflow.mode == UIFlow.RunState.game && !FastMemberAccessor<UIAdv, bool>.Get(_uiAdv, "advHideForced"))
        {
            ShowText(_uiAdv.histtmp[1], _uiAdv.histtmp[0], true);
            // TODO: Reinit Choice UI
        }
    }

    public void ReinitLangFont()
    {
        _advText.fontSize = 16f;
        _advText.lineSpacing = 57f;
        _advText.characterSpacing = 0f;
        
        var pos = _advText.transform.localPosition;
        pos.y = -753;
        _advText.transform.localPosition = pos;
    }

    private static void BuildExpression()
    {
        var instanceExpression = Expression.Parameter(typeof(UIAdv), "instance");
        var arg0Expression = Expression.Parameter(typeof(string), "tx");
        var callExpression = Expression.Call(instanceExpression, AccessTools.Method(typeof(UIAdv), "prepTextTags"), arg0Expression);
        var lambdaExpression = Expression.Lambda(callExpression, instanceExpression, arg0Expression);
        _prepTextTags = (Func<UIAdv, string, string>)lambdaExpression.Compile();
    }

    private string GetFadeColorHex()
    {
        var primaryFader = FastMemberAccessor<UIAdv, TextFader>.Get(_uiAdv, "fader");
        var primaryColorHex = FastMemberAccessor<TextFader, string>.Get(primaryFader, "colorhex");
        return GetTextLighterColorHex(primaryColorHex);
    }

    private static string GetTextLighterColorHex(string colorHex)
    {
        if (!_secondaryColorCache.TryGetValue(colorHex, out var result))
        {
            if (!ColorUtility.TryParseHtmlString("#" + colorHex, out var primaryColor))
                primaryColor = Color.black;
            var secondaryColor = GetLighterColor(primaryColor);
            result = ColorUtility.ToHtmlStringRGB(secondaryColor);
            _secondaryColorCache.Add(colorHex, result);
        }

        return result;
    }

    private static Color GetLighterColor(Color color)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        return Color.HSVToRGB(h, s - 0.2f, v);   
    }

    private bool selectingPrimary = true;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) selectingPrimary = !selectingPrimary;
        var curr = selectingPrimary ? _uiAdv.advtext : _advText;
        if (Input.GetKeyDown(KeyCode.I))
        {
            curr.transform.localPosition += new Vector3(0, 1, 0);
            SecondaryLanguage.MyLogger.LogDebug($"Current {curr.gameObject.name} position: {curr.transform.localPosition}");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            curr.transform.localPosition -= new Vector3(0, 1, 0);
            SecondaryLanguage.MyLogger.LogDebug($"Current {curr.gameObject.name} position: {curr.transform.localPosition}");
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            curr.fontSize -= 1;
            SecondaryLanguage.MyLogger.LogDebug($"Current {curr.gameObject.name} font size: {curr.fontSize}");
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            curr.fontSize += 1;
            SecondaryLanguage.MyLogger.LogDebug($"Current {curr.gameObject.name} font size: {curr.fontSize}");
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            curr.lineSpacing -= 1f;
            SecondaryLanguage.MyLogger.LogDebug($"Current {curr.gameObject.name} line spacing: {curr.lineSpacing}");
        }
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            curr.lineSpacing += 1f;
            SecondaryLanguage.MyLogger.LogDebug($"Current {curr.gameObject.name} line spacing: {curr.lineSpacing}");
        }
    }
}