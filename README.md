# MoreSlugHUD

雨世界 mod，添加了多项实用 HUD。

功能介绍见 [Steam 创意工坊](https://steamcommunity.com/sharedfiles/filedetails/?id=3798937497)

## 开发构建

需要在 `lib/` 中补充依赖文件: 
 - 来自 [Improved Input Config](https://github.com/zombieseatflesh7/improved-input-config) 的 `ImprovedInput.dll` 和 `ImprovedInput.xml`
 - 来自 [Rain Meadow](https://github.com/henpemaz/Rain-Meadow) 的 `Rain Meadow.dll`

在 `GamePaths.local.props` 中配置 `RWDir` 指向 Rain World 安装目录。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <RWDir>D:\SteamLibrary\steamapps\common\Rain World</RWDir>
  </PropertyGroup>
</Project>
```

然后可以使用开发脚本：

```powershell
.\build.ps1          # 编译并将模组打包到 dist/
.\dev.ps1 link       # 把 dist/ 下的 mod 文件夹链接到游戏本地 mod 目录下
# 开发期间修改代码后重新运行 build.ps1
.\dev.ps1 unlink     # 开发结束后解除链接
```

### 代码结构

| 目录 | 职责 |
|------|------|
| `src/Plugin/` | Mod 生命周期与选项 |
| `src/Hud/` | HUD 挂接及通用处理 |
| `src/Inventory/` | 库存 |
| `src/Id/` | 生物标签 |
| `src/History/` | 输入历史 |
| `src/Compat/` | 兼容性处理 |
| `atlases/` | 输入历史图集 |
