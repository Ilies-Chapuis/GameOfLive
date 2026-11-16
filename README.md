GameOfLive — Le jeu de la vie de Conway sous Unity

Projet réalisé en binôme à La Plateforme (Marseille), dans le cadre du sujet « Vie ou crève ».

Composition du groupe
Membre	Rôle
Melvin Vincent	Moteur de simulation : grille, règles de Conway,  gestion du temps, sauvegarde / chargement PNG
Ilies UI:	modes de bord, interface de test
Description du projet

GameOfLive est une simulation du jeu de la vie imaginé par le mathématicien John Conway en 1970. Il n'y a ni joueur, ni score : on pose un état de départ sur une grille, puis on le regarde évoluer seul, génération après génération, selon trois règles :

une cellule morte avec exactement 3 voisines vivantes naît ;
une cellule vivante avec 2 ou 3 voisines vivantes survit ;
dans tous les autres cas, la cellule meurt ou reste morte.

Les 8 cases autour d'une cellule (voisinage de Moore) sont prises en compte, et toutes les cellules évoluent en même temps.

Fonctionnalités :

grille de taille paramétrable (de 5 × 5 à 500 × 500) ;
deux modes de bord : Infini (l'extérieur de la grille est mort) et Symétrique (la grille se referme sur elle-même) ;
lecture automatique avec vitesse réglable, ou pas-à-pas ;
édition des cellules à la souris, avant et pendant la simulation ;
sauvegarde et chargement de la grille sous forme d'image PNG générée par le code ;
simulation déterministe : même état de départ et même mode de bord donnent toujours le même résultat.

Contraintes techniques : Unity 3D (version gratuite), aucun package supplémentaire.

Architecture : les données et les règles (GridModel, Simulation, IBorderRule) sont écrites en C# pur, sans dépendance à Unity. Elles tournent sans affichage, ce qui permet de les tester automatiquement. Le script Game fait le lien avec Unity (temps, Play / Pause, vitesse), et l'affichage ne fait que lire la grille.

Mode d'emploi
Prérequis
Unity 6.3 LTS (6000.3.1f1), installé via Unity Hub.
Ouvrir le projet
Cloner le dépôt :https://github.com/Ilies-Chapuis/GameOfLive.git

Dans Unity Hub : Ajouter → Ajouter un projet depuis le disque, puis sélectionner le dossier GameOfLive.
Ouvrir le projet. La première ouverture est un peu longue : Unity reconstruit son cache.
Dans la fenêtre Project, ouvrir la scène Assets/Scenes/simu.
Appuyer sur Play, puis cliquer une fois dans la vue Game pour que le clavier soit pris en compte.
Réglages de départ

Sélectionner l'objet Simulation dans la Hierarchy. Le composant Game permet de régler, dans l'Inspector :

la largeur et la hauteur de la grille ;
le mode de bord de départ ;
la vitesse (générations par seconde).
Commandes
Touche	Action
Espace	Play / Pause
Flèche droite	Avancer d'une génération (pas-à-pas)
+ / -	Accélérer / ralentir
B	Changer de mode de bord (Infini / Symétrique)
R	Remplir la grille au hasard
G	Poser un planeur au centre
C	Tout effacer
S	Sauvegarder la grille en PNG
L	Charger la sauvegarde la plus récente
O	Ouvrir le dossier des sauvegardes
Clic gauche	Allumer / éteindre une cellule (maintenir pour peindre)

Convention de sauvegarde PNG

La grille n'est pas sauvegardée dans un fichier de données classique, mais dans une image PNG générée par le code.

Taille de l'image ⇄ taille de la grille

1 pixel = 1 cellule.

Grille	Image
largeur de la grille	largeur de l'image en pixels
hauteur de la grille	hauteur de l'image en pixels

Exemple : une grille de 64 × 36 est sauvegardée dans une image de 64 × 36 pixels. Au chargement, la grille prend automatiquement la taille de l'image.

Couleur ⇄ état
État de la cellule	Couleur à la sauvegarde	Règle au chargement
Vivante	Blanc (255, 255, 255)	luminosité > 127
Morte	Noir (0, 0, 0)	luminosité ≤ 127

La luminosité d'un pixel est la moyenne de ses trois composantes : (rouge + vert + bleu) / 3.

Grâce à ce seuil, le chargement fonctionne aussi avec une image retouchée ou dessinée à la main : tout pixel plutôt clair devient une cellule vivante, tout pixel plutôt foncé une cellule morte.

Orientation

L'image se lit dans le même sens que la grille à l'écran : le pixel en haut à gauche de l'image correspond à la case en haut à gauche de la grille. Dans le code, la case (0, 0) est en bas à gauche, comme dans une texture Unity.

Emplacement des sauvegardes

Les fichiers sont enregistrés dans le sous-dossier Saves de Application.persistentDataPath. Sous Windows :

C:\Users\<utilisateur>\AppData\LocalLow\<entreprise>\GameOfLive\Saves

La touche O ouvre directement ce dossier. Chaque sauvegarde est nommée avec sa date et son heure, par exemple grid_2026-11-16_20-15-42.png.

Ce qui n'est pas sauvegardé

Seul l'état des cellules est enregistré. Le mode de bord et la vitesse ne sont pas stockés dans l'image : au chargement, ce sont les réglages actuels qui sont gardés.

Images PNG d'exemple

Les images d'exemple sont dans le dossier examples/. Ce sont de vraies sauvegardes : 1 pixel par cellule, donc elles sont toutes petites. Les aperçus ci-dessous sont agrandis.

Répartition aléatoire — examples/random_64x36.png

Grille de 64 × 36, environ 30 % de cellules vivantes (663 cellules).

Répartition aléatoire, aperçu agrandi

Planeur (glider) — examples/glider_32x32.png

Grille de 32 × 32 avec un planeur en haut à gauche. Il avance d'une case en diagonale vers le bas à droite toutes les 4 générations. En mode Symétrique, il traverse les bords et revient de l'autre côté ; en mode Infini, il s'écrase contre le bord.

Planeur, aperçu agrandi

Charger une image d'exemple
Appuyer sur O pour ouvrir le dossier des sauvegardes.
Y copier l'image d'exemple, en retirant les autres images du dossier : la touche L charge la sauvegarde la plus récente, et une image copiée garde sa date d'origine.
Revenir dans le jeu et appuyer sur L.

Ressources

Tuto Unity: https://www.youtube.com/playlist?list=PLiRrp7UEG13YiTwVr1wPnYtTvt12ZKg1r
