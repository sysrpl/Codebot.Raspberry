// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Raspberry.Board.Spi;

namespace Raspberry.Device
{
    public sealed class Ws28xx : IDisposable, IEnumerable<NeoPixel>
    {
        List<NeoPixel> pixels;
        SpiDevice device;
        NeoPixelData data;

        public Ws28xx(int count)
        {
            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 2_400_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };
            device = SpiDevice.Create(settings);
            pixels = new List<NeoPixel>();
            Count = count;
        }

        public int Count
        {
            get => pixels.Count;
            set 
            {
                if (value == pixels.Count)
                    return;
                while (pixels.Count < value)
                    pixels.Add(new NeoPixel());
                if (pixels.Count > value)
                    pixels.RemoveRange(pixels.Count, value - pixels.Count);
                data = new NeoPixelData(value);
                NeoPixel p;
                for (var i = 0; i < pixels.Count; i++)
                {
                    p = pixels[i];
                    data.SetPixel(i, p.Color);
                    p.Changed = false;
                }
            }
        }

        public NeoPixel this[int index] 
        { 
            get => pixels[index]; 
            set => pixels[index].Color = value.Color;
        }

        public void Reset() 
        {
            foreach (var p in pixels) p.Color = Color.Black;
        }

        public void Update()
        {
            NeoPixel p;
            for (var i = 0; i < pixels.Count; i++)
            {
                p = pixels[i];
                if (p.Changed)
                {
                    data.SetPixel(i, p.Color);
                    p.Changed = false;
                }
            }
            device.Write(data.Data);
        }

        #region interfaces
        bool disposed;

        public void Dispose()
        {
            if (!disposed)
                device.Dispose();
            disposed = true;
        }

        public IEnumerator<NeoPixel> GetEnumerator()
        {
            return ((IEnumerable<NeoPixel>)pixels).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<NeoPixel>)pixels).GetEnumerator();
        }
        #endregion
    }
}
