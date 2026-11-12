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
    
% sudoku(Rows) : Rows is a 9x9 grid satisfying the sudoku constraints.
%
% adapted to this interpreter grammar :
%   - goals in a rule body are separated by spaces, not commas
%   - no directives : the ":- use_module(library(clpfd))." line was dropped
%   - the clpfd range declaration "Vs ins 1..9" is written "Vs ins 1...9"
%   - "Rows = [R1,...,R9]" cannot use the expression parser (no list
%     operands), so as_grid/10 names the rows instead.
%
% clpfd-like builtins : length/2, maplist/2, append/2, same_length/2,
% all_distinct/1, transpose/2, label/1 and the "ins" spread operator
sudoku(Rows) :-
    length(Rows, 9)
    maplist(same_length(Rows), Rows)
    append(Rows, Vs)
    Vs ins 1...9

    % Rows must all be distinct
    maplist(all_distinct, Rows)

    % Columns must all be distinct
    transpose(Rows, Columns)
    maplist(all_distinct, Columns)

    % 3x3 squares must all be distinct
    as_grid(Rows, R1, R2, R3, R4, R5, R6, R7, R8, R9)
    blocks(R1, R2, R3)
    blocks(R4, R5, R6)
    blocks(R7, R8, R9)

    % Labeling triggers the search/instantiation
    label(Vs).

% names the nine rows of a 9x9 grid
as_grid([R1,R2,R3,R4,R5,R6,R7,R8,R9], R1, R2, R3, R4, R5, R6, R7, R8, R9).

blocks([], [], []).
blocks([A,B,C|Bs1], [D,E,F|Bs2], [G,H,I|Bs3]) :-
    all_distinct([A,B,C,D,E,F,G,H,I])
    blocks(Bs1, Bs2, Bs3).