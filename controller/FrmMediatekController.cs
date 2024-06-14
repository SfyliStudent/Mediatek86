using System.Collections.Generic;
using MediaTekDocuments.model;
using MediaTekDocuments.dal;
using System;
using Newtonsoft.Json;
using System.Threading;


namespace MediaTekDocuments.controller
{
    /// <summary>
    /// Contrôleur lié à FrmMediatek
    /// </summary>
    class FrmMediatekController
    {
        #region Commun
        private readonly List<Livre> lesLivres;
        private readonly List<Dvd> lesDvd;
        private readonly List<Revue> lesRevues;
        private readonly List<Categorie> lesRayons;
        private readonly List<Categorie> lesPublics;
        private readonly List<Categorie> lesGenres;
        /// <summary>
        /// Objet d'accès aux données
        /// </summary>
        private readonly Access access;

        /// <summary>
        /// Récupération de l'instance unique d'accès aux données
        /// </summary>
        public FrmMediatekController()
        {
            access = Access.GetInstance();
            lesLivres = access.GetAllLivres();
            lesDvd = access.GetAllDvd();
            lesRevues = access.GetAllRevues();
            lesGenres = access.GetAllGenres();
            lesRayons = access.GetAllRayons();
            lesPublics = access.GetAllPublics();
        }

        /// <summary>
        /// getter sur la liste des genres
        /// </summary>
        /// <returns>Collection d'objets Genre</returns>
        public List<Categorie> GetAllGenres()
        {
            return lesGenres;
        }

        /// <summary>
        /// getter sur la liste des livres
        /// </summary>
        /// <returns>Collection d'objets Livre</returns>
        public List<Livre> GetAllLivres()
        {
            return lesLivres;
        }

        /// <summary>
        /// getter sur la liste des Dvd
        /// </summary>
        /// <returns>Collection d'objets dvd</returns>
        public List<Dvd> GetAllDvd()
        {
            return lesDvd;
        }

        /// <summary>
        /// getter sur la liste des revues
        /// </summary>
        /// <returns>Collection d'objets Revue</returns>
        public List<Revue> GetAllRevues()
        {
            return lesRevues;
        }

        /// <summary>
        /// getter sur les rayons
        /// </summary>
        /// <returns>Collection d'objets Rayon</returns>
        public List<Categorie> GetAllRayons()
        {
            return lesRayons;
        }

        /// <summary>
        /// getter sur les publics
        /// </summary>
        /// <returns>Collection d'objets Public</returns>
        public List<Categorie> GetAllPublics()
        {
            return lesPublics;
        }

        /// <summary>
        /// getter sur les etats
        /// </summary>
        /// <returns></returns>
        public List<Suivi> GetAllSuivis()
        {
            return access.GetAllSuivis();
        }

        #endregion
        /// <summary>
        /// getter sur les commandes de livre et de dvd
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns>Collection d'objets CommandeDocument</returns>
        public List<CommandeDocument> GetCommandesLivreDvd(string idDocument)
        {
            return access.GetCommandesLivreDvd(idDocument);
        }
        /// <summary>
        /// getter sur les commandes de livre et de dvd
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns>Collection d'objets CommandeDocument</returns>
        public List<Abonnement> GetCommandesRevues(string idDocument)
        {
            return access.GetCommandesRevues(idDocument);
        }
        /// <summary>
        /// Crée une commande de revue dans la bdd
        /// </summary>
        /// <param name="commande">L'objet Abonnement concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool CreerCommandeRevue(Abonnement commande)
        {
            return access.CreerCommandeRevue(commande);
        }
        /// <summary>
        /// Crée une commande de livre ou de DVD dans la bdd
        /// </summary>
        /// <param name="commande">L'objet CommandeDocument concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool CreerCommandeLivreDvd(CommandeDocument commande)
        {
            return access.CreerCommandeLivreDvd(commande);
        }
        /// <summary>
        /// Met à jour le suivi d'une commande dans la bdd
        /// </summary>
        /// <param name="commande">L'objet CommandeDocument concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool UpdateCommande(CommandeDocument commande)
        {
            return access.UpdateCommande(commande);
        }
        /// <summary>
        /// Supprime une commande de revue dans la bdd
        /// </summary>
        /// <param name="commande"></param>
        /// <returns>True si la suppression a pu se faire</returns>
        public bool DeleteCommandeRevue(Abonnement commande)
        {
            return access.DeleteCommandeRevue(commande);
        }
        /// <summary>
        /// Supprime une commande de livre ou de DVD dans la bdd
        /// </summary>
        /// <param name="commande">L'objet CommandeDocument concerné</param>
        /// <returns>True si la suppression a pu se faire</returns>
        public bool DeleteCommandeLivreDvd(CommandeDocument commande)
        {
            return access.DeleteCommandeLivreDvd(commande);
        }

        #region Onglet Parutions
        /// <summary>
        /// récupère les exemplaires d'une revue
        /// </summary>
        /// <param name="idDocument">id de la revue concernée</param>
        /// <returns>Liste d'objets Exemplaire</returns>
        public List<Exemplaire> GetExemplairesRevue(string idDocument)
        {
            return access.GetExemplairesRevue(idDocument);
        }

        /// <summary>
        /// Crée un exemplaire d'une revue dans la bdd
        /// </summary>
        /// <param name="exemplaire">L'objet Exemplaire concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool CreerExemplaire(Exemplaire exemplaire)
        {
            return access.CreerExemplaire(exemplaire);
        }
        #endregion



    }
}
