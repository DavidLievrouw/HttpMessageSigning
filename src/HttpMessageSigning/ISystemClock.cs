﻿using System;

 namespace Dalion.HttpMessageSigning {
    internal interface ISystemClock {
        DateTimeOffset UtcNow { get; }
    }
}