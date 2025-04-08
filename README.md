# Mi-transfer-station

作为小米电脑管家的互传中转站，本项目致力于优化 Windows 11 系统中通过右键菜单使用小米互传时等待菜单加载的不佳体验，以及通过打开小米电脑管家进行互传的繁琐操作。感谢 [MiDropShellExtForWindows11](https://github.com/cnbluefire/MiDropShellExtForWindows11) 项目提供的小米互传调用方法。

- **小米互传 Win11 右键菜单支持**：将小米互传功能集成到 Windows 11 系统右键菜单中，显著提升文件传输的便捷性。

## 重要说明

### 图标路径配置

1. **依赖环境**  
   推荐安装 **小米电脑管家 5.3.0.328 版本**，或手动将路径指向：
   ```plaintext
   "C:\Program Files\MI\XiaomiPCManager\5.3.0.328\"
   ```

2. **图标文件要求**  
   需确保以下资源文件存在：
   - `midrop_logo.ico`
   - `NotifyIcon.ico`  
   路径示例：
   ```plaintext
   C:\Program Files\MI\XiaomiPCManager\5.3.0.328\Assets\ 
   ```

> ⚠️ **注意**：若使用非推荐版本，请自行调整路径，避免因目录结构差异导致图标加载失败。

## 安装方法

从 [release](#) 中下载证书和 msix 包，信任证书后安装 msix 包。

安装完成后，重启电脑即可实现自动开机自启。如果未自动启动，可以：
- 打开任务管理器的启动应用，检查是否有 `mibar` 的启动被禁用；
- 如果不想重启电脑，可以使用 [Everything](https://www.voidtools.com/) 等工具搜索 `mibar.exe` 并手动启动。

## 软件兼容性

在使用过程中，可能会遇到其他软件的兼容性问题。例如，`mydocerfinder` 软件的 finder 栏可能会覆盖本软件窗口。解决方法包括：
- 点击其他应用窗口，触发 finder 的挤压窗口布局操作；
- 点击几次 finder 栏；
- 在系统托盘中切换窗口启用状态。

## 关于后续

目前，本软件已达到我最初的设计目标，因此后续可能不会再新增功能。如果遇到软件本身的问题，或者与其他软件的兼容性问题，请随时告知我，我会在有空时进行修复。

---

希望以上内容对你有帮助。
