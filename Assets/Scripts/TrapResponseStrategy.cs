using UnityEngine;

public enum TrapResponseMode
{
    Lethal,
    WarningOnly
}

public interface ITrapResponseStrategy
{
    string Name { get; }
    void Execute(PlayerRespawn player, DeathTrap source);
}

public sealed class LethalTrapResponseStrategy : ITrapResponseStrategy
{
    public string Name => "Lethal";

    public void Execute(PlayerRespawn player, DeathTrap source)
    {
        UmbraGameEvents.PublishInteraction("Estrategia letal en " + source.name);
        player.Die();
    }
}

public sealed class WarningTrapResponseStrategy : ITrapResponseStrategy
{
    public string Name => "WarningOnly";

    public void Execute(PlayerRespawn player, DeathTrap source)
    {
        UmbraGameEvents.PublishInteraction("Estrategia de advertencia en " + source.name);
        UmbraAudio.Instance?.PlayScare();
        CameraFollow2D cameraFollow = Camera.main != null
            ? Camera.main.GetComponent<CameraFollow2D>()
            : null;
        cameraFollow?.AddTrauma(0.65f);
    }
}
