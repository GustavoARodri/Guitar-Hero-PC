# SPDX-License-Identifier: LicenseRef-GDE-PolyForm-NC-1.0.0-plus-terms
# Guitar Dream Engine, https://github.com/mbracali/prj-gd-engine. See LICENSE and LICENSE-ADDITIONAL-TERMS.md.
# Modified by GustavoARodri on 2026-10-27: Added the Windows launcher build and icon generation.
param([string]$OutputName = 'Guitar Hero.exe')
$ErrorActionPreference = 'Stop'
$launcherOutput = Split-Path -Parent $PSScriptRoot
$launcherCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $launcherCompiler)) { throw 'Compilador .NET Framework não encontrado.' }
Add-Type -AssemblyName System.Drawing
function New-IconPng([int]$size) {
    $bitmap = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    if ($size -eq 16) {
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
        $tile=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,18,20,26));$g.FillRectangle($tile,1,0,14,16)
        $outline=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,218,204,174))
        $bodyFill=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,43,47,53))
        $ivory=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,235,226,203))
        $wood=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,70,57,44))
        $g.FillRectangle($outline,7,1,3,2);$g.FillRectangle($bodyFill,8,1,1,1);$g.FillRectangle($outline,6,2,1,1);$g.FillRectangle($outline,10,2,1,1)
        $g.FillRectangle($outline,6,3,4,8);$g.FillRectangle($wood,7,3,2,7)
        $miniColors=@([System.Drawing.Color]::FromArgb(255,93,205,70),[System.Drawing.Color]::FromArgb(255,235,49,62),[System.Drawing.Color]::FromArgb(255,255,198,49),[System.Drawing.Color]::FromArgb(255,48,139,222),[System.Drawing.Color]::FromArgb(255,240,91,42))
        for($i=0;$i -lt 5;$i++){$button=New-Object System.Drawing.SolidBrush($miniColors[$i]);$g.FillRectangle($button,7,4+$i,2,1);$button.Dispose()}
        $g.FillRectangle($outline,6,9,4,1)
        $g.FillRectangle($outline,5,10,6,1);$g.FillRectangle($bodyFill,6,10,4,1)
        $g.FillRectangle($outline,4,11,8,1);$g.FillRectangle($bodyFill,5,11,6,1)
        $g.FillRectangle($outline,3,12,3,1);$g.FillRectangle($outline,10,12,3,1);$g.FillRectangle($bodyFill,4,12,2,1);$g.FillRectangle($bodyFill,10,12,2,1)
        $g.FillRectangle($outline,2,13,4,1);$g.FillRectangle($outline,10,13,4,1);$g.FillRectangle($bodyFill,3,13,3,1);$g.FillRectangle($bodyFill,11,13,2,1)
        $g.FillRectangle($outline,1,14,4,1);$g.FillRectangle($outline,11,14,4,1);$g.FillRectangle($bodyFill,2,14,3,1);$g.FillRectangle($bodyFill,11,14,3,1)
        $g.FillRectangle($outline,2,15,4,1);$g.FillRectangle($outline,10,15,4,1);$g.FillRectangle($bodyFill,3,15,2,1);$g.FillRectangle($bodyFill,11,15,2,1)
        $g.FillRectangle($ivory,7,10,2,1);$g.FillRectangle($ivory,6,11,4,1);$g.FillRectangle($ivory,5,12,2,1);$g.FillRectangle($ivory,9,12,2,1)
        $tile.Dispose();$outline.Dispose();$bodyFill.Dispose();$ivory.Dispose();$wood.Dispose()
        $ms=New-Object System.IO.MemoryStream;$bitmap.Save($ms,[System.Drawing.Imaging.ImageFormat]::Png);$bitmap.Dispose();$bytes=$ms.ToArray();$ms.Dispose();return ,$bytes
    }
    $scale = $size / 256.0
    $g.ScaleTransform([single]$scale, [single]$scale)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = 48
    $path.AddArc(12,12,$d,$d,180,90); $path.AddArc(196,12,$d,$d,270,90)
    $path.AddArc(196,196,$d,$d,0,90); $path.AddArc(12,196,$d,$d,90,90); $path.CloseFigure()
    $bg = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,22,24,31))
    $g.FillPath($bg,$path)
    $metal = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255,216,204,177),5)
    $neckBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.Rectangle(109,31,38,134)),[System.Drawing.Color]::FromArgb(83,66,49),[System.Drawing.Color]::FromArgb(26,28,32),[System.Drawing.Drawing2D.LinearGradientMode]::Horizontal)
    $neck = [System.Drawing.Point[]]@((New-Object System.Drawing.Point(109,31)),(New-Object System.Drawing.Point(147,31)),(New-Object System.Drawing.Point(144,165)),(New-Object System.Drawing.Point(112,165)))
    $g.FillPolygon($neckBrush,$neck); $g.DrawPolygon($metal,$neck)
    $head = [System.Drawing.Point[]]@((New-Object System.Drawing.Point(111,36)),(New-Object System.Drawing.Point(108,20)),(New-Object System.Drawing.Point(119,6)),(New-Object System.Drawing.Point(137,4)),(New-Object System.Drawing.Point(150,19)),(New-Object System.Drawing.Point(145,38)),(New-Object System.Drawing.Point(138,43)),(New-Object System.Drawing.Point(118,42)))
    $g.FillPolygon($bg,$head); $g.DrawPolygon($metal,$head)
    $inlay=New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(230,226,220,204),3);$g.DrawLine($inlay,128,10,122,20);$g.DrawLine($inlay,122,20,132,16);$g.DrawLine($inlay,132,16,124,27);$g.DrawLine($inlay,124,27,134,24)
    $tuner=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,201,192,174));$g.FillEllipse($tuner,104,13,7,7);$g.FillEllipse($tuner,145,14,7,7);$g.FillEllipse($tuner,105,25,7,7);$g.FillEllipse($tuner,144,27,7,7)
    $stringPen=New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(235,226,220,205),2);for($i=0;$i -lt 6;$i++){$g.DrawLine($stringPen,115+$i*5,37,114+$i*5,162)}
    $fret=New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(195,207,194,174),3);for($i=0;$i -lt 7;$i++){$y=50+$i*15;$g.DrawLine($fret,111,$y,145,$y+1)}
    $fretColors=@([System.Drawing.Color]::FromArgb(255,93,205,70),[System.Drawing.Color]::FromArgb(255,235,49,62),[System.Drawing.Color]::FromArgb(255,255,198,49),[System.Drawing.Color]::FromArgb(255,48,139,222),[System.Drawing.Color]::FromArgb(255,240,91,42))
    for($i=0;$i -lt 5;$i++){$y=45+$i*18;$brush=New-Object System.Drawing.SolidBrush($fretColors[$i]);$g.FillEllipse($brush,117,$y,22,12);$g.DrawEllipse($metal,117,$y,22,12);$brush.Dispose()}
    $body=New-Object System.Drawing.Drawing2D.GraphicsPath;$body.StartFigure();$body.AddBezier(112,141,80,128,54,133,54,158);$body.AddBezier(54,158,54,176,35,207,16,237);$body.AddBezier(16,237,13,244,26,251,34,247);$body.AddLine(34,247,120,216);$body.AddBezier(120,216,125,214,131,214,137,216);$body.AddLine(137,216,222,247);$body.AddBezier(222,247,230,251,243,244,239,236);$body.AddBezier(239,236,221,207,201,176,201,158);$body.AddBezier(201,158,200,133,175,128,144,141);$body.CloseFigure()
    $bodyBrush=New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.Rectangle(16,132,227,119)),[System.Drawing.Color]::FromArgb(84,90,96),[System.Drawing.Color]::FromArgb(9,12,16),[System.Drawing.Drawing2D.LinearGradientMode]::ForwardDiagonal);$bodyPen=New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255,216,204,177),5);$g.FillPath($bodyBrush,$body);$g.DrawPath($bodyPen,$body)
    $pickguard=[System.Drawing.Point[]]@((New-Object System.Drawing.Point(112,146)),(New-Object System.Drawing.Point(144,146)),(New-Object System.Drawing.Point(183,189)),(New-Object System.Drawing.Point(137,177)),(New-Object System.Drawing.Point(128,204)),(New-Object System.Drawing.Point(119,177)),(New-Object System.Drawing.Point(73,189)))
    $ivory=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,235,227,205));$g.FillPolygon($ivory,$pickguard);$g.DrawPolygon($metal,$pickguard)
    $pickup=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,52,52,49));$g.FillRectangle($pickup,119,177,18,7);$g.FillRectangle($pickup,120,190,16,8)
    $bright=New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255,209,205,186),3);$g.DrawLine($bright,128,176,128,199);$g.DrawLine($bright,143,184,174,166);$g.FillEllipse($ivory,169,162,8,8)
    $g.Dispose();$bg.Dispose();$metal.Dispose();$neckBrush.Dispose();$body.Dispose();$bodyBrush.Dispose();$bodyPen.Dispose();$ivory.Dispose();$pickup.Dispose();$bright.Dispose();$inlay.Dispose();$tuner.Dispose();$stringPen.Dispose();$fret.Dispose();$path.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bitmap.Save($ms,[System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose(); $bytes=$ms.ToArray(); $ms.Dispose(); return ,$bytes
}
$iconPath = Join-Path $PSScriptRoot 'guitar-hero.ico'
$sizes = @(16,32,48,256)
$images = @($sizes | ForEach-Object { New-IconPng $_ })
$stream = [System.IO.File]::Create($iconPath)
$writer = New-Object System.IO.BinaryWriter($stream)
$writer.Write([UInt16]0); $writer.Write([UInt16]1); $writer.Write([UInt16]$images.Count)
$offset = 6 + 16 * $images.Count
for($i=0;$i -lt $images.Count;$i++) {
    $size=$sizes[$i]; $w=if($size -eq 256){0}else{$size}
    $writer.Write([byte]$w); $writer.Write([byte]$w); $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([UInt16]1); $writer.Write([UInt16]32); $writer.Write([UInt32]$images[$i].Length); $writer.Write([UInt32]$offset)
    $offset += $images[$i].Length
}
foreach($image in $images){$writer.Write($image)}
$writer.Dispose(); $stream.Dispose()
& $launcherCompiler /nologo /target:winexe /platform:x64 /optimize+ /utf8output /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll "/win32icon:$iconPath" "/out:$launcherOutput\$OutputName" "$PSScriptRoot\Program.cs" "$PSScriptRoot\Launcher.UI.cs" "$PSScriptRoot\Launcher.Run.cs"
if ($LASTEXITCODE -ne 0) { throw "Falha ao compilar: $LASTEXITCODE" }
Write-Output "Criado: $launcherOutput\$OutputName"




