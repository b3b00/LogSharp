% 1. maplist/2: Applies a unary predicate to each element of a list
my_maplist(_, []).
my_maplist(Goal, [X | Xs]) :-
    call(Goal, X),
    my_maplist(Goal, Xs).

% 2. maplist/3: Applies a binary predicate to corresponding elements of two lists
my_maplist(_, [], []).
my_maplist(Goal, [X | Xs], [Y | Ys]) :-
    call(Goal, X, Y),
    my_maplist(Goal, Xs, Ys).
    
% Checks if all elements in a list are distinct
distinct_elements_fast(List) :-
    sort(List, Sorted),
    length(List, Len),
    length(Sorted, Len).
    
len(X,Y) :- length(X,Y).    
    