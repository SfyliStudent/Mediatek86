using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaTekDocuments.model
{
    public class Abonnement : Commande
    {
        public Abonnement(string id, DateTime dateCommande, double montant, DateTime dateFinAbonnement, string idRevue)
            : base(id, dateCommande, montant, null, null)
        {
            this.DateFinAbonnement = dateFinAbonnement;
            this.IdRevue = idRevue;
        }

        public DateTime DateFinAbonnement { get; set; }
        public string IdRevue { get; set; }
    }
}
