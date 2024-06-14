using System;
using System.Windows.Forms;
using MediaTekDocuments.model;
using MediaTekDocuments.controller;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;

namespace MediaTekDocuments.view

{
    /// <summary>
    /// Classe d'affichage
    /// </summary>
    public partial class FrmMediatek : Form
    {
        #region Variables globales

        private readonly FrmMediatekController controller;
        const string ETATNEUF = "00001";

        private readonly BindingSource bdgLivresListe = new BindingSource();
        private readonly BindingSource bdgDvdListe = new BindingSource();
        private readonly BindingSource bdgGenres = new BindingSource();
        private readonly BindingSource bdgPublics = new BindingSource();
        private readonly BindingSource bdgRayons = new BindingSource();
        private readonly BindingSource bdgRevuesListe = new BindingSource();
        private readonly BindingSource bdgExemplairesListe = new BindingSource();
        private readonly BindingSource bdgCLListe = new BindingSource();
        private readonly BindingSource bdgSuivi = new BindingSource();
        private readonly BindingSource bdgCDListe = new BindingSource();
        private readonly BindingSource bdgCRListe = new BindingSource();
        private List<Livre> lesLivres = new List<Livre>();
        private List<Dvd> lesDvd = new List<Dvd>();
        private List<Revue> lesRevues = new List<Revue>();
        private List<Exemplaire> lesExemplaires = new List<Exemplaire>();
        private List<CommandeDocument> lesCommandesLivresDvd = new List<CommandeDocument>();
        private List<Abonnement> lesCommandesRevues = new List<Abonnement>();


        #endregion

        /// <summary>
        /// Constructeur : création du contrôleur lié à ce formulaire
        /// </summary>

        internal FrmMediatek()
        {
            InitializeComponent();
            this.controller = new FrmMediatekController();

        }

        #region Commun

        /// <summary>
        /// Rempli un des 3 combo (genre, public, rayon)
        /// </summary>
        /// <param name="lesCategories">liste des objets de type Genre ou Public ou Rayon</param>
        /// <param name="bdg">bindingsource contenant les informations</param>
        /// <param name="cbx">combobox à remplir</param>

