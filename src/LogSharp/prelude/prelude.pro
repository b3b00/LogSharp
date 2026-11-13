% 1. maplist/2: Applies a unary predicate to each element of a list
my_maplist(_, []).
my_maplist(Goal, [X | Xs]) :-
    call(Goal, X)
    my_maplist(Goal, Xs).

% 2. maplist/3: Applies a binary predicate to corresponding elements of two lists
my_maplist(_, [], []).
my_maplist(Goal, [X | Xs], [Y | Ys]) :-
    call(Goal, X, Y)
    my_maplist(Goal, Xs, Ys).

% Checks if all elements in a list are distinct
% grammar note : goals in a rule body are separated by a space, not a comma
distinct_elements_fast(List) :-
    sort(List, Sorted)
    length(List, Len)
    length(Sorted, Len).

len(X,Y) :- length(X,Y).

% ---------------------------------------------------------------------------
% prelude library : predicates that are useful enough not to rewrite them in
% every program. Reminder of the dialect :
%   - goals in a rule body are separated by a space, not a comma
%   - arithmetic goes through is/2 ( Y is X + 1 )
%   - bare comparisons ( X > 0 ) work as goals, but both sides must be bound
% ---------------------------------------------------------------------------

% append/3 : append(Prefix, Suffix, Whole) succeeds when Whole is the
% concatenation of Prefix and Suffix. Useful in every direction :
%   append([1,2],[3,4],W)  -> joins two lists
%   append(P,[3,4],[1,2,3,4]) -> enumerates the prefixes P
% (append/2 is a builtin that flattens a list of lists, not the same thing)
append([], Suffix, Suffix).
append([Head | Tail], Suffix, [Head | Rest]) :- append(Tail, Suffix, Rest).

% prefix/2 : Prefix is an initial segment of Whole
prefix(Prefix, Whole) :- append(Prefix, _, Whole).

% suffix/2 : Suffix is a final segment of Whole
suffix(Suffix, Whole) :- append(_, Suffix, Whole).

% sublist/2 : Sub appears as a contiguous slice of Whole
sublist(Sub, Whole) :- append(_, Rest, Whole) append(Sub, _, Rest).

% last/2 : the last element of a list
last([Element], Element).
last([_ | Tail], Element) :- last(Tail, Element).

% nth0/3 : 0-based element access. Works in both directions :
%   nth0(1,[a,b,c],E) -> checks the element at index 1
%   nth0(I,[a,b,c],E) -> enumerates every (index, element) pair
nth0(0, [Element | _], Element).
nth0(Index, [_ | Tail], Element) :-
    nth0(Prev, Tail, Element)
    Index is Prev + 1.

% nth1/3 : same as nth0/3 but 1-based (element numbering starts at 1)
nth1(Index, List, Element) :-
    nth0(ZeroBased, List, Element)
    Index is ZeroBased + 1.

% reverse/2 : the elements of List in reverse order (accumulator style)
reverse(List, Reversed) :- reverse_acc(List, [], Reversed).
reverse_acc([], Accumulator, Accumulator).
reverse_acc([Head | Tail], Accumulator, Reversed) :-
    reverse_acc(Tail, [Head | Accumulator], Reversed).

% sum_list/2 : Sum is the sum of the (numeric) elements of List
sum_list([], 0).
sum_list([Head | Tail], Sum) :-
    sum_list(Tail, Rest)
    Sum is Head + Rest.

% max_list/2 and min_list/2 : extremum of a non empty numeric list
max_list([Element], Element).
max_list([Head | Tail], Max) :-
    max_list(Tail, Rest)
    Head >= Rest
    Max is Head.
max_list([Head | Tail], Max) :-
    max_list(Tail, Rest)
    Head < Rest
    Max is Rest.
min_list([Element], Element).
min_list([Head | Tail], Min) :-
    min_list(Tail, Rest)
    Head <= Rest
    Min is Head.
min_list([Head | Tail], Min) :-
    min_list(Tail, Rest)
    Head > Rest
    Min is Rest.

% memberchk/2 : succeeds exactly once if Element occurs in List ; unlike
% member/2 it leaves no alternative solutions behind (double negation trick)
memberchk(Element, List) :- not(not(member(Element, List))).

% between/3 : between(Low, High, X) succeeds for every integer X in [Low..High]
between(Low, High, Low) :- number(Low) number(High) Low <= High.
between(Low, High, X) :-
    number(Low)
    number(High)
    Low < High
    Next is Low + 1
    between(Next, High, X).

% range/3 : range(Low, High, List) builds the list [Low, Low+1, ..., High]
range(Low, High, []) :- number(Low) number(High) Low > High.
range(Low, High, [Low | Rest]) :-
    number(Low)
    number(High)
    Low <= High
    Next is Low + 1
    range(Next, High, Rest).

% select/3 : select(Element, List, Rest) removes one occurrence of Element
% from List ; with Element unbound it picks each element of List in turn
select(Element, [Element | Tail], Tail).
select(Element, [Head | Tail], [Head | Rest]) :- select(Element, Tail, Rest).

% permutation/2 : Perm is a permutation of List
permutation([], []).
permutation(List, [Head | Perm]) :-
    select(Head, List, Rest)
    permutation(Rest, Perm).

% include/3 : keeps the elements of List for which Goal succeeds
% (Goal should succeed at most once per element, otherwise duplicates appear)
include(_, [], []).
include(Goal, [Head | Tail], [Head | Included]) :-
    call(Goal, Head)
    include(Goal, Tail, Included).
include(Goal, [Head | Tail], Included) :-
    not(call(Goal, Head))
    include(Goal, Tail, Included).

% exclude/3 : drops the elements of List for which Goal succeeds
exclude(_, [], []).
exclude(Goal, [Head | Tail], Excluded) :-
    call(Goal, Head)
    exclude(Goal, Tail, Excluded).
exclude(Goal, [Head | Tail], [Head | Excluded]) :-
    not(call(Goal, Head))
    exclude(Goal, Tail, Excluded).

% foldl/4 : foldl(Goal, List, AccIn, AccOut) reduces List left to right,
% calling call(Goal, Element, Acc, NextAcc) on each element. With an empty
% list the accumulator is returned unchanged.
foldl(_, [], AccIn, AccIn).
foldl(Goal, [Head | Tail], AccIn, AccOut) :-
    call(Goal, Head, AccIn, AccNext)
    foldl(Goal, Tail, AccNext, AccOut).
