using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaTekDocuments.model
{
    public class Suivi : Categorie
    {
        private readonly string id;
        private readonly string libelle;

        public Suivi(string id, string libelle) : base(id, libelle)
        {
            this.id = id;
            this.libelle = libelle;
        }

        public string Id { get => id; }
        public string Libelle { get => libelle; }
    }

}
