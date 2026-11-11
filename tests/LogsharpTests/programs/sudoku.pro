% sudoku(Rows) : Rows is a 9x9 grid satisfying the sudoku constraints.
%
% adapted to this interpreter grammar :
%   - goals in a rule body are separated by spaces, not commas
%   - no directives : the ":- use_module(library(clpfd))." line was dropped
%   - no infix operators : "Vs ins 1..9" is written ins(Vs, 1, 9)
%   - "Rows = [R1,...,R9]" cannot use the expression parser (no list
%     operands), so as_grid/10 names the rows instead.
%
% clpfd-like builtins : length/2, maplist/2, append/2, same_length/2, ins/3,
% all_distinct/1, transpose/2, label/1
sudoku(Rows) :-
    length(Rows, 9)
    maplist(same_length(Rows), Rows)
    append(Rows, Vs)
    ins(Vs, 1, 9)

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