        public void RemplirComboCategorie(List<Categorie> lesCategories, BindingSource bdg, ComboBox cbx)
        {
            bdg.DataSource = lesCategories;
            cbx.DataSource = bdg;
            if (cbx.Items.Count > 0)
            {
                cbx.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Retourne l'id du suivi passé en paramètre
        /// </summary>
        /// <param name="suivi">Libellé du suivi</param>
        /// <returns>String contenant l'id du suivi</returns>
        private string getIdSuivi(string suivi)
        {
            List<Suivi> lesSuivis = controller.GetAllSuivis();
            foreach (Suivi unSuivi in lesSuivis)
            {
                if (unSuivi.Libelle == suivi)
                {
                    return unSuivi.Id;
                }
            }
            return null;
        }
        /// <summary>
        /// Rempli un des 3 combo (genre, public, rayon)
        /// </summary>
        /// <param name="lesSuivis"></param>
        /// <param name="bdg"></param>
        /// <param name="cbx"></param>
        public void RemplirComboSuivi(List<Suivi> lesSuivis, BindingSource bdg, ComboBox cbx, string suivi)
        {
            switch (suivi)
            {
                case "en cours":
                    lesSuivis.RemoveAt(2);
                    break;
                case "livrée":
                    lesSuivis.RemoveAt(3);
                    lesSuivis.RemoveAt(0);
                    break;
                case "réglée":
                    lesSuivis.Clear();
                    break;
                case "relancée":
                    lesSuivis.RemoveAt(3);
                    lesSuivis.RemoveAt(2);
                    lesSuivis.RemoveAt(0);
                    break;
            }
            bdg.DataSource = lesSuivis;
            cbx.DataSource = bdg;
            if (cbx.Items.Count > 0)
            {
                cbx.SelectedIndex = 0;
            }
        }

        #endregion

        #region Onglet Livres

        /// <summary>
        /// Ouverture de l'onglet Livres : 
        /// appel des méthodes pour remplir le datagrid des livres et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabLivres_Enter(object sender, EventArgs e)
        {
            lesLivres = controller.GetAllLivres();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxLivresGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxLivresPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxLivresRayons);
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="livres">liste de livres</param>
        private void RemplirLivresListe(List<Livre> livres)
        {
            bdgLivresListe.DataSource = livres;
            dgvLivresListe.DataSource = bdgLivresListe;
            dgvLivresListe.Columns["isbn"].Visible = false;
            dgvLivresListe.Columns["idRayon"].Visible = false;
            dgvLivresListe.Columns["idGenre"].Visible = false;
            dgvLivresListe.Columns["idPublic"].Visible = false;
            dgvLivresListe.Columns["image"].Visible = false;
            dgvLivresListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvLivresListe.Columns["id"].DisplayIndex = 0;
            dgvLivresListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage du livre dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbLivresNumRecherche.Text.Equals(""))
            {
                txbLivresTitreRecherche.Text = "";
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
                Livre livre = lesLivres.Find(x => x.Id.Equals(txbLivresNumRecherche.Text));
                if (livre != null)
                {
                    List<Livre> livres = new List<Livre>() { livre };
                    RemplirLivresListe(livres);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirLivresListeComplete();
                }
            }
            else
            {
                RemplirLivresListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des livres dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxbLivresTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbLivresTitreRecherche.Text.Equals(""))
            {
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
                txbLivresNumRecherche.Text = "";
                List<Livre> lesLivresParTitre;
                lesLivresParTitre = lesLivres.FindAll(x => x.Titre.ToLower().Contains(txbLivresTitreRecherche.Text.ToLower()));
                RemplirLivresListe(lesLivresParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxLivresGenres.SelectedIndex < 0 && cbxLivresPublics.SelectedIndex < 0 && cbxLivresRayons.SelectedIndex < 0
                    && txbLivresNumRecherche.Text.Equals(""))
                {
                    RemplirLivresListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations du livre sélectionné
        /// </summary>
        /// <param name="livre">le livre</param>
        private void AfficheLivresInfos(Livre livre)
        {
            txbLivresAuteur.Text = livre.Auteur;
            txbLivresCollection.Text = livre.Collection;
            txbLivresImage.Text = livre.Image;
            txbLivresIsbn.Text = livre.Isbn;
            txbLivresNumero.Text = livre.Id;
            txbLivresGenre.Text = livre.Genre;
            txbLivresPublic.Text = livre.Public;
            txbLivresRayon.Text = livre.Rayon;
            txbLivresTitre.Text = livre.Titre;
            string image = livre.Image;
            try
            {
                pcbLivresImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbLivresImage.Image = null;
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations du livre
        /// </summary>
        private void VideLivresInfos()
        {
            txbLivresAuteur.Text = "";
            txbLivresCollection.Text = "";
            txbLivresImage.Text = "";
            txbLivresIsbn.Text = "";
            txbLivresNumero.Text = "";
            txbLivresGenre.Text = "";
            txbLivresPublic.Text = "";
            txbLivresRayon.Text = "";
            txbLivresTitre.Text = "";
            pcbLivresImage.Image = null;
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresGenres.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Genre genre = (Genre)cbxLivresGenres.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirLivresListe(livres);
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresPublics.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Public lePublic = (Public)cbxLivresPublics.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirLivresListe(livres);
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresRayons.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxLivresRayons.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirLivresListe(livres);
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations du livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgvLivresListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLivresListe.CurrentCell != null)
            {
                try
                {
                    Livre livre = (Livre)bdgLivresListe.List[bdgLivresListe.Position];
                    AfficheLivresInfos(livre);
                }
                catch
                {
                    VideLivresZones();
                }
            }
            else
            {
                VideLivresInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des livres
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirLivresListeComplete()
        {
            RemplirLivresListe(lesLivres);
            VideLivresZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideLivresZones()
        {
            cbxLivresGenres.SelectedIndex = -1;
            cbxLivresRayons.SelectedIndex = -1;
            cbxLivresPublics.SelectedIndex = -1;
            txbLivresNumRecherche.Text = "";
            txbLivresTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgvLivresListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideLivresZones();
            string titreColonne = dgvLivresListe.Columns[e.ColumnIndex].HeaderText;
            List<Livre> sortedList = new List<Livre>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesLivres.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesLivres.OrderBy(o => o.Titre).ToList();
                    break;
                case "Collection":
                    sortedList = lesLivres.OrderBy(o => o.Collection).ToList();
                    break;
                case "Auteur":
                    sortedList = lesLivres.OrderBy(o => o.Auteur).ToList();
                    break;
                case "Genre":
                    sortedList = lesLivres.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesLivres.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesLivres.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirLivresListe(sortedList);
        }
        #endregion

        #region Onglet Dvd

        /// <summary>
        /// Ouverture de l'onglet Dvds : 
        /// appel des méthodes pour remplir le datagrid des dvd et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabDvd_Enter(object sender, EventArgs e)
        {
            lesDvd = controller.GetAllDvd();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxDvdGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxDvdPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxDvdRayons);
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="Dvds">liste de dvd</param>
        private void RemplirDvdListe(List<Dvd> Dvds)
        {
            bdgDvdListe.DataSource = Dvds;
            dgvDvdListe.DataSource = bdgDvdListe;
            dgvDvdListe.Columns["idRayon"].Visible = false;
            dgvDvdListe.Columns["idGenre"].Visible = false;
            dgvDvdListe.Columns["idPublic"].Visible = false;
            dgvDvdListe.Columns["image"].Visible = false;
            dgvDvdListe.Columns["synopsis"].Visible = false;
            dgvDvdListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDvdListe.Columns["id"].DisplayIndex = 0;
            dgvDvdListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage du Dvd dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbDvdNumRecherche.Text.Equals(""))
            {
                txbDvdTitreRecherche.Text = "";
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
                Dvd dvd = lesDvd.Find(x => x.Id.Equals(txbDvdNumRecherche.Text));
                if (dvd != null)
                {
                    List<Dvd> Dvd = new List<Dvd>() { dvd };
                    RemplirDvdListe(Dvd);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirDvdListeComplete();
                }
            }
            else
            {
                RemplirDvdListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des Dvd dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbDvdTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbDvdTitreRecherche.Text.Equals(""))
            {
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
                txbDvdNumRecherche.Text = "";
                List<Dvd> lesDvdParTitre;
                lesDvdParTitre = lesDvd.FindAll(x => x.Titre.ToLower().Contains(txbDvdTitreRecherche.Text.ToLower()));
                RemplirDvdListe(lesDvdParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxDvdGenres.SelectedIndex < 0 && cbxDvdPublics.SelectedIndex < 0 && cbxDvdRayons.SelectedIndex < 0
                    && txbDvdNumRecherche.Text.Equals(""))
                {
                    RemplirDvdListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations du dvd sélectionné
        /// </summary>
        /// <param name="dvd">le dvd</param>
        private void AfficheDvdInfos(Dvd dvd)
        {
            txbDvdRealisateur.Text = dvd.Realisateur;
            txbDvdSynopsis.Text = dvd.Synopsis;
            txbDvdImage.Text = dvd.Image;
            txbDvdDuree.Text = dvd.Duree.ToString();
            txbDvdNumero.Text = dvd.Id;
            txbDvdGenre.Text = dvd.Genre;
            txbDvdPublic.Text = dvd.Public;
            txbDvdRayon.Text = dvd.Rayon;
            txbDvdTitre.Text = dvd.Titre;
            string image = dvd.Image;
            try
            {
                pcbDvdImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbDvdImage.Image = null;
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations du dvd
        /// </summary>
        private void VideDvdInfos()
        {
            txbDvdRealisateur.Text = "";
            txbDvdSynopsis.Text = "";
            txbDvdImage.Text = "";
            txbDvdDuree.Text = "";
            txbDvdNumero.Text = "";
            txbDvdGenre.Text = "";
            txbDvdPublic.Text = "";
            txbDvdRayon.Text = "";
            txbDvdTitre.Text = "";
            pcbDvdImage.Image = null;
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdGenres.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Genre genre = (Genre)cbxDvdGenres.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdPublics.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Public lePublic = (Public)cbxDvdPublics.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdRayons.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxDvdRayons.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations du dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDvdListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDvdListe.CurrentCell != null)
            {
                try
                {
                    Dvd dvd = (Dvd)bdgDvdListe.List[bdgDvdListe.Position];
                    AfficheDvdInfos(dvd);
                }
                catch
                {
                    VideDvdZones();
                }
            }
            else
            {
                VideDvdInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des Dvd
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirDvdListeComplete()
        {
            RemplirDvdListe(lesDvd);
            VideDvdZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideDvdZones()
        {
            cbxDvdGenres.SelectedIndex = -1;
            cbxDvdRayons.SelectedIndex = -1;
            cbxDvdPublics.SelectedIndex = -1;
            txbDvdNumRecherche.Text = "";
            txbDvdTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDvdListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideDvdZones();
            string titreColonne = dgvDvdListe.Columns[e.ColumnIndex].HeaderText;
            List<Dvd> sortedList = new List<Dvd>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesDvd.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesDvd.OrderBy(o => o.Titre).ToList();
                    break;
                case "Duree":
                    sortedList = lesDvd.OrderBy(o => o.Duree).ToList();
                    break;
                case "Realisateur":
                    sortedList = lesDvd.OrderBy(o => o.Realisateur).ToList();
                    break;
                case "Genre":
                    sortedList = lesDvd.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesDvd.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesDvd.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirDvdListe(sortedList);
        }
        #endregion

        #region Onglet Revues

        /// <summary>
        /// Ouverture de l'onglet Revues : 
        /// appel des méthodes pour remplir le datagrid des revues et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabRevues_Enter(object sender, EventArgs e)
        {
            lesRevues = controller.GetAllRevues();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxRevuesGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxRevuesPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxRevuesRayons);
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="revues"></param>
        private void RemplirRevuesListe(List<Revue> revues)
        {
            bdgRevuesListe.DataSource = revues;
            dgvRevuesListe.DataSource = bdgRevuesListe;
            dgvRevuesListe.Columns["idRayon"].Visible = false;
            dgvRevuesListe.Columns["idGenre"].Visible = false;
            dgvRevuesListe.Columns["idPublic"].Visible = false;
            dgvRevuesListe.Columns["image"].Visible = false;
            dgvRevuesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvRevuesListe.Columns["id"].DisplayIndex = 0;
            dgvRevuesListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage de la revue dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbRevuesNumRecherche.Text.Equals(""))
            {
                txbRevuesTitreRecherche.Text = "";
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
                Revue revue = lesRevues.Find(x => x.Id.Equals(txbRevuesNumRecherche.Text));
                if (revue != null)
                {
                    List<Revue> revues = new List<Revue>() { revue };
                    RemplirRevuesListe(revues);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirRevuesListeComplete();
                }
            }
            else
            {
                RemplirRevuesListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des revues dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbRevuesTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbRevuesTitreRecherche.Text.Equals(""))
            {
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
                txbRevuesNumRecherche.Text = "";
                List<Revue> lesRevuesParTitre;
                lesRevuesParTitre = lesRevues.FindAll(x => x.Titre.ToLower().Contains(txbRevuesTitreRecherche.Text.ToLower()));
                RemplirRevuesListe(lesRevuesParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxRevuesGenres.SelectedIndex < 0 && cbxRevuesPublics.SelectedIndex < 0 && cbxRevuesRayons.SelectedIndex < 0
                    && txbRevuesNumRecherche.Text.Equals(""))
                {
                    RemplirRevuesListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations de la revue sélectionné
        /// </summary>
        /// <param name="revue">la revue</param>
        private void AfficheRevuesInfos(Revue revue)
        {
            txbRevuesPeriodicite.Text = revue.Periodicite;
            txbRevuesImage.Text = revue.Image;
            txbRevuesDateMiseADispo.Text = revue.DelaiMiseADispo.ToString();
            txbRevuesNumero.Text = revue.Id;
            txbRevuesGenre.Text = revue.Genre;
            txbRevuesPublic.Text = revue.Public;
            txbRevuesRayon.Text = revue.Rayon;
            txbRevuesTitre.Text = revue.Titre;
            string image = revue.Image;
            try
            {
                pcbRevuesImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbRevuesImage.Image = null;
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations de la reuve
        /// </summary>
        private void VideRevuesInfos()
        {
            txbRevuesPeriodicite.Text = "";
            txbRevuesImage.Text = "";
            txbRevuesDateMiseADispo.Text = "";
            txbRevuesNumero.Text = "";
            txbRevuesGenre.Text = "";
            txbRevuesPublic.Text = "";
            txbRevuesRayon.Text = "";
            txbRevuesTitre.Text = "";
            pcbRevuesImage.Image = null;
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesGenres.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Genre genre = (Genre)cbxRevuesGenres.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesPublics.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Public lePublic = (Public)cbxRevuesPublics.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesRayons.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxRevuesRayons.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations de la revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvRevuesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRevuesListe.CurrentCell != null)
            {
                try
                {
                    Revue revue = (Revue)bdgRevuesListe.List[bdgRevuesListe.Position];
                    AfficheRevuesInfos(revue);
                }
                catch
                {
                    VideRevuesZones();
                }
            }
            else
            {
                VideRevuesInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des revues
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirRevuesListeComplete()
        {
            RemplirRevuesListe(lesRevues);
            VideRevuesZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideRevuesZones()
        {
            cbxRevuesGenres.SelectedIndex = -1;
            cbxRevuesRayons.SelectedIndex = -1;
            cbxRevuesPublics.SelectedIndex = -1;
            txbRevuesNumRecherche.Text = "";
            txbRevuesTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvRevuesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideRevuesZones();
            string titreColonne = dgvRevuesListe.Columns[e.ColumnIndex].HeaderText;
            List<Revue> sortedList = new List<Revue>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesRevues.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesRevues.OrderBy(o => o.Titre).ToList();
                    break;
                case "Periodicite":
                    sortedList = lesRevues.OrderBy(o => o.Periodicite).ToList();
                    break;
                case "DelaiMiseADispo":
                    sortedList = lesRevues.OrderBy(o => o.DelaiMiseADispo).ToList();
                    break;
                case "Genre":
                    sortedList = lesRevues.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesRevues.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesRevues.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirRevuesListe(sortedList);
        }
        #endregion

        #region Onglet Paarutions

        /// <summary>
        /// Ouverture de l'onglet : récupère le revues et vide tous les champs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabReceptionRevue_Enter(object sender, EventArgs e)
        {
            lesRevues = controller.GetAllRevues();
            txbReceptionRevueNumero.Text = "";
        }

        /// <summary>
        /// Remplit le dategrid des exemplaires avec la liste reçue en paramètre
        /// </summary>
        /// <param name="exemplaires">liste d'exemplaires</param>
        private void RemplirReceptionExemplairesListe(List<Exemplaire> exemplaires)
        {
            if (exemplaires != null)
            {
                bdgExemplairesListe.DataSource = exemplaires;
                dgvReceptionExemplairesListe.DataSource = bdgExemplairesListe;
                dgvReceptionExemplairesListe.Columns["idEtat"].Visible = false;
                dgvReceptionExemplairesListe.Columns["id"].Visible = false;
                dgvReceptionExemplairesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvReceptionExemplairesListe.Columns["numero"].DisplayIndex = 0;
                dgvReceptionExemplairesListe.Columns["dateAchat"].DisplayIndex = 1;
            }
            else
            {
                bdgExemplairesListe.DataSource = null;
            }
        }

        /// <summary>
        /// Recherche d'un numéro de revue et affiche ses informations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionRechercher_Click(object sender, EventArgs e)
        {
            if (!txbReceptionRevueNumero.Text.Equals(""))
            {
                Revue revue = lesRevues.Find(x => x.Id.Equals(txbReceptionRevueNumero.Text));
                if (revue != null)
                {
                    AfficheReceptionRevueInfos(revue);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                }
            }
        }

        /// <summary>
        /// Si le numéro de revue est modifié, la zone de l'exemplaire est vidée et inactive
        /// les informations de la revue son aussi effacées
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbReceptionRevueNumero_TextChanged(object sender, EventArgs e)
        {
            txbReceptionRevuePeriodicite.Text = "";
            txbReceptionRevueImage.Text = "";
            txbReceptionRevueDelaiMiseADispo.Text = "";
            txbReceptionRevueGenre.Text = "";
            txbReceptionRevuePublic.Text = "";
            txbReceptionRevueRayon.Text = "";
            txbReceptionRevueTitre.Text = "";
            pcbReceptionRevueImage.Image = null;
            RemplirReceptionExemplairesListe(null);
            AccesReceptionExemplaireGroupBox(false);
        }

        /// <summary>
        /// Affichage des informations de la revue sélectionnée et les exemplaires
        /// </summary>
        /// <param name="revue">la revue</param>
        private void AfficheReceptionRevueInfos(Revue revue)
        {
            // informations sur la revue
            txbReceptionRevuePeriodicite.Text = revue.Periodicite;
            txbReceptionRevueImage.Text = revue.Image;
            txbReceptionRevueDelaiMiseADispo.Text = revue.DelaiMiseADispo.ToString();
            txbReceptionRevueNumero.Text = revue.Id;
            txbReceptionRevueGenre.Text = revue.Genre;
            txbReceptionRevuePublic.Text = revue.Public;
            txbReceptionRevueRayon.Text = revue.Rayon;
            txbReceptionRevueTitre.Text = revue.Titre;
            string image = revue.Image;
            try
            {
                pcbReceptionRevueImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbReceptionRevueImage.Image = null;
            }
            // affiche la liste des exemplaires de la revue
            AfficheReceptionExemplairesRevue();
        }

        /// <summary>
        /// Récupère et affiche les exemplaires d'une revue
        /// </summary>
        private void AfficheReceptionExemplairesRevue()
        {
            string idDocuement = txbReceptionRevueNumero.Text;
            lesExemplaires = controller.GetExemplairesRevue(idDocuement);
            RemplirReceptionExemplairesListe(lesExemplaires);
            AccesReceptionExemplaireGroupBox(true);
        }

        /// <summary>
        /// Permet ou interdit l'accès à la gestion de la réception d'un exemplaire
        /// et vide les objets graphiques
        /// </summary>
        /// <param name="acces">true ou false</param>
        private void AccesReceptionExemplaireGroupBox(bool acces)
        {
            grpReceptionExemplaire.Enabled = acces;
            txbReceptionExemplaireImage.Text = "";
            txbReceptionExemplaireNumero.Text = "";
            pcbReceptionExemplaireImage.Image = null;
            dtpReceptionExemplaireDate.Value = DateTime.Now;
        }

        /// <summary>
        /// Recherche image sur disque (pour l'exemplaire à insérer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionExemplaireImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                // positionnement à la racine du disque où se trouve le dossier actuel
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = "Files|*.jpg;*.bmp;*.jpeg;*.png;*.gif"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            txbReceptionExemplaireImage.Text = filePath;
            try
            {
                pcbReceptionExemplaireImage.Image = Image.FromFile(filePath);
            }
            catch
            {
                pcbReceptionExemplaireImage.Image = null;
            }
        }

        /// <summary>
        /// Enregistrement du nouvel exemplaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionExemplaireValider_Click(object sender, EventArgs e)
        {
            if (!txbReceptionExemplaireNumero.Text.Equals(""))
            {
                try
                {
                    int numero = int.Parse(txbReceptionExemplaireNumero.Text);
                    DateTime dateAchat = dtpReceptionExemplaireDate.Value;
                    string photo = txbReceptionExemplaireImage.Text;
                    string idEtat = ETATNEUF;
                    string idDocument = txbReceptionRevueNumero.Text;
                    Exemplaire exemplaire = new Exemplaire(numero, dateAchat, photo, idEtat, idDocument);
                    if (controller.CreerExemplaire(exemplaire))
                    {
                        AfficheReceptionExemplairesRevue();
                    }
                    else
                    {
                        MessageBox.Show("numéro de publication déjà existant", "Erreur");
                    }
                }
                catch
                {
                    MessageBox.Show("le numéro de parution doit être numérique", "Information");
                    txbReceptionExemplaireNumero.Text = "";
                    txbReceptionExemplaireNumero.Focus();
                }
            }
            else
            {
                MessageBox.Show("numéro de parution obligatoire", "Information");
            }
        }

        /// <summary>
        /// Tri sur une colonne
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvExemplairesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvReceptionExemplairesListe.Columns[e.ColumnIndex].HeaderText;
            List<Exemplaire> sortedList = new List<Exemplaire>();
            switch (titreColonne)
            {
                case "Numero":
                    sortedList = lesExemplaires.OrderBy(o => o.Numero).Reverse().ToList();
                    break;
                case "DateAchat":
                    sortedList = lesExemplaires.OrderBy(o => o.DateAchat).Reverse().ToList();
                    break;
                case "Photo":
                    sortedList = lesExemplaires.OrderBy(o => o.Photo).ToList();
                    break;
            }
            RemplirReceptionExemplairesListe(sortedList);
        }

        /// <summary>
        /// affichage de l'image de l'exemplaire suite à la sélection d'un exemplaire dans la liste
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvReceptionExemplairesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvReceptionExemplairesListe.CurrentCell != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesListe.List[bdgExemplairesListe.Position];
                string image = exemplaire.Photo;
                try
                {
                    pcbReceptionExemplaireRevueImage.Image = Image.FromFile(image);
                }
                catch
                {
                    pcbReceptionExemplaireRevueImage.Image = null;
                }
            }
            else
            {
                pcbReceptionExemplaireRevueImage.Image = null;
            }
        }
        #endregion

        #region Onglet Commandes Livre

        /// <summary>
        /// Recherche et affichage du livre dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCLNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbCLNumRecherche.Text.Equals(""))
            {
                Livre livre = lesLivres.Find(x => x.Id.Equals(txbCLNumRecherche.Text));
                if (livre != null)
                {
                    AfficheCLInfos(livre);
                }
                else
                {
                    VideCLLivreInfos();
                    MessageBox.Show("numéro introuvable");
                }
            }
            else
            {
                VideCLLivreInfos();
            }
        }

        /// <summary>
        /// Affichage des informations du livre sélectionné
        /// </summary>
        /// <param name="livre"></param>
        private void AfficheCLInfos(Livre livre)
        {
            txbCLAuteur.Text = livre.Auteur;
            txbCLCollection.Text = livre.Collection;
            txbCLImage.Text = livre.Image;
            txbCLIsbn.Text = livre.Isbn;
            txbCLGenre.Text = livre.Genre;
            txbCLPublic.Text = livre.Public;
            txbCLRayon.Text = livre.Rayon;
            txbCLTitre.Text = livre.Titre;
            string image = livre.Image;
            try
            {
                pcbCLImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbCLImage.Image = null;
            }
            accesCommandeLivreGroupBox(true);
            afficheCommandesLivres();
        }

        /// <summary>
        /// Permet d'afficher la liste des commandes de livre
        /// </summary>
        private void afficheCommandesLivres()
        {
            string idDocument = txbCLNumRecherche.Text;
            lesCommandesLivresDvd = controller.GetCommandesLivreDvd(idDocument);
            RemplirCommandesLivresListe(lesCommandesLivresDvd);
        }

        /// <summary>
        /// Remplit le datagrid avec la liste reçue en paramètre
        /// </summary>
        private void RemplirCommandesLivresListe(List<CommandeDocument> lesCommandes)
        {
            bdgCLListe.DataSource = lesCommandes;
            dgvCLListe.DataSource = bdgCLListe;
            dgvCLListe.Columns["id"].Visible = false;
            dgvCLListe.Columns["idSuivi"].Visible = false;
            dgvCLListe.Columns["idLivreDvd"].Visible = false;
            dgvCLListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvCLListe.Columns["dateCommande"].DisplayIndex = 0;
            dgvCLListe.Columns["montant"].DisplayIndex = 1;
            dgvCLListe.Columns["suivi"].DisplayIndex = 2;
            dgvCLListe.Columns["nbExemplaire"].DisplayIndex = 3;
        }

        /// <summary>
        /// Ouverture de l'onglet : blocage en saisie des champs de saisie des infos de
        /// la commande d'un livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabCommandesLivres_Enter(object sender, EventArgs e)
        {
            lesLivres = controller.GetAllLivres();
            accesCommandeLivreGroupBox(false);
            RemplirComboSuivi(controller.GetAllSuivis(), bdgSuivi, cbxEtapeSuiviCL, null);
        }

        /// <summary>
        /// Permet ou interdit l'accès à la gestion de la commande d'un livre
        /// et vide les objets graphiques
        /// </summary>
        /// <param name="acces"></param>
        private void accesCommandeLivreGroupBox(bool acces)
        {
            VideCommandesLivreInfos();
            grpCommandeLivre.Enabled = acces;
        }

        /// <summary>
        /// Vide les zones d'affchage des informations du livre
        /// </summary>
        private void VideCLLivreInfos()
        {
            txbCLTitre.Text = "";
            txbCLAuteur.Text = "";
            txbCLCollection.Text = "";
            txbCLGenre.Text = "";
            txbCLPublic.Text = "";
            txbCLRayon.Text = "";
            txbCLIsbn.Text = "";
            txbCLImage.Text = "";
            pcbCLImage.Image = null;
            lesCommandesLivresDvd = new List<CommandeDocument>();
            RemplirCommandesLivresListe(lesCommandesLivresDvd);
            accesCommandeLivreGroupBox(false);
        }

        /// <summary>
        /// Vide les zones d'affchage des informations de la commande du livre
        /// </summary>
        private void VideCommandesLivreInfos()
        {
            txbMontantCL.Text = "";
            txbNbExemplaireCL.Text = "";
            dtpDateCL.Value = DateTime.Now;
            txbNumCL.Text = "";
            cbxEtapeSuiviCL.Enabled = false;
        }

        /// <summary>
        /// Si le numéro de livre est modifié, la zone de la commande est vidée et inactive
        /// les informations du livre son aussi effacées
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbCLNumRecherche_TextChanged(object sender, EventArgs e)
        {
            accesCommandeLivreGroupBox(false);
            VideCLLivreInfos();
        }

        /// <summary>
        /// Enregistrement d'une commande de livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnValiderCL_Click(object sender, EventArgs e)
        {
            if (!txbNumCL.Text.Equals(""))
            {
                try
                {
                    string id = txbNumCL.Text;
                    DateTime dateCommande = dtpDateCL.Value;
                    string montantStr = txbMontantCL.Text;
                    double montant = double.Parse(montantStr);
                    string suivi = "en cours";
                    string idSuivi = "1";
                    string nbExemplaireStr = txbNbExemplaireCL.Text;
                    int nbExemplaire = int.Parse(nbExemplaireStr);
                    string idLivreDvd = txbCLNumRecherche.Text;
                    CommandeDocument commande = new CommandeDocument(id, dateCommande, montant, idSuivi, suivi, nbExemplaire, idLivreDvd);
                    if (controller.CreerCommandeLivreDvd(commande))
                    {
                        VideCommandesLivreInfos();
                        afficheCommandesLivres();
                    }
                    else
                    {
                        MessageBox.Show("numéro de commande déjà existant", "Erreur");
                    }
                }
                catch
                {
                    MessageBox.Show("le montant et le nombre d'exemplaire doivent être numériques", "Information");
                    txbMontantCL.Text = "";
                    txbNbExemplaireCL.Text = "";
                    txbMontantCL.Focus();
                }
            }
            else
            {
                MessageBox.Show("numéro de livre obligatoire", "Information");
            }
        }

        /// <summary>
        /// Sélection d'une ligne complète d'une commande de livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCLListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCLListe.CurrentCell != null)
            {
                CommandeDocument commande = (CommandeDocument)bdgCLListe.List[bdgCLListe.Position];
                afficheCommandesLivresInfos(commande);
            }
        }

        /// <summary>
        /// Affiche les informations de la commande passée en paramètre
        /// </summary>
        /// <param name="commande">Objet de type CommandeDocument</param>
        private void afficheCommandesLivresInfos(CommandeDocument commande)
        {
            dtpDateCL.Value = commande.DateCommande;
            txbMontantCL.Text = (commande.Montant).ToString();
            txbNbExemplaireCL.Text = (commande.NbExemplaire).ToString();
            cbxEtapeSuiviCL.Text = commande.Suivi;
            cbxEtapeSuiviCL.Enabled = true;
            txbNumCL.Text = commande.Id;
            RemplirComboSuivi(controller.GetAllSuivis(), bdgSuivi, cbxEtapeSuiviCL, commande.Suivi);
        }

        /// <summary>
        /// Mise à jour du suivi d'une commande de livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateCL_Click(object sender, EventArgs e)
        {
            if (bdgCLListe.Count != 0)
            {
                string id = txbNumCL.Text;
                DateTime dateCommande = dtpDateCL.Value;
                string montantStr = txbMontantCL.Text;
                double montant = double.Parse(montantStr);
                string suivi = cbxEtapeSuiviCL.Text;
                string idSuivi = getIdSuivi(cbxEtapeSuiviCL.Text);
                string nbExemplaireStr = txbNbExemplaireCL.Text;
                int nbExemplaire = int.Parse(nbExemplaireStr);
                string idLivreDvd = txbCLNumRecherche.Text;
                CommandeDocument commande = new CommandeDocument(id, dateCommande, montant, idSuivi, suivi, nbExemplaire, idLivreDvd);
                if (controller.UpdateCommande(commande))
                {
                    VideCommandesLivreInfos();
                    afficheCommandesLivres();
                }
                else
                {
                    MessageBox.Show("commande invalide", "Erreur");
                }
            }
            else
            {
                MessageBox.Show("liste vide", "Information");
            }
        }

        /// <summary>
        /// Supprime une commande de livre si elle n'est pas livrée ou réglée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelCL_Click(object sender, EventArgs e)
        {
            if (bdgCLListe.Count != 0)
            {
                CommandeDocument commande = (CommandeDocument)bdgCLListe.List[bdgCLListe.Position];
                if (commande.IdSuivi.Equals("1") || commande.IdSuivi.Equals("4"))
                {
                    controller.DeleteCommandeLivreDvd(commande);
                }
                VideCommandesLivreInfos();
                afficheCommandesLivres();
            }
            else
            {
                MessageBox.Show("liste vide", "Information");
            }
        }

        #endregion

        #region Commandes DVD

        /// <summary>
        /// Recherche et affichage du DVD dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCDNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbCDNumRecherche.Text.Equals(""))
            {
                Dvd dvd = lesDvd.Find(x => x.Id.Equals(txbCDNumRecherche.Text));
                if (dvd != null)
                {
                    AfficheCDInfos(dvd);
                }
                else
                {
                    VideCDDvdInfos();
                    MessageBox.Show("numéro introuvable");
                }
            }
            else
            {
                VideCDDvdInfos();
            }
        }

        /// <summary>
        /// Affichage des informations du DVD sélectionné
        /// </summary>
        /// <param name="dvd"></param>
        private void AfficheCDInfos(Dvd dvd)
        {
            txbCDRealisateur.Text = dvd.Realisateur;
            txbCDSynopsis.Text = dvd.Synopsis;
            txbCDImage.Text = dvd.Image;
            txbCDDuree.Text = (dvd.Duree).ToString();
            txbCDGenre.Text = dvd.Genre;
            txbCDPublic.Text = dvd.Public;
            txbCDRayon.Text = dvd.Rayon;
            txbCDTitre.Text = dvd.Titre;
            string image = dvd.Image;
            try
            {
                pcbCDImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbCDImage.Image = null;
            }
            accesCommandeDvdGroupBox(true);
            afficheCommandesDvd();
        }

        /// <summary>
        /// Permet d'afficher la liste des commandes de DVD
        /// </summary>
        private void afficheCommandesDvd()
        {
            string idDocument = txbCDNumRecherche.Text;
            lesCommandesLivresDvd = controller.GetCommandesLivreDvd(idDocument);
            RemplirCommandesDvdListe(lesCommandesLivresDvd);
        }

        /// <summary>
        /// Remplit le datagrid avec la liste reçue en paramètre
        /// </summary>
        private void RemplirCommandesDvdListe(List<CommandeDocument> lesCommandes)
        {
            bdgCDListe.DataSource = lesCommandes;
            dgvCDListe.DataSource = bdgCDListe;
            dgvCDListe.Columns["id"].Visible = false;
            dgvCDListe.Columns["idSuivi"].Visible = false;
            dgvCDListe.Columns["idLivreDvd"].Visible = false;
            dgvCDListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvCDListe.Columns["dateCommande"].DisplayIndex = 0;
            dgvCDListe.Columns["montant"].DisplayIndex = 1;
            dgvCDListe.Columns["suivi"].DisplayIndex = 2;
            dgvCDListe.Columns["nbExemplaire"].DisplayIndex = 3;
        }

        /// <summary>
        /// Ouverture de l'onglet : blocage en saisie des champs de saisie des infos de
        /// la commande d'un DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabCommandesDvd_Enter(object sender, EventArgs e)
        {
            lesDvd = controller.GetAllDvd();
            accesCommandeDvdGroupBox(false);
            RemplirComboSuivi(controller.GetAllSuivis(), bdgSuivi, cbxEtapeSuiviCD, null);
        }

        /// <summary>
        /// Permet ou interdit l'accès à la gestion de la commande d'un DVD
        /// et vide les objets graphiques
        /// </summary>
        /// <param name="acces"></param>
        private void accesCommandeDvdGroupBox(bool acces)
        {
            VideCommandesDvdInfos();
            grpCommandeDvd.Enabled = acces;
        }

        /// <summary>
        /// Vide les zones d'affchage des informations du DVD
        /// </summary>
        private void VideCDDvdInfos()
        {
            txbCDTitre.Text = "";
            txbCDRealisateur.Text = "";
            txbCDSynopsis.Text = "";
            txbCDGenre.Text = "";
            txbCDPublic.Text = "";
            txbCDRayon.Text = "";
            txbCDDuree.Text = "";
            txbCDImage.Text = "";
            pcbCDImage.Image = null;
            lesCommandesLivresDvd = new List<CommandeDocument>();
            RemplirCommandesDvdListe(lesCommandesLivresDvd);
            accesCommandeDvdGroupBox(false);
        }

        /// <summary>
        /// Vide les zones d'affchage des informations de la commande du DVD
        /// </summary>
        private void VideCommandesDvdInfos()
        {
            txbMontantCD.Text = "";
            txbNbExemplaireCD.Text = "";
            dtpDateCD.Value = DateTime.Now;
            txbNumCD.Text = "";
            cbxEtapeSuiviCD.Enabled = false;
        }

        /// <summary>
        /// Si le numéro de DVD est modifié, la zone de la commande est vidée et inactive
        /// les informations du DVD son aussi effacées
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbCDNumRecherche_TextChanged(object sender, EventArgs e)
        {
            accesCommandeDvdGroupBox(false);
            VideCDDvdInfos();
        }

        /// <summary>
        /// Enregistrement d'une commande de DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnValiderCD_Click(object sender, EventArgs e)
        {
            if (!txbNumCD.Text.Equals(""))
            {
                try
                {
                    string id = txbNumCD.Text;
                    DateTime dateCommande = dtpDateCD.Value;
                    string montantStr = txbMontantCD.Text;
                    double montant = double.Parse(montantStr);
                    string suivi = "en cours";
                    string idSuivi = "1";
                    string nbExemplaireStr = txbNbExemplaireCD.Text;
                    int nbExemplaire = int.Parse(nbExemplaireStr);
                    string idLivreDvd = txbCDNumRecherche.Text;
                    CommandeDocument commande = new CommandeDocument(id, dateCommande, montant, idSuivi, suivi, nbExemplaire, idLivreDvd);
                    if (controller.CreerCommandeLivreDvd(commande))
                    {
                        VideCommandesDvdInfos();
                        afficheCommandesDvd();
                    }
                    else
                    {
                        MessageBox.Show("numéro de commande déjà existant", "Erreur");
                    }
                }
                catch
                {
                    MessageBox.Show("le montant et le nombre d'exemplaire doivent être numériques", "Information");
                    txbMontantCD.Text = "";
                    txbNbExemplaireCD.Text = "";
                    txbMontantCD.Focus();
                }
            }
            else
            {
                MessageBox.Show("numéro de DVD obligatoire", "Information");
            }
        }

        /// <summary>
        /// Sélection d'une ligne complète d'une commande de DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCDListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCDListe.CurrentCell != null)
            {
                CommandeDocument commande = (CommandeDocument)bdgCDListe.List[bdgCDListe.Position];
                afficheCommandesDvdInfos(commande);
            }
        }

        /// <summary>
        /// Affiche les informations de la commande passée en paramètre
        /// </summary>
        /// <param name="commande">Objet de type CommandeDocument</param>
        private void afficheCommandesDvdInfos(CommandeDocument commande)
        {
            dtpDateCD.Value = commande.DateCommande;
            txbMontantCD.Text = (commande.Montant).ToString();
            txbNbExemplaireCD.Text = (commande.NbExemplaire).ToString();
            cbxEtapeSuiviCD.Text = commande.Suivi;
            cbxEtapeSuiviCD.Enabled = true;
            txbNumCD.Text = commande.Id;
            RemplirComboSuivi(controller.GetAllSuivis(), bdgSuivi, cbxEtapeSuiviCD, commande.Suivi);
        }

        /// <summary>
        /// Mise à jour du suivi d'une commande de DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateCD_Click(object sender, EventArgs e)
        {
            if (bdgCDListe.Count != 0)
            {
                string id = txbNumCD.Text;
                DateTime dateCommande = dtpDateCD.Value;
                string montantStr = txbMontantCD.Text;
                double montant = double.Parse(montantStr);
                string suivi = cbxEtapeSuiviCD.Text;
                string idSuivi = getIdSuivi(cbxEtapeSuiviCD.Text);
                string nbExemplaireStr = txbNbExemplaireCD.Text;
                int nbExemplaire = int.Parse(nbExemplaireStr);
                string idLivreDvd = txbCDNumRecherche.Text;
                CommandeDocument commande = new CommandeDocument(id, dateCommande, montant, idSuivi, suivi, nbExemplaire, idLivreDvd);
                if (controller.UpdateCommande(commande))
                {
                    VideCommandesDvdInfos();
                    afficheCommandesDvd();
                }
                else
                {
                    MessageBox.Show("commande invalide", "Erreur");
                }
            }
            else
            {
                MessageBox.Show("liste vide", "Information");
            }
        }

        /// <summary>
        /// Supprime une commande de DVD si elle n'est pas livrée ou réglée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelCD_Click(object sender, EventArgs e)
        {
            if (bdgCDListe.Count != 0)
            {
                CommandeDocument commande = (CommandeDocument)bdgCDListe.List[bdgCDListe.Position];
                if (commande.IdSuivi.Equals("1") || commande.IdSuivi.Equals("4"))
                {
                    controller.DeleteCommandeLivreDvd(commande);
                }
                VideCommandesDvdInfos();
                afficheCommandesDvd();
            }
            else
            {
                MessageBox.Show("liste vide", "Information");
            }
        }

        #endregion
        #region Commandes Revue

        /// <summary>
        /// Recherche et affichage de la revue dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCRNumRecherche_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Affichage des informations de la revue sélectionnée
        /// </summary>
        /// <param name="revue"></param>
        private void AfficheCRInfos(Revue revue)
        {
            txbCRPeriodicite.Text = revue.Periodicite;
            txbCRDispo.Text = (revue.DelaiMiseADispo).ToString();
            txbCRImage.Text = revue.Image;
            chkCREmpruntable.Checked = revue.Empruntable;
            txbCRGenre.Text = revue.Genre;
            txbCRPublic.Text = revue.Public;
            txbCRRayon.Text = revue.Rayon;
            txbCRTitre.Text = revue.Titre;
            string image = revue.Image;
            try
            {
                pcbCRImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbCRImage.Image = null;
            }
            accesCommandeRevueGroupBox(true);
            afficheCommandesRevues();
        }

        /// <summary>
        /// Permet d'afficher la liste des commandes de revues
        /// </summary>
        private void afficheCommandesRevues()
        {
            string idDocument = txbCRNumRecherche.Text;
            lesCommandesRevues = controller.GetCommandesRevues(idDocument);
            RemplirCommandesRevuesListe(lesCommandesRevues);
        }

        /// <summary>
        /// Remplit le datagrid avec la liste reçue en paramètre
        /// </summary>
        private void RemplirCommandesRevuesListe(List<Abonnement> lesCommandes)
        {
            bdgCRListe.DataSource = lesCommandes;
            dgvCRListe.DataSource = bdgCRListe;
            dgvCRListe.Columns["id"].Visible = false;
            dgvCRListe.Columns["idRevue"].Visible = false;
            dgvCRListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvCRListe.Columns["dateCommande"].DisplayIndex = 0;
            dgvCRListe.Columns["montant"].DisplayIndex = 1;
            dgvCRListe.Columns["dateFinAbonnement"].DisplayIndex = 2;
        }

        /// <summary>
        /// Ouverture de l'onglet : blocage en saisie des champs de saisie des infos de
        /// la commande d'une revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabCommandesRevues_Enter(object sender, EventArgs e)
        {
            lesRevues = controller.GetAllRevues();
            accesCommandeRevueGroupBox(false);
        }

        /// <summary>
        /// Permet ou interdit l'accès à la gestion de la commande d'une revue
        /// et vide les objets graphiques
        /// </summary>
        /// <param name="acces"></param>
        private void accesCommandeRevueGroupBox(bool acces)
        {
            VideCommandesRevuesInfos();
            grpCommandeRevue.Enabled = acces;
        }

        /// <summary>
        /// Vide les zones d'affchage des informations de la revue
        /// </summary>
        private void VideCRRevueInfos()
        {
            txbCRTitre.Text = "";
            txbCRPeriodicite.Text = "";
            txbCRDispo.Text = "";
            txbCRGenre.Text = "";
            txbCRPublic.Text = "";
            txbCRRayon.Text = "";
            chkCREmpruntable.Checked = false;
            txbCRImage.Text = "";
            pcbCRImage.Image = null;
            lesCommandesRevues = new List<Abonnement>();
            RemplirCommandesRevuesListe(lesCommandesRevues);
            accesCommandeRevueGroupBox(false);
        }

        /// <summary>
        /// Vide les zones d'affchage des informations de la commande de la revue
        /// </summary>
        private void VideCommandesRevuesInfos()
        {
            txbMontantCR.Text = "";
            dtpFinAboCR.Value = DateTime.Now;
            dtpDateCR.Value = DateTime.Now;
            txbNumCR.Text = "";
        }

        /// <summary>
        /// Si le numéro de revue est modifié, la zone de la commande est vidée et inactive
        /// les informations de la revue sont aussi effacées
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbCRNumRecherche_TextChanged(object sender, EventArgs e)
        {
            accesCommandeRevueGroupBox(false);
            VideCRRevueInfos();
        }

        /// <summary>
        /// Enregistrement d'une commande de revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnValiderCR_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Sélection d'une ligne complète d'une commande de revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCRListe_SelectionChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Affiche les informations de la commande passée en paramètre
        /// </summary>
        /// <param name="commande">Objet de type Abonnement</param>
        private void afficheCommandesRevuesInfos(Abonnement commande)
        {
            dtpDateCR.Value = commande.DateCommande;
            txbMontantCR.Text = (commande.Montant).ToString();
            dtpFinAboCR.Value = commande.DateFinAbonnement;
            txbNumCR.Text = commande.Id;
        }

        /// <summary>
        /// Supprime une commande de revue si elle n'a pas au moins un exemplaire associé
        /// compris entre la date de commande et la date de fin d'abonnement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelCR_Click(object sender, EventArgs e)
        {

        }

        #endregion

    }
}
