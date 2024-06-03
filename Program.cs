using MediaTekDocuments.view;
using System;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace MediaTekDocuments
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMediatek());

           

          
            
            // Pour éviter que la console ne se ferme immédiatement
            Console.WriteLine("Appuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
    }
}
