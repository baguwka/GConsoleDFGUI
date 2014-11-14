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

      public dfControl Control {
         get { return _control ?? (_control = GetComponent<dfControl>()); }
      }

      public dfTextbox Input {
         get {
            if (_input != null) return _input;

            _input = Control.Find<dfTextbox>("Input");
            if (_input == null) {
               throw new NullReferenceException("_inputBox is null or script " + typeof(dfTextbox).Name + " not attached, attach it in the inspector to \"Input\" GameObject.");
            }

            return _input;
         }
      }

      public dfScrollPanel ScrollPanel {
         get { return _scrollPanel ?? (_scrollPanel = Control.Find<dfScrollPanel>("ScrollView")); }
      }

      public dfLabel Output {
         get {
            if (_output != null) return _output;

            _output = Control.Find<dfLabel>("Output");
            if (_output == null) {
               throw new NullReferenceException("_outputBox is null or script " + typeof(dfLabel).Name + " not attached, attach it in the inspector to \"Output\" GameObject.");
            }

            return _output;
         }
      }

      public bool IsVisible {
         get { return Control.IsVisible; }
         set {
            Control.IsVisible = value;
            if (_resetScrollPositionOnShow && value) updateScroll();
         }
      }

      public bool ClearOnSubmit {
         get { return _clearOnSubmit; }
      }

      private void Start() {
         if (_suggestions.Length == 0) {
            Debug.LogWarning("There is no suggestions attached, attach them in Unity Inspector on this script.");
         }

         GConsole.Color = (text, color) => string.Format("[color #{0}]{1}[/color]", color, text);
      }

      private void OnEnable() {
         Input.Text = string.Empty;
         Output.Text = string.Empty;

         Input.TextSubmitted += onSubmit;
         Input.TextChanged += onInputTextChanged;
         Input.EnterFocus += onEnterFocus;
         Input.LostFocus += onLostFocus;
         GConsole.OnOutput += onOutput;

         foreach (var suggestion in _suggestions) {
            suggestion.Click += onSuggestionClick;
         }
      }

      private void OnDisable() {
         Input.TextSubmitted -= onSubmit;
         Input.TextChanged -= onInputTextChanged;
         Input.EnterFocus -= onEnterFocus;
         Input.LostFocus -= onLostFocus;
         GConsole.OnOutput -= onOutput;

         foreach (var suggestion in _suggestions) {
            suggestion.Click -= onSuggestionClick;
         }
      }

      private void onInputTextChanged(dfControl control, string value) {
         loadSuggestions();
      }

      private void onSubmit(dfControl control, string cmd) {
         if (string.IsNullOrEmpty(cmd)) return;

         GConsole.Eval(cmd);
         if (_clearOnSubmit) {
            Input.Text = string.Empty;
         }
         updateFocus();
      }

      private void onOutput(string line) {
         Output.Text += string.Format("{0}{1}", Environment.NewLine, line);
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
         if (Input.Text.Length < minCharBeforeSuggestions) {
            hideSuggestions();
            return;
         }

         var sugStrings = GConsole.GetSuggestionItems(Input.Text);

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
         Input.Text = sender.Text;
         updateFocus();
      }

      private void updateFocus() {
         if (_reselectOnSubmit) {
            Input.Focus();
            Input.CursorIndex = Input.Text.Length;
         }
      }

      private void updateScroll() {
         ScrollPanel.ScrollPosition = new Vector2(0, float.MaxValue);
      }

      public void Show() {
         IsVisible = true;
      }

      public void Hide() {
         IsVisible = false;
      }

      public void Toggle() {
         IsVisible = !Control.IsVisible;
      }
   }
}