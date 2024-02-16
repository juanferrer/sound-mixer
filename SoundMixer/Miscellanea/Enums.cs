using System;
using System.Collections.Generic;
using System.Text;

namespace SoundMixer.Miscellanea
{
    public static class Enums
    {
        public enum ProgramStatus
        {
            Loading,
            Ready
        }

        public enum Setting
        {
            WorkspaceDirectory,
            SoundDirectory,
            OutputDeviceId
        }
    }
}
