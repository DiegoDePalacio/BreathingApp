using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BreathingApp
{
    [RequireComponent(typeof(RawImage))]
    public class ColorPicker : MonoBehaviour
    {
        [SerializeField] private List<Color> _colors;

        private RawImage _image;

        private void Awake()
        {
            _image = GetComponent<RawImage>();
            Assert.IsNotNull(_image);
        }

        public void OnEnable()
        {
            Recolor();
        }

        public void Recolor()
        {
            int childNumber = transform.GetSiblingIndex();
            int colorIndex = childNumber % _colors.Count;
            _image.color = _colors[colorIndex];
        }
    }
}