using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum DIRECTION
{
    RIGHT = 0,
    UP = 90,
    LEFT = 180,
    DOWN = 270,
    NONE = 1
}

public enum CLASS
{
    VANGUARD = 1,
    MEDIC = 2,
    DEFENDER = 4,
    GUARD = 8,
    SNIPER = 16,
    CASTER = 32,
    SUPPORTER = 64,
    SPECIALIST = 128
}

public enum STAT
{
    HITPOINTS = 1,
    ATTACK = 2,
    DEFENSE = 4,
    RESISTANCE = 8,
    ATTACK_SPEED = 16,
    DODGE = 32,
    MOVE_SPEED = 64,
    BLOCK = 128,
    PHYSICAL_DAMAGE_MULTIPLIER = 256,
    ARTS_DAMAGE_MULTIPLIER = 512,
    DAMAGE_MULTIPLIER = 1024,
    UNIQUE = 2048
}

public enum DAMAGE_TYPE
{
    PHYSICAL = 1,
    ARTS = 2,
    PURE = 4
}

public static class Constants
{
    public static float NODE_WIDTH = 5f;
    public static int COLLISION_LAYER = 11;
    public static int NO_COLLISION_LAYER = 0;
    public static float SLOW_TIME_SCALE = 0.2f;
    public static float NORMAL_TIME_SCALE = 1.0f;
    public static float DOUBLE_TIME_SCALE = 2.0f;

    public static bool DISPLAY_DAMAGE_NUMBER = true;
    public static bool DONT_DISPLAY_DAMAGE_NUMBER = false;
}

public static class Buffs
{
    public static StatBuff ChannelerCasterDEFBuff = new StatBuff("Channeler Caster DEF", STAT.DEFENSE, 0f, 2.00f, 9999f);
    public static StatBuff ChannelerCasterRESBuff = new StatBuff("Channeler Caster RES", STAT.RESISTANCE, 20f, 0f, 9999f);
}