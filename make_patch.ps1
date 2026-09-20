# make_patch.ps1
# 基于官方 5.8.1.121 PcControlCenter.dll, 仅在 NotifyToastUIService.ShowCommonToast(IToastModel)
# 开头插入 null 安全的摄像头弹窗拦截, 输出到工作目录 patched\ (不修改安装目录任何文件)。
#
# 等价 C# 逻辑(注意: 用静态 string.Equals, Title 为 null 时返回 false 而不抛 NullReferenceException):
#   public void ShowCommonToast(IToastModel model) {
#       if (string.Equals(model.Title, "请确认摄像头状态") || string.Equals(model.Title, "相机协同异常")) return;
#       ... // 官方原方法体保持不变
#   }
$ErrorActionPreference="Stop"
$cecil="C:\Users\Aoongu\Doubao\chats\2026-09-18\new-chat\tools\cecil\lib\net40\Mono.Cecil.dll"
[Reflection.Assembly]::LoadFrom($cecil) | Out-Null

$src="C:\Program Files\MI\XiaomiPCManager\5.8.1.121\PcControlCenter.dll"
$outDir="C:\Users\Aoongu\Doubao\chats\2026-09-18\new-chat\patched"
$out=Join-Path $outDir "PcControlCenter.dll"
New-Item -ItemType Directory -Force $outDir | Out-Null

$cam1="请确认摄像头状态"
$cam2="相机协同异常"

$resolver=New-Object Mono.Cecil.DefaultAssemblyResolver
$resolver.AddSearchDirectory((Split-Path $src -Parent))
$rp=New-Object Mono.Cecil.ReaderParameters
$rp.InMemory=$true
$rp.AssemblyResolver=$resolver
$asm=[Mono.Cecil.AssemblyDefinition]::ReadAssembly($src,$rp)
try{
  $mod=$asm.MainModule

  # 1) 定位目标方法
  $svc=$mod.GetTypes() | Where-Object { $_.FullName -eq "PcControlCenter.Services.UI.MainView.Instances.NotifyToastUIService" } | Select-Object -First 1
  if(-not $svc){ throw "未找到 NotifyToastUIService" }
  $target=$svc.Methods | Where-Object { $_.Name -eq "ShowCommonToast" -and $_.Parameters.Count -eq 1 -and $_.Parameters[0].ParameterType.Name -eq "IToastModel" } | Select-Object -First 1
  if(-not $target){ throw "未找到 ShowCommonToast(IToastModel)" }
  Write-Output ("目标方法: {0}.ShowCommonToast  原IL={1} MaxStack={2}" -f $svc.FullName,$target.Body.CodeSize,$target.Body.MaxStackSize)

  # 2) IToastModel.get_Title 引用
  $itmDef=$target.Parameters[0].ParameterType.Resolve()
  $titleGetDef=($itmDef.Properties | Where-Object { $_.Name -eq "Title" } | Select-Object -First 1).GetMethod
  if(-not $titleGetDef){ throw "IToastModel 无 Title getter" }
  $titleGet=$mod.ImportReference($titleGetDef)

  # 3) 静态 System.String.Equals(object,object): bool 引用(null 安全), 手工绑定到目标模块核心库
  $strT=$mod.TypeSystem.String; $boolT=$mod.TypeSystem.Boolean; $objT=$mod.TypeSystem.Object
  $eq=New-Object Mono.Cecil.MethodReference("Equals",$boolT,$strT)
  $eq.HasThis=$false
  [void]$eq.Parameters.Add((New-Object Mono.Cecil.ParameterDefinition("",[Mono.Cecil.ParameterAttributes]::None,$objT)))
  [void]$eq.Parameters.Add((New-Object Mono.Cecil.ParameterDefinition("",[Mono.Cecil.ParameterAttributes]::None,$objT)))

  # 4) 幂等保护: 若已含拦截则中止
  $already=$false
  foreach($ins in $target.Body.Instructions){ if(($ins.OpCode -eq [Mono.Cecil.Cil.OpCodes]::Ldstr) -and ($ins.Operand -in @($cam1,$cam2))){ $already=$true } }
  if($already){ throw "目标方法已包含摄像头拦截字符串, 中止(避免重复打补丁)" }

  $ilp=$target.Body.GetILProcessor()
  $first=$target.Body.Instructions[0]

  # 先建第二段指令(其开头要作为第一段跳转目标, 须先存在)
  $b1=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Ldarg_1)
  $b2=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Callvirt,$titleGet)
  $b3=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Ldstr,$cam2)
  $b4=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Call,$eq)
  $b5=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Brfalse,$first)   # 都不匹配 -> 进入原方法体
  $b6=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Ret)
  # 第一段: if (string.Equals(model.Title, cam1)) return;
  $a1=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Ldarg_1)
  $a2=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Callvirt,$titleGet)
  $a3=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Ldstr,$cam1)
  $a4=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Call,$eq)
  $a5=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Brfalse,$b1)      # 第一段不匹配 -> 进入第二段
  $a6=$ilp.Create([Mono.Cecil.Cil.OpCodes]::Ret)

  foreach($ins in @($a1,$a2,$a3,$a4,$a5,$a6,$b1,$b2,$b3,$b4,$b5,$b6)){
    $ilp.InsertBefore($first,$ins)
  }
  if($target.Body.MaxStackSize -lt 2){ $target.Body.MaxStackSize=2 }

  Write-Output ("补丁后方法 IL={0} MaxStack={1}" -f $target.Body.CodeSize,$target.Body.MaxStackSize)

  # 5) 写出(原程序集无强名称, 不签名)
  if(Test-Path $out){ Remove-Item $out -Force }
  $wp=New-Object Mono.Cecil.WriterParameters
  $asm.Write($out,$wp)
  Write-Output "已写出: $out"
}
finally{ $asm.Dispose() }

# 汇总
$h=(Get-FileHash $out -Algorithm SHA256).Hash
$fi=Get-Item $out
Write-Output ("补丁文件: {0}  {1} 字节  {2:yyyy-MM-dd}" -f $fi.Name,$fi.Length,$fi.LastWriteTime)
Write-Output ("SHA256: {0}" -f $h)
