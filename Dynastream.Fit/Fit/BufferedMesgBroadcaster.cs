#region Copyright
/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2023 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////
// ****WARNING****  This file is auto-generated!  Do NOT edit this file.
// Profile Version = 21.105Release
// Tag = production/release/21.105.00-0-gdc65d24
/////////////////////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

using Dynastream.Utility;
using Dynastream.Fit;

namespace Dynastream.Fit
{
    public delegate void MesgBroadcastEventHandler(object sender, MesgBroadcastEventArgs e);
    public delegate void IncomingMesgEventHandler(object sender, IncomingMesgEventArgs e);

    public class MesgBroadcastEventArgs : EventArgs
    {
        public List<Mesg> mesgs = null;

        public MesgBroadcastEventArgs()
        {
        }

        public MesgBroadcastEventArgs(List<Mesg> newMesgs)
        {
            mesgs = newMesgs;
        }
    }

    public class IncomingMesgEventArgs : EventArgs
    {
        public Mesg mesg = null;

        public IncomingMesgEventArgs()
        {
        }

        public IncomingMesgEventArgs(Mesg newMesg)
        {
            mesg = new Mesg(newMesg);
        }
    }

    /// <summary>
    /// <para>
    /// BufferedMesgBroadcaster intercepts the incoming messages
    /// from the given decode stream, buffers them, and offers
    /// an opportunity to edit the messages before broadcasting
    /// the messages to all registered listeners.
    /// </para>
    /// <para>
    /// To edit the messages, an IMesgBroadcastPlugin must be
    /// registered. All registered IMesgBroadcastPlugins are given
    /// the opportunity to see each message as they are decoded,
    /// as well as to see and edit the final list of
    /// messages before broadcast to listeners
    /// </para>
    /// </summary>
    public class BufferedMesgBroadcaster : MesgBroadcaster
    {
        #region Fields
        private List<Mesg> mesgs = new List<Mesg>();
        public event MesgBroadcastEventHandler MesgBroadcastEvent;
        public event IncomingMesgEventHandler IncomingMesgEvent;
        #endregion

        #region Methods

        public void RegisterMesgBroadcastPlugin(IMesgBroadcastPlugin plugin)
        {
            MesgBroadcastEvent += plugin.OnBroadcast;
            IncomingMesgEvent += plugin.OnIncomingMesg;
        }

        public new void OnMesg(object sender, MesgEventArgs e)
        {
            // Notify any subscribers of either our general mesg event or specific profile mesg event
            mesgs.Add(e.mesg);
            if (IncomingMesgEvent != null)
            {
                IncomingMesgEvent(sender, new IncomingMesgEventArgs(e.mesg));
            }
        }

        public void Broadcast()
        {
            if (MesgBroadcastEvent != null)
            {
                MesgBroadcastEvent(this, new MesgBroadcastEventArgs(mesgs));
            }

            foreach (Mesg mesg in mesgs)
            {
                base.OnMesg(this, new MesgEventArgs(mesg));
            }

        }
        #endregion
    }
} // namespace
