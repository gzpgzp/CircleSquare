// AdvancedColorCircleController.cs
using UnityEngine;
using System.Collections;
using Tools.Singletons;
using UnityEngine.Events;

public class CircleWallEffect : MMSingleton<CircleWallEffect>
{
    [System.Serializable]
    public class ColorTransition
    {
        public Color startEdgeColor = Color.red;
        public Color endEdgeColor = Color.red;
        public Color startInsideColor = new Color(0.5f, 0.5f, 1f, 0.3f);
        public Color endInsideColor = new Color(0.5f, 0.5f, 1f, 0.3f);
        public Color startOutsideColor = Color.white;
        public Color endOutsideColor = Color.white;
        
        public Color GetEdgeColor(float t) => Color.Lerp(startEdgeColor, endEdgeColor, t);
        public Color GetInsideColor(float t) => Color.Lerp(startInsideColor, endInsideColor, t);
        public Color GetOutsideColor(float t) => Color.Lerp(startOutsideColor, endOutsideColor, t);
    }
    
    [System.Serializable]
    public class EffectPreset
    {
        public string presetName = "New Preset";
        public ColorTransition colors;
        public float edgeWidth = 0.1f;
        public float growthSpeed = 20f;
    }

    [Header("组件引用")]
    [SerializeField] private Material effectMaterial;
    [SerializeField] private Transform circleCenter;
    [SerializeField] private Material[] hideMaterials;
    
    [Header("默认设置")]
    [SerializeField] private EffectPreset defaultPreset;
    [SerializeField] private EffectPreset[] customPresets;
    
    [Header("运行时设置")]
    [SerializeField] private float minRadius = 0.5f;
    [SerializeField] private float maxRadius = 50f;
    [SerializeField] private bool useColorTransition = true;
    
    [Header("事件")]
    public UnityEvent<Color> onEdgeColorChanged;
    public UnityEvent<Color> onInsideColorChanged;
    public UnityEvent<Color> onOutsideColorChanged;
    public UnityEvent onEffectStarted;
    public UnityEvent onEffectCompleted;
    
    // 私有变量
    private float currentRadius;
    private bool isActive = false;
    private ColorTransition currentColors;
    private Coroutine effectCoroutine;
    private EffectPreset currentPreset;
    
    // 属性
    public float CurrentRadius => currentRadius;
    public bool IsActive => isActive;
    public EffectPreset CurrentPreset => currentPreset;

    public void Awake()
    {
        currentColors = new ColorTransition();
        if (defaultPreset != null)
        {
            ApplyPreset(defaultPreset);
        }
    }

    public void Reset(Color color)
    {
        currentColors = new ColorTransition();
        
        if (defaultPreset != null)
        {
            defaultPreset.colors.startOutsideColor = color;
            defaultPreset.colors.endOutsideColor = color;
            
            defaultPreset.colors.startInsideColor = color;
            defaultPreset.colors.endInsideColor = color;
            ApplyPreset(defaultPreset);
        }
    }

    #region 主要控制方法

    /// <summary>
    /// 开始效果，使用当前预设
    /// </summary>
    public void StartEffect(Vector2 position, bool useTransition = true, bool showHideArea = false)
    {
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);
        
