using Iot.Device.Modbus.Structures;
using System;

namespace Iot.Device.Modbus.Server
{
    public abstract class ModbusDevice
    {
        public ModbusDevice(byte id = Consts.MinDeviceId)
        {
            if (id < Consts.MinDeviceId || id > Consts.MaxDeviceId)
                throw new ArgumentNullException(nameof(id));

            DeviceId = id;
        }

        public virtual byte DeviceId { get; protected set; }

        protected abstract bool TryReadDiscreteInput(ushort address, out bool value);
        protected abstract bool TryReadInputRegister(ushort address, out ushort value);

        protected abstract bool TryReadCoil(ushort address, out bool value);
        protected abstract bool TryWriteCoil(ushort address, bool value);

        protected abstract bool TryReadHoldingRegister(ushort address, out ushort value);
        protected abstract bool TryWriteHoldingRegister(ushort address, ushort value);

        internal protected virtual void BeginRead() { }
        internal protected virtual void EndRead() { }

        internal protected virtual void BeginWrite() { }
        internal protected virtual void EndWrite() { }

        internal bool GetDiscreteInput(ushort address, out DiscreteInput discreteInput)
        {
            discreteInput = null;

            var result = TryReadDiscreteInput(address, out bool value);
            if (result)
                discreteInput = new DiscreteInput { Address = address, Value = value };

            return result;
        }

        internal bool GetInputRegister(ushort address, out InputRegister inputRegister)
        {
            inputRegister = null;

            var result = TryReadInputRegister(address, out ushort value);
            if (result)
                inputRegister = new InputRegister() { Address = address, Value = value };

            return result;
        }

        internal bool GetCoil(ushort address, out Coil coil)
        {
            coil = null;

            var result = TryReadCoil(address, out bool value);
            if (result)
                coil = new Coil { Address = address, Value = value };

            return result;
        }

        internal bool SetCoil(ushort address, bool value)
        {
            return TryWriteCoil(address, value);
        }

        internal bool GetHoldingRegister(ushort address, out HoldingRegister holdingRegister)
        {
            holdingRegister = null;

            var result = TryReadHoldingRegister(address, out ushort value);
            if (result)
                holdingRegister = new HoldingRegister { Address = address, Value = value };

            return result;
        }

        internal bool SetHoldingRegister(ushort address, ushort value)
        {
            return TryWriteHoldingRegister(address, value);
        }
    }
}
