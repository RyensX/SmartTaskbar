﻿namespace SmartTaskbar;

internal struct UserConfiguration
{
    public AutoModeType AutoModeType { get; set; }

    public bool ShowTaskbarWhenExit { get; set; }

    public bool AlignLeftWhenTheMouseIsLeft { get; set; }
}