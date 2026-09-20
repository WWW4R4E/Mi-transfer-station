# 小米电脑管家 5.8.1.121「去摄像头弹窗」修复版补丁

## 这是什么

- 基于**官方原版** `PcControlCenter.dll` 重新生成的单文件补丁。
- 作用：屏蔽「请确认摄像头状态」「相机协同异常」两个摄像头协同 Toast 弹窗。
- 同时**修复了网上流传补丁的闪退 bug**（见下）。仅适用版本 **5.8.1.121**，其他版本自行测试。

## 为什么网上的补丁会让管家自动退出

- 所有弹窗（摄像头 / 耳机快连 / 手机来电 / 投屏 / 驱动等）都经过同一个方法
  `NotifyToastUIService.ShowCommonToast(IToastModel model)`。

- 网上补丁在开头插入的判断是 `model.Title.Equals("请确认摄像头状态")`。

- 但耳机（EarphoneModel / EarphonePopModel）、手机来电（TeleSynergyModel）等弹窗的 `Title` **本来就是 null**，
  对 null 调用 `.Equals(...)` 会抛 `NullReferenceException`，且该调用没有 try-catch，
  异常发生在 UI 线程 / 手机事件回调线程，导致整个 WinUI 界面崩溃、宿主有序卸载全部组件
  （日志里的 `Exit signal received`），表现为管家自己关闭并伴随"嘀嗒"提示音。

- 本补丁改成**静态、null 安全**写法（等价 C#）：

  ```csharp
  if (string.Equals(model.Title, "请确认摄像头状态") ||
      string.Equals(model.Title, "相机协同异常")) return;
  ```

  `Title` 为 null 时 `string.Equals` 返回 false、不抛异常，耳机/来电等弹窗照常显示，不再闪退。

## 安装步骤（需管理员）

1. 完全退出小米电脑管家：托盘图标右键退出
2. 打开安装目录：`C:\Program Files\MI\XiaomiPCManager\5.8.1.121\`
3. **先备份**原版 `PcControlCenter.dll`（复制到别处，或同目录改名为 `PcControlCenter.dll.官方备份`）。
4. 把本文件夹里的 `PcControlCenter.dll` 复制到安装目录覆盖。
5. 重新打开小米电脑管家。

## 验证是否生效

- 触发摄像头协同：不再弹「请确认摄像头状态 / 相机协同异常」。
- **重点**：连接/弹窗耳机快连、用另一部手机拨打绑定手机触发来电 Toast：管家**不再自动退出**，弹窗正常显示。
- 投屏、驱动、电源等其他 Toast 仍正常。

## 回滚

- 用第 3 步备份的官方 `PcControlCenter.dll` 覆盖回去即可。

## 注意事项

- 补丁 DLL **没有数字签名**（改写后 Authenticode 必然失效，与网上补丁同性质；小米加载器不校验该 DLL）。
  若 Windows Defender / 杀毒拦截或 SmartScreen 提示，需自行选择信任。
- 小米电脑管家**升级版本后**该文件会被官方 DLL 覆盖，补丁失效；需对新版本用 `make_patch.ps1` 重新生成
  （修改脚本里的安装目录版本号路径）。
- 已做的静态校验：类型总数与官方完全一致（1416，含嵌套）、8992 个方法中**仅 ShowCommonToast 这 1 个方法被修改**，
  其余方法 IL 与所有类型成员结构零差异。

## 文件

- `PcControlCenter.dll`：修复版补丁（复制到安装目录的就是它）。
- `make_patch.ps1`：可复现的生成脚本（用 Mono.Cecil 从官方 DLL 生成，透明可审计），仅作留存，安装用不到。
