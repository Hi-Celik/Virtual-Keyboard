using System.Collections.Generic;
using CyberKeyboard.Native;
using CyberKeyboard.Services;

namespace CyberKeyboard.Models
{
    public class KeyboardRow
    {
        public List<KeyModel> Keys { get; set; } = new();
    }

    public static class KeyboardLayouts
    {
        public static List<KeyModel> CreateFunctionRow()
        {
            var keys = new List<KeyModel>
            {
                new() { NormalText = "ESC", VkCode = NativeMethods.VK_ESCAPE, Type = KeyType.Function, WidthRatio = 1.0, SpecialAction = "esc" },
                new() { NormalText = "F1", VkCode = NativeMethods.VK_F1, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F2", VkCode = NativeMethods.VK_F2, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F3", VkCode = NativeMethods.VK_F3, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F4", VkCode = NativeMethods.VK_F4, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F5", VkCode = NativeMethods.VK_F5, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F6", VkCode = NativeMethods.VK_F6, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F7", VkCode = NativeMethods.VK_F7, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F8", VkCode = NativeMethods.VK_F8, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F9", VkCode = NativeMethods.VK_F9, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F10", VkCode = NativeMethods.VK_F10, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F11", VkCode = NativeMethods.VK_F11, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "F12", VkCode = NativeMethods.VK_F12, Type = KeyType.Function, WidthRatio = 1.0 },
                new() { NormalText = "PRT SC", VkCode = NativeMethods.VK_SNAPSHOT, Type = KeyType.Function, WidthRatio = 1.1 }
            };
            return keys;
        }

        public static List<KeyboardRow> GetLayout(SupportedLanguage lang)
        {
            return lang switch
            {
                SupportedLanguage.Turkish => CreateTurkishQ(),
                SupportedLanguage.English => CreateEnglishUS(),
                SupportedLanguage.FrenchAzerty => CreateFrenchAZERTY(),
                SupportedLanguage.FrenchQwerty => CreateFrenchQWERTY(),
                SupportedLanguage.German => CreateGermanQWERTZ(),
                _ => CreateTurkishQ()
            };
        }

