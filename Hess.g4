grammar Hess;

// Top-level rules
program: defineBoard line* startGame;

// Statement, conditional, and loop rules
line: (assignment) ';';

// Board definition and game start rules
defineBoard: 'BOARD(' BOARDPOSITION ')' ';';

startGame: 'STARTGAME(' IDENTIFIER ',' IDENTIFIER ')';

// Assignment rules
assignment:
    IDENTIFIER '=' moveList //PIECE ASSIGNMENT
    | IDENTIFIER '=' player //PLAYER ASSIGNMENT
    | IDENTIFIER '=' place //PLACE ASSIGNMENT
    | IDENTIFIER '=' constant; //NN ASSIGNMENT

// Move-related rules [KINDA SOM ET ARRAY]
moveList: '{' move (',' move)* '}';
move: movetype collision attacktype direction moveExtra;
moveExtra:
    NATURAL_NUMBER
    | NATURAL_NUMBER direction NATURAL_NUMBER
    | direction NATURAL_NUMBER;

constant: NATURAL_NUMBER;

// Player and board position rules [KINDA SOM ET ARRAY]
player: '{' placeType (',' placeType)* '}';
place: 'PLACE(' IDENTIFIER ',' boardpositionlist ')';
placeType: place | IDENTIFIER;
boardpositionlist: '{' BOARDPOSITION (',' BOARDPOSITION)* '}';

// Lexer rules for identifiers and literals
BOARDPOSITION: CAP_LETTER [1-9][0-9]*;
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
NATURAL_NUMBER: [1-9][0-9]*;
CAP_LETTER: [A-Z];

// Move-related keywords
movetype: 'Direct' | 'Path';
collision: 'true' | 'false';
attacktype: 'ATTACK' | 'MOVE' | 'ATKMOVE';
direction: 'UP' | 'LEFT' | 'RIGHT' | 'DOWN';

// Lexer rule for whitespace
WS: [ \t\r\n]+ -> skip;