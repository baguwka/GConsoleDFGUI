using System;
using UnityEngine;

namespace Assets.GConsoleDFGUI {
   [RequireComponent(typeof(dfButton))]
   public class GConsoleDFGUISuggestion : MonoBehaviour {
      public event Action<GConsoleDFGUISuggestion> Click;

      private dfButton _button;

      private string _text;

      public dfButton Button {
         get { return _button; }
      }

      public string Text {
         get { return _text; }
         private set { _text = value; }
      }

      private void Start() {
         _button = GetComponent<dfButton>();
         _button.Click += (control, @event) => onClick(this);
         Hide();
      }

      public void Focus() {
         _button.Focus();
      }

      public void Show(GConsoleItem item) {
         if (string.IsNullOrEmpty(item.Colored)) {
            Hide();
         }
         else {
            Text = item.Raw;
            Button.Text = item;
            Button.IsVisible = true;
         }
      }

      public void Hide() {
         //for DFGUI IsVisible toggle faster than GameObject.SetActive
         Button.IsVisible = false;
      }

      protected virtual void onClick(GConsoleDFGUISuggestion obj) {
         var handler = Click;
         if (handler != null) handler(obj);
      }
   }
}