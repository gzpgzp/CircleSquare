using System;
using Sounds;
using UnityEngine;
using UnityEngine.UI;

namespace NormalUI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonWithSound : MonoBehaviour
    {
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private Button button;

        private void Reset()
        {
            button = GetComponent<Button>();
#if UNITY_EDITOR
            clickSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Sound/UISound/kenney_ui-audio/Audio/click4.ogg");
#endif
        }

        private void Start()
        {
            button.onClick.AddListener(PlaySound);
        }

        private void PlaySound()
        {
            SoundManager.Instance.PlaySound(clickSound);
        }
    }
}