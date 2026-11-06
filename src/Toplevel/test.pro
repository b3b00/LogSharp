t(x).
t(y).
t(z).
lt10(X) :- X < 10.
gte10(X) :- X >= 10.
add(X,Y) :-  X < 10 Y is X + 1.
add(X,Y) :-  X >= 10 Y is X + 1.
