using PromeRotation.Data;
using PromeRotation.Rotation;

namespace Wotou.Bard.Opener;

public class BardOpener: IOpener
{
    public string OpenerName => "Test Opener";
    public void InitializeCountdown(CountDownHandler countdownHandler)
    {
    }

    public List<PAction> InCombatSequence => new()
    {
    };
}
