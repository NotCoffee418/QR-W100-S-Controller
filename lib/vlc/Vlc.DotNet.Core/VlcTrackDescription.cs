﻿using System;
using System.Runtime.InteropServices;
using Vlc.DotNet.Core.Interops.Signatures.LibVlc.MediaPlayer;

namespace Vlc.DotNet.Core
{
    /// <summary>
    /// VlcTrackDescription class
    /// </summary>
    public sealed class VlcTrackDescription
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public VlcTrackDescription Next { get; private set; }

        internal VlcTrackDescription(TrackDescription trackDescription)
        {
            Name = IntPtrExtensions.ToStringAnsi(trackDescription.name);
            Id = trackDescription.id;
            if (trackDescription.next != IntPtr.Zero)
            {
#if SILVERLIGHT
                var next = new TrackDescription();
                Marshal.PtrToStructure(trackDescription.next, next);
#else
                var next = (TrackDescription)Marshal.PtrToStructure(trackDescription.next, typeof(TrackDescription));
#endif
                Next = new VlcTrackDescription(next);
            }

        }
    }
}
