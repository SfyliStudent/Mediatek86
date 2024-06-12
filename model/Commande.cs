using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaTekDocuments.model
{
    public class Commande
    {
        public string Id { get; set; }
        public DateTime DateCommande { get; set; }
        public double Montant { get; }
        public string IdSuivi { get; set; }
        public string Suivi { get; set; }


        public Commande(string id, DateTime dateCommande, double montant, string idSuivi, string suivi)
        {
            Id = id;
            DateCommande = dateCommande;
            Montant = montant;
            IdSuivi = idSuivi;
            Suivi = suivi;

        }
    }
}
