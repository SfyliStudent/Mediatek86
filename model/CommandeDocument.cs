using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaTekDocuments.model
{
    public class CommandeDocument : Commande
    {
        public int NbExemplaire { get; set; }
        public string IdLivreDvd { get; set; }
        

        public CommandeDocument(string id, DateTime dateCommande, double montant, string idSuivi, string suivi, int nbExemplaire, string idLivreDvd)
            : base(id, dateCommande, montant, idSuivi, suivi)
        {
            this.NbExemplaire = nbExemplaire;
            this.IdLivreDvd = idLivreDvd;
            
            

        }

    }
}
