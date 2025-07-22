

namespace WindTurbine
{
    public interface IWindTurbine
    {
        void Initialize(TurbineData data);
        float GetCurrentSpeed();
        int GetTurbineID();
    }
}
