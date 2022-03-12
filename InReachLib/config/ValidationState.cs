﻿namespace J4JSoftware.InReach;

[Flags]
public enum ValidationState
{
    Unvalidated = 0,

    CredentialsValid = 1 << 0, 
    ImeiValid = 1 << 1,

    Validated = CredentialsValid | ImeiValid
}
