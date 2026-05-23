using LayerGenForDotNet4.Forms;

namespace LayerGenForDotNet4;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}