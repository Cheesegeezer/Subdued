using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaBrowser.Library.Persistance;

namespace Subdued
{
    [Serializable]
    internal class FolderConfigData
    {
        #region constructor
        public FolderConfigData ()
        {
        }

        public FolderConfigData (Guid folderId)
        {
            this.FolderId = folderId.ToString ();
        }

        public FolderConfigData (Guid folderId, string file)
        {
            this.FolderId = folderId.ToString ();
            this.file = file;
            this.settings = XmlSettings<FolderConfigData>.Bind (this, file);
        }
        #endregion

        [SkipField]
        public string   FolderId = Guid.Empty.ToString ();

        public bool     ShowAsText = false;
        public string   Orientation = "Vertical";
        public string   WrapItemList = "When Too Big";
        public string   FolderInfoDisplay = "Overview";
        public string   FolderClearLogosList = "Logo";
        public bool     ShowBackdrop = true;
        public bool     ShowBackdropOverlay = false;
        public bool     ShowNPV = true;
        public bool     ShowTextBG = true;
        public bool     ShowThumbsBG = true;
        public bool     ShowTopPanel = false;
        public bool     ShowSelectedInfo = true;
        public string   SelectedInfoStyle = "Split";
        public bool     EnableWatchedIndicators = true;
        public bool     EnableNewItemIndicator = true;
        public bool     RotateBackdrops = true;
        public bool     EnableVideoBackdrop = true;
        public bool     ShowClearLogos = true;
        public bool     ShowWeather = false;
        public bool     ShowDiscImage = false;

        #region Load / Save Data
        public static FolderConfigData FromFile (Guid folderId, string file)
        {
            return new FolderConfigData (folderId, file);
        }

        public void Save ()
        {
            this.settings.Write ();
        }

        [SkipField]
        string file;

        [SkipField]
        XmlSettings<FolderConfigData> settings;
        
        
        #endregion
    }

    [Serializable]
    internal class ConfigData
    {
        #region constructor
        public ConfigData ()
        {
        }
        public ConfigData (string file)
        {
            this.file = file;
            this.settings = XmlSettings<ConfigData>.Bind (this, file);
        }
        #endregion

        public string   ThemeStyle = "The Original";
        public string   MediaInfoIconsSet = "Color";
        public bool     ShowWatchedOnMovies = false;
        public bool     ShowEndTime = false;
        public bool     AskToQuit = true;
        public bool     ShowMiniMediaInfo = false;
        public bool     ShowPrevNextButtons = true;
        public bool     ShowIndexOfCount = false;
        public bool     RotateBackdropsAllLevels = true;
        public bool     ShowMasterPCIcon = true;
        public bool     ShowClockConfigAtTop = false;
        public string   ScrollSpeed = "Fast";
        public string   NowPlayingStyle = "Backdrop";
        public string   TopPanelStyle = "Double Panel";
        public string   StarRatingStyle = "Off";
        public int      ThumbSpacing = 15;
        public int      SelThumbGrow = 25;
        public string   WatchedIndicatorPosition = "Bottom-Right";
        public int      CoverArtAdjustment = 0;
        public int      UnselectedThumbAlpha = 100;
        public bool     GraduatedAlpha = false;
        public bool     ColorizeStudioImages = false;
        public bool     ColorizeRatingsImages = true;
        public bool     ColorizeCustomRatingsImages = false;
        public bool     ColorizePosterOverlayImages = true;
        public bool     EnableNewItemIndicator = true;
        public bool     EnableQuickPlay = false;
        public bool     ShowWeather = false;
        public bool     ShowDiscImage = false;

        public bool     ShowMediaInfoMediaImage = true;
        public bool     ShowMediaInfoVideoImage = true;
        public bool     ShowMediaInfoHDImage = true;
        public bool     ShowMediaInfoAspectImage = true;
        public bool     ShowMediaInfoAudioImage = true;
        public bool     ShowMediaInfoAudioChannelImage = true;
        public bool     ShowMediaInfoMovieRatingImage = true;

        public bool     CoverWallRootBackdrop = false;
        public bool     CoverWallScreenSaver = false;
        public string   CoverWallStyle = "Down";
        public string   CoverWallScroll = "Undefined";
        public string   CoverWallScrollSpeed = "Slow";
        public string   CoverWallRotation = "None";
        public int      CoverWallSSTimeout = 5;

        public List<string> CoverWallBackdropFolders = new List<string> ();
        public List<string> CoverWallScreenSaverFolders = new List<string> ();

        public string   CoverflowRotation = "None";
        public string   PosterRotation = "None";
        public string   ThumbRotation = "None";
        public string   ThumbstripRotation = "None";

        public int      RootEHSHListSize = 100;
        public int      RootEHSThumbSize = 200;
        public int      RootEHSRight = 35;
        public int      RootEHSRALLeft = 55;
        public string   RootEHSRALPos = "Middle";
        public bool     RootEHSShowInfo = true;
        public bool     RootEHSRALBackdrop = true;
        public bool     RootEHSEpisodeThumbs = false;
        public string   RootCoverflowPos = "Bottom";
        public int      RootDetailsRight = 35;
        public int      RootPosterTop = 50;

        public string   ChildCoverflowPos = "Bottom";
        public int      ChildDetailsTop = 50;
        public int      ChildDetailsSep = 35;
        public int      ChildPosterTop = 30;
        public int      ChildThumbTop = 35;
        public int      ChildThumbSep = 65;
        public bool     ChildThumbBanner = true;
        public int      ChildThumbstripTop = 20;
        public bool     ChildThumbMiniBackdrop = false;
        public bool     ChildThumbstripMiniBackdrop = true;
        public bool     ChildSeasonEpisodeOverlay = false;
        

        #region Load / Save Data
        public static ConfigData FromFile (string file)
        {
            return new ConfigData (file);
        }

        public void Save ()
        {
            this.settings.Write ();
        }

        [SkipField]
        string file;

        [SkipField]
        XmlSettings<ConfigData> settings;
        #endregion
    }
}