        circleCenter.position = new Vector3(position.x, position.y, circleCenter.position.z);
        effectCoroutine = StartCoroutine(EffectRoutine(useTransition,showHideArea));
    }
    
    /// <summary>
    /// 开始效果并指定预设
    /// </summary>
    public void StartEffect(Vector2 position, string presetName)
    {
        var preset = GetPresetByName(presetName);
        if (preset != null)
        {
            ApplyPreset(preset);
            StartEffect(position, true);
        }
    }
    
    /// <summary>
    /// 开始效果并自定义颜色
    /// </summary>
    public void StartEffect(Vector2 position, Color edgeColor, Color insideColor, Color outsideColor, bool showHideArea)
    {
        SetEdgeColor(edgeColor);
        SetInsideColor(insideColor);
        SetOutsideColor(outsideColor);
        StartEffect(position, false, showHideArea);
    }

    public void StartEffect(Vector2 position, Color insideColor)
    {
        SetEdgeColor(currentColors.startEdgeColor);
        SetInsideColor(insideColor);
        SetOutsideColor(currentColors.startOutsideColor);
        StartEffect(position,false);
    }

    /// <summary>
    /// 停止效果
    /// </summary>
    public void StopEffect(bool immediate = false)
    {
        if (immediate)
        {
            isActive = false;
            if (effectCoroutine != null)
            {
                StopCoroutine(effectCoroutine);
                effectCoroutine = null;
            }
        }
        else
        {
            maxRadius = currentRadius; // 立即达到最大半径
        }
    }
    
    #endregion
    
    #region 颜色控制方法
    
    /// <summary>
    /// 设置边缘颜色（即时生效）
    /// </summary>
    public void SetEdgeColor(Color color)
    {
        currentColors.startEdgeColor = color;
        currentColors.endEdgeColor = color;
        UpdateMaterialColorsImmediate(color, currentColors.startInsideColor, currentColors.startOutsideColor);
        onEdgeColorChanged?.Invoke(color);
    }
    
    /// <summary>
    /// 设置内部颜色（即时生效）
    /// </summary>
    public void SetInsideColor(Color color)
    {
        currentColors.startInsideColor = color;
        currentColors.endInsideColor = color;
        UpdateMaterialColorsImmediate(currentColors.startEdgeColor, color, currentColors.startOutsideColor);
        onInsideColorChanged?.Invoke(color);
    }
    
    /// <summary>
    /// 设置外部颜色（即时生效）
    /// </summary>
    public void SetOutsideColor(Color color)
    {
        currentColors.startOutsideColor = color;
        currentColors.endOutsideColor = color;
        UpdateMaterialColorsImmediate(currentColors.startEdgeColor, currentColors.startInsideColor, color);
        onOutsideColorChanged?.Invoke(color);
    }
    
    /// <summary>
    /// 设置颜色过渡
    /// </summary>
    public void SetColorTransition(Color startEdge, Color endEdge, 
                                  Color startInside, Color endInside,
                                  Color startOutside, Color endOutside)
    {
        currentColors.startEdgeColor = startEdge;
        currentColors.endEdgeColor = endEdge;
        currentColors.startInsideColor = startInside;
        currentColors.endInsideColor = endInside;
        currentColors.startOutsideColor = startOutside;
        currentColors.endOutsideColor = endOutside;
    }
    
    /// <summary>
    /// 渐变色到目标颜色
    /// </summary>
    public void TransitionToColors(Color targetEdge, Color targetInside, Color targetOutside, float duration)
    {
        StartCoroutine(ColorTransitionRoutine(targetEdge, targetInside, targetOutside, duration));
    }
    
    #endregion
    
    #region 预设管理
    
    /// <summary>
    /// 应用预设
    /// </summary>
    public void ApplyPreset(EffectPreset preset)
    {
        if (preset == null) return;
        
        currentPreset = preset;
        currentColors = preset.colors;
        
        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_EdgeWidth", preset.edgeWidth);
            UpdateMaterialColorsImmediate(preset.colors.startEdgeColor, 
                                         preset.colors.startInsideColor, 
                                         preset.colors.startOutsideColor);
        }

        effectMaterial.SetFloat("_CircleRadius", 0);
        
        foreach (var material in hideMaterials)
        {
            material.SetFloat("_CircleRadius", 0);
        }
    }
    
    /// <summary>
    /// 根据名称获取预设
    /// </summary>
    public EffectPreset GetPresetByName(string name)
    {
        foreach (var preset in customPresets)
        {
            if (preset.presetName == name)
                return preset;
        }
        return null;
    }
    
    /// <summary>
    /// 添加新的预设
    /// </summary>
    public void AddPreset(string name, Color edgeColor, Color insideColor, Color outsideColor)
    {
        var newPreset = new EffectPreset
        {
            presetName = name,
            colors = new ColorTransition
            {
                startEdgeColor = edgeColor,
                endEdgeColor = edgeColor,
                startInsideColor = insideColor,
                endInsideColor = insideColor,
                startOutsideColor = outsideColor,
                endOutsideColor = outsideColor
            }
        };
        
        System.Array.Resize(ref customPresets, customPresets.Length + 1);
        customPresets[customPresets.Length - 1] = newPreset;
    }
    
    #endregion
    
    #region 协程和私有方法
    
    private IEnumerator EffectRoutine(bool useTransition, bool showHideArea)
    {
        isActive = true;
        currentRadius = minRadius;
        float targetRadius = maxRadius;
        float speed = currentPreset?.growthSpeed ?? 20f;
        
        onEffectStarted?.Invoke();
        
        while (currentRadius < targetRadius)
        {
            // 更新半径
            currentRadius += speed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, targetRadius);
            
            // 更新中心位置和半径
            Vector2 centerPos = new Vector2(circleCenter.position.x, circleCenter.position.y);
            effectMaterial.SetVector("_CircleCenter", centerPos);
            effectMaterial.SetFloat("_CircleRadius", currentRadius);

            if (showHideArea)
            {
                foreach (var material in hideMaterials)
                {
                    material.SetVector("_CircleCenter", centerPos);
                    material.SetFloat("_CircleRadius", currentRadius);
                }
            }
            else
            {
                foreach (var material in hideMaterials)
                {
                    material.SetFloat("_CircleRadius", 0);
                }
            }

            // 更新颜色（如果使用过渡）
            if (useTransition)
            {
                float progress = (currentRadius - minRadius) / (targetRadius - minRadius);
                UpdateMaterialColorsProgress(progress);
            }
            
            yield return null;
        }
        
        isActive = false;
        effectCoroutine = null;
        onEffectCompleted?.Invoke();
    }
    
    private IEnumerator ColorTransitionRoutine(Color targetEdge, Color targetInside, 
                                              Color targetOutside, float duration)
    {
        Color startEdge = currentColors.startEdgeColor;
        Color startInside = currentColors.startInsideColor;
        Color startOutside = currentColors.startOutsideColor;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color edgeColor = Color.Lerp(startEdge, targetEdge, t);
            Color insideColor = Color.Lerp(startInside, targetInside, t);
            Color outsideColor = Color.Lerp(startOutside, targetOutside, t);
            
            UpdateMaterialColorsImmediate(edgeColor, insideColor, outsideColor);
            yield return null;
        }
        
        // 更新颜色状态
        currentColors.startEdgeColor = targetEdge;
        currentColors.endEdgeColor = targetEdge;
        currentColors.startInsideColor = targetInside;
        currentColors.endInsideColor = targetInside;
        currentColors.startOutsideColor = targetOutside;
        currentColors.endOutsideColor = targetOutside;
    }
    
    private void UpdateMaterialColorsProgress(float progress)
    {
        if (effectMaterial == null) return;
        
        Color edgeColor = currentColors.GetEdgeColor(progress);
        Color insideColor = currentColors.GetInsideColor(progress);
        Color outsideColor = currentColors.GetOutsideColor(progress);
        
        effectMaterial.SetColor("_EdgeColor", edgeColor);
        effectMaterial.SetColor("_InnerColor", insideColor);
        effectMaterial.SetColor("_OutsideColor", outsideColor);
        
        onEdgeColorChanged?.Invoke(edgeColor);
        onInsideColorChanged?.Invoke(insideColor);
        onOutsideColorChanged?.Invoke(outsideColor);
    }
    
    private void UpdateMaterialColorsImmediate(Color edgeColor, Color insideColor, Color outsideColor)
    {
        if (effectMaterial == null) return;
        
        effectMaterial.SetColor("_EdgeColor", edgeColor);
        effectMaterial.SetColor("_InnerColor", insideColor);
        effectMaterial.SetColor("_OutsideColor", outsideColor);
    }
    
    #endregion
    
    #region 工具方法
    
    /// <summary>
    /// 获取当前颜色值
    /// </summary>
    public void GetCurrentColors(out Color edge, out Color inside, out Color outside)
    {
        edge = currentColors.startEdgeColor;
        inside = currentColors.startInsideColor;
        outside = currentColors.startOutsideColor;
    }
    
    /// <summary>
    /// 复制当前颜色设置
    /// </summary>
    public ColorTransition CopyCurrentColors()
    {
        return new ColorTransition
        {
            startEdgeColor = currentColors.startEdgeColor,
            endEdgeColor = currentColors.endEdgeColor,
            startInsideColor = currentColors.startInsideColor,
            endInsideColor = currentColors.endInsideColor,
            startOutsideColor = currentColors.startOutsideColor,
            endOutsideColor = currentColors.endOutsideColor
        };
    }
    
    #endregion
}