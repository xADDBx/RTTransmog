// Copyright < 2024 > Narria (github user Cabarius) - License: MIT
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RTTransmog {
    public static class Extensions {
        public static bool Matches(this string source, string query) {
            if (source == null || query == null)
                return false;
            return source.IndexOf(query, 0, StringComparison.InvariantCultureIgnoreCase) != -1;
        }
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default) {
            if (dictionary == null) { throw new ArgumentNullException(nameof(dictionary)); }
            if (key == null) { throw new ArgumentNullException(nameof(key)); }

            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
        public static Dictionary<TKey, TElement> ToDictionaryIgnoringDuplicates<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null) {
            if (source == null)
                throw new ArgumentException("source");
            if (keySelector == null)
                throw new ArgumentException("keySelector");
            if (elementSelector == null)
                throw new ArgumentException("elementSelector");
            Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource element in source) {
                if (!d.ContainsKey(keySelector(element)))
                    d.Add(keySelector(element), elementSelector(element));
            }
            return d;
        }
        public static GUILayout.HorizontalScope HorizontalScope(params GUILayoutOption[] options) => new(options);
        public static GUILayout.VerticalScope VerticalScope(params GUILayoutOption[] options) => new(options);
        public static GUILayoutOption AutoWidth() => GUILayout.ExpandWidth(false);
        public static GUILayoutOption MinWidth(int width) => GUILayout.MinWidth(width);
        public static GUILayoutOption Width(float width) => GUILayout.Width(width);
        public static GUILayoutOption Width(int width) => GUILayout.Width(width);
        public static void Space(float size) => GUILayout.Space(size);
        public static void Space(int size) => GUILayout.Space(size);
        public static void Div(float indent = 0, float height = 0, float width = 0) => DrawDiv(new(1f, 1f, 1f, 0.65f), indent, height, width);
        public static void DrawDiv(Color color, float indent = 0, float height = 0, float width = 0) {
            var fillTexture = new Texture2D(1, 1);
            //if (divStyle == null) {
            var divStyle = new GUIStyle {
                fixedHeight = 1,
            };
            //}
            fillTexture.SetPixel(0, 0, color);
            fillTexture.Apply();
            divStyle.normal.background = fillTexture;
            if (divStyle.margin == null) {
                divStyle.margin = new RectOffset((int)indent, 0, 4, 4);
            } else {
                divStyle.margin.left = (int)indent + 3;
            }
            if (width > 0)
                divStyle.fixedWidth = width;
            else
                divStyle.fixedWidth = 0;
            Space(2f * height / 3f);
            GUILayout.Box(GUIContent.none, divStyle);
            Space(height / 3f);
        }
        public static bool ActionButton(string title, Action action, params GUILayoutOption[] options) {
            if (!GUILayout.Button(title, options)) {
                return false;
            }

            action?.Invoke();

            return true;
        }
        public static void Label(string title, params GUILayoutOption[] options) => GUILayout.Label(title, options);
        public static string Color(this string str, string rrggbbaa) => $"<color=#{rrggbbaa}>{str}</color>";
        public static string color(this string str, RGBA color) => $"<color=#{color:X}>{str}</color>";
        public static string Orange(this string s) => s.Color("orange");
        public static string Cyan(this string s) => s.Color("cyan");
        public static string Green(this string s) => s.Color("#00ff00ff");
        public static string Bold(this string str) => $"<b>{str}</b>";
        private static GUIStyle _toggleStyle;
        public static GUIStyle toggleStyle {
            get {
                if (_toggleStyle == null)
                    _toggleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
                return _toggleStyle;
            }
        }
        public static bool DisclosureToggle(string title, ref bool value, float width = 175, params Action[] actions) {
            var changed = TogglePrivate(title, ref value, false, true, width);
            If(value, actions);
            return changed;
        }
        public static bool TogglePrivate(string title, ref bool value, bool isEmpty, bool disclosureStyle = false, float width = 0, params GUILayoutOption[] options) {
            var changed = false;
            if (width == 0 && !disclosureStyle) {
                width = toggleStyle.CalcSize(new GUIContent(title.Bold())).x + GUI.skin.box.CalcSize(CheckOn).x + 10;
            }
            options = options.AddItem(width == 0 ? AutoWidth() : Width(width)).ToArray();
            if (!disclosureStyle) {
                title = value ? title.Bold() : title.color(RGBA.medgrey).Bold();
                if (CheckBox(title, value, isEmpty, toggleStyle, options)) { value = !value; changed = true; }
            } else {
                if (DisclosureToggle(title, value, isEmpty, options)) { value = !value; changed = true; }
            }
            return changed;
        }
        public static bool Toggle(GUIContent label, bool value, GUIContent on, GUIContent off, GUIStyle stateStyle, GUIStyle labelStyle, bool isEmpty = false, params GUILayoutOption[] options) {
            var state = value ? on : off;
            var sStyle = new GUIStyle(stateStyle);
            var lStyle = new GUIStyle(labelStyle) {
                wordWrap = false
            };
            var stateSize = sStyle.CalcSize(state);
            lStyle.fixedHeight = stateSize.y - 2;
            var padding = new RectOffset(0, (int)stateSize.x + 5, 0, 0);
            lStyle.padding = padding;
            var rect = GUILayoutUtility.GetRect(label, lStyle, options);
            return Toggle(rect, label, value, isEmpty, on, off, stateStyle, labelStyle);
        }
        public static bool CheckBox(string label, bool value, bool isEmpty, GUIStyle style, params GUILayoutOption[] options) => Toggle(LabelContent(label), value, CheckOn, CheckOff, GUI.skin.box, style, isEmpty, options);
        public static bool DisclosureToggle(string label, bool value, bool isEmpty = false, params GUILayoutOption[] options) => DisclosureToggle(label, value, GUI.skin.box, GUI.skin.label, isEmpty, options);
        public static bool DisclosureToggle(string label, bool value, GUIStyle stateStyle, GUIStyle labelStyle, bool isEmpty = false, params GUILayoutOption[] options) => Toggle(LabelContent(label), value, DisclosureOn, DisclosureOff, stateStyle, labelStyle, isEmpty, options);

        public static readonly GUIContent DisclosureOn = new("<color=orange><b>▼</b></color>");
        public static readonly GUIContent DisclosureOff = new("<color=#C0C0C0FF><b>▶</b></color>");
        public static readonly GUIContent DisclosureEmpty = new(" <color=#B8B8B8FF>▪</color> ");
        public static readonly GUIContent CheckOn = new("<color=green><b>✔</b></color>");
        public static readonly GUIContent CheckOff = new("<color=#B8B8B8FF>✖</color>");
        private static readonly GUIContent _LabelContent = new();
        private static readonly int s_ButtonHint = "MyGUI.Button".GetHashCode();
        private static GUIContent LabelContent(string text) {
            _LabelContent.text = text;
            _LabelContent.image = null;
            _LabelContent.tooltip = null;
            return _LabelContent;
        }
        public static bool Toggle(Rect rect, GUIContent label, bool value, bool isEmpty, GUIContent on, GUIContent off, GUIStyle stateStyle, GUIStyle labelStyle) {
            var controlID = GUIUtility.GetControlID(s_ButtonHint, FocusType.Passive, rect);
            var result = false;
            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (GUI.enabled && rect.Contains(Event.current.mousePosition)) {
                        GUIUtility.hotControl = controlID;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID) Event.current.Use();
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID) {
                        GUIUtility.hotControl = 0;

                        if (rect.Contains(Event.current.mousePosition)) {
                            result = true;
                            Event.current.Use();
                        }
                    }
                    break;

                case EventType.KeyDown:
                    if (GUIUtility.hotControl == controlID)
                        if (Event.current.keyCode == KeyCode.Escape) {
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                        }
                    break;

                case EventType.Repaint: {
                        var rightAlign = stateStyle.alignment == TextAnchor.MiddleRight
                                         || stateStyle.alignment == TextAnchor.UpperRight
                                         || stateStyle.alignment == TextAnchor.LowerRight
                            ;
                        var state = isEmpty
                                        ? DisclosureEmpty
                                        : value
                                            ? on
                                            : off;
                        var stateSize = stateStyle.CalcSize(value ? on : off);
                        var x = rightAlign ? rect.xMax - stateSize.x : rect.x;
                        Rect stateRect = new(x, rect.y, stateSize.x, stateSize.y);

                        var labelSize = labelStyle.CalcSize(label);
                        x = rightAlign ? stateRect.x - stateSize.x - 5 : stateRect.xMax + 5;
                        Rect labelRect = new(x, rect.y, labelSize.x, labelSize.y);

                        stateStyle.Draw(stateRect, state, controlID);
                        labelStyle.Draw(labelRect, label, controlID);
                    }
                    break;
            }
            return result;
        }
        public static void If(bool value, params Action[] actions) {
            if (value) {
                foreach (var action in actions) {
                    action();
                }
            }
        }
        public enum RGBA : uint {
            aqua = 0x00ffffff,
            blue = 0x8080ffff,
            brown = 0xC09050ff,
            crimson = 0x7b0340ff,
            cyan = 0x00ffffff,
            darkblue = 0x0000a0ff,
            charcoal = 0x202020ff,
            darkgrey = 0x808080ff,
            darkred = 0xa0333bff,
            fuchsia = 0xff40ffff,
            green = 0x40C040ff,
            gold = 0xED9B1Aff,
            lightblue = 0xd8e6ffff,
            lightgrey = 0xE8E8E8ff,
            lime = 0x40ff40ff,
            magenta = 0xff40ffff,
            maroon = 0xFF6060ff,
            medred = 0xd03333ff,
            navy = 0x3b5681ff,
            olive = 0xb0b000ff,
            orange = 0xffa500ff,
            darkorange = 0xb1521fff,
            pink = 0xf03399ff,
            purple = 0xC060F0ff,
            red = 0xFF4040ff,
            black = 0x000000ff,
            medgrey = 0xA8A8A8ff,
            grey = 0xC0C0C0ff,
            silver = 0xD0D0D0ff,
            teal = 0x80f0c0ff,
            yellow = 0xffff00ff,
            white = 0xffffffff,
            none = silver
        }
        public static Color Color(this RGBA rga, float adjust = 0) {
            var red = (float)((long)rga >> 24) / 256f;
            var green = (float)(0xFF & ((long)rga >> 16)) / 256f;
            var blue = (float)(0xFF & ((long)rga >> 8)) / 256f;
            var alpha = (float)(0xFF & ((long)rga)) / 256f;
            var color = new Color(red, green, blue, alpha);
            if (adjust < 0)
                color = UnityEngine.Color.Lerp(color, UnityEngine.Color.black, -adjust);
            if (adjust > 0)
                color = UnityEngine.Color.Lerp(color, UnityEngine.Color.white, adjust);
            return color;
        }
        public static void ActionIntTextField(ref int value, string name, Action<int> action, Action enterAction, params GUILayoutOption[] options) => ActionIntTextField(ref value, name, action, enterAction, int.MinValue, int.MaxValue, options);
        public static void ActionIntTextField(ref int value, string name, Action<int> action, Action enterAction, int min = 0, int max = int.MaxValue, params GUILayoutOption[] options) {
            var changed = false;
            var hitEnter = false;
            var str = $"{value}";
            ActionTextField(ref str, name, (text) => { changed = true; }, () => { hitEnter = true; }, options);
            int.TryParse(str, out value);
            value = Math.Min(max, Math.Max(value, min));
            if (changed) { action?.Invoke(value); }
            if (hitEnter && enterAction != null) { enterAction(); }
        }
        public static void ActionTextField(ref string text, string name, Action<string> action, Action enterAction, params GUILayoutOption[] options) {
            if (name != null)
                GUI.SetNextControlName(name);
            options ??= [AutoWidth()];
            var newText = GUILayout.TextField(text, options);
            if (newText != text) {
                text = newText;
                action?.Invoke(text);
            }
            if (name != null && enterAction != null && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == name) {
                enterAction();
            }
        }
        private static Texture2D _rarityTexture = null;
        public static Texture2D RarityTexture {
            get {
                if (_rarityTexture == null)
                    _rarityTexture = new Texture2D(1, 1);
                _rarityTexture.SetPixel(0, 0, RGBA.black.Color());
                _rarityTexture.Apply();
                return _rarityTexture;
            }
        }
        private static GUIStyle _rarityStyle;
        public static GUIStyle rarityStyle {
            get {
                if (_rarityStyle == null) {
                    _rarityStyle = new GUIStyle(GUI.skin.button);
                    _rarityStyle.normal.background = RarityTexture;
                }
                return _rarityStyle;
            }
        }
    }
}