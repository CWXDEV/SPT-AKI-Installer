﻿using System.Collections.Generic;
using SPTInstaller.Models.Mirrors;

namespace SPTInstaller.Models;

public class InternalData
{
    /// <summary>
    /// The folder to install SPT into
    /// </summary>
    public string? TargetInstallPath { get; set; }

    /// <summary>
    /// The orginal EFT game path
    /// </summary>
    public string? OriginalGamePath { get; set; }

    /// <summary>
    /// The original EFT game version
    /// </summary>
    public string OriginalGameVersion { get; set; }

    /// <summary>
    /// Patcher zip file info
    /// </summary>
    public FileInfo PatcherZipInfo { get; set; }

    /// <summary>
    /// SPT-AKI zip file info
    /// </summary>
    public FileInfo AkiZipInfo { get; set; }

    /// <summary>
    /// The release download link for SPT-AKI
    /// </summary>
    public string AkiReleaseDownloadLink { get; set; }

    /// <summary>
    /// The release zip hash
    /// </summary>
    public string AkiReleaseHash { get; set; } = null;

    /// <summary>
    /// The release download link for the patcher mirror list
    /// </summary>
    public string PatcherMirrorsLink { get; set; }

    /// <summary>
    /// Whether or not a patch is needed to downgrade the client files
    /// </summary>
    public bool PatchNeeded { get; set; }
}