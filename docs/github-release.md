# Wotou — 自动发布流程

> 本文档面向希望克隆此仓库或为自己的职业创建类似 ACR 的开发者。

## 项目简介

Wotou 是一个基于 PromeRotation 框架的 FFXIV 自动循环（ACR），当前实现的是 **吟游诗人（BRD）** 职业。
整个项目通过 GitHub Actions 自动化编译、打包和发布。

---

## 目录

- [前置条件](#前置条件)
- [本地开发环境](#本地开发环境)
- [本地编译](#本地编译)
- [lib/ 引用 DLL 说明](#lib-引用-dll-说明)
- [发布到 GitHub Actions](#发布到-github-actions)
- [触发发布](#触发发布)
- [更新到新版本 PromeRotation](#更新到新版本-promerotation)
- [Fork 到其他职业](#fork-到其他职业)

---

## 前置条件

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) 或更高版本
- 已安装 XIVLauncher CN 和 Dalamud
- 已安装 PromeRotation 插件

---

## 本地开发环境

### 1. 克隆仓库

```powershell
git clone https://github.com/<你的用户名>/Wotou.git
cd Wotou
```

### 2. 配置 PromeRotation 引用路径

`Wotou.csproj` 中有三个 MSBuild 属性控制 PromeRotation 引用的查找路径：

```xml
<!-- PromeRotation 安装目录（已安装的版本） -->
<PromeRotationInstalledRoot>
  C:\Users\<用户名>\AppData\Roaming\XIVLauncherCN\installedPlugins\PromeRotation
</PromeRotationInstalledRoot>

<!-- PromeRotation 本地编译目录 -->
<PromeRotationLocalRoot>
  I:\repos\PromeRotation-1.0\PromeRotation\bin\x64\Debug
</PromeRotationLocalRoot>

<!-- 切换开关：true = 使用本地目录，false = 使用已安装版本 -->
<PromeRotationUseLocal>true</PromeRotationUseLocal>
```

**推荐**：将 `PromeRotationUseLocal` 设为 `true`，并把 `PromeRotationLocalRoot` 指向你自己本地编译的 PromeRotation 输出目录。

### 3. 配置 Dalamud 引用路径

```xml
<DalamudReferenceRoot>
  C:\Users\<用户名>\AppData\Roaming\XIVLauncherCN\addon\Hooks\dev
</DalamudReferenceRoot>
```

如果你使用国际服或不同版本的 XIVLauncher，修改上述路径即可。

---

## 本地编译

### 命令行编译

```powershell
dotnet build Wotou.csproj --configuration Release -p:AppendTargetFrameworkToOutputPath=false
```

### 编译结果输出

默认输出到：

```
C:\Users\<用户名>\AppData\Roaming\XIVLauncherCN\pluginConfigs\PromeRotation\ACR\Wotou\
```

也可以用 `-p:OutputPath=<自定义路径>` 覆盖输出目录。

### 自动同步引用 DLL

项目内置了一个名为 `SyncReferenceDllsToLib` 的 MSBuild Target：

- **仅在本地编译时**执行（检测到 `GITHUB_ACTIONS` 环境变量不存在时触发）
- 自动将 Dalamud 和 PromeRotation 的引用 DLL 复制到 `lib/` 目录
- 复制后的 DLL 可以提交到 Git，供 CI 使用

> 如果本地缺少某些 DLL，编译时会先报错。先运行一次 `dotnet build` 自动补齐 `lib/`，然后再重新编译即可。

---

## lib/ 引用 DLL 说明

`lib/` 目录存放 CI 编译时需要的所有引用 DLL。因为 GitHub Actions Runner 无法访问你的本地 Dalamud 和 PromeRotation 目录，所以需要把这些 DLL 提交到仓库。

### 需要的 DLL 清单

| DLL | 来源 |
|---|---|
| `Dalamud.dll` | XIVLauncher 开发目录 |
| `Dalamud.Bindings.ImGui.dll` | XIVLauncher 开发目录 |
| `FFXIVClientStructs.dll` | XIVLauncher 开发目录 |
| `Lumina.dll` | XIVLauncher 开发目录 |
| `Lumina.Excel.dll` | XIVLauncher 开发目录 |
| `ECommons.dll` | PromeRotation 目录 |
| `PromeRotation.dll` | PromeRotation 目录 |

### 要点

- 这些 DLL **仅用于编译期引用**，项目中已设置 `<Private>false</Private>`，不会被打入 `latest.zip` 发布包
- 首次设置时，执行一次 `dotnet build`，`SyncReferenceDllsToLib` Target 会自动复制这些 DLL
- 把复制后的结果提交到 Git：

```powershell
git add lib/
git commit -m "Add reference DLLs for CI build"
git push
```

---

## 发布到 GitHub Actions

CI/CD 工作流文件位于：

```
.github/workflows/release.yml
```

### 完整工作流程

触发发布后，CI 依次执行以下步骤：

| # | 步骤 | 说明 |
|---|---|---|
| 1 | 解析版本号 | 从 tag 或手动输入的版本字符串解析为 `System.Version` 格式 |
| 2 | 检查引用 DLL | 确保 `lib/` 中必需的 7 个 DLL 都存在 |
| 3 | 读取 PromeRotation 版本 | 从 `lib/PromeRotation.dll` 提取版本号，填入 `repo.json` |
| 4 | 同步 RotationMetadata 版本 | 将 `Bard/BardRotation.cs` 中的 `RotationMetadata` 版本更新为本次发布版本 |
| 5 | **编译** | `dotnet build --configuration Release` |
| 6 | **打包** | 将 `Wotou.dll` 和 `Wotou.deps.json` 压缩为 `latest.zip` |
| 7 | **生成清单** | 创建 PromeRotation 远程 ACR 格式的 `repo.json`（含 SHA256） |
| 8 | 上传 Artifact | 保存产物到 GitHub Actions |
| 9 | **创建 GitHub Release** | 发布 Release，附带 `latest.zip` 和 `repo.json` |
| 10 | **更新 release 分支** | 将文件推送到 `release` 分支（提供直链下载） |

### repo.json 格式说明

生成的 `repo.json` 是 **PromeRotation 远程 ACR 格式**，不是 Dalamud 插件仓库格式。内容示例：

```json
{
  "author": "Wotou",
  "version": "1.5.2.2",
  "description": "一个简单的ACR",
  "supportedJobs": [
    {
      "job": "BRD",
      "contentScope": "Unspecified"
    }
  ],
  "apiVersion": 15,
  "referencePromeVersion": "1.0.0.0",
  "downloadUrl": "https://raw.githubusercontent.com/<owner>/<repo>/release/latest.zip",
  "sha256": "abcdef..."
}
```

### 下载链接

每次成功发布后，可从以下链接获取文件：

| 文件 | 链接 |
|---|---|
| `latest.zip` | `https://github.com/<owner>/<repo>/releases/latest/download/latest.zip` |
| `repo.json` | `https://github.com/<owner>/<repo>/releases/latest/download/repo.json` |

---

## 触发发布

### 方式一：推送 Git Tag（推荐）

```powershell
git tag v1.5.2.2
git push origin v1.5.2.2
```

- Tag 名称必须为 `v` 开头 + 有效的四段版本号（例如 `v1.5.2.2`）
- 工作流会自动去掉 `v` 前缀，将版本号传入编译和 `repo.json`


### 版本号格式

版本号必须是符合 .NET `System.Version` 的四段数字格式：

```
<主版本>.<次版本>.<构建号>.<修订号>
```

例如：`1.0.0.0`、`2.3.1.5`、`10.0.0.1`。

---

## 更新到新版本 PromeRotation

当你本地更新了 PromeRotation 插件后，需要同步更新 `lib/` 中的引用 DLL：

```powershell
# 1. 确保 PromeRotation 已更新到最新版本
# 2. 本地编译（会自动把新版 DLL 复制到 lib/）
dotnet build Wotou.csproj

# 3. 查看更新的文件
git diff --stat lib/

# 4. 提交变更
git add lib/
git commit -m "Update PromeRotation reference to x.x.x.x"
git push
```

CI 会自动从 `lib/PromeRotation.dll` 提取版本号填入 `repo.json` 的 `referencePromeVersion` 字段。

---

## Fork 到其他职业

如果你想为其他职业创建类似的 ACR，需要修改以下内容：

### 1. 创建职业目录

将 `Bard/` 目录复制重命名为目标职业的目录名，例如 `Mch/`、`Dnc/`、`Wm/`。

### 2. 修改核心类

| 需要修改的内容 | 位置 | 说明 |
|---|---|---|
| 命名空间 | Rotation 类文件 | 将 `Wotou.Bard` 改为目标职业命名空间 |
| 职业 ID | `RotationMetadata` 属性 | 将 `(uint)Job.BRD` 改为目标职业，例如 `(uint)Job.MCH` |
| 职业技能 | `BRDData/BRDSkill.cs` | 替换为目标职业的技能 ID |
| 职业 Buff | `BRDData/BRDBuff.cs` | 替换为目标职业的 Buff ID |
| 全局引用 | `GlobalUsings.cs` | 添加新命名空间的引用 |

### 3. 修改 CI 配置

在 `.github/workflows/release.yml` 中：

| 环境变量或配置 | 说明 |
|---|---|
| `ACR_JOB` | 改为对应职业缩写，例如 `MCH` |
| `ACR_DESCRIPTION` | 改为你的 ACR 描述 |
| `ACR_AUTHOR` | 改为你的名字 |
| `Sync RotationMetadata version` 步骤 | 将正则匹配的文件路径从 `Bard/BardRotation.cs` 改为新路径 |

### 4. 修改 RotationMetadata

```csharp
// 将：
[RotationMetadata((uint)Job.BRD, "诗人", "Wotou", "1.0.0.0")]

// 改为：
[RotationMetadata((uint)Job.MCH, "机工士", "Wotou", "1.0.0.0")]
```

---
