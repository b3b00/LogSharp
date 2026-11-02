# Récursion et freshening des variables

## Problème

La règle Prolog suivante recherche un élément en parcourant une liste récursivement :

```prolog
member_of(X, [X|_]).
member_of(X, [_|L]) :- member_of(X, L).
```

Les recherches des premiers éléments fonctionnaient, mais `member_of(c,[a,b,c])?` échouait. Le troisième élément nécessite plusieurs appels récursifs, ce qui a révélé que les variables des différentes applications d'une même règle étaient confondues.

## Portée des variables

En Prolog, les variables d'une clause sont locales à une application de cette clause. Les occurrences d'une variable doivent rester liées entre la tête et le corps de cette application, mais une autre application récursive doit recevoir de nouvelles variables.

Par exemple, dans `member_of(X,[_|L]) :- member_of(X,L)`, les deux occurrences de `X` d'un même appel désignent la même variable. En revanche, le `X` de l'appel récursif suivant est une variable distincte. Le nom écrit dans le programme est le même, mais sa portée ne l'est pas.

Sans cette distinction, LogSharp stocke les bindings dans un environnement indexé par nom. Le premier appel lie `L` au suffixe `[b,c]`; si l'appel suivant réutilise le même nom interne `L`, il hérite de cette liaison au lieu de créer une variable indépendante pour son propre suffixe `[c]`.

## Ce que fait le freshening

Le *freshening* (renommage frais) crée une copie de la règle pour chaque nouvelle application et remplace chacune de ses variables par un nom interne unique. Une table de correspondance garantit que les occurrences d'une variable dans cette règle copiée partagent le même nouveau nom. La variable anonyme `_` reste anonyme.

Ainsi, des appels distincts peuvent contenir `__goal1_X` et `__goal3_X`, tandis que les occurrences de `X` dans chaque appel restent cohérentes. `Goal.FreshenRule` applique ce renommage à la tête et au corps de la règle dans [Goal.cs](LogSharp/interpretation/Goal.cs).

Les continuations créées après un built-in réutilisent au contraire la règle déjà instanciée: elles ne doivent pas freshen une deuxième fois ses variables, sinon les bindings du built-in ne correspondraient plus aux variables du corps.

## Corrections complémentaires

Le freshening ne suffisait pas à lui seul. Deux autres problèmes empêchaient une récursion correcte :

- [NestedTermUnifier.cs](LogSharp/interpretation/unification/NestedTermUnifier.cs) détectait les différences de nom ou d'arité, mais poursuivait l'unification et pouvait ensuite marquer le résultat comme réussi. Le rejet est maintenant immédiat. En particulier, la liste vide `[]` ne peut plus correspondre au motif non vide `[_|L]`.
- À la fin d'une règle, [Interpreter.cs](LogSharp/interpretation/Interpreter.cs) doit unifier sa tête avec le goal actif de l'appelant, et non avec la tête de la règle parente. Une continuation de built-in est un cas distinct et conserve le raccordement à la règle en cours.

## Résultat

Avec ces changements, la règle récursive peut avancer de suffixe en suffixe, garder les variables de chaque appel isolées et transmettre le succès au bon appelant. `MemberThirdTest` passe, et la suite complète passe également : 7 tests réussis.