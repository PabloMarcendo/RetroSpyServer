﻿using System;
namespace ServerBrowser.Entity.Structure
{
    public class SBStringFlag
    {
        public static readonly byte[] AllServerEndFlag = { 0, 255, 255, 255, 255 };
        public static readonly byte StringSpliter = 0;
        public static readonly byte NTSStringFlag = 0xFF;

    }
}
