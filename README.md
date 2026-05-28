# Open in Windsurf

A Visual Studio 2022 extension that adds a menu command to open any solution, project, folder, or file in [Windsurf](https://windsurf.com/).

## Prerequisites

- Visual Studio 2022
- [Windsurf](https://windsurf.com/) installed

## Features

### Solution Explorer

Right-click any solution, project, folder, or file in Solution Explorer and select **Open in Windsurf**.

### Open current file

Open the active document in Windsurf via **Extensions → Open in Windsurf**, or use the default keybinding:

```
Ctrl+Shift+W
```

The current cursor line and column are passed to Windsurf so it opens at the exact position.

## Path to windsurf.exe

The extension auto-detects `windsurf.exe` from:

1. Registry (`HKCU\SOFTWARE\Classes\*\shell\Windsurf\`)
2. `PATH` environment variable
3. `%LOCALAPPDATA%\Programs\Windsurf\windsurf.exe`

If none of those locations contain a valid executable, you will be prompted to locate it manually.

You can always change the path (and pass extra command-line arguments) in **Tools → Options → Web → Open in Windsurf**.

## License

[MIT](src/Resources/LICENSE)
