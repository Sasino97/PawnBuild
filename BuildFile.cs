/*
 * *********************************************************************
 *                              PawnBuild
 * *********************************************************************
 *                  Copyright (c) 2021 - Sasinosoft Games
 * *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * *********************************************************************
 */
namespace PawnBuild
{
    public class BuildFile
    {
        public string ProjectName { get; set; } = "project";
        public BuildFolder[] BuildFolders { get; set; }
        public string[] IncludeFolders { get; set; }
        public string[] Files { get; set; }
        public string[] Run { get; set; }
        public string Args { get; set; }
    }
}
