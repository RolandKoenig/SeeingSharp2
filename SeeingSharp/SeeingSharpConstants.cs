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
using System;

namespace SeeingSharp
{
    public static class SeeingSharpConstants
    {
        // Component group names
        public const string COMPONENT_GROUP_CAMERA = "Camera";

        // Rendering constants
        public const double MINIMUM_FRAME_TIME_MS = 1000.0 / 60.0;
        public const double MINIMUM_DELAY_TIME_MS = 0.0;
        public const int INPUT_FRAMES_PER_SECOND = 60;
        public const int MAX_PER_FRAME_TIME_VALUE = 10000;

        // View constants
        public const int MIN_VIEW_WIDTH = 32;
        public const int MIN_VIEW_HEIGHT = 32;

        // Constants for time duration measuring
        public const string PERF_GLOBAL_PER_FRAME = "Graphics.Global.OneFrame";
        public const string PERF_GLOBAL_WAIT_TIME = "Graphics.Global.WaitTime";
        public const string PERF_GLOBAL_UPDATE_AND_PREPARE = "Graphics.Global.UpdateAndPrepare";
        public const string PERF_GLOBAL_UPDATE_SCENE = "Graphics.Global.Update (Scene: {0})";
        public const string PERF_GLOBAL_RENDER_AND_UPDATE_BESIDE = "Graphics.Global.RenderAndUpdateBeside";
        public const string PERF_GLOBAL_RENDER_DEVICE = "Graphics.Global.Render (Device: {0})";
        public const string PERF_GLOBAL_UPDATE_BESIDE = "Graphics.Global.UpdateBeside (Scene: {0})";
        public const string PERF_RENDERLOOP_PRESENT = "Graphics.RenderLoop.Present (Scene: {0}, View: {1})";
        public const string PERF_RENDERLOOP_RENDER = "Graphics.RenderLoop.Render (Scene: {0}, View: {1})";
        public const string PERF_RENDERLOOP_RENDER_2D = "Graphics.RenderLoop.Render.2D (Scene: {0}, View: {1})";
        public const long UPDATE_DEFAULT_CYCLES_TICKS = 500000;

        // Constants for animation system
        public static readonly TimeSpan UPDATE_STATE_MAX_TIME = new TimeSpan(0, 0, 0, 0, int.MaxValue);
        public static readonly TimeSpan UPDATE_DEFAULT_CYLCE = new TimeSpan(UPDATE_DEFAULT_CYCLES_TICKS);
    }
}