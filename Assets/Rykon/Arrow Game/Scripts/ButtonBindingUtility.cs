using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ArrowGame
{
    internal static class ButtonBindingUtility
    {
        public static void Bind(Button button, UnityAction action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        public static void BindAction(Button button, Action action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action.Invoke());
        }
    }
}
