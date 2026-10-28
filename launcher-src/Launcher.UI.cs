// SPDX-License-Identifier: LicenseRef-GDE-PolyForm-NC-1.0.0-plus-terms
// Guitar Dream Engine, https://github.com/mbracali/prj-gd-engine. See LICENSE and LICENSE-ADDITIONAL-TERMS.md.
// Modified by GustavoARodri on 2026-10-27: Added the native Windows launcher.
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace GuitarHeroNative {
 internal sealed partial class Launcher : Form {
  readonly Color bg=Color.FromArgb(13,16,22), panel=Color.FromArgb(25,30,40), panel2=Color.FromArgb(31,38,50), accent=Color.FromArgb(255,196,72), muted=Color.FromArgb(155,168,189), white=Color.FromArgb(241,244,249);
  readonly TextBox iso=new TextBox();
  readonly ComboBox resolution=new ComboBox(),cap=new ComboBox(),mode=new ComboBox();
  readonly Button browse=new Button(),play=new Button(),logs=new Button();
  readonly CheckBox splitKeyboard=new CheckBox();
  readonly Label status=new Label();
  readonly string settingsFile=Path.Combine(Program.Root,"launcher-settings.json");
  Process game; StreamWriter logWriter; readonly object logLock=new object(); bool launching;
  internal Launcher() {
   Text="Guitar Hero PC";BackColor=bg;ForeColor=white;Font=new Font("Segoe UI",10F);
   ClientSize=new Size(860,690);MinimumSize=new Size(820,650);StartPosition=FormStartPosition.CenterScreen;AutoScaleMode=AutoScaleMode.Dpi;
   try{Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);}catch{Icon=SystemIcons.Application;}
   FormBorderStyle=FormBorderStyle.FixedSingle;MaximizeBox=false;
   var root=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(24),ColumnCount=1,RowCount=6,BackColor=bg};
   root.RowStyles.Add(new RowStyle(SizeType.Absolute,146));root.RowStyles.Add(new RowStyle(SizeType.Absolute,18));root.RowStyles.Add(new RowStyle(SizeType.Absolute,144));root.RowStyles.Add(new RowStyle(SizeType.Absolute,130));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,68));
   Controls.Add(root);
   root.Controls.Add(CreateHeroHeader(),0,0);
   root.Controls.Add(new Panel{Height=18,Dock=DockStyle.Fill,BackColor=bg,Margin=Padding.Empty},0,1);
   root.Controls.Add(CreateIsoCard(),0,2);
   root.Controls.Add(CreateOptionsCard(),0,3);
   root.Controls.Add(CreateControlsCard(),0,4);
   root.Controls.Add(CreateBottomBar(),0,5);
   AcceptButton=play;AllowDrop=true;
   DragEnter+=delegate(object s,DragEventArgs e){if(e.Data.GetDataPresent(DataFormats.FileDrop))e.Effect=DragDropEffects.Copy;};
   DragDrop+=delegate(object s,DragEventArgs e){var f=(string[])e.Data.GetData(DataFormats.FileDrop);if(f.Length>0&&!launching&&game==null)iso.Text=f[0];};
   LoadSettings();
   FormClosing+=delegate(object s,FormClosingEventArgs e){if(launching){e.Cancel=true;WindowState=FormWindowState.Minimized;}};
  }
  Control CreateHeroHeader(){
   var header=new Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(18,19,24),Margin=Padding.Empty};
   header.Paint+=delegate(object s,PaintEventArgs e){
    e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;var r=header.ClientRectangle;
    using(var brush=new LinearGradientBrush(r,Color.FromArgb(31,29,31),Color.FromArgb(13,17,24),LinearGradientMode.Horizontal))e.Graphics.FillRectangle(brush,r);
    using(var glow=new SolidBrush(Color.FromArgb(22,171,35,30)))e.Graphics.FillEllipse(glow,r.Width-285,-138,350,300);
    using(var pen=new Pen(Color.FromArgb(105,174,41,32),2F))e.Graphics.DrawLine(pen,132,132,r.Width-20,132);
    using(var pen=new Pen(Color.FromArgb(43,54,68)))e.Graphics.DrawRectangle(pen,0,0,r.Width-1,r.Height-1);
    DrawLogo(e.Graphics,new Rectangle(23,22,100,100));
   };
   header.Controls.Add(new Label{Text="GUITAR HERO",AutoSize=true,Location=new Point(145,35),Font=new Font("Arial Black",22F,FontStyle.Italic),ForeColor=Color.FromArgb(255,211,102),BackColor=Color.Transparent});
   header.Controls.Add(new Label{Text="O clássico do rock. Reimaginado para PC.",AutoSize=true,Location=new Point(148,83),Font=new Font("Segoe UI",10F),ForeColor=muted,BackColor=Color.Transparent});
   var native=Badge("PC NATIVO",Color.FromArgb(48,42,39));native.Location=new Point(650,31);header.Controls.Add(native);
   var fps=Badge("TETO 120 FPS",Color.FromArgb(73,48,27));fps.Location=new Point(650,76);header.Controls.Add(fps);
   return header;
  }
  void DrawLogo(Graphics g,Rectangle r){
   var state=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;
   using(var path=new GraphicsPath()){
    int d=20;path.AddArc(r.X,r.Y,d,d,180,90);path.AddArc(r.Right-d,r.Y,d,d,270,90);path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);path.AddArc(r.X,r.Bottom-d,d,d,90,90);path.CloseFigure();
    using(var fill=new LinearGradientBrush(r,Color.FromArgb(45,49,58),Color.FromArgb(16,19,26),LinearGradientMode.ForwardDiagonal))g.FillPath(fill,path);
    using(var edge=new Pen(Color.FromArgb(139,157,113,61),1.4F))g.DrawPath(edge,path);
   }
   DrawGuitar(g,new Rectangle(r.X+5,r.Y+5,r.Width-10,r.Height-10));g.Restore(state);
  }
  void DrawGuitar(Graphics g,Rectangle r){
   var state=g.Save();g.TranslateTransform(r.X,r.Y);g.ScaleTransform(r.Width/256F,r.Height/256F);g.SmoothingMode=SmoothingMode.AntiAlias;
   using(var neck=new GraphicsPath()){
    neck.AddPolygon(new Point[]{new Point(109,31),new Point(147,31),new Point(144,165),new Point(112,165)});
    using(var fill=new LinearGradientBrush(new Rectangle(109,31,38,134),Color.FromArgb(83,66,49),Color.FromArgb(26,28,32),LinearGradientMode.Horizontal))g.FillPath(fill,neck);
    using(var edge=new Pen(Color.FromArgb(210,212,193,164),3F))g.DrawPath(edge,neck);
   }
   using(var strings=new Pen(Color.FromArgb(235,226,220,205),1.2F))for(int i=0;i<6;i++)g.DrawLine(strings,115+i*5,37,114+i*5,162);
   using(var fret=new Pen(Color.FromArgb(195,207,194,174),2F))for(int i=0;i<7;i++){int y=50+i*15;g.DrawLine(fret,111,y,145,y+1);}
   Color[] frets={Color.FromArgb(93,205,70),Color.FromArgb(235,49,62),Color.FromArgb(255,198,49),Color.FromArgb(48,139,222),Color.FromArgb(240,91,42)};
   for(int i=0;i<5;i++){
    int y=45+i*18;var box=new Rectangle(117,y,22,12);
    using(var shadow=new SolidBrush(Color.FromArgb(150,0,0,0)))g.FillEllipse(shadow,box.X-2,box.Y+1,box.Width+4,box.Height+2);
    using(var fill=new LinearGradientBrush(box,ControlPaint.Light(frets[i],.18F),ControlPaint.Dark(frets[i],.14F),LinearGradientMode.Vertical))g.FillEllipse(fill,box);
    using(var edge=new Pen(Color.FromArgb(240,239,226,199),1F))g.DrawEllipse(edge,box);
    using(var shine=new SolidBrush(Color.FromArgb(85,255,255,255)))g.FillEllipse(shine,box.X+4,box.Y+2,box.Width-8,3);
   }
   using(var head=new GraphicsPath()){
    head.AddPolygon(new Point[]{new Point(111,36),new Point(108,20),new Point(119,6),new Point(137,4),new Point(150,19),new Point(145,38),new Point(138,43),new Point(118,42)});
    using(var fill=new LinearGradientBrush(new Rectangle(108,4,42,39),Color.FromArgb(65,69,73),Color.FromArgb(13,16,20),LinearGradientMode.ForwardDiagonal))g.FillPath(fill,head);
    using(var edge=new Pen(Color.FromArgb(225,213,199,177),2F))g.DrawPath(edge,head);
   }
   using(var inlay=new Pen(Color.FromArgb(230,226,220,204),2F)){g.DrawLine(inlay,128,10,122,20);g.DrawLine(inlay,122,20,132,16);g.DrawLine(inlay,132,16,124,27);g.DrawLine(inlay,124,27,134,24);}
   using(var tuner=new SolidBrush(Color.FromArgb(255,201,192,174))){g.FillEllipse(tuner,104,13,7,7);g.FillEllipse(tuner,145,14,7,7);g.FillEllipse(tuner,105,25,7,7);g.FillEllipse(tuner,144,27,7,7);}
   using(var body=new GraphicsPath()){
    body.StartFigure();body.AddBezier(112,141,80,128,54,133,54,158);body.AddBezier(54,158,54,176,35,207,16,237);body.AddBezier(16,237,13,244,26,251,34,247);body.AddLine(34,247,120,216);body.AddBezier(120,216,125,214,131,214,137,216);body.AddLine(137,216,222,247);body.AddBezier(222,247,230,251,243,244,239,236);body.AddBezier(239,236,221,207,201,176,201,158);body.AddBezier(201,158,200,133,175,128,144,141);body.CloseFigure();
    using(var fill=new LinearGradientBrush(new Rectangle(16,132,227,119),Color.FromArgb(84,90,96),Color.FromArgb(9,12,16),LinearGradientMode.ForwardDiagonal))g.FillPath(fill,body);
    using(var edge=new Pen(Color.FromArgb(255,216,204,177),5F))g.DrawPath(edge,body);
   }
   Point[] pickguard={new Point(112,146),new Point(144,146),new Point(183,189),new Point(137,177),new Point(128,204),new Point(119,177),new Point(73,189)};
   using(var guard=new LinearGradientBrush(new Rectangle(73,146,110,58),Color.FromArgb(255,244,236,215),Color.FromArgb(255,199,187,160),LinearGradientMode.Vertical))g.FillPolygon(guard,pickguard);
   using(var binding=new Pen(Color.FromArgb(230,255,245,224),2F))g.DrawPolygon(binding,pickguard);
   using(var metal=new SolidBrush(Color.FromArgb(255,52,52,49))){g.FillRectangle(metal,119,177,18,7);g.FillRectangle(metal,120,190,16,8);}
   using(var bright=new Pen(Color.FromArgb(255,209,205,186),3F)){g.DrawLine(bright,128,176,128,199);g.DrawLine(bright,143,184,174,166);}
   using(var tip=new SolidBrush(Color.FromArgb(255,227,222,203)))g.FillEllipse(tip,169,162,8,8);
   using(var screw=new SolidBrush(Color.FromArgb(255,202,190,162))){g.FillEllipse(screw,86,185,5,5);g.FillEllipse(screw,168,185,5,5);g.FillEllipse(screw,124,207,5,5);}
   g.Restore(state);
  }  Label Badge(string text,Color color){return new Label{Text="  "+text+"  ",AutoSize=true,Padding=new Padding(8,7,8,7),BackColor=color,ForeColor=white,Font=new Font("Segoe UI",8F,FontStyle.Bold)};}  Control CreateIsoCard(){
   var card=Card();var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20,15,20,13),ColumnCount=2,RowCount=3,BackColor=panel};
   layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,156));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,27));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,46));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));card.Controls.Add(layout);
   var title=SectionTitle("1", "ARQUIVO DO JOGO");layout.Controls.Add(title,0,0);layout.SetColumnSpan(title,2);
   iso.Dock=DockStyle.Fill;iso.Margin=new Padding(0,2,12,5);iso.BackColor=Color.FromArgb(15,19,27);iso.ForeColor=white;iso.BorderStyle=BorderStyle.FixedSingle;iso.Font=new Font("Segoe UI",10F);iso.AccessibleName="Caminho da ISO";
   iso.TextChanged+=delegate{status.Text="ISO selecionada. Pronta para iniciar.";};layout.Controls.Add(iso,0,1);
   browse.Text="◉   Procurar ISO";StyleButton(browse,false);browse.Dock=DockStyle.Fill;browse.Margin=new Padding(0,2,0,5);browse.Click+=ChooseIso;layout.Controls.Add(browse,1,1);
   var hint=new Label{Text="Arraste sua ISO para esta janela  •  A imagem é validada e usada no local, sem cópia ou alteração.",Dock=DockStyle.Fill,ForeColor=muted,Font=new Font("Segoe UI",8.5F),Margin=new Padding(0,3,0,0),TextAlign=ContentAlignment.MiddleLeft};layout.Controls.Add(hint,0,2);layout.SetColumnSpan(hint,2);return card;
  }
  Control CreateOptionsCard(){
   var card=Card();var outer=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20,14,20,15),ColumnCount=1,RowCount=2,BackColor=panel};outer.RowStyles.Add(new RowStyle(SizeType.Absolute,30));outer.RowStyles.Add(new RowStyle(SizeType.Percent,100));card.Controls.Add(outer);
   outer.Controls.Add(SectionTitle("2", "AJUSTES DE VÍDEO"),0,0);
   var choices=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=3,RowCount=2,BackColor=panel,Margin=Padding.Empty};choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,40));choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35));choices.RowStyles.Add(new RowStyle(SizeType.Absolute,23));choices.RowStyles.Add(new RowStyle(SizeType.Percent,100));
   AddChoice(choices,resolution,"RESOLUÇÃO",0,new string[]{"720p · desempenho","1080p · equilibrado","1440p · qualidade"});AddChoice(choices,cap,"LIMITE DE FPS",1,new string[]{"120 FPS","60 FPS"});AddChoice(choices,mode,"MODO DE TELA",2,new string[]{"Janela","Tela sem bordas"});outer.Controls.Add(choices,0,1);return card;
  }
  Control CreateControlsCard(){
   var wrap=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=bg,Margin=Padding.Empty};wrap.RowStyles.Add(new RowStyle(SizeType.Absolute,48));wrap.RowStyles.Add(new RowStyle(SizeType.Percent,100));
   wrap.Controls.Add(SectionTitle("3", "JOGUE DO SEU JEITO"),0,0);
   var card=Card();var content=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(18,10,16,10),ColumnCount=2,RowCount=2,BackColor=panel};content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,38));content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));content.RowStyles.Add(new RowStyle(SizeType.Absolute,34));content.RowStyles.Add(new RowStyle(SizeType.Percent,100));card.Controls.Add(content);
   var icon=new Panel{Dock=DockStyle.Fill,BackColor=panel,Margin=Padding.Empty};icon.Paint+=delegate(object s,PaintEventArgs e){DrawGuitar(e.Graphics,new Rectangle(1,2,36,54));};content.Controls.Add(icon,0,0);content.SetRowSpan(icon,2);
   splitKeyboard.Text="Teclado dividido para 2 jogadores";splitKeyboard.Dock=DockStyle.Fill;splitKeyboard.ForeColor=white;splitKeyboard.Font=new Font("Segoe UI",9.5F,FontStyle.Bold);splitKeyboard.Margin=Padding.Empty;splitKeyboard.AccessibleName="Teclado dividido para dois jogadores";content.Controls.Add(splitKeyboard,1,0);
   content.Controls.Add(new Label{Text="P1: 1–5 + setas     |     P2: H J K L ;  •  N/M palhetada  •  B Star Power  •  O Start",Dock=DockStyle.Fill,ForeColor=muted,Font=new Font("Segoe UI",8.5F),Margin=Padding.Empty,TextAlign=ContentAlignment.MiddleLeft},1,1);
   wrap.Controls.Add(card,0,1);return wrap;
  }
  Control CreateBottomBar(){
   var bottom=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=2,BackColor=bg,Margin=Padding.Empty};bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,68));bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,32));bottom.RowStyles.Add(new RowStyle(SizeType.Absolute,42));bottom.RowStyles.Add(new RowStyle(SizeType.Absolute,24));
   play.Text="▶   INICIAR JOGO";StyleButton(play,true);play.Font=new Font("Segoe UI",11F,FontStyle.Bold);play.Dock=DockStyle.Fill;play.Margin=new Padding(0,0,12,5);play.Click+=Launch;bottom.Controls.Add(play,0,0);
   logs.Text="☰  Logs";StyleButton(logs,false);logs.Dock=DockStyle.Fill;logs.Margin=new Padding(0,0,0,5);logs.Click+=delegate{Directory.CreateDirectory(Program.LogDir);Process.Start(new ProcessStartInfo(Program.LogDir){UseShellExecute=true});};bottom.Controls.Add(logs,1,0);
   status.Text="Selecione sua ISO para começar.";status.Dock=DockStyle.Fill;status.ForeColor=Color.FromArgb(161,193,161);status.Font=new Font("Segoe UI",8.5F);status.Margin=new Padding(2,7,0,0);bottom.Controls.Add(status,0,1);
   var footer=new LinkLabel{Text="GD01 1.1.3  •  ALPHA",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleRight,LinkColor=muted,ActiveLinkColor=accent,VisitedLinkColor=muted,Font=new Font("Segoe UI",8F),Margin=new Padding(0,4,0,0)};footer.LinkClicked+=delegate{Process.Start(new ProcessStartInfo("https://github.com/mbracali/prj-gd-engine"){UseShellExecute=true});};bottom.Controls.Add(footer,1,1);return bottom;
  }
  Panel Card(){return new Panel{Dock=DockStyle.Fill,BackColor=panel,Margin=Padding.Empty,Padding=Padding.Empty};}
  Label SectionTitle(string n,string text){return new Label{Text=n+"     "+text,Dock=DockStyle.Fill,ForeColor=white,Font=new Font("Segoe UI",9F,FontStyle.Bold),TextAlign=ContentAlignment.MiddleLeft,Margin=Padding.Empty};}
  void StyleButton(Button b,bool primary){b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderSize=primary?0:1;b.FlatAppearance.BorderColor=Color.FromArgb(57,68,86);b.FlatAppearance.MouseOverBackColor=primary?Color.FromArgb(255,211,111):Color.FromArgb(39,48,63);b.BackColor=primary?accent:panel2;b.ForeColor=primary?Color.FromArgb(22,26,34):white;b.Cursor=Cursors.Hand;b.UseVisualStyleBackColor=false;}
  void AddChoice(TableLayoutPanel p,ComboBox box,string title,int col,string[] values){p.Controls.Add(new Label{Text=title,Dock=DockStyle.Fill,Font=new Font("Segoe UI",8F,FontStyle.Bold),ForeColor=muted,Margin=Padding.Empty,TextAlign=ContentAlignment.MiddleLeft},col,0);box.DropDownStyle=ComboBoxStyle.DropDownList;box.Items.AddRange(values);box.SelectedIndex=0;box.Dock=DockStyle.Top;box.Height=32;box.BackColor=Color.FromArgb(15,19,27);box.ForeColor=white;box.FlatStyle=FlatStyle.Flat;box.Margin=new Padding(0,2,col==2?0:13,0);box.AccessibleName=title;p.Controls.Add(box,col,1);}
  void LoadSettings(){try{var s=Program.ReadJson(settingsFile);if(s.ContainsKey("iso"))iso.Text=Convert.ToString(s["iso"]);if(s.ContainsKey("resolution"))resolution.SelectedIndex=Math.Max(0,Math.Min(2,Convert.ToInt32(s["resolution"])));if(s.ContainsKey("cap"))cap.SelectedIndex=Math.Max(0,Math.Min(1,Convert.ToInt32(s["cap"])));if(s.ContainsKey("mode"))mode.SelectedIndex=Math.Max(0,Math.Min(1,Convert.ToInt32(s["mode"])));if(s.ContainsKey("kb2p"))splitKeyboard.Checked=Convert.ToBoolean(s["kb2p"]);}catch{status.Text="Preferências anteriores inválidas. Selecione sua ISO novamente.";}}
  void ChooseIso(object sender,EventArgs e){using(var d=new OpenFileDialog{Title="Escolha sua ISO de Guitar Hero 1 (PS2)",Filter="Imagem do jogo (*.iso)|*.iso",CheckFileExists=true,Multiselect=false}){if(File.Exists(iso.Text))d.FileName=iso.Text;if(d.ShowDialog(this)==DialogResult.OK){iso.Text=d.FileName;status.Text="ISO selecionada. Clique em INICIAR JOGO.";}}}
  void Busy(bool value){launching=value;iso.Enabled=!value;browse.Enabled=!value;resolution.Enabled=!value;cap.Enabled=!value;mode.Enabled=!value;splitKeyboard.Enabled=!value;play.Enabled=!value;play.Text=value?"JOGO EM EXECUÇÃO…":"▶   INICIAR JOGO";}
 }
}




