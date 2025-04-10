using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace CecilDll;

internal partial class Program
{
    private static void Main()
    {
        try
        {
            // 获取应用路径
            string[] lines =
            {
                @"C:\Program Files\MI\XiaomiPCManager\*\PcControlCenter.dll",
                "PcControlCenter.Services.UI.MainView.Instances.NotifyToastUIService",
                "ShowCommonToast",
            };

            // 关闭小米电脑管家进程
            Console.WriteLine("正在关闭小米电脑管家进程...");
            KillProcessByName("XiaomiPCManager");

            var rawPath = lines[0].Trim().Trim('\"');
            var dllPath = ResolveLatestVersionPath(rawPath);
            Console.WriteLine($"找到最新版本路径：{dllPath},请确认该路径是否正确(Y/N)");
            var flag = Console.ReadLine();
            if (flag != "y" && flag != "Y")
            {
                Console.WriteLine("请输入dll路径(例如:C:\\Program Files\\MI\\XiaomiPCManager\\*\\PcControlCenter.dll)");
                dllPath = Console.ReadLine();
            }

            var module = ModuleDefMD.Load(dllPath);
            var className = lines[1].Trim();
            var methodName = lines[2].Trim();

            // 查找目标类型和方法
            var targetType = FindType(module, className);
            var targetMethod = FindMethod(targetType, methodName);

            // 修改方法体
            ModifyMethod(targetMethod, module);

            Console.WriteLine("请选择是直接替换还是生成到该目录");
            Console.WriteLine("1.直接替换");
            Console.WriteLine("2.生成到该目录");
            var input = Console.ReadLine();
            string outputDllPath = $"{Path.GetFileNameWithoutExtension(dllPath)}.dll";
            switch (input)
            {
                case "1":
                    outputDllPath = dllPath;
                    break;
                case "2":
                    outputDllPath = $"{Path.GetFileNameWithoutExtension(dllPath)}.dll";
                    break;
                default:
                    Console.WriteLine("输入错误,现在已经默认保存到该目录");
                    break;
            }

            // 保存修改后的DLL
            module.Write(outputDllPath);

            Console.WriteLine($"修改成功！新DLL已保存至：{outputDllPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"操作失败：{ex.Message}");
        }

        // 按任意键退出
        Console.WriteLine("按任意键退出...");
        Console.ReadLine();
    }

    // 查找目标类型
    private static TypeDef FindType(ModuleDef module, string className)
    {
        var targetType = module.GetTypes().FirstOrDefault(t => t.FullName == className);
        if (targetType == null)
            throw new ArgumentException($"未找到类：{className}");
        return targetType;
    }

    // 查找目标方法
    private static MethodDef FindMethod(TypeDef type, string methodName)
    {
        var targetMethod = type.Methods.FirstOrDefault(m => m.Name == methodName);
        if (targetMethod == null)
            throw new ArgumentException($"未找到方法：{methodName}");

        if (targetMethod.IsAbstract)
            throw new NotSupportedException("无法修改抽象方法");
        return targetMethod;
    }

    // 修改方法体
    private static void ModifyMethod(MethodDef method, ModuleDef module)
    {
        var methodBody = method.Body;

        // 获取参数
        var param = method.Parameters[1];
        var paramType = param.Type;
        var modelType = paramType.ScopeType;

        var resolvedModelType = modelType.ResolveTypeDef();
        if (resolvedModelType == null)
        {
            throw new InvalidOperationException($"[ERROR] 无法解析参数类型：{paramType.FullName}");
        }

        Console.WriteLine($"成功解析类型：{resolvedModelType.FullName}");

        // 查找Title属性或字段
        var titleProperty = resolvedModelType.Properties.FirstOrDefault(p => p.Name == "Title");
        MethodDef getter = null;
        FieldDef field = null;

        if (titleProperty != null)
        {
            getter = titleProperty.GetMethod;
            Console.WriteLine($"找到Title属性，访问器方法：{getter.FullName}");
        }
        else
        {
            field = resolvedModelType.Fields.FirstOrDefault(f => f.Name == "Title") ??
                    throw new InvalidOperationException();
            if (field == null)
            {
                var props = string.Join(", ", resolvedModelType.Properties.Select(p => p.Name));
                var flds = string.Join(", ", resolvedModelType.Fields.Select(f => f.Name));
                throw new InvalidOperationException(
                    $"[ERROR] 类型 {resolvedModelType.FullName} 中未找到Title属性或字段。" +
                    $"可用属性：{props}，字段：{flds}");
            }

            Console.WriteLine($"找到Title字段：{field.FullName}");
        }

        var instructions = new List<Instruction>();
        var endLabel = Instruction.Create(OpCodes.Nop);

        // 生成字符串比较逻辑
        var stringsToCheck = new[] { "请确认摄像头状态", "相机协同异常" };
        foreach (var str in stringsToCheck)
        {
            // 使用 Ldarg 并指定参数索引
            instructions.Add(Instruction.Create(OpCodes.Ldarg, param));
            if (getter != null)
                instructions.Add(Instruction.Create(OpCodes.Callvirt, module.Import(getter)));
            else
                instructions.Add(Instruction.Create(OpCodes.Ldfld, module.Import(field)));

            instructions.Add(Instruction.Create(OpCodes.Ldstr, str));
            var equalsMethod = module.Import(typeof(string).GetMethod("Equals", new[] { typeof(string) }));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, equalsMethod));
            instructions.Add(Instruction.Create(OpCodes.Brtrue, endLabel));
        }

        // 获取原始指令
        var originalInstructions = new List<Instruction>(methodBody.Instructions);

        // 将原始指令添加到条件满足后的执行路径
        instructions.AddRange(originalInstructions);
        instructions.Add(endLabel);
        instructions.Add(Instruction.Create(OpCodes.Ret));

        // 替换方法体并修复宏指令
        Console.WriteLine($"开始替换方法体指令");
        methodBody.Instructions.Clear();
        foreach (var instr in instructions) methodBody.Instructions.Add(instr);
        methodBody.SimplifyMacros(method.Parameters);

        // 设置保留旧最大堆栈值以避免计算错误
        methodBody.KeepOldMaxStack = true;

        Console.WriteLine($"方法体替换完成，宏指令修复成功");
    }
    // 新增路径解析方法
    private static string ResolveLatestVersionPath(string rawPath)
    {
        var adjustedPath = rawPath.Replace("\\*", "\\");
        var baseDir = Path.GetDirectoryName(adjustedPath);
        var fileName = Path.GetFileName(adjustedPath);

        var versionDirs = Directory.EnumerateDirectories(baseDir)
            .Where(d => Regex.IsMatch(Path.GetFileName(d), @"^\d+\.\d+\.\d+\.\d+$"))
            .OrderByDescending(d => Version.Parse(Path.GetFileName(d)))
            .ToList();

        if (versionDirs.Count == 0)
            throw new DirectoryNotFoundException("未找到任何版本目录");

        var latestVersionDir = versionDirs.First();
        return Path.Combine(latestVersionDir, fileName);
    }

    // 关闭应用进程

    public static void KillProcessByName(string processName)
    {
        // 获取所有匹配进程
        var processes = Process.GetProcessesByName(processName);

        foreach (var process in processes)
        {
            try
            {
                process.CloseMainWindow();

                // 若未退出则使用强制终止
                if (!process.WaitForExit(2000))
                {
                    process.Kill();
                    process.WaitForExit();
                }

                Console.WriteLine($"成功终止进程：{process.ProcessName} (PID: {process.Id})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"终止进程失败：{ex.Message},请手动关闭");
            }
        }
    }
}