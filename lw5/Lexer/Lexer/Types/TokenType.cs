namespace Lexer.Types
{
    public enum TokenType
    {
        Unknown,
        Identifier, // <- string

        Keyword, // <- string
                 //BEGIN
                 //END
                 //READ
                 //WRITE
                 //GET
                 //CONST
                 //LET
                 //VAR
                 //IF
                 //ELSE
                 //WHILE
                 //THEN
                 //DO
                 //FOR
                 //TRUE
                 //FALSE
                 //INTEGER
                 //STRING

        Number,// <- char
               // {1234567890}

        Separator,// <- char
                  // _, (, ), ;

        // arithmetic:
        //
        Addition,// <- char
                 // +

        Subtraction,// <- char
                    // -

        Multiplication,// <- char
                       // *

        Division,// <- char
                 // /

        Equating,// <- char
                 // =

        // comparison:
        //
        LogicEquivalent,// <- string
                        // ==

        LogicNotEquivalent,// <- string
                           // !=

        LogicLess,// <- char
                  // <

        LogicMore,// <- char
                  // >

        LogicLessOrEquivalent,// <- string
                              // <=

        LogicMoreOrEquivalent,// <- string
                              // >=

        // logical:
        //
        LogicAnd,// <- string
                 // &&

        LogicOr,// <- string
                // ||

        // string :
        QuotationMark,// <- char
                      // "

        Comment,// <- string
    }
}
