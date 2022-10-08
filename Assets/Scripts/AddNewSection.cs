using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BreathingApp
{
    [RequireComponent(typeof(Button))]
    public class AddNewSection : MonoBehaviour
    {
        [SerializeField] private LayoutGroup _layoutGroup;
        [SerializeField] private Breathe _breatheSection;

        private Button _button;

        private void Awake()
        {
            Assert.IsNotNull(_layoutGroup);
            Assert.IsNotNull(_breatheSection);
            
            _button = GetComponent<Button>();
            Assert.IsNotNull(_button);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnAddNewSectionButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnAddNewSectionButtonClicked);
        }

        private void OnAddNewSectionButtonClicked()
        {
            Instantiate(_breatheSection, _breatheSection.transform.parent);
        }
    }
}