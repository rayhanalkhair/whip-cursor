# Whip Cursor

A whip hangs from your mouse pointer in every Windows app. Flick the mouse to crack it.

![Whip Cursor demo: the whip swings from the pointer and cracks with a spark on a fast flick](docs/demo.gif)

## Run

Download or build `WhipCursor.exe` and double-click it. A whip icon appears in the tray (check the `^` overflow).

- **Ctrl+Alt+W** turns the whip on and off.
- Right-click the tray icon for volume, whip length, crack sensitivity, sparks, Start with Windows and Quit.
- Cracks are muted automatically while a full-screen app (video, presentation, game) is in front.

## Privacy

Whip Cursor makes no network connections and collects nothing. It only reads the mouse pointer position (it never hooks or records keyboard or mouse input) and stores its settings and log in `%AppData%\WhipCursor\`.

## Windows warnings

`WhipCursor.exe` is not code-signed, so Windows SmartScreen may show "Windows protected your PC" the first time you run it: click **More info** → **Run anyway**. Some antivirus tools may also be cautious about a small app that registers a global hotkey and draws a topmost overlay window. The full source is in this repository if you want to build it yourself (see Build below).

## Your own crack sounds

Put one or more `.wav` files in `%AppData%\WhipCursor\sounds\` and restart the app. If none of them can be read, the built-in sounds are used. Errors are written to `%AppData%\WhipCursor\log.txt`.

## Optional: whip-handle pointer

Right-click `cursor/install.inf` → **Install** (Windows asks for admin permission because it copies into `C:\Windows\Cursors`). Then open **Mouse settings → Additional mouse settings → Pointers**, pick the **Whip Cursor** scheme and click OK. To undo, pick **Windows Default** there.

## Known limits

The whip can't be drawn over exclusive-fullscreen games, admin (elevated) windows, the lock screen or UAC prompts. Windows does not allow it.

## Build

Needs the .NET 8 SDK.

    dotnet test
    dotnet run --project src/WhipCursor
    dotnet publish src/WhipCursor -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish

Debug tip-speed logging: set `WHIPCURSOR_DEBUG=1` before starting.
