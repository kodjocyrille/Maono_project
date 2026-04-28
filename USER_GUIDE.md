# Guide Utilisateur — Maono Ops

> Version 1.0 — Avril 2026

---

## Table des matières

1. [Introduction](#1-introduction)
2. [Premiers pas](#2-premiers-pas)
3. [Gestion des campagnes](#3-gestion-des-campagnes)
4. [Calendrier éditorial](#4-calendrier-éditorial)
5. [File de production (Contenus)](#5-file-de-production-contenus)
6. [Tâches de production](#6-tâches-de-production)
7. [Gestion des assets](#7-gestion-des-assets)
8. [Workflow d'approbation](#8-workflow-dapprobation)
9. [Portail client](#9-portail-client)
10. [File de publication](#10-file-de-publication)
11. [Analytics](#11-analytics)
12. [Administration](#12-administration)
13. [Bonnes pratiques éditoriales](#13-bonnes-pratiques-éditoriales)
14. [Raccourcis et astuces](#14-raccourcis-et-astuces)
15. [FAQ](#15-faq)

---

## 1. Introduction

**Maono Ops** est une plateforme de gestion des opérations marketing éditoriaux conçue pour les agences et équipes créatives. Elle centralise l'ensemble du cycle de vie d'un contenu, depuis la conception de la campagne jusqu'à la publication et l'analyse des performances.

### À qui s'adresse Maono Ops ?

- **Stratèges** qui définissent les objectifs et les KPI de chaque campagne
- **Planificateurs de contenu** qui organisent le calendrier éditorial
- **Créatifs** (designers, photographes, vidéastes) qui produisent les assets
- **Managers des réseaux sociaux** qui publient et planifient les contenus
- **Clients** qui approuvent les contenus via un portail dédié
- **Administrateurs** qui gèrent les utilisateurs, intégrations et paramètres

### Problèmes résolus

Sans Maono Ops, les équipes jonglent entre des feuilles de calcul, des fils d'e-mails et des outils dispersés. Maono Ops réunit en un seul endroit :

- Le suivi du statut de chaque contenu en temps réel
- Les workflows d'approbation interne et client
- La planification visuelle du calendrier éditorial
- Le reporting analytique par campagne et par plateforme

---

## 2. Premiers pas

### Connexion

Accédez à l'application via `/auth/login`. Saisissez votre **adresse e-mail** et votre **mot de passe**, puis cliquez sur **Se connecter**.

⚠️ Si vous ne parvenez pas à vous connecter, vérifiez que votre adresse e-mail est correcte et contactez votre administrateur pour réinitialiser votre mot de passe.

### Présentation de l'interface

L'interface se compose de trois zones principales :

| Zone | Description |
|---|---|
| **Barre de navigation latérale** | Accès rapide à tous les modules selon votre rôle |
| **Fil d'Ariane (breadcrumbs)** | Affiche votre position dans l'application (ex. : Campagnes › Ma Campagne › Contenu) |
| **Zone de contenu principale** | Affiche les données du module actif |

### Guide des couleurs

Maono Ops utilise un code couleur cohérent dans toute l'interface :

| Couleur | Signification |
|---|---|
| **Bleu éditorial** | Action disponible, lien, bouton principal |
| **Ambre** | Urgence, deadline proche, attention requise |
| **Émeraude** | Terminé, publié, validé |
| **Violet** | En cours de révision ou d'approbation |

### Rôles et accès

Votre rôle détermine les modules visibles dans votre interface :

| Rôle | Accès | Description |
|---|---|---|
| `ADMIN` | Tout | Gestion utilisateurs, intégrations, paramètres |
| `STRATEGIST` | Stratégie, campagnes, analytics | Définit la stratégie éditoriale |
| `CONTENT_PLANNER` | Calendrier, campagnes, contenus | Planifie le calendrier |
| `DESIGNER` | Contenus assignés | Produit les visuels |
| `PHOTOGRAPHER` | Contenus assignés | Produit les photos |
| `VIDEOGRAPHER` | Contenus assignés | Produit les vidéos |
| `SOCIAL_MEDIA_MANAGER` | File de publication | Publie le contenu |
| `CLIENT` | Portail client uniquement | Approuve les contenus via token |

---

## 3. Gestion des campagnes

### Créer une campagne

Rendez-vous sur `/campaigns/new` ou cliquez sur **Nouvelle campagne** depuis la liste des campagnes. Remplissez le formulaire :

1. **Nom** — titre de la campagne
2. **Client** — sélectionnez le client dans la liste déroulante
3. **Objectif** — description de l'objectif marketing
4. **Dates** — date de début et date de fin
5. **Plateformes** — cochez les plateformes ciblées (Instagram, LinkedIn, TikTok, etc.)
6. **Cibles KPI** — définissez les indicateurs clés (portée, CTR, conversions, etc.)

💡 Définissez toujours vos cibles KPI avant de créer des contenus : elles alimenteront directement les graphiques Analytics.

### Statuts de campagne

Une campagne suit ce cycle de vie :

`DRAFT` → `ACTIVE` → `PAUSED` ou `COMPLETED`

- `DRAFT` : campagne en cours de configuration
- `ACTIVE` : campagne en production, contenus en cours de création
- `PAUSED` : campagne temporairement suspendue
- `COMPLETED` : campagne terminée et archivée

### Vue détail d'une campagne

La page de détail d'une campagne affiche :

- **Barres de progression KPI** — comparaison entre les objectifs définis et les résultats réels
- **Liste des contenus** — tous les contenus rattachés à la campagne avec leur statut
- **Deadlines à venir** — chronologie des prochaines échéances
- **Risques ouverts** — alertes signalées par l'équipe

💡 Survolez une ligne dans le tableau des campagnes pour faire apparaître le lien **Voir →** permettant d'accéder rapidement au détail.

---

## 4. Calendrier éditorial

Le calendrier éditorial offre une vue mensuelle de tous les contenus planifiés.

### Navigation

- Utilisez les flèches **← Précédent** et **Suivant →** pour changer de mois
- Chaque cellule de jour affiche un ou plusieurs **points de statut colorés** correspondant aux contenus prévus ce jour-là

### Points de statut sur le calendrier

| Couleur du point | Statut |
|---|---|
| Gris | `DRAFT` |
| Bleu | `IN_PRODUCTION` |
| Violet | `INTERNAL_REVIEW` ou `CLIENT_REVIEW` |
| Émeraude | `APPROVED`, `SCHEDULED` ou `PUBLISHED` |
| Ambre | `REVISION_REQUIRED` |

### Panneau latéral droit

Le panneau latéral affiche :

- **Répartition par plateforme** — nombre de contenus par réseau social ce mois-ci
- **Prochaines entrées** — les 5 prochains contenus à publier
- **Légende des statuts** — rappel du code couleur

---

## 5. File de production (Contenus)

La file de production liste l'ensemble des contenus en cours de création.

### Cycle de vie complet d'un contenu

```
DRAFT → IN_PRODUCTION → INTERNAL_REVIEW → CLIENT_REVIEW → APPROVED → SCHEDULED → PUBLISHED
```

Des statuts complémentaires existent :
- `REVISION_REQUIRED` — des modifications ont été demandées
- `ARCHIVED` — contenu refusé ou archivé

### Filtrer et rechercher

- **Onglets d'équipe** : filtrez par équipe — Design, Vidéo, Photo, Copy
- **Barre de recherche** : saisissez un titre et appuyez sur **Entrée** pour filtrer
- **URL partageable** : les filtres actifs sont persistés dans l'URL — copiez-la pour partager une vue filtrée avec un collègue

### Indicateur d'urgence

Une **barre ambre** accompagnée d'une icône d'avertissement signale que la deadline du contenu est dans **moins de 2 jours**. Traitez ces contenus en priorité.

### Créer un nouveau contenu

Cliquez sur **Nouveau contenu** et remplissez le formulaire :

1. **Campagne** — sélectionnez la campagne associée
2. **Plateforme** — sélectionnez via les boutons radio (Instagram, YouTube, LinkedIn…)
3. **Équipe** — sélectionnez via les boutons radio (Design, Vidéo, Photo, Copy)
4. **Brief créatif** — décrivez précisément les attentes créatives
5. **Deadline** — date limite de livraison

⚠️ Ne passez pas un contenu en `IN_PRODUCTION` sans avoir rempli le brief créatif. L'équipe de production en dépend.

### Vue détail d'un contenu

La page de détail d'un contenu affiche :

- **Stepper de cycle de vie** — représentation visuelle des étapes franchies et à venir
- **Liste des tâches** — tâches associées à ce contenu
- **Assets** — fichiers attachés (visuels, vidéos, documents)
- **Statut d'approbation** — historique des décisions et commentaires

---

## 6. Tâches de production

Chaque contenu peut comporter plusieurs tâches assignées à des membres de l'équipe.

### Statuts de tâche

| Statut | Libellé affiché | Description |
|---|---|---|
| `PENDING` | À faire | Tâche non démarrée |
| `IN_PROGRESS` | En cours | Tâche en cours de traitement |
| `COMPLETED` | Terminé | Tâche finalisée |
| `BLOCKED` | Bloqué | Tâche bloquée par un obstacle |

### Niveaux de priorité

| Priorité | Couleur | Usage |
|---|---|---|
| `HIGH` | Rouge | Tâche critique, à traiter immédiatement |
| `MEDIUM` | Ambre | Tâche importante, sous surveillance |
| `LOW` | Gris | Tâche secondaire |

💡 Passez une tâche en `BLOCKED` dès qu'un obstacle est identifié, et laissez un commentaire explicatif pour que le responsable puisse intervenir rapidement.

---

## 7. Gestion des assets

### Uploader un asset

Rendez-vous sur `/contents/[id]/assets` et déposez vos fichiers dans la **zone de dépôt** (drag-and-drop) ou cliquez sur la zone pour ouvrir l'explorateur de fichiers.

### Types d'assets acceptés

| Type | Exemples |
|---|---|
| `IMAGE` | JPG, PNG, WebP, GIF |
| `VIDEO` | MP4, MOV, AVI |
| `DOCUMENT` | PDF, DOCX, XLSX |
| `AUDIO` | MP3, WAV, AAC |
| `RAW` | PSD, AI, Figma export |

### Versionnage

Chaque nouvel upload d'un fichier portant le même nom **incrémente automatiquement le numéro de version**. Les versions précédentes sont conservées et marquées **Archivé** dans l'interface.

⚠️ Ne supprimez pas les versions archivées manuellement sauf instruction expresse de votre administrateur — elles peuvent être requises pour des audits clients.

### Téléchargement et prévisualisation

- Cliquez sur l'icône **Aperçu** pour visualiser l'asset directement dans le navigateur (images et vidéos)
- Cliquez sur l'icône **Télécharger** pour sauvegarder le fichier localement

---

## 8. Workflow d'approbation

### Étape 1 — Revue interne

Lorsqu'un contenu est prêt à être révisé en interne, son statut passe à `INTERNAL_REVIEW`.

Le relecteur ouvre `/approvals/[id]` et effectue les actions suivantes :

1. Lit le contenu et vérifie les assets uploadés
2. Complète la **checklist de validation** (cohérence avec le brief, qualité visuelle, respect de la charte)
3. Rédige un **commentaire** si nécessaire

Il prend ensuite l'une des trois décisions :

| Décision | Bouton | Statut résultant | Suite |
|---|---|---|---|
| Valider | **Approuver** | `APPROVED` | Le contenu rejoint la file de publication |
| Corriger | **Demander des modifications** | `REVISION_REQUIRED` | Le contenu retourne en production |
| Rejeter | **Refuser** | `ARCHIVED` | Le contenu est archivé définitivement |

### Étape 2 — Revue client (si requise)

Si une validation client est nécessaire, le statut passe à `CLIENT_REVIEW`. Le client accède au contenu via son **portail client** (voir section 9).

ℹ️ La file d'approbation est vide si aucun contenu n'est actuellement en statut `INTERNAL_REVIEW` ou `CLIENT_REVIEW`.

---

## 9. Portail client

### Accès

Le portail client est accessible via une URL unique de la forme :

```
https://[domaine]/client/[token]
```

**Aucun compte ni mot de passe n'est requis.** L'accès est sécurisé par le token unique intégré dans l'URL.

### Créer un token de portail

1. Rendez-vous dans `/admin/clients`
2. Cliquez sur le client concerné
3. Cliquez sur **Générer un token**
4. Partagez l'URL générée avec le client

⚠️ Traitez cette URL comme un mot de passe. Ne la partagez pas publiquement.

### Ce que voit le client

Dans son portail, le client peut :

- **Consulter** les contenus en attente de sa validation (statut `CLIENT_REVIEW`)
- **Approuver** chaque contenu — le statut passe à `APPROVED`
- **Demander des modifications** — le statut passe à `REVISION_REQUIRED` avec son commentaire
- **Consulter les contenus publiés** avec leurs métriques de performance associées

💡 Guidez votre client lors de sa première connexion : le portail est conçu pour être simple d'utilisation, mais un accompagnement initial améliore la qualité des retours.

---

## 10. File de publication

La file de publication est accessible aux rôles `SOCIAL_MEDIA_MANAGER` et `ADMIN`. Elle se divise en deux onglets.

### Prêts à publier (`APPROVED`)

Liste les contenus approuvés en attente de publication.

- Une **barre ambre urgente** s'affiche si la deadline est aujourd'hui ou demain
- Deux actions disponibles :
  - **Publier** — marque le contenu comme `PUBLISHED` immédiatement
  - **Planifier** — ouvre un sélecteur de date/heure pour planifier la publication (`SCHEDULED`)

### Planifiés (`SCHEDULED`)

Liste les contenus dont la publication est programmée.

- La **date et heure de publication** prévue sont affichées
- Deux actions disponibles :
  - **Modifier** — permet de changer la date/heure planifiée
  - **Annuler** — repasse le contenu en `APPROVED` et annule la planification

---

## 11. Analytics

Le module Analytics offre une vue consolidée des performances marketing.

### Cartes KPI globales

En haut de la page, trois cartes affichent les indicateurs clés agrégés :

| Indicateur | Description |
|---|---|
| **Portée totale** | Nombre d'impressions cumulées |
| **CTR** | Taux de clic moyen toutes campagnes confondues |
| **Conversions** | Nombre d'actions cibles réalisées |

ℹ️ Si un indicateur affiche `—`, les données de performance n'ont pas encore été importées pour la période concernée.

### Graphique d'évolution mensuelle

Un graphique en courbes affiche l'évolution de la portée sur les **12 derniers mois**, permettant d'identifier les tendances saisonnières.

### Répartition par plateforme

Un graphique en secteurs ou en barres décompose les performances par réseau social.

### Tableau des performances par campagne

Le tableau liste toutes les campagnes actives et terminées avec leurs KPI respectifs. Cliquez sur **Détails →** pour accéder à la vue analytique détaillée d'une campagne spécifique.

---

## 12. Administration

> Cette section est réservée aux utilisateurs avec le rôle `ADMIN`.

### Tableau de bord admin

Le tableau de bord affiche :

- **Santé des services** en temps réel : PostgreSQL, Redis, MinIO, Maono API, BullMQ
- **Utilisateurs récents** — les derniers comptes créés ou modifiés
- **Liens rapides** vers les sections d'administration fréquentes

### Gestion des utilisateurs

Depuis `/admin/users`, vous pouvez :

- **Créer** un nouvel utilisateur (e-mail, rôle, statut actif/inactif)
- **Modifier** le rôle ou les informations d'un utilisateur existant
- **Désactiver** un compte sans le supprimer (statut inactif)

### Gestion des clients

Depuis `/admin/clients`, vous pouvez :

- **Ajouter** un nouveau client
- **Consulter** les informations du token de portail associé à chaque client
- **Générer** ou **révoquer** un token de portail client

### Intégrations

Depuis `/admin/integrations`, gérez les connexions aux services externes :

| Service | Description | Action disponible |
|---|---|---|
| **Maono API** | Synchronisation des données CRM et projet | Tester la connexion, Synchroniser maintenant |
| **MinIO** | Stockage des assets | Tester la connexion, voir les credentials |
| **Redis** | Cache et files de tâches | Tester la connexion |
| **SMTP** | Envoi des notifications e-mail | Tester la connexion |

💡 En cas de problème d'upload d'assets, commencez par tester la connexion MinIO depuis cette page.

### Paramètres

Depuis `/admin/settings`, configurez :

- **Identité de l'agence** — nom, logo, couleurs
- **Notifications** — activez ou désactivez les alertes e-mail par événement
- **Valeurs par défaut du workflow** — statut initial des nouveaux contenus, délais de relance
- **Sécurité** — durée de session, politique de mot de passe

---

## 13. Bonnes pratiques éditoriales

Respectez ces recommandations pour garantir un workflow fluide et une qualité de production constante.

1. **Remplissez toujours le brief créatif** avant de passer un contenu en `IN_PRODUCTION`. Un brief incomplet génère des allers-retours coûteux en temps.

2. **Uploadez au moins un asset** avant de demander une revue interne. Ne soumettez pas un contenu vide à l'approbation.

3. **Utilisez le système de signalement des risques** dès qu'un contenu est en retard ou qu'une réponse client tarde. Cela permet au stratège de prendre des décisions informées.

4. **Définissez les cibles KPI dans la campagne avant de créer les contenus**. Les indicateurs n'apparaîtront dans Analytics que si les cibles ont été configurées en amont.

5. **Commentez vos décisions d'approbation**. Même pour une approbation, un commentaire positif encourage l'équipe créative.

6. **Archivez les campagnes terminées** pour maintenir une liste de campagnes actives lisible et à jour.

---

## 14. Raccourcis et astuces

### Compteur J-N

Le compteur affiché à côté de chaque deadline indique le nombre de jours restants :

| Affichage | Signification |
|---|---|
| `J-0` | Deadline aujourd'hui |
| `J-3` | 3 jours restants |
| `En retard` | Deadline dépassée |

Les contenus publiés avec une deadline dépassée s'affichent en **gris** — c'est normal et attendu.

### Navigation rapide

- **Survolez** une ligne dans le tableau des campagnes pour faire apparaître le lien **Voir →**
- Les **onglets de filtre** sur la page des contenus sont persistés dans l'URL : copiez l'URL pour partager une vue filtrée avec un collègue
- La **barre de recherche** des contenus se soumet en appuyant sur **Entrée** (formulaire GET natif)

### Raccourcis clavier courants

| Action | Raccourci |
|---|---|
| Valider un formulaire | `Entrée` |
| Fermer une modale | `Échap` |

---

## 15. FAQ

**1. Je ne peux pas me connecter**

Vérifiez que votre adresse e-mail est orthographiée correctement et que le verrouillage des majuscules n'est pas activé. Si le problème persiste, contactez votre administrateur pour réinitialiser votre mot de passe.

---

**2. Je ne vois pas mes contenus dans la file de production**

Vérifiez que le filtre d'équipe correspond bien à votre équipe. Si vous êtes designer, l'onglet **Design** doit être actif. Effacez également la barre de recherche si elle contient du texte résiduel.

---

**3. Comment partager un contenu avec un client ?**

Créez un token de portail client dans `/admin/clients`, cliquez sur le client concerné, puis cliquez sur **Générer un token**. Partagez l'URL générée avec le client.

---

**4. La deadline est dépassée mais le contenu est `PUBLISHED` — est-ce un problème ?**

Non, c'est un comportement normal. Les contenus publiés dont la deadline est passée s'affichent en gris dans l'interface. Il n'y a pas d'action à entreprendre.

---

**5. Comment créer un token de portail client ?**

Rendez-vous dans `/admin/clients`, cliquez sur le client concerné, puis cliquez sur **Générer un token**. L'URL du portail est immédiatement disponible pour être partagée.

---

**6. La file d'approbation est vide**

La file d'approbation n'affiche que les contenus en statut `INTERNAL_REVIEW` ou `CLIENT_REVIEW`. Si elle est vide, aucun contenu ne se trouve actuellement dans ces statuts.

---

**7. Analytics affiche `—` pour l'engagement**

Les données de performance n'ont pas encore été importées pour la période concernée. Vérifiez que la synchronisation Maono API est à jour dans `/admin/integrations`.

---

**8. Comment supprimer une campagne ?**

La suppression définitive n'est pas disponible pour préserver l'intégrité des données historiques. Seul un `ADMIN` peut **archiver** une campagne, ce qui la retire de la vue active sans effacer les données.

---

**9. Qui peut publier du contenu ?**

Les rôles `SOCIAL_MEDIA_MANAGER` et `ADMIN` ont accès à la file de publication et peuvent utiliser les boutons **Publier** et **Planifier**.

---

**10. Comment synchroniser avec Maono API ?**

Rendez-vous dans `/admin/integrations`, section **Maono API**, puis cliquez sur **Synchroniser maintenant**. Une confirmation s'affichera une fois la synchronisation terminée. En cas d'erreur, cliquez sur **Tester la connexion** pour diagnostiquer le problème.

---

*Guide rédigé pour Maono Ops — Avril 2026*
