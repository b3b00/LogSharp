# Synthèse des performances : Indexation sur le premier argument

Ce document résume les résultats des benchmarks mesurant l'impact de l'indexation sur le premier argument (*first-argument indexing*) implémentée dans LogSharp, telle que spécifiée dans [`predicate_index.md`](predicate_index.md).

---

## 1. Contexte et Problématique

Dans un moteur Prolog sans indexation, la résolution d'un but `p(Arg1, ...)` effectue un scan linéaire $O(N)$ sur l'ensemble des $N$ clauses associées au prédicat `p/k` :
1. Pour **chaque clause** de la base, le moteur alloue un nœud `Goal` enfant.
2. Il applique le rafraîchissement des variables de la règle (`FreshenRule` via copie profonde `DeepCopier`).
3. Il tente l'unification via `Unifier.UnifyTerms(goal.Head, ..., child.Head, ...)`.
4. Sur une base de $5\,000$ faits, $4\,999$ unifications échouent immédiatement après avoir consommé du temps CPU et alloué de la mémoire sur le tas.

L'indexation sur le premier argument inspecte la valeur déréférencée du premier argument du but (`Scalar` constant ou `Term` composé) et effectue une recherche en table de hachage $O(1)$ dans [`PredicateIndex`](../src/LogSharp/model/PredicateIndex.cs) pour restreindre immédiatement l'exploration aux seules clauses compatibles ($K \ll N$).

---

## 2. Protocole de Mesure

Les mesures ont été réalisées avec le projet de benchmark dédié [`benchmarks/LogSharpBenchmarks`](../benchmarks/LogSharpBenchmarks) :
- **Comparaison A/B stricte** : activation / désactivation de l'index via la propriété `UseIndex` sur [`LogSharpProgram`](../src/LogSharp/model/LogSharpProgram.cs) et [`Interpreter`](../src/LogSharp/interpretation/Interpreter.cs) sur des requêtes et états de départ identiques.
- **Base de faits extensible (EDB)** : prédicat `person(id_i, Name_i, Age_i)` testé avec $N \in \{100, 500, 1\,000, 2\,500, 5\,000\}$ faits.
- **Pire cas pour le parcours linéaire** : recherche d'une clé située en fin de base (`id_{N-1}`).
- **Métriques relevées** :
  - Nombre de clauses candidates inspectées.
  - Latence moyenne par requête (mesurée au `Stopwatch` haute résolution, après phase de chauffe).
  - Octets alloués sur le tas GC (`GC.GetTotalAllocatedBytes()`).

---

## 3. Résultats Expérimentaux

### Recherche dans une base de faits (`person/3`)

| Taille ($N$) | Métrique | Parcours Linéaire (sans index) | Avec Index ([`PredicateIndex`](../src/LogSharp/model/PredicateIndex.cs)) | Gain / Facteur d'amélioration |
| :--- | :--- | :--- | :--- | :--- |
| **$N = 100$** | **Candidats** | $100$ règles | **$1$ règle** | **$100\times$ moins** |
| | **Latence** | $260{,}2\ \mu\text{s}$ | **$20{,}2\ \mu\text{s}$** | **$12{,}9\times$ plus rapide** |
| | **Allocations** | $249{,}6\ \text{KB}$ | **$14{,}0\ \text{KB}$** | **$17{,}8\times$ moins de mémoire** |
| **$N = 500$** | **Candidats** | $500$ règles | **$1$ règle** | **$500\times$ moins** |
| | **Latence** | $1{,}31\ \text{ms}$ | **$17{,}8\ \mu\text{s}$** | **$73{,}5\times$ plus rapide** |
| | **Allocations** | $1{,}17\ \text{MB}$ | **$14{,}0\ \text{KB}$** | **$85{,}8\times$ moins de mémoire** |
| **$N = 1\,000$** | **Candidats** | $1\,000$ règles | **$1$ règle** | **$1\,000\times$ moins** |
| | **Latence** | $2{,}81\ \text{ms}$ | **$17{,}2\ \mu\text{s}$** | **$163{,}4\times$ plus rapide** |
| | **Allocations** | $2{,}34\ \text{MB}$ | **$14{,}1\ \text{KB}$** | **$169{,}6\times$ moins de mémoire** |
| **$N = 2\,500$** | **Candidats** | $2\,500$ règles | **$1$ règle** | **$2\,500\times$ moins** |
| | **Latence** | $6{,}22\ \text{ms}$ | **$11{,}1\ \mu\text{s}$** | **$558{,}2\times$ plus rapide** |
| | **Allocations** | $6{,}02\ \text{MB}$ | **$14{,}3\ \text{KB}$** | **$430{,}2\times$ moins de mémoire** |
| **$N = 5\,000$** | **Candidats** | $5\,000$ règles | **$1$ règle** | **$5\,000\times$ moins** |
| | **Latence** | $6{,}25\ \text{ms}$ | **$10{,}1\ \mu\text{s}$** | **$616{,}0\times$ plus rapide** |
| | **Allocations** | $12{,}17\ \text{MB}$ | **$14{,}3\ \text{KB}$** | **$868{,}8\times$ moins de mémoire** |

---

## 4. Analyse des Enseignements

### 1. Complexité temporelle : $O(1)$ vs $O(N)$
- **Sans index** : la latence croît de façon strictement linéaire avec le nombre de clauses ($260\ \mu\text{s} \to 6{,}25\ \text{ms}$).
- **Avec index** : la latence reste constante et plate ($\approx 10\text{--}20\ \mu\text{s}$), quelle que soit la taille de la base ($N = 100$ ou $N = 5\,000$).

### 2. Réduction drastique de la pression sur le ramasse-miettes (GC)
Le gain le plus spectaculaire concerne les allocations mémoire :
- Pour une base de $5\,000$ faits, chaque requête linéaire allouait **$12{,}17\ \text{MB}$** de mémoire temporaire (création de $5\,000$ objets `Goal` et copies profondes de chaque règle).
- Avec index, la consommation chute à **$14{,}3\ \text{KB}$** (**réduction de $868\times$**), évitant la saturation des générations Gen0/Gen1 du GC sur les programmes intensifs.

### 3. Dispatch de termes composés (Listes)
Pour les prédicats récursifs manipulant des listes (ex. `len([], 0)` vs `len([_|T], N)`) :
- Lors d'une requête sur liste vide `len([], N)?`, l'index identifie la clé `list/0` et ne propose que le cas de base ($1$ clause), éliminant d'emblée la règle récursive `list/2`.
- Sur $500$ requêtes consécutives du cas de base, le temps passe de $7{,}97\ \text{ms}$ à $6{,}36\ \text{ms}$ ($1{,}3\times$ plus rapide) tout en évitant la création de points de choix superflus.

### 4. Coût du repli sur variable non instanciée
Lorsqu'une requête utilise une variable libre en premier argument (ex. `person(X, Name, Age)?`), l'algorithme détecte `derefArg is Variable` et renvoie directement la séquence ordonnée `_allClauses`. L'overhead se résume à une simple vérification de type, garantissant une régression nulle sur les requêtes non indexables.

---

## 5. Comment reproduire ces benchmarks

Le projet de benchmark est intégré à la solution globale [`LogSharp.sln`](../LogSharp.sln).

Pour exécuter le rapport rapide :
```bash
dotnet run --project benchmarks/LogSharpBenchmarks
```

Pour exécuter la suite statistique complète avec [BenchmarkDotNet](https://benchmarkdotnet.org/) (en mode Release avec analyseurs mémoire) :
```bash
dotnet run -c Release --project benchmarks/LogSharpBenchmarks -- --benchmark
```
