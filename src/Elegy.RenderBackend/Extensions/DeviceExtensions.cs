
using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderBackend.Extensions
{
    public static class DeviceExtensions
    {
        public static DeviceBuffer CreateBufferFromList<T>(this GraphicsDevice device, BufferUsage usage, List<T> list) where T : unmanaged
        {
            ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(list);
            var buffer = device.ResourceFactory.CreateBufferForList(usage, list);
            device.UpdateBuffer(buffer, 0, span);
            return buffer;
        }

        public static DeviceBuffer CreateBufferFromStruct<T>(this GraphicsDevice device, BufferUsage usage, ref T obj) where T : unmanaged
        {
            var buffer = device.ResourceFactory.CreateBufferForStruct<T>(usage);
            device.UpdateBuffer(buffer, 0, ref obj);
            return buffer;
        }
    }
}
