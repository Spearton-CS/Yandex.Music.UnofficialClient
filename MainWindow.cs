namespace Yandex.Music.UnofficialClient
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
            DwmApi.CustomWindow(BackColor, ForeColor, BackColor, Handle);
        }
        private void ShowPasswordBox_CheckedChanged(object sender, EventArgs e) => PasswordBox.UseSystemPasswordChar = ShowPasswordBox.Checked;
    }
}