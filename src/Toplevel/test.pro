t(x).
t(y).
t(z).
add(X,Y) :-  X < 10 Y is X + 1.
add(X,Y) :-  X >= 10 Y is X + 10.

grade(alice, math, 85).
grade(alice, science, 90).
grade(bob, math, 75).
grade(bob, science, 80).

digit(zero) --> [0].
greeting() --> [hello, world].
empty() --> [].
empty2() --> .


% A statement: variable = expression
statement(ast_assign(Var, ExprAST)) --> 
    [Var], ['='], expr(ExprAST).

% Expressions (handling addition for simplicity)
expr(ast_add(Left, Right)) --> 
    term(Left), ['+'], expr(Right).
expr(AST) --> 
    term(AST).

% Terms: numbers or variables
term(ast_num(N)) --> 
    [N], { number(N) }.
term(ast_var(V)) --> 
    [V], { atom(V) }.  % %V \= (=), V \= (+) }.