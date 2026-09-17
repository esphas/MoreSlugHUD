using UnityEngine;

namespace MoreSlugHUD;

internal sealed class PyroRecovery
{
    private int _heat = -1;
    private float _cooldown;
    private float _span = 150f;
    private bool _known;

    internal void Observe(int heat, float cooldown)
    {
        if (heat <= 0)
        {
            _heat = 0;
            _cooldown = cooldown;
            _span = 150f;
            _known = true;
            return;
        }

        if (_heat < 0)
        {
            if (cooldown > 60f)
            {
                _span = 150f;
                _known = true;
            }
            else
            {
                _span = Mathf.Max(1f, cooldown);
                _known = false;
            }
        }
        else if (heat > _heat || cooldown > _cooldown + 0.5f)
        {
            _span = Mathf.Max(1f, cooldown);
            _known = true;
        }
        else if (heat < _heat)
        {
            _span = Mathf.Max(1f, cooldown);
            _known = true;
        }

        _heat = heat;
        _cooldown = cooldown;
    }

    internal float Fill
    {
        get
        {
            if (_heat <= 0)
            {
                return 1f;
            }

            if (!_known)
            {
                return 0f;
            }

            return 1f - Mathf.Clamp01(_cooldown / _span);
        }
    }

    internal void Reset()
    {
        _heat = -1;
        _cooldown = 0f;
        _span = 150f;
        _known = false;
    }
}
