public enum EffectType
{
  Link
}

public class Effect
{
  public EffectType type;
  public Unit source;
  public int timeLeft = 5;

  public Effect(EffectType type)
  {
    this.type = type;
  }
}