// SPDX-License-Identifier: LicenseRef-GDE-PolyForm-NC-1.0.0-plus-terms
// Guitar Dream Engine, https://github.com/mbracali/prj-gd-engine. See LICENSE and LICENSE-ADDITIONAL-TERMS.md.
// Modified by GustavoARodri on 2026-10-27: Added the native Windows launcher.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuitarHeroNative {
 internal sealed partial class Launcher {
  string ConfigureEngine() {
   string prefs=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"gd-engine","gd01");
   try {
    Directory.CreateDirectory(prefs);
    string probe=Path.Combine(prefs,"launcher-probe-"+Guid.NewGuid().ToString("N")+".tmp");
    using(var p=File.Create(probe)){} File.Delete(probe);
   }catch(UnauthorizedAccessException){prefs=Program.EngineDir;}catch(IOException){prefs=Program.EngineDir;}
   string file=Path.Combine(prefs,"enhanced.json");
   Dictionary<string,object> d;
   try{d=Program.ReadJson(file);}catch(Exception ex){throw new InvalidDataException("Não foi possível ler as configurações existentes. Elas foram preservadas: "+ex.Message);}
   var preset=new Dictionary<string,object>{
    {"max_fps",cap.SelectedIndex==0?"120":"60"},{"fps_counter",true},
    {"internal_res",new string[]{"720","1080","1440"}[resolution.SelectedIndex]},
    {"screen_mode",mode.SelectedIndex==0?"windowed":"borderless"},{"antialiasing","none"},{"ai_upscaling","none"},
    {"bloom","off"},{"cinematic_color",false},{"color_grade","off"},{"film_grain","off"},{"chromatic",false},
    {"light_streaks",false},{"ssao",false},{"sharpening","off"},{"star_blur","none"},{"shadows_all",false},
    {"light_dust",false},{"ground_fog","none"},{"more_lights","off"},{"bokeh","regular"},{"sharp_ui",true},
    {"particles",true},{"hopo_mode","original"},{"autoplay",false},{"no_fail",false},{"cut_content",false}
   };
   foreach(var item in preset)d[item.Key]=item.Value;
   foreach(string k in d.Keys.Where(k=>k.StartsWith("cheat_",StringComparison.Ordinal)).ToArray())d[k]=false;
   string marker=file+".launcher-initial-backup";
   Program.WriteJson(file,d,File.Exists(file)&&!File.Exists(marker));
   if(!File.Exists(marker))File.WriteAllText(marker,"Configuração gerenciada pelo launcher; backup original, se existente, está ao lado.");
   return prefs;
  }
  string GetWindowSize() {
   if(mode.SelectedIndex==1) {
    var bounds=Screen.FromControl(this).Bounds;
    return bounds.Width+"x"+bounds.Height;
   }
   string[] sizes={"1280x720","1920x1080","2560x1440"};
   var area=Screen.FromControl(this).WorkingArea;
   string[] dimensions=sizes[resolution.SelectedIndex].Split('x');
   int width=Math.Min(Int32.Parse(dimensions[0]),area.Width);
   int height=Math.Min(Int32.Parse(dimensions[1]),area.Height);
   return width+"x"+height;
  }
  async void Launch(object sender,EventArgs e) {
   if(launching||game!=null)return;
   string path=iso.Text.Trim();Busy(true);status.Text="Verificando a ISO…";
   try {
    string serial=await Task.Run(()=>IsoCheck.Validate(path));
    string exe=Path.Combine(Program.EngineDir,"gd01-fixed.exe");
    if(!File.Exists(exe)||!File.Exists(Path.Combine(Program.EngineDir,"SDL3.dll")))throw new FileNotFoundException("A pasta gd01 está incompleta. Mantenha o launcher ao lado dela.");
    string prefs=ConfigureEngine();
    Program.WriteJson(settingsFile,new Dictionary<string,object>{{"iso",path},{"resolution",resolution.SelectedIndex},{"cap",cap.SelectedIndex},{"mode",mode.SelectedIndex},{"kb2p",splitKeyboard.Checked}},false);
    Directory.CreateDirectory(Program.LogDir);
    string logPath=Path.Combine(Program.LogDir,"jogo-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".log");
    logWriter=new StreamWriter(logPath,false,new UTF8Encoding(false));logWriter.AutoFlush=true;
    logWriter.WriteLine("Guitar Hero PC launcher | GD01 1.1.3 + correção do painel de fim de música | "+DateTime.Now.ToString("O"));
    logWriter.WriteLine(serial+" | "+cap.Text+" | "+resolution.Text+" | "+mode.Text);
    logWriter.WriteLine("Configurações: "+prefs);
    logWriter.WriteLine("Janela: "+GetWindowSize()+" | modo "+(mode.SelectedIndex==0?"windowed":"borderless"));
    string arguments=Program.Quote(path)+(splitKeyboard.Checked?" --kb2p":"");
    var psi=new ProcessStartInfo(exe,arguments){
     WorkingDirectory=Program.EngineDir,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true
    };
    foreach(string k in new string[]{"GD01_SCREENSHOT","GD01_SCREENSHOT_AT","GD01_SCREENSHOT_WALL","GD01_KEYS","GD01_SCREEN","GD01_SLOT","GD01_VIRTUAL_PAD","GH1RE_SCREENSHOT","GH1RE_KEYS","GD01_SKIP_BOOT","GH1RE_SKIP_BOOT"})
     psi.EnvironmentVariables.Remove(k);
    psi.EnvironmentVariables["GD01_WINDOW"]=GetWindowSize();psi.EnvironmentVariables["GD01_FRAMELOG"]="1";
    var process=new Process{StartInfo=psi,EnableRaisingEvents=true};
    process.OutputDataReceived+=LogLine;process.ErrorDataReceived+=LogLine;
    process.Exited+=delegate{
     process.WaitForExit();int exit=process.ExitCode;
     if(!IsDisposed)BeginInvoke((Action)(()=>GameEnded(exit)));
    };
    game=process;
    if(!process.Start())throw new IOException("O Windows não iniciou o jogo.");
    process.BeginOutputReadLine();process.BeginErrorReadLine();
    status.Text="Jogo aberto • "+serial+" • feche a janela do jogo para voltar.";
    WindowState=FormWindowState.Minimized;
   }catch(Exception ex){
    if(game!=null){game.Dispose();game=null;}
    lock(logLock){if(logWriter!=null){logWriter.Dispose();logWriter=null;}}
    Busy(false);status.Text="Não foi possível iniciar. Confira a mensagem.";
    MessageBox.Show(this,ex.Message,"Não foi possível iniciar",MessageBoxButtons.OK,MessageBoxIcon.Warning);
   }
  }
  void LogLine(object sender,DataReceivedEventArgs e){
   if(e.Data==null)return;lock(logLock){if(logWriter!=null)logWriter.WriteLine(e.Data);}
  }
  void GameEnded(int code){
   lock(logLock){if(logWriter!=null){logWriter.WriteLine("EXIT_CODE="+code);logWriter.Dispose();logWriter=null;}}
   if(game!=null){game.Dispose();game=null;}
   Busy(false);WindowState=FormWindowState.Normal;Activate();
   status.Text=code==0?"Jogo encerrado. Pronto para jogar novamente.":"O jogo terminou com erro ("+code+"). Consulte os logs.";
  }
 }
}

