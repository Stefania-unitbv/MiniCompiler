grammar MiniLang;
@namespace{Tema2LFC.Grammar}

program: (globalDeclaration | functionDeclaration | structDeclaration)* EOF;

// Declarations
globalDeclaration: typeSpecifier IDENTIFIER ('=' expression)? ';';
functionDeclaration: typeSpecifier IDENTIFIER '(' parameterList? ')' block;
structDeclaration: 'struct' IDENTIFIER '{' structMember* '}' ';';

structMember
    : typeSpecifier IDENTIFIER ';'                           # StructField
    | typeSpecifier IDENTIFIER '(' parameterList? ')' block  # StructMethod
    | IDENTIFIER '(' parameterList? ')' block                # StructConstructor
    | '~' IDENTIFIER '(' ')' block                          # StructDestructor
    ;

// Parameters and Variables
parameterList: parameter (',' parameter)*;
parameter: typeSpecifier IDENTIFIER;
variableDeclaration: typeSpecifier IDENTIFIER ('=' expression)? ';';

// Types
typeSpecifier: 'int' | 'float' | 'double' | 'string' | 'void';

// Statements
block: '{' statement* '}';
statement
    : block
    | variableDeclaration
    | expressionStatement
    | ifStatement
    | whileStatement
    | forStatement
    | returnStatement
    | ';'
    ;

ifStatement: 'if' '(' expression ')' statement ('else' statement)?;
whileStatement: 'while' '(' expression ')' statement;
forStatement: 'for' '(' forInit? ';' expression? ';' forUpdate? ')' statement;
returnStatement: 'return' expression? ';';

forInit
    : variableDeclarationNoSemi  // new rule for 'for loop' without  ;
    | expressionNoSemi           // new rule for 'for loop' without  ;
    | /* empty */
    ;

//  expresssions and declarations without  ;
variableDeclarationNoSemi: typeSpecifier IDENTIFIER ('=' expression)?;
expressionNoSemi: expression;

// Restul regulilor rămân la fel, dar modificăm forUpdate să folosească expressionNoSemi
forUpdate: expressionNoSemi;


expressionStatement: expression ';';

// Expressions
expression
    : primary                                                # PrimaryExpr
    | expression op=('++' | '--')                           # PostfixExpr
    | op=('++' | '--') expression                          # PrefixExpr
    | op=('!' | '-') expression                            # UnaryExpr
    | expression op=('*' | '/' | '%') expression           # MultiplicativeExpr
    | expression op=('+' | '-') expression                 # AdditiveExpr
    | expression op=('<' | '>' | '<=' | '>=') expression   # RelationalExpr
    | expression op=('==' | '!=') expression               # EqualityExpr
    | expression '&&' expression                           # LogicalAndExpr
    | expression '||' expression                           # LogicalOrExpr
    | <assoc=right> expression
      op=('=' | '+=' | '-=' | '*=' | '/=' | '%=')
      expression                                           # AssignmentExpr
    ;

primary
    : IDENTIFIER                                # IdentifierExpr
    | IDENTIFIER '(' argumentList? ')'          # FunctionCall
    | INTLITERAL                               # IntLiteral
    | FLOATLITERAL                             # FloatLiteral
    | STRINGLITERAL                            # StringLiteral
    | '(' expression ')'                       # ParenExpr
    ;

argumentList: expression (',' expression)*;

// Lexer Rules
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
INTLITERAL: [0-9]+;
FLOATLITERAL: [0-9]+ '.' [0-9]* | '.' [0-9]+;
STRINGLITERAL: '"' (~["\r\n])* '"';

// Comments
COMMENT: '//' ~[\r\n]* -> skip;
MULTILINE_COMMENT: '/*' .*? '*/' -> skip;

// Whitespace
WS: [ \t\r\n]+ -> skip;