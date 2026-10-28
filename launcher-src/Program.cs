// SPDX-License-Identifier: LicenseRef-GDE-PolyForm-NC-1.0.0-plus-terms
// Guitar Dream Engine, https://github.com/mbracali/prj-gd-engine. See LICENSE and LICENSE-ADDITIONAL-TERMS.md.
// Modified by GustavoARodri on 2026-10-27: Added the native Windows launcher.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace GuitarHeroNative {
    internal static class Program {
        internal static readonly string Root = AppDomain.CurrentDomain.BaseDirectory;
        internal static readonly string EngineDir = Path.Combine(Root, "gd01");
        internal static readonly string LogDir = Path.Combine(Root, "logs");
        internal static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
        [STAThread] static int Main(string[] args) {
            if (args.Length == 2 && args[0] == "--validate-iso") {
                try {
                    string serial = IsoCheck.Validate(args[1]);
                    Directory.CreateDirectory(Path.Combine(Root,"diagnostico"));
                    File.WriteAllText(Path.Combine(Root,"diagnostico","launcher-validation.txt"),"OK: "+serial,new UTF8Encoding(false));
                    return 0;
                } catch(Exception ex) {
                    Directory.CreateDirectory(LogDir);
                    File.WriteAllText(Path.Combine(LogDir,"validation-error.txt"),ex.Message);
                    return 2;
                }
            }
            bool first;
            using(var mutex=new Mutex(true,@"Local\GuitarHeroNativeLauncher",out first)) {
                if(!first) {MessageBox.Show("O launcher já está aberto.","Guitar Hero PC");return 0;}
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e) {
                    MessageBox.Show(e.Exception.Message,"Guitar Hero PC",MessageBoxButtons.OK,MessageBoxIcon.Error);
                };
                try { Application.Run(new Launcher()); }
                catch(Exception ex) { MessageBox.Show(ex.Message,"Falha ao abrir o launcher",MessageBoxButtons.OK,MessageBoxIcon.Error); }
            }
            return 0;
        }
        internal static Dictionary<string,object> ReadJson(string file) {
            if(!File.Exists(file))return new Dictionary<string,object>();
            return Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(file,Encoding.UTF8))??new Dictionary<string,object>();
        }
        internal static void WriteJson(string file,Dictionary<string,object> data,bool backup) {
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            string tmp=file+"."+Guid.NewGuid().ToString("N")+".tmp";
            File.WriteAllText(tmp,Json.Serialize(data),new UTF8Encoding(false));
            try {
                if(File.Exists(file)) {
                    string bak=backup?file+".backup-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"):null;
                    File.Replace(tmp,file,bak);
                } else File.Move(tmp,file);
            } finally {if(File.Exists(tmp))File.Delete(tmp);}
        }
        internal static string Quote(string value) {
            var b=new StringBuilder("\"");int slashes=0;
            foreach(char ch in value) {
                if(ch=='\\'){slashes++;continue;}
                if(ch=='"'){b.Append('\\',slashes*2+1);b.Append(ch);slashes=0;continue;}
                b.Append('\\',slashes);slashes=0;b.Append(ch);
            }
            b.Append('\\',slashes*2);b.Append('"');return b.ToString();
        }
    }

    internal sealed class IsoCheck {
        private readonly FileStream stream;
        private sealed class Entry {internal uint Lba;internal uint Size;internal byte Flags;internal string Name;}
        private IsoCheck(FileStream source){stream=source;}
        private byte[] Read(long offset,int count) {
            if(offset<0||count<0||offset+count>stream.Length)throw new InvalidDataException("A imagem ISO está truncada.");
            stream.Position=offset;byte[] b=new byte[count];int used=0;
            while(used<count){int n=stream.Read(b,used,count-used);if(n==0)throw new EndOfStreamException();used+=n;}return b;
        }
        private static Entry Record(byte[] b,int p) {
            if(p+34>b.Length||b[p]<34||p+b[p]>b.Length||p+33+b[p+32]>b.Length)throw new InvalidDataException("Diretório ISO inválido.");
            return new Entry{Lba=BitConverter.ToUInt32(b,p+2),Size=BitConverter.ToUInt32(b,p+10),Flags=b[p+25],Name=Encoding.ASCII.GetString(b,p+33,b[p+32]).Split(';')[0]};
        }
        private List<Entry> Entries(Entry dir) {
            if(dir.Size>16*1024*1024)throw new InvalidDataException("Diretório ISO excessivo.");
            byte[] b=Read((long)dir.Lba*2048,(int)dir.Size);var list=new List<Entry>();
            for(int p=0;p<b.Length;) {
                if(b[p]==0){p=(p/2048+1)*2048;continue;}Entry e=Record(b,p);
                if((e.Flags&128)!=0)throw new InvalidDataException("ISO com múltiplas extensões não suportada.");
                if((long)e.Lba*2048+e.Size>stream.Length)throw new InvalidDataException("Arquivo fora dos limites da ISO.");
                list.Add(e);p+=b[p];
            }return list;
        }
        internal static string Validate(string path) {
            if(String.IsNullOrWhiteSpace(path)||!File.Exists(path))throw new InvalidDataException("Selecione uma ISO existente.");
            if(!String.Equals(Path.GetExtension(path),".iso",StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Selecione um arquivo .iso de Guitar Hero 1 para PS2.");
            using(var f=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)) {
                var iso=new IsoCheck(f);byte[] pvd=null;
                for(int s=16;s<80;s++) {
                    byte[] d=iso.Read(s*2048,2048);
                    if(Encoding.ASCII.GetString(d,1,5)!="CD001")throw new InvalidDataException("A ISO não tem estrutura ISO9660 reconhecida.");
                    if(d[0]==1){pvd=d;break;}if(d[0]==255)break;
                }
                if(pvd==null||BitConverter.ToUInt16(pvd,128)!=2048)throw new InvalidDataException("Formato de ISO não suportado.");
                List<Entry> root=iso.Entries(Record(pvd,156));
                Entry cnf=root.Find(e=>String.Equals(e.Name,"SYSTEM.CNF",StringComparison.OrdinalIgnoreCase));
                Entry gen=root.Find(e=>String.Equals(e.Name,"GEN",StringComparison.OrdinalIgnoreCase)&&(e.Flags&2)!=0);
                if(cnf==null||cnf.Size>65536||gen==null)throw new InvalidDataException("Não encontrei os arquivos de Guitar Hero 1 nesta ISO.");
                string boot=Encoding.ASCII.GetString(iso.Read((long)cnf.Lba*2048,(int)cnf.Size));
                string serial=boot.Contains("SLES_541.32")?"SLES-54132 / PAL":boot.Contains("SLUS_212.24")?"SLUS-21224 / USA":null;
                if(serial==null)throw new InvalidDataException("Edição não identificada como GH1 PAL ou USA. A edição PAL SLES-54132 foi testada.");
                List<Entry> assets=iso.Entries(gen);
                Entry hdr=assets.Find(e=>String.Equals(e.Name,"MAIN.HDR",StringComparison.OrdinalIgnoreCase));
                Entry ark=assets.Find(e=>String.Equals(e.Name,"MAIN_0.ARK",StringComparison.OrdinalIgnoreCase));
                if(hdr==null||hdr.Size<20||ark==null||ark.Size==0||BitConverter.ToUInt32(iso.Read((long)hdr.Lba*2048,4),0)!=3)
                    throw new InvalidDataException("MAIN.HDR / MAIN_0.ARK ausentes ou incompatíveis.");
                return serial;
            }
        }
    }
}



