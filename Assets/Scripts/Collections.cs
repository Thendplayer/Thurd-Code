public enum S_Direction
{
    Right,
    Left,
    Up,
    Down,
    Custom
}

public enum S_PlayerState
{
    Spawn,
    Idle,
    Run,
    Jump,
    Die
}

public enum S_JumpState
{
    Grounded,
    Single,
    Double,
    Climb,
    WallJump,
    Fall,
}

public enum S_ShieldState
{
    Hide,
    Protect,
    Released,
    Returning,
    Platform,
    Parry
}

public enum S_GameState
{
    Menu,
    Play,
    Pause
}

public enum S_Parry
{
    JumpHorizontal,
    JumpVertical,
    GroundHorizontal,
    GroundVertical
}

public enum S_Door
{
    Closed,
    Opening,
    Opened
}

public enum S_PlayerAnimation
{
    Idle,
    Run,
    IdleBlock,
    IdleBlockDirect,
    WalkBlock,
    BlockProjectile,
    Jump,
    DoubleJump,
    Climb,
    Throw,
    ParryJump,
    ParryGround,
    Fall,
    Crouch,
    Damage,

    Death,

    IdleNoShield,
    RunNoShield,
    JumpNoShield,
    DoubleJumpNoShield,
    ClimbNoShield,
    FallNoShield,
    DamageNoShield,
    CrouchNoShield
}