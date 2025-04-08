作为小米电脑管家的互传的中转站使用，主要是优化win11通过右键菜单互传要等待菜单加载的糟糕体验，或者是使用打开小米电脑管家进行互传的繁琐操作，感谢https:
//github.com/cnbluefire/MiDropShellExtForWindows11 这个项目提供的调用小米互传的方法

- **小米互传Win11右键菜单支持**：集成小米互传功能到Windows 11系统右键菜单，提升文件传输便捷性。

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
> ⚠️ 注意：若使用非推荐版本需自行调整路径，避免因目录结构差异导致图标加载失败。

## 安装方法
   ```plaintext
   release中下载证书和msix包,信任证书然后安装misx
   ```
安装完成后重启电脑会自动开机自启,如果没有,可以打开任务管理器的启动应用看看是不是有mibar的启动被禁止了(如果不想重启电脑可以用everything等工具搜索mibar.exe来点击启动)
