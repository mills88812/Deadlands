namespace Deadlands;

public static class NomadExtension
{
    private static readonly ConditionalWeakTable<Player, NomadData> _ndctw = new();

    public static NomadData Nomad(this Player player) => _ndctw.GetValue(player, _ => new NomadData(player));

    public static bool IsNomad(this Player player) => player.Nomad().IsNomad;

    public static bool IsNomad(this Player player, out NomadData nomad)
    {
        nomad = player.Nomad();
        return nomad.IsNomad;
    }
}