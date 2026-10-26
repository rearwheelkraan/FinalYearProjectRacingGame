using System.Runtime.CompilerServices;
using UnityEngine.Lumin;

public struct TorqueCurve
{
    public float a;
    public float b;
    public float c;
    public float d;
    public float e;
    public float f;

    public TorqueCurve(
        float inputA,
        float inputB,
        float inputC,
        float inputD,
        float inputE,
        float inputF
    )
    {
        a = inputA;
        b = inputB;
        c = inputC;
        d = inputD;
        e = inputE;
        f = inputF;
    }

    public float GetTorqueAtPoint(float rpm)
    {
        return a / b * UnityEngine.Mathf.Pow(((c / d * rpm) + e), 2) + f;
    }
}
