# Build reference DLLs

GitHub Actions needs compile-time references here:

- `Dalamud.dll`
- `Dalamud.Bindings.ImGui.dll`
- `FFXIVClientStructs.dll`
- `Lumina.dll`
- `Lumina.Excel.dll`
- `ECommons.dll`
- `PromeRotation.dll`

They are referenced with `<Private>false</Private>` and are not packaged into `latest.zip`.
