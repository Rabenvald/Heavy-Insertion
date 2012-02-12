using System.Collections;

public static class GameUtils {

    public static bool Toggle(bool b)
    {
        if (b == false)
            b = true;
        else b = false;
        return b;
    }

}
