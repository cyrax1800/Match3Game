public enum Direction
{
    NONE = -1,
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public enum ItemColor
{
    RANDOM = -1,
    RED,
    BLUE,
    GREEN,
    YELLOW,
    PURPLE,
    WHITE
}

public enum ItemType
{
    NONE,
    VERTICAL, // 4 line
    HORIZONTAL, // 4 line
    AIRPLANE, // 4 square
    BOMB, // 5 ( 2 hor, 2 ver)
    DYNAMITE, // 6
    COLOR_BOMB, // 5 line
    BIOHAZARD, // 7
    DROP
}

public enum GameState
{
    Start,
    Initialize,
    Playing,
    GameOver
}