using System;
using System.Windows.Forms;

namespace SimuladorTrocaCalor
{
    /// <summary>
    /// Ponto de entrada da aplicação.
    /// Apenas configura o Windows Forms e abre a janela principal.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Configurações padrão de aparência do Windows Forms
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new FormPrincipal());
        }
    }
}
