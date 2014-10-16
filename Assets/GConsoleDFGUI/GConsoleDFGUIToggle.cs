using System;
using UnityEngine;

namespace Assets.GConsoleDFGUI {
   public class GConsoleDFGUIToggle : MonoBehaviour {
#if UNITY_EDITOR
      [Tooltip("Set the toggle key for console show/hide")]
#endif
      [SerializeField]
      private KeyCode _toggleKey = KeyCode.BackQuote;

#if UNITY_EDITOR
      [Tooltip("Drag GConsoleDFGUI object here, so toggler can manage it")]
#endif
      [SerializeField]
      private GConsoleDFGUI _console;

      [SerializeField]
      private bool showOnAwake;

      private void Start() {
         if (_console == null) {
            throw new NullReferenceException("_console field is null, attach DFGUI console in inspector to " + name);
         }

         if (showOnAwake) {
            _console.Show();
         }
         else {
            _console.Hide();
         }
      }

      private void Update() {
         if (Input.GetKeyDown(_toggleKey)) {
            if (_console != null) {
               _console.Toggle();
            }
         }
      }
   }
}
