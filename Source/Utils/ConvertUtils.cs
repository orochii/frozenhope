using System;
using Godot;

public static class ConvertUtils {
    const float FACTOR = 20;
    public static float DbToPerc(float db) {
        float perc = Mathf.Pow(10, db/FACTOR);
        return perc;
    }
    public static float PercToDb(float perc) {
        if (perc <= 0) return -80f;
        float p = perc / 100f;
        float db = FACTOR * MathF.Log10(p);
        return db;
    }
}
