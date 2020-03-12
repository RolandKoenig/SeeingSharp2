/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using SeeingSharp.Util;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    // Overview Feature levels:
    //http://msdn.microsoft.com/en-us/library/windows/desktop/ff476876(v=vs.85).aspx

    // Information on WARP
    //http://msdn.microsoft.com/en-us/library/windows/desktop/gg615082(v=vs.85).aspx#capabilities

    /// <summary>
    /// All initialization logic for the D3D11 device
    /// </summary>
    public class DeviceHandlerD3D11
    {
        // Resources from Direct3D11 api
        private Adapter1 _dxgiAdapter;
        private D3D11.Device1 _device1;
        private D3D11.Device3 _device3;
        private D3D11.DeviceContext _immediateContext;
        private D3D11.DeviceContext3 _immediateContext3;

        // Parameters of created device
        private D3D11.DeviceCreationFlags _creationFlags;
        private D3D.FeatureLevel _featureLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerD3D11"/> class.
        /// </summary>
        internal DeviceHandlerD3D11(GraphicsDeviceConfiguration deviceConfig, Adapter1 dxgiAdapter)
        {
            _dxgiAdapter = dxgiAdapter;

            // Define possible create flags
            var createFlags = D3D11.DeviceCreationFlags.BgraSupport;
            if (deviceConfig.CoreConfiguration.DebugEnabled)
            {
                createFlags |= D3D11.DeviceCreationFlags.Debug;
            }

            // Define all steps on which we try to initialize Direct3D
            var initParameterQueue =
                new List<Tuple<D3D.FeatureLevel, D3D11.DeviceCreationFlags, HardwareDriverLevel>>();

            // Define all tries for hardware initialization
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_11_1, createFlags, HardwareDriverLevel.Direct3D11));
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_11_0, createFlags, HardwareDriverLevel.Direct3D11));
            initParameterQueue.Add(Tuple.Create(
                D3D.FeatureLevel.Level_10_0, createFlags, HardwareDriverLevel.Direct3D10));

            // Try to create the device, each defined configuration step by step
            foreach (var (actFeatureLevel, actCreateFlags, actDriverLevel) in initParameterQueue)
            {
                try
                {
                    // Try to create the device using current parameters
                    using (var device = new D3D11.Device(dxgiAdapter, actCreateFlags, actFeatureLevel))
                    {
                        _device1 = device.QueryInterface<D3D11.Device1>();
                        _device3 = SeeingSharpUtil.TryExecute(() => _device1.QueryInterface<D3D11.Device3>());

                        if (_device3 != null)
                        {
                            _immediateContext3 = _device3.ImmediateContext3;
                        }
                    }

                    // Device successfully created, save all parameters and break this loop
                    _featureLevel = actFeatureLevel;
                    _creationFlags = actCreateFlags;
                    this.DriverLevel = actDriverLevel;
                    break;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            // Throw exception on failure
            if (_device1 == null)
            {
                throw new SeeingSharpGraphicsException("Unable to initialize d3d11 device!");
            }

            // Get immediate context from the device
            _immediateContext = _device1.ImmediateContext;
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        public void UnloadResources()
        {
            _immediateContext = SeeingSharpUtil.DisposeObject(_immediateContext);
            _immediateContext3 = SeeingSharpUtil.DisposeObject(_immediateContext3);
            _device1 = SeeingSharpUtil.DisposeObject(_device1);
            _device3 = SeeingSharpUtil.DisposeObject(_device3);

            _creationFlags = D3D11.DeviceCreationFlags.None;
            _featureLevel = D3D.FeatureLevel.Level_11_0;
        }

        /// <summary>
        /// Is the hardware Direct3D 11 or upper?
        /// </summary>
        public bool IsDirect3D11OrUpperHardware =>
            _featureLevel == D3D.FeatureLevel.Level_11_0 ||
            _featureLevel == D3D.FeatureLevel.Level_11_1;

        /// <summary>
        /// Is device successfully initialized?
        /// </summary>
        public bool IsInitialized => _device1 != null;

        /// <summary>
        /// Gets a short description containing info about the created device.
        /// </summary>
        public string DeviceModeDescription
        {
            get
            {
                if (_device1 == null) { return "None"; }

                return _dxgiAdapter + " - " + _featureLevel;
            }
        }

        /// <summary>
        /// Gets the driver level.
        /// </summary>
        public HardwareDriverLevel DriverLevel { get; }

        /// <summary>
        /// Gets current feature level.
        /// </summary>
        internal D3D.FeatureLevel FeatureLevel => _featureLevel;

        /// <summary>
        /// Gets the Direct3D 11 device.
        /// </summary>
        internal D3D11.Device1 Device1 => _device1;

        internal D3D11.Device3 Device3 => _device3;

        /// <summary>
        /// Gets the immediate context.
        /// </summary>
        internal D3D11.DeviceContext ImmediateContext => _immediateContext;

        /// <summary>
        /// Gets the immediate context.
        /// </summary>
        internal D3D11.DeviceContext3 ImmediateContext3 => _immediateContext3;
    }
}