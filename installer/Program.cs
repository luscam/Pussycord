using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace PussycordInstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!IsAdministrator())
            {
                var processInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                try { Process.Start(processInfo); }
                catch { MessageBox.Show("Administrator privileges are required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                return;
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }

        private static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }

    public class InstallerForm : Form
    {
        private TextBox pathTextBox;
        private ModernButton installButton;
        private ModernButton browseButton;
        private RichTextBox logBox;
        private System.Windows.Forms.Timer fadeTimer;

        private readonly Color ColBackground = Color.FromArgb(32, 34, 37);
        private readonly Color ColPanel = Color.FromArgb(47, 49, 54);
        private readonly Color ColBlurple = Color.FromArgb(88, 101, 242);
        private readonly Color ColBlurpleHover = Color.FromArgb(71, 82, 196);
        private readonly Color ColText = Color.FromArgb(220, 221, 222);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public InstallerForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(720, 520);
            this.BackColor = ColBackground;
            this.ForeColor = ColText;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.Opacity = 0;

            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            InitializePremiumUI();
            
            fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 10;
            fadeTimer.Tick += (s, e) => { if (this.Opacity < 1) this.Opacity += 0.05; else fadeTimer.Stop(); };
            fadeTimer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            AutoDetectPath();
        }

        private void InitializePremiumUI()
        {
            Panel titleBar = new Panel();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 45;
            titleBar.BackColor = Color.FromArgb(32, 34, 37);
            titleBar.MouseDown += TitleBar_MouseDown;
            this.Controls.Add(titleBar);

            MacButton closeBtn = new MacButton(MacButtonType.Close);
            closeBtn.Location = new Point(15, 15);
            closeBtn.Click += (s, e) => Application.Exit();
            titleBar.Controls.Add(closeBtn);

            MacButton minBtn = new MacButton(MacButtonType.Minimize);
            minBtn.Location = new Point(40, 15);
            minBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            titleBar.Controls.Add(minBtn);

            MacButton maxBtn = new MacButton(MacButtonType.Maximize);
            maxBtn.Location = new Point(65, 15);
            maxBtn.Enabled = false; 
            titleBar.Controls.Add(maxBtn);

            Label titleLabel = new Label();
            titleLabel.Text = "PUSSYCORD INSTALLER";
            titleLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(114, 118, 125);
            titleLabel.AutoSize = true;
            titleBar.Controls.Add(titleLabel);
            titleLabel.Location = new Point((this.Width - titleLabel.PreferredWidth) / 2, 14);
            titleLabel.MouseDown += TitleBar_MouseDown;

            Label headerLabel = new Label();
            headerLabel.Text = "Welcome to Pussycord";
            headerLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            headerLabel.ForeColor = Color.White;
            headerLabel.AutoSize = true;
            headerLabel.Location = new Point(35, 60);
            this.Controls.Add(headerLabel);

            Label subHeader = new Label();
            subHeader.Text = "Let's get your client patched and ready.";
            subHeader.Font = new Font("Segoe UI", 11);
            subHeader.ForeColor = Color.FromArgb(185, 187, 190);
            subHeader.AutoSize = true;
            subHeader.Location = new Point(38, 95);
            this.Controls.Add(subHeader);

            Label descLabel = new Label();
            descLabel.Text = "TARGET DIRECTORY";
            descLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            descLabel.ForeColor = Color.FromArgb(185, 187, 190);
            descLabel.Location = new Point(40, 140);
            descLabel.AutoSize = true;
            this.Controls.Add(descLabel);

            Panel inputContainer = new Panel();
            inputContainer.Location = new Point(35, 160);
            inputContainer.Size = new Size(540, 40);
            inputContainer.BackColor = Color.Transparent;
            inputContainer.Paint += (s, e) => DrawRoundedBox(s, e, ColPanel);
            this.Controls.Add(inputContainer);

            pathTextBox = new TextBox();
            pathTextBox.BorderStyle = BorderStyle.None;
            pathTextBox.BackColor = ColPanel;
            pathTextBox.ForeColor = ColText;
            pathTextBox.Font = new Font("Segoe UI", 11);
            pathTextBox.Location = new Point(10, 10);
            pathTextBox.Width = 510;
            inputContainer.Controls.Add(pathTextBox);

            browseButton = new ModernButton();
            browseButton.Text = "...";
            browseButton.Location = new Point(585, 160);
            browseButton.Size = new Size(60, 40);
            browseButton.BackColor = ColPanel;
            browseButton.HoverColor = Color.FromArgb(64, 68, 75);
            browseButton.Radius = 10;
            browseButton.Click += BrowseButton_Click;
            this.Controls.Add(browseButton);

            installButton = new ModernButton();
            installButton.Text = "Install Pussycord";
            installButton.Location = new Point(35, 220);
            installButton.Size = new Size(200, 45);
            installButton.BackColor = ColBlurple;
            installButton.HoverColor = ColBlurpleHover;
            installButton.Radius = 10;
            installButton.Click += InstallButton_Click;
            this.Controls.Add(installButton);

            Panel logContainer = new Panel();
            logContainer.Location = new Point(35, 290);
            logContainer.Size = new Size(650, 200);
            logContainer.BackColor = Color.Transparent;
            logContainer.Paint += (s, e) => DrawRoundedBox(s, e, Color.Black);
            this.Controls.Add(logContainer);

            logBox = new RichTextBox();
            logBox.BorderStyle = BorderStyle.None;
            logBox.BackColor = Color.Black;
            logBox.ForeColor = Color.FromArgb(0, 255, 0);
            logBox.Font = new Font("Consolas", 9);
            logBox.Location = new Point(15, 15);
            logBox.Size = new Size(620, 170);
            logBox.ReadOnly = true;
            logBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            logContainer.Controls.Add(logBox);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void DrawRoundedBox(object sender, PaintEventArgs e, Color bgColor)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Panel p = sender as Panel;
            using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10))
            using (SolidBrush brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillPath(brush, path);
            }
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();
            if (radius == 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void AutoDetectPath()
        {
            Log("System initialized.");
            Log("Scanning for Discord Canary...");
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string canaryPath = Path.Combine(userPath, "AppData", "Local", "DiscordCanary");

            if (Directory.Exists(canaryPath))
            {
                var directories = Directory.GetDirectories(canaryPath, "app-*");
                if (directories.Length > 0)
                {
                    var sortedDirs = directories.OrderByDescending(d => d).ToList();
                    string corePath = Path.Combine(sortedDirs[0], "modules", "discord_desktop_core-1", "discord_desktop_core");
                    if (Directory.Exists(corePath))
                    {
                        pathTextBox.Text = corePath;
                        Log($"Target found: {corePath}");
                        return;
                    }
                }
            }
            Log("Auto-detection failed. Please select path manually.");
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
                if (fbd.ShowDialog() == DialogResult.OK)
                    pathTextBox.Text = fbd.SelectedPath;
        }

        private void ExtractResource(string fileName, string destPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith(fileName));

            if (resourceName == null) throw new Exception($"Embedded resource '{fileName}' not found!");

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (FileStream fileStream = new FileStream(destPath, FileMode.Create))
            {
                stream.CopyTo(fileStream);
            }
        }

        private void DownloadFile(string url, string destPath)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            logBox.Clear();
            string targetCorePath = pathTextBox.Text;

            if (string.IsNullOrWhiteSpace(targetCorePath) || !Directory.Exists(targetCorePath))
            {
                Log("ERROR: Invalid target path.");
                return;
            }

            installButton.Enabled = false;
            installButton.Text = "Installing...";
            Application.DoEvents();

            try
            {
                Log("Extracting Core...");
                ExtractResource("core.asar", Path.Combine(targetCorePath, "core.asar"));
                Log("Core module updated.");

                Log("Downloading Loader...");
                string pcordDir = @"C:\pcord";
                if (!Directory.Exists(pcordDir)) Directory.CreateDirectory(pcordDir);
                string loaderUrl = "https://raw.githubusercontent.com/luscam/pussycord-loader/refs/heads/main/loader.js";
                string loaderDestPath = Path.Combine(pcordDir, "loader.js");
                DownloadFile(loaderUrl, loaderDestPath);
                Log($"Loader downloaded to {pcordDir}");

                Log("Extracting Icons...");
                string vDir = Directory.GetParent(Directory.GetParent(targetCorePath).FullName).FullName;
                string rDir = Directory.GetParent(vDir).FullName;

                string tempIcon = Path.Combine(Path.GetTempPath(), "pcord_app.ico");
                ExtractResource("app.ico", tempIcon);

                try { File.Copy(tempIcon, Path.Combine(vDir, "app.ico"), true); } catch { }
                try { File.Copy(tempIcon, Path.Combine(rDir, "app.ico"), true); } catch { }
                
                Log("Visual assets updated.");
                Log("DONE.");

                DialogResult result = MessageBox.Show("Pussycord has been successfully installed.", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (result == DialogResult.OK) Application.Exit();
            }
            catch (Exception ex)
            {
                Log($"CRITICAL ERROR: {ex.Message}");
                installButton.Enabled = true;
                installButton.Text = "Install Pussycord";
            }
        }

        private void Log(string msg)
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action<string>(Log), msg);
                return;
            }
            logBox.AppendText($"> {msg}\n");
            logBox.ScrollToCaret();
        }
    }

    public enum MacButtonType { Close, Minimize, Maximize }
    public class MacButton : Control
    {
        private MacButtonType _type;
        public MacButton(MacButtonType type) { _type = type; Size = new Size(12, 12); DoubleBuffered = true; Cursor = Cursors.Hand; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color c = _type == MacButtonType.Close ? Color.FromArgb(237, 101, 88) : _type == MacButtonType.Minimize ? Color.FromArgb(224, 194, 5) : Color.FromArgb(97, 196, 84);
            using (SolidBrush b = new SolidBrush(c)) e.Graphics.FillEllipse(b, 0, 0, 11, 11);
        }
    }

    public class ModernButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color HoverColor { get; set; } = Color.Gray;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public int Radius { get; set; } = 8;
        private bool _isHovered = false;
        public ModernButton() { FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; Font = new Font("Segoe UI", 10, FontStyle.Bold); ForeColor = Color.White; Cursor = Cursors.Hand; }
        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = InstallerForm.RoundedRect(new Rectangle(0, 0, Width, Height), Radius))
            using (SolidBrush brush = new SolidBrush(_isHovered ? HoverColor : BackColor))
            {
                this.Region = new Region(path);
                e.Graphics.FillPath(brush, path);
            }
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(0, 0, Width, Height), ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}