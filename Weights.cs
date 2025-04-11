namespace Blaze;

public static class Weights
{
    // files and ranks are reversed, files go from a to h on the y-axis
    
    public static readonly int[,,] Pieces = new[,,]
    {
        { // white pawn
            {0,10,20,10,30,40,100,0},
            {0,25,10,10,30,40,100,0},
            {0,30,15,10,30,40,100,0},
            {0,0,30,70,55,55,100,0},
            {0,0,30,70,55,55,100,0},
            {0,30,15,10,30,40,100,0},
            {0,25,10,10,30,40,100,0},
            {0,10,20,10,30,40,100,0},
        },
        { // white rook
            {20,5,5,5,5,5,55,30},
            {15,10,10,10,10,10,55,30},
            {30,10,10,10,10,10,55,30},
            {30,10,10,10,10,10,55,30},
            {30,10,10,10,10,10,55,30},
            {30,10,10,10,10,10,55,30},
            {30,10,10,10,10,10,55,30},
            {20,5,5,5,5,5,55,30},
        },
        { // white knight
            {-10,0,15,10,10,15,5,-5},
            {0,10,30,20,20,20,10,0},
            {5,15,40,25,20,25,20,0},
            {0,30,35,40,25,30,10,10},
            {0,30,35,40,25,30,10,10},
            {5,15,40,25,20,25,20,0},
            {0,10,30,20,20,20,10,0},
            {-10,0,15,10,10,15,5,-5},
        },
        { // white bishop
            {10,25,20,25,20,15,15,20},
            {-5,50,30,15,40,10,20,-5},
            {-20,15,25,45,20,20,10,-5},
            {-5,30,15,15,20,10,10,0},
            {-5,30,15,15,20,10,10,0},
            {-20,15,25,45,20,20,10,-5},
            {-5,50,30,15,40,10,20,-5},
            {10,25,20,25,20,15,15,20},
        },
        { // white queen
            {-10,10,20,20,20,30,10,-10},
            {0,15,30,10,10,5,15,0},
            {5,20,25,35,35,10,5,0},
            {-5,20,30,40,40,20,0,10},
            {0,20,30,40,40,20,0,0},
            {5,20,25,35,35,10,5,0},
            {0,15,30,10,10,5,15,0},
            {-10,10,20,20,20,30,10,-10},
        },
        { // white king - 5
            {90,20,10,0,-10,-20,-30,-50},
            {100,15,10,15,-5,-15,-25,-35},
            {40,25,20,20,10,0,-15,-30},
            {40,30,25,20,15,5,-15,-30},
            {50,30,25,20,15,5,-15,-30},
            {40,25,20,20,10,0,-15,-30},
            {100,15,10,15,-5,-15,-25,-35},
            {90,20,10,0,-10,-20,-30,-50},
        },
        { // not a valid piece
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
        },
        { // not a valid piece
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
        },
        { // black pawn - 8
            {0,-100,-40,-30,-10,-20,-10,0},
            {0,-100,-40,-30,-10,-10,-25,0},
            {0,-100,-40,-30,-10,-15,-30,0},
            {0,-100,-55,-55,-70,-30,0,0},
            {0,-100,-55,-55,-70,-30,0,0},
            {0,-100,-40,-30,-10,-15,-30,0},
            {0,-100,-40,-30,-10,-10,-25,0},
            {0,-100,-40,-30,-10,-20,-10,0},
        },
        { // black rook
            {-30,-55,-5,-5,-5,-5,-5,-20},
            {-30,-55,-10,-10,-10,-10,-10,-15},
            {-30,-55,-10,-10,-10,-10,-10,-30},
            {-30,-55,-10,-10,-10,-10,-10,-30},
            {-30,-55,-10,-10,-10,-10,-10,-30},
            {-30,-55,-10,-10,-10,-10,-10,-30},
            {-30,-55,-10,-10,-10,-10,-10,-15},
            {-30,-55,-5,-5,-5,-5,-5,-20},
        },
        { // black knight
            {5,-5,-15,-10,-10,-15,0,10},
            {0,-10,-20,-20,-20,-30,-10,0},
            {0,-20,-25,-20,-25,-40,-15,-5},
            {-10,-10,-30,-25,-40,-35,-30,0},
            {-10,-10,-30,-25,-40,-35,-30,0},
            {0,-20,-25,-20,-25,-40,-15,-5},
            {0,-10,-20,-20,-20,-30,-10,0},
            {5,-5,-15,-10,-10,-15,0,10},
        },
        { // black bishop
            {-20,-15,-15,-20,-25,-20,-25,-10},
            {5,-20,-10,-40,-15,-30,-50,5},
            {5,-10,-20,-20,-45,-25,-15,20},
            {0,-10,-10,-20,-15,-15,-30,5},
            {0,-10,-10,-20,-15,-15,-30,5},
            {5,-10,-20,-20,-45,-25,-15,20},
            {5,-20,-10,-40,-15,-30,-50,5},
            {-20,-15,-15,-20,-25,-20,-25,-10},
        },
        { // black queen
            {10,-10,-30,-20,-20,-20,-10,10},
            {0,-15,-5,-10,-10,-30,-15,0},
            {0,-5,-10,-35,-35,-25,-20,-5},
            {-10,0,-20,-40,-40,-30,-20,5},
            {0,0,-20,-40,-40,-30,-20,0},
            {0,-5,-10,-35,-35,-25,-20,-5},
            {0,-15,-5,-10,-10,-30,-15,0},
            {10,-10,-30,-20,-20,-20,-10,10},
        },
        { // black king - 13
            {50,30,20,10,0,-10,-20,-90},
            {35,25,15,5,-15,-10,-15,-100},
            {30,15,0,-10,-20,-20,-25,-40},
            {30,15,-5,-15,-20,-25,-30,-40},
            {30,15,-5,-15,-20,-25,-30,-50},
            {30,15,0,-10,-20,-20,-25,-40},
            {35,25,15,5,-15,-10,-15,-100},
            {50,30,20,10,0,-10,-20,-90},
        },
    };
        public static readonly int[,,] EndgamePieces = new[,,]
    {
        { // white pawn
            {0,5,5,10,20,50,110,0},
            {0,5,5,10,20,50,110,0},
            {0,5,5,10,20,50,110,0},
            {0,5,5,10,20,50,110,0},
            {0,5,5,10,20,50,110,0},
            {0,5,5,10,20,50,110,0},
            {0,5,5,10,20,50,110,0},
            {0,5,5,10,20,50,110,0},
        },
        { // white rook
            {25,5,5,5,5,5,35,50},
            {20,10,10,10,10,10,35,50},
            {20,10,10,10,10,10,35,50},
            {20,10,10,10,10,10,35,50},
            {20,10,10,10,10,10,35,50},
            {20,10,10,10,10,10,35,50},
            {20,10,10,10,10,10,35,50},
            {25,5,5,5,5,5,35,50},
        },
        { // white knight
            {-15,-5,5,15,15,5,-5,-15},
            {-5,5,15,20,20,15,5,-5},
            {5,15,20,35,35,20,15,5},
            {15,20,35,40,40,35,20,15},
            {15,20,35,40,40,35,20,15},
            {5,15,20,35,35,20,15,5},
            {-5,5,15,20,20,15,5,-5},
            {-15,-5,5,15,15,5,-5,-15},
        },
        { // white bishop
            {-15,-5,5,15,15,5,-5,-15},
            {-5,5,15,20,20,15,5,-5},
            {5,15,20,35,35,20,15,5},
            {15,20,35,40,40,35,20,15},
            {15,20,35,40,40,35,20,15},
            {5,15,20,35,35,20,15,5},
            {-5,5,15,20,20,15,5,-5},
            {-15,-5,5,15,15,5,-5,-15},
        },
        { // white queen
            {-15,-5,5,15,15,5,-5,-15},
            {-5,5,15,20,20,15,5,-5},
            {5,15,20,35,35,20,15,5},
            {15,20,35,40,40,35,20,15},
            {15,20,35,40,40,35,20,15},
            {5,15,20,35,35,20,15,5},
            {-5,5,15,20,20,15,5,-5},
            {-15,-5,5,15,15,5,-5,-15},
        },
        { // white king - 5
            {-15,-5,5,15,15,5,-5,-15},
            {-5,5,15,20,20,15,5,-5},
            {5,15,20,35,35,20,15,5},
            {15,20,35,40,40,35,20,15},
            {15,20,35,40,40,35,20,15},
            {5,15,20,35,35,20,15,5},
            {-5,5,15,20,20,15,5,-5},
            {-15,-5,5,15,15,5,-5,-15},
        },
        { // not a valid piece
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
        },
        { // not a valid piece
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
        },
        { // black pawn - 8
            {0,-110,-50,-20,-10,-5,-5,0},
            {0,-110,-50,-20,-10,-5,-5,0},
            {0,-110,-50,-20,-10,-5,-5,0},
            {0,-110,-50,-20,-10,-5,-5,0},
            {0,-110,-50,-20,-10,-5,-5,0},
            {0,-110,-50,-20,-10,-5,-5,0},
            {0,-110,-50,-20,-10,-5,-5,0},
            {0,-110,-50,-20,-10,-5,-5,0},
        },
        { // black rook
            {-50,-35,-5,-5,-5,-5,-5,-25},
            {-50,-35,-10,-10,-10,-10,-10,-20},
            {-50,-35,-10,-10,-10,-10,-10,-20},
            {-50,-35,-10,-10,-10,-10,-10,-20},
            {-50,-35,-10,-10,-10,-10,-10,-20},
            {-50,-35,-10,-10,-10,-10,-10,-20},
            {-50,-35,-10,-10,-10,-10,-10,-20},
            {-50,-35,-5,-5,-5,-5,-5,-25},
        },
        { // black knight
            {15,5,-5,-15,-15,-5,5,15},
            {5,-5,-15,-20,-20,-15,-5,5},
            {-5,-15,-20,-35,-35,-20,-15,-5},
            {-15,-20,-35,-40,-40,-35,-20,-15},
            {-15,-20,-35,-40,-40,-35,-20,-15},
            {-5,-15,-20,-35,-35,-20,-15,-5},
            {5,-5,-15,-20,-20,-15,-5,5},
            {15,5,-5,-15,-15,-5,5,15},
        },
        { // black bishop
            {15,5,-5,-15,-15,-5,5,15},
            {5,-5,-15,-20,-20,-15,-5,5},
            {-5,-15,-20,-35,-35,-20,-15,-5},
            {-15,-20,-35,-40,-40,-35,-20,-15},
            {-15,-20,-35,-40,-40,-35,-20,-15},
            {-5,-15,-20,-35,-35,-20,-15,-5},
            {5,-5,-15,-20,-20,-15,-5,5},
            {15,5,-5,-15,-15,-5,5,15},
        },
        { // black queen
            {15,5,-5,-15,-15,-5,5,15},
            {5,-5,-15,-20,-20,-15,-5,5},
            {-5,-15,-20,-35,-35,-20,-15,-5},
            {-15,-20,-35,-40,-40,-35,-20,-15},
            {-15,-20,-35,-40,-40,-35,-20,-15},
            {-5,-15,-20,-35,-35,-20,-15,-5},
            {5,-5,-15,-20,-20,-15,-5,5},
            {15,5,-5,-15,-15,-5,5,15},
        },
        { // black king - 13
            {15,5,-5,-15,-15,-5,5,15},
            {5,-5,-15,-20,-20,-15,-5,5},
            {-5,-15,-20,-35,-35,-20,-15,-5},
            {-15,-20,-35,-40,-40,-35,-20,-15},
            {-15,-20,-35,-40,-40,-35,-20,-15},
            {-5,-15,-20,-35,-35,-20,-15,-5},
            {5,-5,-15,-20,-20,-15,-5,5},
            {15,5,-5,-15,-15,-5,5,15},
        },
    };
}