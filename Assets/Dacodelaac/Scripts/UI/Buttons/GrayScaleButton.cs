using System;
using System.Collections.Generic;
using System.Linq;
using Dacodelaac.Core;
using Dacodelaac.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dacodelaac.UI.Buttons
{
    public class GrayScaleButton : BaseMono
    {
        [SerializeField] Material grayScaleMat;

        Dictionary<Image, Material> imageMatDicts;
        Dictionary<Text, Material> textMatDicts;
        Dictionary<TextMeshPro, Color> tmpColorDicts;
        Dictionary<TextMeshProUGUI, Color> tmpUIMatDicts;

        protected override void OnEnable()
        {
            base.OnEnable();

            imageMatDicts = GetComponentsInChildren<Image>(true).ToDictionary(i => i, i => i.material);
            textMatDicts = GetComponentsInChildren<Text>(true).ToDictionary(i => i, i => i.material);
            tmpColorDicts = GetComponentsInChildren<TextMeshPro>(true).ToDictionary(i => i, i => i.color);
            tmpUIMatDicts = GetComponentsInChildren<TextMeshProUGUI>(true).ToDictionary(i => i, i => i.color);

            foreach (var i in imageMatDicts)
            {
                i.Key.material = grayScaleMat;
            }
            foreach (var i in textMatDicts)
            {
                i.Key.material = grayScaleMat;
            }
            foreach (var i in tmpColorDicts)
            {
                i.Key.color = SimpleMath.DotColor(i.Key.color, new Color(0.3f, 0.59f, 0.11f));
            }
            foreach (var i in tmpUIMatDicts)
            {
                i.Key.color = SimpleMath.DotColor(i.Key.color, new Color(0.3f, 0.59f, 0.11f));
            }
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();

            foreach (var i in imageMatDicts)
            {
                if (i.Key != null)
                {
                    i.Key.material = i.Value;
                }
            }
            foreach (var i in textMatDicts)
            {
                if (i.Key != null)
                {
                    i.Key.material = i.Value;
                }
            }
            foreach (var i in tmpColorDicts)
            {
                if (i.Key != null)
                {
                    i.Key.color = i.Value;
                }
            }
            foreach (var i in tmpUIMatDicts)
            {
                if (i.Key != null)
                {
                    i.Key.color = i.Value;
                }
            }
        }
    }
}