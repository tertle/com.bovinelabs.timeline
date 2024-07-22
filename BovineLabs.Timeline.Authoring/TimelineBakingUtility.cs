// <copyright file="TimelineBakingUtility.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    public static class TimelineBakingUtility
    {
        public static string TrackToIdentifier(DOTSTrack dotsTrack)
        {
            return dotsTrack.name;
        }
    }
}
