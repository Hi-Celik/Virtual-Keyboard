# CyberKeyboard – Touchscreen Virtual Keyboard

A modern, futuristic, hardware-accelerated virtual keyboard for touchscreens, with support for multiple languages.

Turn your external touchscreen monitor into a standalone, modern, and responsive touch keyboard.

---

## 🚀 Key Features

### 1. Non-Activating Window Architecture (`WS_EX_NOACTIVATE`)
- Built using Windows' native `WS_EX_NOACTIVATE` and `WM_MOUSEACTIVATE -> MA_NOACTIVATE` APIs.
- **Never steals focus from the target window** (Notepad, Word, VS Code, browsers, etc.) when you tap keys, shortcuts, or HUD buttons.
- Types directly into the currently active input field, just like a physical keyboard, wherever the cursor is positioned.

### 2. Automatic and Manual Keyboard Layout Switching
- Automatically responds when you change your keyboard input language in Windows (for example, using `Alt + Shift` or `Win + Space`):
  - **Turkish (TR-Q):** `Ğ`, `Ü`, `Ş`, `İ`, `Ö`, `Ç`, and AltGr symbols (`@`, `€`, `₺`, `{`, `}`, `[`, `]`).
  - **French (FR-AZ):** Classic French `AZERTY` layout, including `é`, `è`, `ç`, `à`, `ù`, `^`, and `$`.
  - **French (FR-Q):** French `QWERTY` layout (Français QWERTY / Canadian & International), with direct access to French accented characters (`é`, `è`, `à`, `ç`, `ù`, `^`, `¨`, `«`, `»`).
  - **English (EN-US):** Standard US `QWERTY` layout.
  - **German (DE-QZ):** `QWERTZ` layout, including `ä`, `ö`, `ü`, and `ß`.
- Switch layouts with a single tap using the neon `[ TR-Q ]`, `[ EN-US ]`, `[ FR-AZ ]`, `[ FR-Q ]`, and `[ DE-QZ ]` buttons in the keyboard's top HUD panel. The active application's input language is updated automatically as well.
- An active-target indicator shows which application is currently targeted (e.g. `TARGET: NOTEPAD.EXE`).

### 3. Futuristic Cyber Interface
- **Hardware-accelerated background at 60 FPS:**
  - Smooth, animated perspective cyber-grid lines and scanning waves.
  - An interactive neural particle network (constellation network) connected by laser-like lines.
- **Dynamic shockwaves and spark effects:**
  - Tapping any key triggers expanding neon shockwave rings and radial bursts of sparks from the touch point.
- **Key press feedback:**
  - Keys animate with a pressed-in effect and a glowing neon highlight.
  - All key labels update dynamically when Shift or AltGr is pressed.
- **Synthesized touch sounds:**
  - Low-latency, procedurally generated futuristic click sounds that can be toggled with a single tap in the HUD.
- **Five color themes:**
  - 🌐 **Cyber Cyan:** Cyber blue / neon cyan
  - 🌆 **Synthwave Pink:** Neon purple / bright pink
  - 🟩 **Matrix Green:** Vivid emerald cyber green
  - ☀️ **Solar Gold:** Amber / golden yellow
  - 🕹️ **Retro:** Warm 1980s retro orange, arcade amber, and nostalgic golden-yellow CRT glow

### 4. External Touchscreen and Full-Screen Support
- **Multi-monitor detection:** Automatically lists all displays connected to the system (Monitor 1, Monitor 2, Monitor 3, etc.).
- **Move to display with one tap:** Tap the `[ 🖥️ SCREEN ]` button in the top bar to instantly move the keyboard to your external touchscreen monitor.
- **Full-screen mode (`⛶ FULLSCREEN`):** Fits your external monitor's resolution and switches to borderless full-screen mode.
- **Freely movable window:** When you exit full-screen mode, drag the top bar to move the window wherever you want.
- **Numpad and navigation block:** Toggle the numeric keypad and arrow keys with the `[ 🔢 NUMPAD ]` button to adjust the keyboard's width to fit your screen.

### 5. Quick-Action Toolbar
One-tap shortcuts that make touchscreen use easier:
- `[ ✂ CUT ]`
- `[ 📋 COPY ]`
- `[ 📄 PASTE ]`
- `[ 🔍 SEL ALL ]` (Select All)
- `[ ↺ UNDO ]`

---

## 💻 How to Run

Simply double-click one of the launchers in the project root directory:
- **`Start-CyberKeyboard.bat`**, or
- **`publish\CyberKeyboard.exe`**

To build and run from source:

```bash
cd CyberKeyboard
dotnet run
```
