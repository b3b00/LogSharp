father(christian,olivier).
father(christian,frederic).
father(christian,julien).
father(frederic,lontzi).
father(frederic,eneko).
father(frederic,kepa).
father(julien,oihana).
grandpa(X,Z) :- father(X,Y) father(Y,Z).
gp(X,Y,Z) :- father(X,Y) father(Y,Z).
brother(X,Y) :- father(Z,X) father(Z,Y).

myl([a,b,c]).
myl([d,e,f]).

tester(X,Y) :- testPrim(X,Y).

member_of(X,[X|_]).
member_of(X,[_|L]) :- member_of(X,L).

cof(C,F) :- bag_of(CC,father(F,CC), L) member(C,L).
