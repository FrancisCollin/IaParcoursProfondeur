using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Francis Collin, 1738286
/// ParcoursProfondeur
/// ParcoursProfondeur s'occupe de tous les traitements à faire pour arriver à la solution.
/// Elle gère l'application
/// </summary>
public class ParcoursProfondeur : MonoBehaviour
{
    //Variables pour l'affichage du labyrinthe
    public Camera cam;
    public Sprite sprite;
    public int[,] grid;
    [SerializeField] public int columns = 5;
    [SerializeField] public int rows = 5;
    int vertical, horizontal;

    //Création des couleurs des états des cases du labyrinthe
    private Color couleurDepart = new Color(0, 0, 1, 1);
    private Color couleurArriver = new Color(1, 0, 0, 1);
    private Color couleurMur = new Color(0, 0, 0, 1);
    private Color couleurNonVisiter = new Color(1, 1, 1, 1);
    private Color couleurVisiter = new Color(0, 1, 0, 1);
    private Color couleurTrajet = new Color(1, 0.92f, 0.016f, 1);

    //Variables pour les noeuds
    public List<Noeud> Noeuds;
    private Noeud sommet;

    /// <summary>
    /// Start
    /// Start initialise le programme en créant la grille de cases et les noeuds, donne les états de départ des cases,
    /// puis commence l'algorithme de parcours en profondeur.
    /// </summary>
    void Start()
    {
        vertical = (int)cam.orthographicSize;
        horizontal = (int)(vertical * cam.aspect);

        //Initialisation de la grille du labyrinthe
        grid = new int[columns, rows];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                grid[x, y] = 1;
                SpawnTile(x, y);
            }
        }

        //Initialisation des cases départ(2), Arrivé(3), Mur(0)
        grid[0, 4] = 2; ChangeTile(0, 4);
        grid[1, 4] = 0; ChangeTile(1, 4);
        grid[1, 3] = 0; ChangeTile(1, 3);
        grid[1, 1] = 0; ChangeTile(1, 1);
        grid[3, 3] = 0; ChangeTile(3, 3);
        grid[3, 2] = 0; ChangeTile(3, 2);
        grid[3, 1] = 0; ChangeTile(3, 1);
        grid[4, 3] = 3; ChangeTile(4, 3);

        //Création des noeuds pour les cases
        Noeuds = new List<Noeud>();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (grid[x, y] == 0) //Si c'est un mur
                {
                    Noeuds.Add(new Noeud(x, y));
                }
                else if (grid[x, y] == 3) //Si c'est la case arriver
                {
                    Noeuds.Add(new Noeud(x, y, 1, 0, null, true));
                }
                else
                {
                    Noeuds.Add(new Noeud(x, y, 1));
                }
            }
        }

        //Initialisation du sommet départ
        sommet = Noeuds.Find(s => (s.PosX == 0) && (s.PosY == 4));
        ChangeState(sommet);

        //Lance l'algo du parcours en profondeur
        Parcours();
    }

    /// <summary>
    /// SpawnTile
    /// Crée le gameObject de la case à la position [x,y], ajoute un SpriteRenderer à ce gameObject, puis change sa couleur.
    /// </summary>
    /// <param name="x"> Position x dans la grille </param>
    /// <param name="y"> Position y dans la grille </param>
    private void SpawnTile(int x, int y)
    {
        GameObject g = new GameObject("X : " + x + " Y : " + y);
        g.transform.position = new Vector3(x - (horizontal * 0.5f), y - (vertical * 0.5f));

        var s = g.AddComponent<SpriteRenderer>();
        s.sprite = sprite;

        SwitchColor(grid[x, y], s);
    }

    /// <summary>
    /// ChangeTile
    /// Trouve le gameObject à la position [x,y], puis change sa couleur.
    /// </summary>
    /// <param name="x"> Position x dans la grille </param>
    /// <param name="y"> Position y dans la grille </param>
    private void ChangeTile(int x, int y)
    {
        GameObject g = GameObject.Find("X : " + x + " Y : " + y);
        var s = g.GetComponent<SpriteRenderer>();

        SwitchColor(grid[x, y], s);
    }

    /// <summary>
    /// SwitchColor
    /// Change la couleur du SpriteRenderer, selon l'état de la case.
    /// </summary>
    /// <param name="state"> État de la case </param>
    /// <param name="s"> SpriteRenderer du gameObject que l'on veut changer la couleur </param>
    private void SwitchColor(int state, SpriteRenderer s)
    {
        switch (state)
        {
            case 0:
                s.color = couleurMur;
                break;
            case 1:
                s.color = couleurNonVisiter;
                break;
            case 2:
                s.color = couleurDepart;
                break;
            case 3:
                s.color = couleurArriver;
                break;
            case 5:
                s.color = couleurVisiter;
                break;
            case 6:
                s.color = couleurTrajet;
                break;
        }
    }

    /// <summary>
    /// ChangeState
    /// Change l'état d'une case, selon l'état du noeud de cette case.
    /// </summary>
    /// <param name="noeud"> le noeud associer à la case que l'on veux changer </param>
    private void ChangeState(Noeud noeud)
    {
        grid[noeud.PosX, noeud.PosY] = noeud.State;
        ChangeTile(noeud.PosX, noeud.PosY);
    }

    /// <summary>
    /// Parcours
    /// Lance la méthode récursive Visiter pour parcourir les noeuds en profondeur.
    /// </summary>
    private void Parcours()
    {
        Visiter(sommet);
    }

    /// <summary>
    /// TracerTrajet
    /// À partir de la case arriver, temps que le noeud analyser à un père,
    /// change l'état du noeud à trajet,
    /// si le noeud analyser n'à pas de père, c'est donc la case départ donc remet l'état de cette case à départ.
    /// </summary>
    private void TracerTrajet(Noeud head)
    {
        while (head.Pere != null)
        {
            head.State = 6;
            ChangeState(head);
            head = head.Pere;
        }
        if (head.Pere == null)
        {
            head.State = 2;
            ChangeState(head);
        }
    }

    /// <summary>
    /// Visiter
    /// Si la case visiter est l'arriver tracer le trajet,
    /// sinon marquer la case visiter et regarder ses voisins.
    /// </summary>
    /// <param name="n"> La case que l'on veut visiter </param>
    private void Visiter(Noeud n)
    {
        if (n.CaseArriver)
        {
            TracerTrajet(n);
            n.State = 3;
            ChangeState(n);
        }
        else
        {
            n.State = 5;
            ChangeState(n);

            LookRight(n);
            LookDown(n);
            LookLeft(n);
            LookUp(n);
        }
    }

    /// <summary>
    /// TraitementVoisin
    /// Si il y a une voisin et que sont état est nonVisiter,
    /// calcule la distance entre le voisin et la case départ,
    /// ajout le noeud présentement analysé comme étant le père du voisin,
    /// visite le voisin.
    /// </summary>
    /// <param name="voisin"> Noeud du voisin</param>
    /// <param name="head"> Noeud présentement analysé </param>
    private IEnumerator TraitementVoisin(Noeud voisin, Noeud head)
    {
        if (voisin != null)
        {
            if (voisin.State == 1)
            {

                voisin.Distance = head.Distance + 1;
                voisin.Pere = head;

                yield return new WaitForSeconds(1); //Attend 1 seconde
                Visiter(voisin);
            }
        }
    }

    /// <summary>
    /// LookDown
    /// Trouve le noeud en dessous du noeud présentement analysé.
    /// </summary>
    /// <param name="head"> Noeud présentement analysé </param>
    private void LookDown(Noeud head)
    {
        Noeud noeudDown = Noeuds.Find(n => (n.PosX == head.PosX) && (n.PosY == head.PosY - 1));

        StartCoroutine(TraitementVoisin(noeudDown, head));
    }

    /// <summary>
    /// LookLeft
    /// Trouve le noeud à gauche du noeud présentement analysé.
    /// </summary>
    /// <param name="head"> Noeud présentement analysé </param>
    private void LookLeft(Noeud head)
    {
        Noeud noeudLeft = Noeuds.Find(n => (n.PosX == head.PosX - 1) && (n.PosY == head.PosY));

        StartCoroutine(TraitementVoisin(noeudLeft, head));
    }

    /// <summary>
    /// LookUp
    /// Trouve le noeud en haut du noeud présentement analysé.
    /// </summary>
    /// <param name="head"> Noeud présentement analysé </param>
    private void LookUp(Noeud head)
    {
        Noeud noeudUp = Noeuds.Find(n => (n.PosX == head.PosX) && (n.PosY == head.PosY + 1));

        StartCoroutine(TraitementVoisin(noeudUp, head));
    }

    /// <summary>
    /// LookRight
    /// Trouve le noeud à droite du noeud présentement analysé.
    /// </summary>
    /// <param name="head"> Noeud présentement analysé </param>
    private void LookRight(Noeud head)
    {
        Noeud noeudRight = Noeuds.Find(n => (n.PosX == head.PosX + 1) && (n.PosY == head.PosY));

        StartCoroutine(TraitementVoisin(noeudRight, head));
    }
}
