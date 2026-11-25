using UnityEngine;

/// <summary>Correndo no chão.</summary>
public class PlayerRunState : State
{
    PlayerController p;
    public PlayerRunState(PlayerController player) { p = player; }

    public override void Enter() { p.PlayAnim("run"); }

    public override void PhysicsUpdate()
    {
        p.ApplyGravity(Time.fixedDeltaTime);
        p.ApplyHorizontal(Time.fixedDeltaTime, p.InputX, p.stats.groundAccel, p.stats.groundDecel);
    }

    public override void LogicUpdate()
    {
        if (!p.IsGrounded()) { p.coyoteTimer = p.stats.coyoteTime; machine.ChangeState("fall"); return; }
        if (Mathf.Abs(p.InputX) < 0.01f && Mathf.Abs(p.rb.velocity.x) < 0.05f) { machine.ChangeState("idle"); return; }
        if (Controles.PularApertou) { machine.ChangeState("jump"); return; }
        if (Controles.AtacarApertou) { machine.ChangeState("attack"); return; }
        if (Controles.ImpulsoApertou && p.CanDashInput) { machine.ChangeState("dash"); return; }
    }
}
