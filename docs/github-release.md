# GitHub 自动构建发布

这个仓库的 workflow 会在推送 `v*` tag 或手动运行时：

1. 编译 `Wotou.csproj`
2. 生成 `latest.zip`
3. 生成 Dalamud repo 清单 `repo.json`
4. 把两个文件上传到 GitHub Release

## 需要准备的引用 DLL

GitHub runner 不能访问你本机的 Dalamud 和 PromeRotation 目录，所以需要在仓库根目录创建 `lib/`，放入这些编译引用：

- `Dalamud.dll`
- `Dalamud.Bindings.ImGui.dll`
- `FFXIVClientStructs.dll`
- `Lumina.dll`
- `Lumina.Excel.dll`
- `ECommons.dll`
- `PromeRotation.dll`

这些 DLL 只用于编译，项目里 `<Private>false</Private>`，不会被打进发布 zip。

注意：这里生成的 `repo.json` 是 Dalamud 插件仓库格式。当前仓库产物是 `Wotou` ACR，如果要发布 PromeRotation 主插件，请把同样的 workflow 放到 PromeRotation 主插件仓库，并让 zip 内容包含主插件 DLL。

## 触发发布

推送 tag：

```powershell
git tag v1.5.2.2
git push origin v1.5.2.2
```

发布完成后可用：

- `https://github.com/<owner>/<repo>/releases/latest/download/latest.zip`
- `https://github.com/<owner>/<repo>/releases/latest/download/repo.json`

如果仍要使用 123 网盘 CDN 链接，需要在 workflow 里追加上传到 123 的步骤，并把 `DownloadLinkInstall` / `DownloadLinkUpdate` 改成对应 CDN 地址。
