using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValueInterpolator
{
    public static float MoveTowards(float current, float target, float speed, float deltaTime)
    {
        float step = speed * deltaTime;

        return Mathf.MoveTowards(current, target, step);
    }

    public static int GetDamage(int current, int armor, int resistence, int damage)
    {
        int calculatedDamage = damage - armor - (damage * resistence);

        return calculatedDamage;
    }
}
