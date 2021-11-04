namespace SeeingSharp.Core
{
    public interface IEngineDeviceResource
    {
        void UnloadResources(EngineDevice device);

        int GetDeviceResourceIndex(EngineDevice device);

        void SetDeviceResourceIndex(EngineDevice device, int resourceIndex);
    }
}
