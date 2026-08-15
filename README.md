# Wotou

Wotou 是一个基于 PromeRotation 框架的 FFXIV 吟游诗人（BRD）和舞者（DNC）自动循环（ACR）。

## 快速开始

### 本地开发

```powershell
git clone <本仓库>
dotnet build Wotou.csproj
```

编译前确保 `Wotou.csproj` 中的 `DalamudReferenceRoot` 和 `PromeRotationLocalRoot`（或 `PromeRotationInstalledRoot`）指向你本机正确的路径。

### 自动发布

本仓库使用 GitHub Actions 自动构建和发布，详情见 [docs/github-release.md](docs/github-release.md)。

## 项目结构

| 目录/文件 | 说明 |
|---|---|
| `Bard/` | 吟游诗人 ACR 逻辑（技能、Buff、开场、战斗数据） |
| `Dancer/` | 舞者 ACR 逻辑（舞步、资源、开场、时间轴节点） |
| `lib/` | CI 编译引用 DLL（Dalamud + PromeRotation） |
| `docs/` | 开发文档 |
| `.github/workflows/release.yml` | GitHub Actions 发布工作流 |
| `Wotou.csproj` | 项目文件，含 MSBuild 引用解析和 CI 同步逻辑 |

## 许可证

见仓库根目录。