        private static List<KeyboardRow> CreateTurkishQ()
        {
            var rows = new List<KeyboardRow>();

            // Row 1: Numbers & Symbols
            var r1 = new KeyboardRow();
            r1.Keys.Add(new() { NormalText = "\"", ShiftText = "é", AltGrText = "<" });
            r1.Keys.Add(new() { NormalText = "1", ShiftText = "!", AltGrText = "" });
            r1.Keys.Add(new() { NormalText = "2", ShiftText = "'", AltGrText = "£" });
            r1.Keys.Add(new() { NormalText = "3", ShiftText = "^", AltGrText = "#" });
            r1.Keys.Add(new() { NormalText = "4", ShiftText = "+", AltGrText = "$" });
            r1.Keys.Add(new() { NormalText = "5", ShiftText = "%", AltGrText = "½" });
            r1.Keys.Add(new() { NormalText = "6", ShiftText = "&", AltGrText = "" });
            r1.Keys.Add(new() { NormalText = "7", ShiftText = "/", AltGrText = "{" });
            r1.Keys.Add(new() { NormalText = "8", ShiftText = "(", AltGrText = "[" });
            r1.Keys.Add(new() { NormalText = "9", ShiftText = ")", AltGrText = "]" });
            r1.Keys.Add(new() { NormalText = "0", ShiftText = "=", AltGrText = "}" });
            r1.Keys.Add(new() { NormalText = "*", ShiftText = "?", AltGrText = "\\" });
            r1.Keys.Add(new() { NormalText = "-", ShiftText = "_", AltGrText = "|" });
            r1.Keys.Add(new() { NormalText = "⌫ BACK", VkCode = NativeMethods.VK_BACK, WidthRatio = 1.8, Type = KeyType.Action, SpecialAction = "backspace" });
            rows.Add(r1);

            // Row 2: QWERTY
            var r2 = new KeyboardRow();
            r2.Keys.Add(new() { NormalText = "TAB ⇥", VkCode = NativeMethods.VK_TAB, WidthRatio = 1.4, Type = KeyType.Action, SpecialAction = "tab" });
            r2.Keys.Add(new() { NormalText = "q", ShiftText = "Q", AltGrText = "@" });
            r2.Keys.Add(new() { NormalText = "w", ShiftText = "W" });
            r2.Keys.Add(new() { NormalText = "e", ShiftText = "E", AltGrText = "€" });
            r2.Keys.Add(new() { NormalText = "r", ShiftText = "R" });
            r2.Keys.Add(new() { NormalText = "t", ShiftText = "T" });
            r2.Keys.Add(new() { NormalText = "y", ShiftText = "Y" });
            r2.Keys.Add(new() { NormalText = "u", ShiftText = "U" });
            r2.Keys.Add(new() { NormalText = "ı", ShiftText = "I" });
            r2.Keys.Add(new() { NormalText = "o", ShiftText = "O" });
            r2.Keys.Add(new() { NormalText = "p", ShiftText = "P" });
            r2.Keys.Add(new() { NormalText = "ğ", ShiftText = "Ğ" });
            r2.Keys.Add(new() { NormalText = "ü", ShiftText = "Ü", AltGrText = "~" });
            r2.Keys.Add(new() { NormalText = ",", ShiftText = ";", AltGrText = "`" });
            rows.Add(r2);

            // Row 3: ASDF
            var r3 = new KeyboardRow();
            r3.Keys.Add(new() { NormalText = "CAPS ⇪", VkCode = NativeMethods.VK_CAPITAL, WidthRatio = 1.7, Type = KeyType.Modifier, SpecialAction = "caps" });
            r3.Keys.Add(new() { NormalText = "a", ShiftText = "A", AltGrText = "æ" });
            r3.Keys.Add(new() { NormalText = "s", ShiftText = "S", AltGrText = "ß" });
            r3.Keys.Add(new() { NormalText = "d", ShiftText = "D" });
            r3.Keys.Add(new() { NormalText = "f", ShiftText = "F" });
            r3.Keys.Add(new() { NormalText = "g", ShiftText = "G" });
            r3.Keys.Add(new() { NormalText = "h", ShiftText = "H" });
            r3.Keys.Add(new() { NormalText = "j", ShiftText = "J" });
            r3.Keys.Add(new() { NormalText = "k", ShiftText = "K" });
            r3.Keys.Add(new() { NormalText = "l", ShiftText = "L" });
            r3.Keys.Add(new() { NormalText = "ş", ShiftText = "Ş" });
            r3.Keys.Add(new() { NormalText = "i", ShiftText = "İ" });
            r3.Keys.Add(new() { NormalText = ";", ShiftText = ":" });
            r3.Keys.Add(new() { NormalText = "ENTER ↵", VkCode = NativeMethods.VK_RETURN, WidthRatio = 1.8, Type = KeyType.Action, SpecialAction = "enter" });
            rows.Add(r3);

            // Row 4: ZXCV
            var r4 = new KeyboardRow();
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 1.6, Type = KeyType.Modifier, SpecialAction = "shift" });
            r4.Keys.Add(new() { NormalText = "<", ShiftText = ">", AltGrText = "|" });
            r4.Keys.Add(new() { NormalText = "z", ShiftText = "Z" });
            r4.Keys.Add(new() { NormalText = "x", ShiftText = "X" });
            r4.Keys.Add(new() { NormalText = "c", ShiftText = "C" });
            r4.Keys.Add(new() { NormalText = "v", ShiftText = "V" });
            r4.Keys.Add(new() { NormalText = "b", ShiftText = "B" });
            r4.Keys.Add(new() { NormalText = "n", ShiftText = "N" });
            r4.Keys.Add(new() { NormalText = "m", ShiftText = "M" });
            r4.Keys.Add(new() { NormalText = "ö", ShiftText = "Ö" });
            r4.Keys.Add(new() { NormalText = "ç", ShiftText = "Ç" });
            r4.Keys.Add(new() { NormalText = ".", ShiftText = ":" });
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 2.1, Type = KeyType.Modifier, SpecialAction = "shift" });
            rows.Add(r4);

            // Row 5: Bottom row
            var r5 = new KeyboardRow();
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_CONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_LWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "ALT", VkCode = NativeMethods.VK_MENU, WidthRatio = 1.2, Type = KeyType.Modifier, SpecialAction = "alt" });
            r5.Keys.Add(new() { NormalText = "SPACE", VkCode = NativeMethods.VK_SPACE, WidthRatio = 5.6, Type = KeyType.Space, SpecialAction = "space" });
            r5.Keys.Add(new() { NormalText = "ALT GR", VkCode = NativeMethods.VK_RMENU, WidthRatio = 1.4, Type = KeyType.Modifier, SpecialAction = "altgr" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_RWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "☰", VkCode = NativeMethods.VK_APPS, WidthRatio = 1.1, Type = KeyType.Action });
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_RCONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            rows.Add(r5);

            return rows;
        }

        private static List<KeyboardRow> CreateEnglishUS()
        {
            var rows = new List<KeyboardRow>();

            // Row 1
            var r1 = new KeyboardRow();
            r1.Keys.Add(new() { NormalText = "`", ShiftText = "~" });
            r1.Keys.Add(new() { NormalText = "1", ShiftText = "!" });
            r1.Keys.Add(new() { NormalText = "2", ShiftText = "@" });
            r1.Keys.Add(new() { NormalText = "3", ShiftText = "#" });
            r1.Keys.Add(new() { NormalText = "4", ShiftText = "$" });
            r1.Keys.Add(new() { NormalText = "5", ShiftText = "%" });
            r1.Keys.Add(new() { NormalText = "6", ShiftText = "^" });
            r1.Keys.Add(new() { NormalText = "7", ShiftText = "&" });
            r1.Keys.Add(new() { NormalText = "8", ShiftText = "*" });
            r1.Keys.Add(new() { NormalText = "9", ShiftText = "(" });
            r1.Keys.Add(new() { NormalText = "0", ShiftText = ")" });
            r1.Keys.Add(new() { NormalText = "-", ShiftText = "_" });
            r1.Keys.Add(new() { NormalText = "=", ShiftText = "+" });
            r1.Keys.Add(new() { NormalText = "⌫ BACK", VkCode = NativeMethods.VK_BACK, WidthRatio = 1.8, Type = KeyType.Action, SpecialAction = "backspace" });
            rows.Add(r1);

            // Row 2
            var r2 = new KeyboardRow();
            r2.Keys.Add(new() { NormalText = "TAB ⇥", VkCode = NativeMethods.VK_TAB, WidthRatio = 1.4, Type = KeyType.Action, SpecialAction = "tab" });
            r2.Keys.Add(new() { NormalText = "q", ShiftText = "Q" });
            r2.Keys.Add(new() { NormalText = "w", ShiftText = "W" });
            r2.Keys.Add(new() { NormalText = "e", ShiftText = "E" });
            r2.Keys.Add(new() { NormalText = "r", ShiftText = "R" });
            r2.Keys.Add(new() { NormalText = "t", ShiftText = "T" });
            r2.Keys.Add(new() { NormalText = "y", ShiftText = "Y" });
            r2.Keys.Add(new() { NormalText = "u", ShiftText = "U" });
            r2.Keys.Add(new() { NormalText = "i", ShiftText = "I" });
            r2.Keys.Add(new() { NormalText = "o", ShiftText = "O" });
            r2.Keys.Add(new() { NormalText = "p", ShiftText = "P" });
            r2.Keys.Add(new() { NormalText = "[", ShiftText = "{" });
            r2.Keys.Add(new() { NormalText = "]", ShiftText = "}" });
            r2.Keys.Add(new() { NormalText = "\\", ShiftText = "|" });
            rows.Add(r2);

            // Row 3
            var r3 = new KeyboardRow();
            r3.Keys.Add(new() { NormalText = "CAPS ⇪", VkCode = NativeMethods.VK_CAPITAL, WidthRatio = 1.7, Type = KeyType.Modifier, SpecialAction = "caps" });
            r3.Keys.Add(new() { NormalText = "a", ShiftText = "A" });
            r3.Keys.Add(new() { NormalText = "s", ShiftText = "S" });
            r3.Keys.Add(new() { NormalText = "d", ShiftText = "D" });
            r3.Keys.Add(new() { NormalText = "f", ShiftText = "F" });
            r3.Keys.Add(new() { NormalText = "g", ShiftText = "G" });
            r3.Keys.Add(new() { NormalText = "h", ShiftText = "H" });
            r3.Keys.Add(new() { NormalText = "j", ShiftText = "J" });
            r3.Keys.Add(new() { NormalText = "k", ShiftText = "K" });
            r3.Keys.Add(new() { NormalText = "l", ShiftText = "L" });
            r3.Keys.Add(new() { NormalText = ";", ShiftText = ":" });
            r3.Keys.Add(new() { NormalText = "'", ShiftText = "\"" });
            r3.Keys.Add(new() { NormalText = "ENTER ↵", VkCode = NativeMethods.VK_RETURN, WidthRatio = 1.9, Type = KeyType.Action, SpecialAction = "enter" });
            rows.Add(r3);

            // Row 4
            var r4 = new KeyboardRow();
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 2.1, Type = KeyType.Modifier, SpecialAction = "shift" });
            r4.Keys.Add(new() { NormalText = "z", ShiftText = "Z" });
            r4.Keys.Add(new() { NormalText = "x", ShiftText = "X" });
            r4.Keys.Add(new() { NormalText = "c", ShiftText = "C" });
            r4.Keys.Add(new() { NormalText = "v", ShiftText = "V" });
            r4.Keys.Add(new() { NormalText = "b", ShiftText = "B" });
            r4.Keys.Add(new() { NormalText = "n", ShiftText = "N" });
            r4.Keys.Add(new() { NormalText = "m", ShiftText = "M" });
            r4.Keys.Add(new() { NormalText = ",", ShiftText = "<" });
            r4.Keys.Add(new() { NormalText = ".", ShiftText = ">" });
            r4.Keys.Add(new() { NormalText = "/", ShiftText = "?" });
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 2.3, Type = KeyType.Modifier, SpecialAction = "shift" });
            rows.Add(r4);

            // Row 5
            var r5 = new KeyboardRow();
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_CONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_LWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "ALT", VkCode = NativeMethods.VK_MENU, WidthRatio = 1.2, Type = KeyType.Modifier, SpecialAction = "alt" });
            r5.Keys.Add(new() { NormalText = "SPACE", VkCode = NativeMethods.VK_SPACE, WidthRatio = 5.8, Type = KeyType.Space, SpecialAction = "space" });
            r5.Keys.Add(new() { NormalText = "ALT", VkCode = NativeMethods.VK_RMENU, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "alt" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_RWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "☰", VkCode = NativeMethods.VK_APPS, WidthRatio = 1.1, Type = KeyType.Action });
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_RCONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            rows.Add(r5);

            return rows;
        }

        private static List<KeyboardRow> CreateFrenchAZERTY()
        {
            var rows = new List<KeyboardRow>();

            // Row 1
            var r1 = new KeyboardRow();
            r1.Keys.Add(new() { NormalText = "²" });
            r1.Keys.Add(new() { NormalText = "&", ShiftText = "1" });
            r1.Keys.Add(new() { NormalText = "é", ShiftText = "2", AltGrText = "~" });
            r1.Keys.Add(new() { NormalText = "\"", ShiftText = "3", AltGrText = "#" });
            r1.Keys.Add(new() { NormalText = "'", ShiftText = "4", AltGrText = "{" });
            r1.Keys.Add(new() { NormalText = "(", ShiftText = "5", AltGrText = "[" });
            r1.Keys.Add(new() { NormalText = "-", ShiftText = "6", AltGrText = "|" });
            r1.Keys.Add(new() { NormalText = "è", ShiftText = "7", AltGrText = "`" });
            r1.Keys.Add(new() { NormalText = "_", ShiftText = "8", AltGrText = "\\" });
            r1.Keys.Add(new() { NormalText = "ç", ShiftText = "9", AltGrText = "^" });
            r1.Keys.Add(new() { NormalText = "à", ShiftText = "0", AltGrText = "@" });
            r1.Keys.Add(new() { NormalText = ")", ShiftText = "°", AltGrText = "]" });
            r1.Keys.Add(new() { NormalText = "=", ShiftText = "+", AltGrText = "}" });
            r1.Keys.Add(new() { NormalText = "⌫ BACK", VkCode = NativeMethods.VK_BACK, WidthRatio = 1.8, Type = KeyType.Action, SpecialAction = "backspace" });
            rows.Add(r1);

            // Row 2
            var r2 = new KeyboardRow();
            r2.Keys.Add(new() { NormalText = "TAB ⇥", VkCode = NativeMethods.VK_TAB, WidthRatio = 1.4, Type = KeyType.Action, SpecialAction = "tab" });
            r2.Keys.Add(new() { NormalText = "a", ShiftText = "A" });
            r2.Keys.Add(new() { NormalText = "z", ShiftText = "Z" });
            r2.Keys.Add(new() { NormalText = "e", ShiftText = "E", AltGrText = "€" });
            r2.Keys.Add(new() { NormalText = "r", ShiftText = "R" });
            r2.Keys.Add(new() { NormalText = "t", ShiftText = "T" });
            r2.Keys.Add(new() { NormalText = "y", ShiftText = "Y" });
            r2.Keys.Add(new() { NormalText = "u", ShiftText = "U" });
            r2.Keys.Add(new() { NormalText = "i", ShiftText = "I" });
            r2.Keys.Add(new() { NormalText = "o", ShiftText = "O" });
            r2.Keys.Add(new() { NormalText = "p", ShiftText = "P" });
            r2.Keys.Add(new() { NormalText = "^", ShiftText = "¨" });
            r2.Keys.Add(new() { NormalText = "$", ShiftText = "£", AltGrText = "¤" });
            r2.Keys.Add(new() { NormalText = "*", ShiftText = "µ" });
            rows.Add(r2);

            // Row 3
            var r3 = new KeyboardRow();
            r3.Keys.Add(new() { NormalText = "CAPS ⇪", VkCode = NativeMethods.VK_CAPITAL, WidthRatio = 1.7, Type = KeyType.Modifier, SpecialAction = "caps" });
            r3.Keys.Add(new() { NormalText = "q", ShiftText = "Q" });
            r3.Keys.Add(new() { NormalText = "s", ShiftText = "S" });
            r3.Keys.Add(new() { NormalText = "d", ShiftText = "D" });
            r3.Keys.Add(new() { NormalText = "f", ShiftText = "F" });
            r3.Keys.Add(new() { NormalText = "g", ShiftText = "G" });
            r3.Keys.Add(new() { NormalText = "h", ShiftText = "H" });
            r3.Keys.Add(new() { NormalText = "j", ShiftText = "J" });
            r3.Keys.Add(new() { NormalText = "k", ShiftText = "K" });
            r3.Keys.Add(new() { NormalText = "l", ShiftText = "L" });
            r3.Keys.Add(new() { NormalText = "m", ShiftText = "M" });
            r3.Keys.Add(new() { NormalText = "ù", ShiftText = "%" });
            r3.Keys.Add(new() { NormalText = "ENTER ↵", VkCode = NativeMethods.VK_RETURN, WidthRatio = 1.9, Type = KeyType.Action, SpecialAction = "enter" });
            rows.Add(r3);

            // Row 4
            var r4 = new KeyboardRow();
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 1.5, Type = KeyType.Modifier, SpecialAction = "shift" });
            r4.Keys.Add(new() { NormalText = "<", ShiftText = ">" });
            r4.Keys.Add(new() { NormalText = "w", ShiftText = "W" });
            r4.Keys.Add(new() { NormalText = "x", ShiftText = "X" });
            r4.Keys.Add(new() { NormalText = "c", ShiftText = "C" });
            r4.Keys.Add(new() { NormalText = "v", ShiftText = "V" });
            r4.Keys.Add(new() { NormalText = "b", ShiftText = "B" });
            r4.Keys.Add(new() { NormalText = "n", ShiftText = "N" });
            r4.Keys.Add(new() { NormalText = ",", ShiftText = "?" });
            r4.Keys.Add(new() { NormalText = ";", ShiftText = "." });
            r4.Keys.Add(new() { NormalText = ":", ShiftText = "/" });
            r4.Keys.Add(new() { NormalText = "!", ShiftText = "§" });
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 2.1, Type = KeyType.Modifier, SpecialAction = "shift" });
            rows.Add(r4);

            // Row 5
            var r5 = new KeyboardRow();
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_CONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_LWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "ALT", VkCode = NativeMethods.VK_MENU, WidthRatio = 1.2, Type = KeyType.Modifier, SpecialAction = "alt" });
            r5.Keys.Add(new() { NormalText = "SPACE", VkCode = NativeMethods.VK_SPACE, WidthRatio = 5.8, Type = KeyType.Space, SpecialAction = "space" });
            r5.Keys.Add(new() { NormalText = "ALT GR", VkCode = NativeMethods.VK_RMENU, WidthRatio = 1.4, Type = KeyType.Modifier, SpecialAction = "altgr" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_RWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "☰", VkCode = NativeMethods.VK_APPS, WidthRatio = 1.1, Type = KeyType.Action });
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_RCONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            rows.Add(r5);

            return rows;
        }

        private static List<KeyboardRow> CreateFrenchQWERTY()
        {
            var rows = new List<KeyboardRow>();

            // Row 1: Numbers & Symbols (French QWERTY with è, é, à, etc.)
            var r1 = new KeyboardRow();
            r1.Keys.Add(new() { NormalText = "è", ShiftText = "È", AltGrText = "|" });
            r1.Keys.Add(new() { NormalText = "1", ShiftText = "!" });
            r1.Keys.Add(new() { NormalText = "2", ShiftText = "\"", AltGrText = "@" });
            r1.Keys.Add(new() { NormalText = "3", ShiftText = "#", AltGrText = "£" });
            r1.Keys.Add(new() { NormalText = "4", ShiftText = "$", AltGrText = "¢" });
            r1.Keys.Add(new() { NormalText = "5", ShiftText = "%", AltGrText = "¤" });
            r1.Keys.Add(new() { NormalText = "6", ShiftText = "?", AltGrText = "¬" });
            r1.Keys.Add(new() { NormalText = "7", ShiftText = "&", AltGrText = "{" });
            r1.Keys.Add(new() { NormalText = "8", ShiftText = "*", AltGrText = "[" });
            r1.Keys.Add(new() { NormalText = "9", ShiftText = "(", AltGrText = "]" });
            r1.Keys.Add(new() { NormalText = "0", ShiftText = ")", AltGrText = "}" });
            r1.Keys.Add(new() { NormalText = "-", ShiftText = "_", AltGrText = "\\" });
            r1.Keys.Add(new() { NormalText = "=", ShiftText = "+", AltGrText = "~" });
            r1.Keys.Add(new() { NormalText = "⌫ BACK", VkCode = NativeMethods.VK_BACK, WidthRatio = 1.8, Type = KeyType.Action, SpecialAction = "backspace" });
            rows.Add(r1);

            // Row 2: QWERTY
            var r2 = new KeyboardRow();
            r2.Keys.Add(new() { NormalText = "TAB ⇥", VkCode = NativeMethods.VK_TAB, WidthRatio = 1.4, Type = KeyType.Action, SpecialAction = "tab" });
            r2.Keys.Add(new() { NormalText = "q", ShiftText = "Q" });
            r2.Keys.Add(new() { NormalText = "w", ShiftText = "W" });
            r2.Keys.Add(new() { NormalText = "e", ShiftText = "E", AltGrText = "€" });
            r2.Keys.Add(new() { NormalText = "r", ShiftText = "R" });
            r2.Keys.Add(new() { NormalText = "t", ShiftText = "T" });
            r2.Keys.Add(new() { NormalText = "y", ShiftText = "Y" });
            r2.Keys.Add(new() { NormalText = "u", ShiftText = "U" });
            r2.Keys.Add(new() { NormalText = "i", ShiftText = "I" });
            r2.Keys.Add(new() { NormalText = "o", ShiftText = "O" });
            r2.Keys.Add(new() { NormalText = "p", ShiftText = "P" });
            r2.Keys.Add(new() { NormalText = "^", ShiftText = "¨", AltGrText = "[" });
            r2.Keys.Add(new() { NormalText = "ç", ShiftText = "Ç", AltGrText = "]" });
            rows.Add(r2);

            // Row 3: ASDF
            var r3 = new KeyboardRow();
            r3.Keys.Add(new() { NormalText = "CAPS ⇪", VkCode = NativeMethods.VK_CAPITAL, WidthRatio = 1.7, Type = KeyType.Modifier, SpecialAction = "caps" });
            r3.Keys.Add(new() { NormalText = "a", ShiftText = "A", AltGrText = "æ" });
            r3.Keys.Add(new() { NormalText = "s", ShiftText = "S", AltGrText = "ß" });
            r3.Keys.Add(new() { NormalText = "d", ShiftText = "D" });
            r3.Keys.Add(new() { NormalText = "f", ShiftText = "F" });
            r3.Keys.Add(new() { NormalText = "g", ShiftText = "G" });
            r3.Keys.Add(new() { NormalText = "h", ShiftText = "H" });
            r3.Keys.Add(new() { NormalText = "j", ShiftText = "J" });
            r3.Keys.Add(new() { NormalText = "k", ShiftText = "K" });
            r3.Keys.Add(new() { NormalText = "l", ShiftText = "L" });
            r3.Keys.Add(new() { NormalText = ";", ShiftText = ":" });
            r3.Keys.Add(new() { NormalText = "`", ShiftText = "~" });
            r3.Keys.Add(new() { NormalText = "à", ShiftText = "À" });
            r3.Keys.Add(new() { NormalText = "ENTER ↵", VkCode = NativeMethods.VK_RETURN, WidthRatio = 1.8, Type = KeyType.Action, SpecialAction = "enter" });
            rows.Add(r3);

            // Row 4: ZXCV with ù and é
            var r4 = new KeyboardRow();
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 1.6, Type = KeyType.Modifier, SpecialAction = "shift" });
            r4.Keys.Add(new() { NormalText = "ù", ShiftText = "Ù" });
            r4.Keys.Add(new() { NormalText = "z", ShiftText = "Z", AltGrText = "«" });
            r4.Keys.Add(new() { NormalText = "x", ShiftText = "X", AltGrText = "»" });
            r4.Keys.Add(new() { NormalText = "c", ShiftText = "C", AltGrText = "©" });
            r4.Keys.Add(new() { NormalText = "v", ShiftText = "V" });
            r4.Keys.Add(new() { NormalText = "b", ShiftText = "B" });
            r4.Keys.Add(new() { NormalText = "n", ShiftText = "N" });
            r4.Keys.Add(new() { NormalText = "m", ShiftText = "M", AltGrText = "µ" });
            r4.Keys.Add(new() { NormalText = ",", ShiftText = "'", AltGrText = "<" });
            r4.Keys.Add(new() { NormalText = ".", ShiftText = ".", AltGrText = ">" });
            r4.Keys.Add(new() { NormalText = "é", ShiftText = "É" });
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 2.1, Type = KeyType.Modifier, SpecialAction = "shift" });
            rows.Add(r4);

            // Row 5: Space & Modifiers
            var r5 = new KeyboardRow();
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_CONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_LWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "ALT", VkCode = NativeMethods.VK_MENU, WidthRatio = 1.2, Type = KeyType.Modifier, SpecialAction = "alt" });
            r5.Keys.Add(new() { NormalText = "SPACE", VkCode = NativeMethods.VK_SPACE, WidthRatio = 5.6, Type = KeyType.Space, SpecialAction = "space" });
            r5.Keys.Add(new() { NormalText = "ALT GR", VkCode = NativeMethods.VK_RMENU, WidthRatio = 1.4, Type = KeyType.Modifier, SpecialAction = "altgr" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_RWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "☰", VkCode = NativeMethods.VK_APPS, WidthRatio = 1.1, Type = KeyType.Action });
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_RCONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            rows.Add(r5);

            return rows;
        }

        private static List<KeyboardRow> CreateGermanQWERTZ()
        {
            var rows = new List<KeyboardRow>();

            // Row 1
            var r1 = new KeyboardRow();
            r1.Keys.Add(new() { NormalText = "^", ShiftText = "°" });
            r1.Keys.Add(new() { NormalText = "1", ShiftText = "!" });
            r1.Keys.Add(new() { NormalText = "2", ShiftText = "\"", AltGrText = "²" });
            r1.Keys.Add(new() { NormalText = "3", ShiftText = "§", AltGrText = "³" });
            r1.Keys.Add(new() { NormalText = "4", ShiftText = "$" });
            r1.Keys.Add(new() { NormalText = "5", ShiftText = "%" });
            r1.Keys.Add(new() { NormalText = "6", ShiftText = "&" });
            r1.Keys.Add(new() { NormalText = "7", ShiftText = "/", AltGrText = "{" });
            r1.Keys.Add(new() { NormalText = "8", ShiftText = "(", AltGrText = "[" });
            r1.Keys.Add(new() { NormalText = "9", ShiftText = ")", AltGrText = "]" });
            r1.Keys.Add(new() { NormalText = "0", ShiftText = "=", AltGrText = "}" });
            r1.Keys.Add(new() { NormalText = "ß", ShiftText = "?", AltGrText = "\\" });
            r1.Keys.Add(new() { NormalText = "´", ShiftText = "`" });
            r1.Keys.Add(new() { NormalText = "⌫ BACK", VkCode = NativeMethods.VK_BACK, WidthRatio = 1.8, Type = KeyType.Action, SpecialAction = "backspace" });
            rows.Add(r1);

            // Row 2
            var r2 = new KeyboardRow();
            r2.Keys.Add(new() { NormalText = "TAB ⇥", VkCode = NativeMethods.VK_TAB, WidthRatio = 1.4, Type = KeyType.Action, SpecialAction = "tab" });
            r2.Keys.Add(new() { NormalText = "q", ShiftText = "Q", AltGrText = "@" });
            r2.Keys.Add(new() { NormalText = "w", ShiftText = "W" });
            r2.Keys.Add(new() { NormalText = "e", ShiftText = "E", AltGrText = "€" });
            r2.Keys.Add(new() { NormalText = "r", ShiftText = "R" });
            r2.Keys.Add(new() { NormalText = "t", ShiftText = "T" });
            r2.Keys.Add(new() { NormalText = "z", ShiftText = "Z" }); // QWERTZ
            r2.Keys.Add(new() { NormalText = "u", ShiftText = "U" });
            r2.Keys.Add(new() { NormalText = "i", ShiftText = "I" });
            r2.Keys.Add(new() { NormalText = "o", ShiftText = "O" });
            r2.Keys.Add(new() { NormalText = "p", ShiftText = "P" });
            r2.Keys.Add(new() { NormalText = "ü", ShiftText = "Ü" });
            r2.Keys.Add(new() { NormalText = "+", ShiftText = "*", AltGrText = "~" });
            rows.Add(r2);

            // Row 3
            var r3 = new KeyboardRow();
            r3.Keys.Add(new() { NormalText = "CAPS ⇪", VkCode = NativeMethods.VK_CAPITAL, WidthRatio = 1.7, Type = KeyType.Modifier, SpecialAction = "caps" });
            r3.Keys.Add(new() { NormalText = "a", ShiftText = "A" });
            r3.Keys.Add(new() { NormalText = "s", ShiftText = "S" });
            r3.Keys.Add(new() { NormalText = "d", ShiftText = "D" });
            r3.Keys.Add(new() { NormalText = "f", ShiftText = "F" });
            r3.Keys.Add(new() { NormalText = "g", ShiftText = "G" });
            r3.Keys.Add(new() { NormalText = "h", ShiftText = "H" });
            r3.Keys.Add(new() { NormalText = "j", ShiftText = "J" });
            r3.Keys.Add(new() { NormalText = "k", ShiftText = "K" });
            r3.Keys.Add(new() { NormalText = "l", ShiftText = "L" });
            r3.Keys.Add(new() { NormalText = "ö", ShiftText = "Ö" });
            r3.Keys.Add(new() { NormalText = "ä", ShiftText = "Ä" });
            r3.Keys.Add(new() { NormalText = "#", ShiftText = "'" });
            r3.Keys.Add(new() { NormalText = "ENTER ↵", VkCode = NativeMethods.VK_RETURN, WidthRatio = 1.9, Type = KeyType.Action, SpecialAction = "enter" });
            rows.Add(r3);

            // Row 4
            var r4 = new KeyboardRow();
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 1.5, Type = KeyType.Modifier, SpecialAction = "shift" });
            r4.Keys.Add(new() { NormalText = "<", ShiftText = ">", AltGrText = "|" });
            r4.Keys.Add(new() { NormalText = "y", ShiftText = "Y" });
            r4.Keys.Add(new() { NormalText = "x", ShiftText = "X" });
            r4.Keys.Add(new() { NormalText = "c", ShiftText = "C" });
            r4.Keys.Add(new() { NormalText = "v", ShiftText = "V" });
            r4.Keys.Add(new() { NormalText = "b", ShiftText = "B" });
            r4.Keys.Add(new() { NormalText = "n", ShiftText = "N" });
            r4.Keys.Add(new() { NormalText = "m", ShiftText = "M", AltGrText = "µ" });
            r4.Keys.Add(new() { NormalText = ",", ShiftText = ";" });
            r4.Keys.Add(new() { NormalText = ".", ShiftText = ":" });
            r4.Keys.Add(new() { NormalText = "-", ShiftText = "_" });
            r4.Keys.Add(new() { NormalText = "SHIFT ⇧", VkCode = NativeMethods.VK_SHIFT, WidthRatio = 2.1, Type = KeyType.Modifier, SpecialAction = "shift" });
            rows.Add(r4);

            // Row 5
            var r5 = new KeyboardRow();
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_CONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_LWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "ALT", VkCode = NativeMethods.VK_MENU, WidthRatio = 1.2, Type = KeyType.Modifier, SpecialAction = "alt" });
            r5.Keys.Add(new() { NormalText = "SPACE", VkCode = NativeMethods.VK_SPACE, WidthRatio = 5.8, Type = KeyType.Space, SpecialAction = "space" });
            r5.Keys.Add(new() { NormalText = "ALT GR", VkCode = NativeMethods.VK_RMENU, WidthRatio = 1.4, Type = KeyType.Modifier, SpecialAction = "altgr" });
            r5.Keys.Add(new() { NormalText = "WIN ⊞", VkCode = NativeMethods.VK_RWIN, WidthRatio = 1.1, Type = KeyType.Modifier, SpecialAction = "win" });
            r5.Keys.Add(new() { NormalText = "☰", VkCode = NativeMethods.VK_APPS, WidthRatio = 1.1, Type = KeyType.Action });
            r5.Keys.Add(new() { NormalText = "CTRL", VkCode = NativeMethods.VK_RCONTROL, WidthRatio = 1.3, Type = KeyType.Modifier, SpecialAction = "ctrl" });
            rows.Add(r5);

            return rows;
        }

        public static List<KeyboardRow> CreateNavigationCluster()
        {
            var rows = new List<KeyboardRow>();

            var r1 = new KeyboardRow();
            r1.Keys.Add(new() { NormalText = "INS", VkCode = NativeMethods.VK_INSERT, Type = KeyType.Navigation });
            r1.Keys.Add(new() { NormalText = "HOME", VkCode = NativeMethods.VK_HOME, Type = KeyType.Navigation });
            r1.Keys.Add(new() { NormalText = "PG UP", VkCode = NativeMethods.VK_PRIOR, Type = KeyType.Navigation });
            rows.Add(r1);

            var r2 = new KeyboardRow();
            r2.Keys.Add(new() { NormalText = "DEL", VkCode = NativeMethods.VK_DELETE, Type = KeyType.Navigation });
            r2.Keys.Add(new() { NormalText = "END", VkCode = NativeMethods.VK_END, Type = KeyType.Navigation });
            r2.Keys.Add(new() { NormalText = "PG DN", VkCode = NativeMethods.VK_NEXT, Type = KeyType.Navigation });
            rows.Add(r2);

            var r3 = new KeyboardRow();
            r3.Keys.Add(new() { NormalText = "", Type = KeyType.Standard, WidthRatio = 1.0 }); // spacer
            r3.Keys.Add(new() { NormalText = "▲", VkCode = NativeMethods.VK_UP, Type = KeyType.Navigation });
            r3.Keys.Add(new() { NormalText = "", Type = KeyType.Standard, WidthRatio = 1.0 }); // spacer
            rows.Add(r3);

            var r4 = new KeyboardRow();
            r4.Keys.Add(new() { NormalText = "◄", VkCode = NativeMethods.VK_LEFT, Type = KeyType.Navigation });
            r4.Keys.Add(new() { NormalText = "▼", VkCode = NativeMethods.VK_DOWN, Type = KeyType.Navigation });
            r4.Keys.Add(new() { NormalText = "►", VkCode = NativeMethods.VK_RIGHT, Type = KeyType.Navigation });
            rows.Add(r4);

            return rows;
        }

        public static List<KeyboardRow> CreateNumpad()
        {
            var rows = new List<KeyboardRow>();

            var r1 = new KeyboardRow();
            r1.Keys.Add(new() { NormalText = "NUM", VkCode = NativeMethods.VK_NUMLOCK, Type = KeyType.Modifier });
            r1.Keys.Add(new() { NormalText = "/", VkCode = NativeMethods.VK_DIVIDE, Type = KeyType.Standard });
            r1.Keys.Add(new() { NormalText = "*", VkCode = NativeMethods.VK_MULTIPLY, Type = KeyType.Standard });
            r1.Keys.Add(new() { NormalText = "-", VkCode = NativeMethods.VK_SUBTRACT, Type = KeyType.Standard });
            rows.Add(r1);

            var r2 = new KeyboardRow();
            r2.Keys.Add(new() { NormalText = "7", VkCode = NativeMethods.VK_NUMPAD7, Type = KeyType.Standard });
            r2.Keys.Add(new() { NormalText = "8", VkCode = NativeMethods.VK_NUMPAD8, Type = KeyType.Standard });
            r2.Keys.Add(new() { NormalText = "9", VkCode = NativeMethods.VK_NUMPAD9, Type = KeyType.Standard });
            r2.Keys.Add(new() { NormalText = "+", VkCode = NativeMethods.VK_ADD, Type = KeyType.Standard });
            rows.Add(r2);

            var r3 = new KeyboardRow();
            r3.Keys.Add(new() { NormalText = "4", VkCode = NativeMethods.VK_NUMPAD4, Type = KeyType.Standard });
            r3.Keys.Add(new() { NormalText = "5", VkCode = NativeMethods.VK_NUMPAD5, Type = KeyType.Standard });
            r3.Keys.Add(new() { NormalText = "6", VkCode = NativeMethods.VK_NUMPAD6, Type = KeyType.Standard });
            r3.Keys.Add(new() { NormalText = "=", ShiftText = "=", Type = KeyType.Standard });
            rows.Add(r3);

            var r4 = new KeyboardRow();
            r4.Keys.Add(new() { NormalText = "1", VkCode = NativeMethods.VK_NUMPAD1, Type = KeyType.Standard });
            r4.Keys.Add(new() { NormalText = "2", VkCode = NativeMethods.VK_NUMPAD2, Type = KeyType.Standard });
            r4.Keys.Add(new() { NormalText = "3", VkCode = NativeMethods.VK_NUMPAD3, Type = KeyType.Standard });
            r4.Keys.Add(new() { NormalText = "ENT", VkCode = NativeMethods.VK_RETURN, Type = KeyType.Action, SpecialAction = "enter" });
            rows.Add(r4);

            var r5 = new KeyboardRow();
            r5.Keys.Add(new() { NormalText = "0", VkCode = NativeMethods.VK_NUMPAD0, WidthRatio = 2.0, Type = KeyType.Standard });
            r5.Keys.Add(new() { NormalText = ".", VkCode = NativeMethods.VK_DECIMAL, Type = KeyType.Standard });
            r5.Keys.Add(new() { NormalText = "ENT", VkCode = NativeMethods.VK_RETURN, Type = KeyType.Action, SpecialAction = "enter" });
            rows.Add(r5);

            return rows;
        }
    }
}
