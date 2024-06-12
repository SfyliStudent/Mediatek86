
namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Etat (état d'usure d'un document)
    /// </summary>
    public class Etat
    {
        public string Id { get; }
        public string Libelle { get; }

        public Etat(string id, string libelle)
        {
            Id = id;
            Libelle = libelle;
        }
        /// <summary
        /// Récupération du libellé pour l'affichage dans les combos
        /// </summary>
        /// <returns>Libelle</returns>
        public override string ToString()
        {
            return Libelle;
        }
    }
}
