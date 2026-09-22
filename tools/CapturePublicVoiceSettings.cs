using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CursorImeIndicator;
class PublicSettingsCapture {
 [STAThread] static void Main(string[] args) {
  Application.EnableVisualStyles();
  Application.SetCompatibleTextRenderingDefault(false);
  VoiceSettings values = new VoiceSettings();
  values.Engine = VoiceSettings.EngineCosyVoice;
  values.Enabled = true; values.SpeedPercent = 100;
  values.VoiceId = ""; values.Style = ""; values.CosyVoiceStudioPath = ""; values.CosyVoiceSpeaker = "";
  using(var form = new VoiceSettingsForm(values, delegate {}, delegate {}, Color.FromArgb(42,119,91))) {
   form.Font = new Font("Malgun Gothic", 9);
   form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-30000,-30000); form.TopMost = false; form.ShowInTaskbar = false; form.Show(); Application.DoEvents(); form.PerformLayout();
   using(var bitmap = new Bitmap(form.Width, form.Height)) {
    form.DrawToBitmap(bitmap, new Rectangle(0,0,bitmap.Width,bitmap.Height));
    bitmap.Save(args[0], ImageFormat.Png); Environment.Exit(0);
   }
  }
 }
}