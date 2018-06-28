﻿using System;
using System.Linq;
using B83.Win32;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EditMap.TerrainTypes
{
    public class LayerSettingsItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text styleText;
        [SerializeField] private Image colorImage;
        [SerializeField] private GameObject blockingImageGO;

        [SerializeField] private RectTransform moreInfoRectTransform;
        [SerializeField] private Toggle toggle;

        public Action<byte> onActive;
        private Action<Rect, string, string> showMoreInfo;
        private Action hideMoreInfo;

        private byte index;
        private string description;

        public void Init(TerrainTypeLayerSettings layerSettings, ToggleGroup layersToggleGroup, Action<Rect, string, string> showMoreInfoCallback,
            Action hideMoreInfoCallback)
        {
            nameText.text = layerSettings.name;
            styleText.text = layerSettings.style.ToString();
            colorImage.color = layerSettings.color;
            blockingImageGO.SetActive(layerSettings.blocking);
            
            toggle.onValueChanged.AddListener(OnToggleChanged);
            toggle.group = layersToggleGroup;
            layersToggleGroup.RegisterToggle(toggle);
            
            showMoreInfo = showMoreInfoCallback;
            hideMoreInfo = hideMoreInfoCallback;
            
            index = layerSettings.index;
            description = layerSettings.description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (showMoreInfo != null)
            {
                Vector3[] corners = new Vector3[4];
                moreInfoRectTransform.GetWorldCorners(corners);
//                World Corners: [0](278.0, 480.2, 0.0), [1](278.0, 536.0, 0.0), [2](539.6, 536.0, 0.0), [3](539.6, 480.2, 0.0)
                
                Rect rect = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
                Debug.LogFormat("World Corners: {0}", String.Join(", ", corners));
//                Debug.LogFormat("New Local Corners: {0}", String.Join(", ", corners.Select(vector => transform.InverseTransformPoint(vector))));
                showMoreInfo(rect, index.ToString(), description);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hideMoreInfo != null)
            {
                hideMoreInfo();
            }
        }

        private void OnToggleChanged(bool isActive)
        {
            if (isActive && onActive!=null)
            {
                onActive(index);
            }
        }
        
        private void Clear()
        {
            onActive = null;
        }
    }
}