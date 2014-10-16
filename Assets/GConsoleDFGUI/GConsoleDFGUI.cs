using System;
using System.Linq;
using UnityEngine;

namespace Assets.GConsoleDFGUI {
   [RequireComponent(typeof(dfControl))]
   public class GConsoleDFGUI : MonoBehaviour {
      private dfControl _control;
      private dfTextbox _input;
      private dfScrollPanel _scrollPanel;
      private dfLabel _output;

#if UNITY_EDITOR
      [Tooltip("Will clear input control from text after user submit it.")]
#endif
      [SerializeField]
      private bool _clearOnSubmit = true;

#if UNITY_EDITOR
      [Tooltip("Will re select input control when user submit text.")]
#endif
      [SerializeField]
      private bool _reselectOnSubmit = true;

#if UNITY_EDITOR
      [Tooltip("Will reset scroll position to bottom on panel show.")]
#endif
      [SerializeField]
      private bool _resetScrollPositionOnShow = true;

#if UNITY_EDITOR
      [Tooltip("How much characters user needs to type to see any suggestions.")]
#endif
      [SerializeField]
      private int minCharBeforeSuggestions = 1;

      [SerializeField]
      private GConsoleDFGUISuggestion[] _suggestions;

      public bool IsVisible {
         get { return _control.IsVisible; }
         set {
            _control.IsVisible = value;
            if (_resetScrollPositionOnShow && value) updateScroll();
         }
      }

      public bool ClearOnSubmit {
         get { return _clearOnSubmit; }
      }

      private void Start() {
         _control = GetComponent<dfControl>();
         _input = _control.Find<dfTextbox>("Input");
         if (_input == null) {
            throw new NullReferenceException("_inputBox is null or script " + typeof(dfTextbox).Name + " not attached, attach it in the inspector to \"Input\" GameObject.");
         }
         _input.Text = string.Empty;

         _scrollPanel = _control.Find<dfScrollPanel>("ScrollView");
         _output = _scrollPanel.Find<dfLabel>("Output");
         if (_output == null) {
            throw new NullReferenceException("_outputBox is null or script " + typeof(dfTextbox).Name + " not attached, attach it in the inspector to \"Output\" GameObject.");
         }
         _output.Text = string.Empty;

         if (_suggestions.Length == 0) {
            Debug.LogWarning("There is no suggestions attached, attach them in Unity Inspector on this script.");
         }

         GConsole.SetColorCode((text, color) => string.Format("[color #{0}]{1}[/color]", color, text));

         foreach (var suggestion in _suggestions) {
            suggestion.Click += onSuggestionClick;
         }

         _input.TextSubmitted += onSubmit;
         _input.TextChanged += onInputTextChanged;
         _input.EnterFocus += onEnterFocus;
         _input.LostFocus += onLostFocus;
         GConsole.OnOutput += onOutput;
      }

      private void OnDestroy() {
         foreach (var suggestion in _suggestions) {
            suggestion.Click -= onSuggestionClick;
         }

         _input.TextSubmitted -= onSubmit;
         _input.TextChanged -= onInputTextChanged;
         _input.EnterFocus -= onEnterFocus;
         _input.LostFocus -= onLostFocus;
         GConsole.OnOutput -= onOutput;
      }

      private void onInputTextChanged(dfControl control, string value) {
         loadSuggestions();
      }

      private void onSubmit(dfControl control, string cmd) {
         if (string.IsNullOrEmpty(cmd)) return;

         GConsole.Eval(cmd);
         if (_clearOnSubmit) {
            _input.Text = string.Empty;
         }
         updateFocus();
      }

      private void onOutput(string line) {
         _output.Text += string.Format("{0}{1}", Environment.NewLine, line);
         updateScroll();
      }

      private void onEnterFocus(dfControl control, dfFocusEventArgs args) {
         loadSuggestions();
      }

      private void onLostFocus(dfControl control, dfFocusEventArgs args) {
         if (_suggestions.Any(s => s.Button.HasFocus)) return;
         hideSuggestions();
      }

      private void loadSuggestions() {
         if (_input.Text.Length < minCharBeforeSuggestions) {
            hideSuggestions();
            return;
         }

         var sugStrings = GConsole.GetSuggestionItems(_input.Text);

         for (int i = 0; i < _suggestions.Length; i++) {
            if (i < sugStrings.Count) {
               _suggestions[i].Show(sugStrings[i]);
            }
            else {
               _suggestions[i].Hide();
            }
         }
      }

      private void hideSuggestions() {
         foreach (var suggestion in _suggestions) {
            suggestion.Hide();
         }
      }

      private void onSuggestionClick(GConsoleDFGUISuggestion sender) {
         _input.Text = sender.Text;
         updateFocus();
      }

      private void updateFocus() {
         if (_reselectOnSubmit) {
            _input.Focus();
            _input.CursorIndex = _input.Text.Length;
         }
      }

      private void updateScroll() {
         _scrollPanel.ScrollPosition = new Vector2(0, float.MaxValue);
      }

      public void Show() {
         IsVisible = true;
      }

      public void Hide() {
         IsVisible = false;
      }

      public void Toggle() {
         IsVisible = !_control.IsVisible;
      }
   }
}