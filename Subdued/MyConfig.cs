using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.MediaCenter.UI;
using System.IO;
using MediaBrowser;
using MediaBrowser.Library;
using MediaBrowser.Library.Entities;
using MediaBrowser.Library.Configuration;
using MediaBrowser.Library.Logging;
using MediaBrowser.Util;
using Microsoft.MediaCenter;
using Ionic.Zip;

namespace Subdued
{
    public class MyConfig : ModelItem
    {
        private ConfigData data;
        private FolderConfigData folderData = null;
        private bool isValid;

        const string STYLE_RANDOM = "[Random]";

        const string STYLES_CORE_TAG = "CORE VERSION (DO NOT EDIT):";
        const string STYLES_VERSION_TAG = "STYLE VERSION:";
        const string STYLES_END_TAG = "-->";
        const int STYLES_CORE_VERSION = 6;

        private static readonly string configFilePath = Path.Combine (ApplicationPaths.AppPluginPath, "Configurations\\Subdued.xml");
        private static readonly string configFolderPath = Path.Combine (ApplicationPaths.AppPluginPath, "Configurations");
        private static readonly string stylesFolderPath = Path.Combine (ApplicationPaths.AppPluginPath, "Configurations\\SubduedStyles");
        private static readonly string styleFilePath = Path.Combine (ApplicationPaths.AppConfigPath, "Subdued_Styles.mcml");
        private static readonly string mediInfoFolderPath = Path.Combine (Config.Instance.ImageByNameLocation, "MediaInfo\\Subdued");
        private static readonly string displayFolderPathOld = Path.Combine (ApplicationPaths.AppPluginPath, "Configurations\\SubduedDisplay");
        private static readonly string displayFolderPath = Path.Combine (ApplicationPaths.AppUserSettingsPath, "SubduedDisplay");
        
        readonly Choice installedStyles = new Choice ();
                
        public static void InitDisplayPrefs ()
        {
            string[]    displayFiles;
            string      destFile;

            if (!Directory.Exists (displayFolderPath))
            {
                Logger.ReportInfo ("Subdued - Creating display prefs folder: " + displayFolderPath);
                Directory.CreateDirectory (displayFolderPath);
            }

            // Migrate display files from root of config
            displayFiles = Directory.GetFiles (configFolderPath, "Subdued_*.xml");

            foreach (string displayFile in displayFiles)
            {
                Logger.ReportInfo ("Subdued - Moving display prefs file: " + displayFile);

                destFile = Path.Combine (displayFolderPath, Path.GetFileName (displayFile));

                try
                {
                    if (File.Exists (destFile))
                        File.Delete (displayFile);
                    else
                        File.Move (displayFile, destFile);
                }
                catch
                {
                    // NOP
                }
            }

            // Migrate display files from old SubudedDisplay
            if (Directory.Exists (displayFolderPathOld))
            {
                displayFiles = Directory.GetFiles (displayFolderPathOld, "Subdued_*.xml");

                foreach (string displayFile in displayFiles)
                {
                    Logger.ReportInfo ("Subdued - Moving display prefs file: " + displayFile);

                    destFile = Path.Combine (displayFolderPath, Path.GetFileName (displayFile));

                    try
                    {
                        if (File.Exists (destFile))
                            File.Delete (displayFile);
                        else
                            File.Move (displayFile, destFile);
                    }
                    catch
                    {
                        // NOP
                    }
                }

                try
                {
                    Directory.Delete (displayFolderPathOld);
                }
                catch
                {
                    // NOP
                }
            }
        }

        public bool AskToQuit
        {
            get { return this.data.AskToQuit; }
            set
            {
                if (this.data.AskToQuit != value)
                {
                    this.data.AskToQuit = value;
                    Save();
                    FirePropertyChanged("AskToQuit");
                }
            }
        }

        #region Styles Implementation
        public static void InitStyles ()
        {
            string[]    styleFiles;
            bool        bValid = false;

            if (!Directory.Exists (stylesFolderPath))
            {
                Logger.ReportInfo ("Subdued - Creating styles folder: " + stylesFolderPath);
                Directory.CreateDirectory (stylesFolderPath);
            }

            VerifyBundledStyle (Path.Combine (stylesFolderPath, "The Original.mcml"), Resources.The_Original);
            VerifyBundledStyle (Path.Combine (stylesFolderPath, "White Shadow.mcml"), Resources.White_Shadow);
            VerifyBundledStyle (Path.Combine (stylesFolderPath, "Spy vs. Spy 1.mcml"), Resources.Spy_vs_Spy_1);
            VerifyBundledStyle (Path.Combine (stylesFolderPath, "Spy vs. Spy 2.mcml"), Resources.Spy_vs_Spy_2);
            VerifyBundledStyle (Path.Combine (stylesFolderPath, "Blue Note.mcml"), Resources.Blue_Note);
            VerifyBundledStyle (Path.Combine (stylesFolderPath, "Cold as Ice.mcml"), Resources.Cold_as_Ice);
            VerifyBundledStyle (Path.Combine (stylesFolderPath, "Neon.mcml"), Resources.Neon);
            VerifyBundledStyle (Path.Combine (stylesFolderPath, "Redrum.mcml"), Resources.Redrum);
            VerifyBundledStyle(Path.Combine(stylesFolderPath, "Subtle Red.mcml"), Resources.Subtle_Red);
            VerifyBundledStyle(Path.Combine(stylesFolderPath, "Subtle Blue.mcml"), Resources.Subtle_Blue);
            VerifyBundledStyle(Path.Combine(stylesFolderPath, "Subtle Green.mcml"), Resources.Subtle_Green);
            VerifyBundledStyle(Path.Combine(stylesFolderPath, "Subtle Orange.mcml"), Resources.Subtle_Orange);
            VerifyBundledStyle(Path.Combine(stylesFolderPath, "Subtle Yellow.mcml"), Resources.Subtle_Yellow);


            Logger.ReportInfo ("Subdued - Validating installed styles");

            styleFiles = Directory.GetFiles (stylesFolderPath, "*.mcml");

            foreach (string styleFile in styleFiles)
            {
                Logger.ReportInfo ("Subdued - Checking installed style: " + styleFile);

                bValid = false;

                try
                {
                    StreamReader    styleReader = new StreamReader (styleFile);
                    string          line = styleReader.ReadLine ();
                    int             nPos = -1;
                    int             nStart = -1;
                    int             nCoreVersion = -1;

                    styleReader.Close ();

                    nPos = line.IndexOf (STYLES_CORE_TAG);

                    if (nPos > -1)
                    {
                        nStart = nPos + STYLES_CORE_TAG.Length;
                        nPos = line.IndexOf (STYLES_END_TAG, nStart);

                        if (nPos > -1)
                        {
                            nCoreVersion = Convert.ToInt32 (line.Substring (nStart, nPos - nStart).Trim ());

                            if (nCoreVersion == STYLES_CORE_VERSION)
                                bValid = true;
                            else
                                Logger.ReportError ("Subdued - Core version mismatch: " + styleFile);
                        }
                        else
                        {
                            Logger.ReportError ("Subdued - Core version not found: " + styleFile);
                        }
                    }
                    else
                    {
                        Logger.ReportError ("Subdued - Core version not found: " + styleFile);
                    }
                }
                catch
                {
                    /* NOP */
                }

                if (bValid == false)
                {
                    string  backupPath = styleFile + ".ERR";

                    if (File.Exists (backupPath))
                        File.Delete (backupPath);

                    File.Move (styleFile, backupPath);
                }
            }
        }

        private static void VerifyBundledStyle (string styleFile, byte[] bundledStyle)
        {
            bool    installStyle = true;

            Logger.ReportInfo ("Subdued - Verifying bundled style: " + styleFile);

            if (File.Exists (styleFile))
            {
                Encoding        encoder = new UTF8Encoding ();
                StreamReader    styleReader = new StreamReader (styleFile);
                string          lineCore = styleReader.ReadLine ();
                string          lineVersion = styleReader.ReadLine ();
                int             nPos = -1;
                int             nStart = -1;
                int             nCoreVersion = -1;
                int             nStyleVersion = -1;
                int             nBundledStyleVersion = -1;

                styleReader.Close ();

                nPos = lineCore.IndexOf (STYLES_CORE_TAG);

                if (nPos > -1)
                {
                    nStart = nPos + STYLES_CORE_TAG.Length;
                    nPos = lineCore.IndexOf (STYLES_END_TAG, nStart);

                    if (nPos > -1)
                    {
                        nCoreVersion = Convert.ToInt32 (lineCore.Substring (nStart, nPos - nStart).Trim ());

                        if (nCoreVersion == STYLES_CORE_VERSION)
                        {
                            nPos = lineVersion.IndexOf (STYLES_VERSION_TAG);

                            if (nPos > -1)
                            {
                                nStart = nPos + STYLES_VERSION_TAG.Length;
                                nPos = lineVersion.IndexOf (STYLES_END_TAG, nStart);

                                if (nPos > -1)
                                {
                                    string  bundledData = encoder.GetString (bundledStyle);

                                    nStyleVersion = Convert.ToInt32 (lineVersion.Substring (nStart, nPos - nStart).Trim ());

                                    nPos = bundledData.IndexOf (STYLES_VERSION_TAG);

                                    if (nPos > -1)
                                    {
                                        nStart = nPos + STYLES_VERSION_TAG.Length;
                                        nPos = bundledData.IndexOf (STYLES_END_TAG, nStart);

                                        if (nPos > -1)
                                        {
                                            nBundledStyleVersion = Convert.ToInt32 (bundledData.Substring (nStart, nPos - nStart).Trim ());

                                            if (nStyleVersion >= nBundledStyleVersion)
                                                installStyle = false;
                                        }
                                        else
                                        {
                                            Logger.ReportError ("Subdued - Bundled style version not found: " + styleFile);
                                        }
                                    }
                                    else
                                    {
                                        Logger.ReportError ("Subdued - Bundled style version not found: " + styleFile);
                                    }
                                }
                                else
                                {
                                    Logger.ReportError ("Subdued - Style version not found: " + styleFile);
                                }
                            }
                            else
                            {
                                Logger.ReportError ("Subdued - Style version not found: " + styleFile);
                            }
                        }
                        else
                        {
                            Logger.ReportInfo ("Subdued - Core version mismatch: " + styleFile);
                        }
                    }
                    else
                    {
                        Logger.ReportError ("Subdued - Core version not found: " + styleFile);
                    }
                }
                else
                {
                    Logger.ReportError ("Subdued - Core version not found: " + styleFile);
                }
            }

            if (installStyle)
            {
                Logger.ReportInfo ("Subdued - Installing bundled style: " + styleFile);

                try
                {
                    if (File.Exists (styleFile))
                    {
                        DateTime    now = DateTime.Now;
                        string      backupFile = Path.Combine (Path.GetDirectoryName (styleFile),
                                                               Path.GetFileNameWithoutExtension (styleFile) +
                                                                    "_" +
                                                                    now.Year.ToString () +
                                                                    now.Month.ToString ("D2") +
                                                                    now.Day.ToString ("D2") +
                                                                    now.Hour.ToString ("D2") +
                                                                    now.Minute.ToString ("D2") +
                                                                    now.Second.ToString ("D2") +
                                                                    ".BAK");

                        Logger.ReportInfo ("Subdued - Backing up existing style to: " + backupFile);

                        if (File.Exists (backupFile))
                            File.Delete (backupFile);

                        File.Move (styleFile, backupFile);
                    }

                    BinaryWriter    stylesWriter = new BinaryWriter (File.Open (styleFile, FileMode.OpenOrCreate));

                    stylesWriter.Write (bundledStyle);
                    stylesWriter.Close ();
                }
                catch
                {
                    /* NOP */
                }
            }
            else
            {
                Logger.ReportInfo ("Subdued - Skipping bundled style: " + styleFile);
            }
        }

        public MyConfig ()
        {
            ArrayList   styleOptions = new ArrayList ();
            string[]    styleFiles;

            styleOptions.Add (STYLE_RANDOM);

            styleFiles = Directory.GetFiles (stylesFolderPath, "*.mcml");

            foreach (string styleFile in styleFiles)
                styleOptions.Add (Path.GetFileNameWithoutExtension (styleFile));

            styleOptions.Sort ();

            installedStyles.Options = styleOptions;

            isValid = Load ();
        }

        public void CheckActiveStyle ()
        {
            string  activeStyle;

            Logger.ReportInfo ("Subdued - Checking active style: " + styleFilePath);

            // Force Subdued_Styles.mcml to be updated with active style
            activeStyle = ThemeStyle;
            this.data.ThemeStyle = "";  // Force a style change
            ThemeStyle = activeStyle;

            // Fallback to The Original if active style didn't work
            if (!File.Exists (styleFilePath))
            {
                this.data.ThemeStyle = "";  // Force a style change
                ThemeStyle = "The Original";
            }
        }
        #endregion

        #region MediaInfo Icons Extraction
        private static bool _mediaInfoIconsExtracted = false;
        public bool MediaInfoIconsExtracted
        {
            get { return (_mediaInfoIconsExtracted); }
        }

        public void CheckActiveMediaInfoIconsSet ()
        {
            if (!MediaInfoIconsExtracted)
                Microsoft.MediaCenter.UI.Application.DeferredInvokeOnWorkerThread (DeferredCheckActiveMediaInfoIconsSet, ActiveMediaInfoIconsSetChecked, null);
        }

        private void DeferredCheckActiveMediaInfoIconsSet (object obj)
        {
            Logger.ReportInfo ("Subdued - Checking active MediaInfo icons set");

            if ((MediaInfoIconsSet == "Color") || (MediaInfoIconsSet == "Mono"))
            {
                try
                {
                    string  packagePath;
                    byte[]  package;

                    if (!Directory.Exists (mediInfoFolderPath))
                    {
                        Logger.ReportInfo ("Subdued - Creating MediaInfo icons folder: " + mediInfoFolderPath);
                        Directory.CreateDirectory (mediInfoFolderPath);
                    }

                    if (MediaInfoIconsSet == "Color")
                    {
                        package = Resources.mediainfo_color;
                        packagePath = Path.Combine (mediInfoFolderPath, "mediainfo_color.zip");
                    }
                    else
                    {
                        package = Resources.mediainfo_mono;
                        packagePath = Path.Combine (mediInfoFolderPath, "mediainfo_mono.zip");
                    }

                    using (new Profiler ("Subdued - Setting active MediaInfo icons set: " + packagePath))
                    {
                        BinaryWriter    zipWriter = new BinaryWriter (File.Open (packagePath, FileMode.OpenOrCreate));

                        zipWriter.Write (package);
                        zipWriter.Close ();

                        ZipFile zipFile = new ZipFile (packagePath);

                        zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                        zipFile.ExtractAll (mediInfoFolderPath);

                        zipFile.Dispose ();

                        File.Delete (packagePath);
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.ReportError ("Subdued - Error setting active MediaInfo icons set: " + ex.Message);
                }
            }
        }

        private void ActiveMediaInfoIconsSetChecked (object obj)
        {
            if (!IsDisposed)
            {
                _mediaInfoIconsExtracted = true;
                FirePropertyChanged ("MediaInfoIconsExtracted");
            }
        }
#endregion
        
        #region  Live Layout

        private bool _allowLiveLayout = false;
        public bool AllowLiveLayout
        {
            get { return _allowLiveLayout; }
            set
            {
                if (_allowLiveLayout != value)
                {
                    _allowLiveLayout = value;
                    FirePropertyChanged ("AllowLiveLayout");
                }
            }
        }

        private string _liveLayoutLeftRightLabel = "";
        public string LiveLayoutLeftRightLabel
        {
            get { return _liveLayoutLeftRightLabel; }
            set { _liveLayoutLeftRightLabel = value; FirePropertyChanged ("LiveLayoutLeftRightLabel"); }
        }

        private string _liveLayoutUpDownLabel = "";
        public string LiveLayoutUpDownLabel
        {
            get { return _liveLayoutUpDownLabel; }
            set { _liveLayoutUpDownLabel = value; FirePropertyChanged ("LiveLayoutUpDownLabel"); }
        }

        private string _liveLayoutFwdRewLabel = "";
        public string LiveLayoutFwdRewLabel
        {
            get { return _liveLayoutFwdRewLabel; }
            set { _liveLayoutFwdRewLabel = value; FirePropertyChanged ("LiveLayoutFwdRewLabel"); }
        }

        private string _liveLayoutPgUpDownLabel = "";
        public string LiveLayoutPgUpDownLabel
        {
            get { return _liveLayoutPgUpDownLabel; }
            set { _liveLayoutPgUpDownLabel = value; FirePropertyChanged ("LiveLayoutPgUpDownLabel"); }
        }

        private Command _layoutLeftCommand = new Command ();
        public Command LayoutLeftCommand
        {
            get { return _layoutLeftCommand; }
        }

        private Command _layoutRightCommand = new Command ();
        public Command LayoutRightCommand
        {
            get { return _layoutRightCommand; }
        }

        private Command _layoutUpCommand = new Command ();
        public Command LayoutUpCommand
        {
            get { return _layoutUpCommand; }
        }

        private Command _layoutDownCommand = new Command ();
        public Command LayoutDownCommand
        {
            get { return _layoutDownCommand; }
        }

        private Command _layoutFwdCommand = new Command ();
        public Command LayoutFwdCommand
        {
            get { return _layoutFwdCommand; }
        }

        private Command _layoutRewCommand = new Command ();
        public Command LayoutRewCommand
        {
            get { return _layoutRewCommand; }
        }

        private Command _layoutPgUpCommand = new Command ();
        public Command LayoutPgUpCommand
        {
            get { return _layoutPgUpCommand; }
        }

        private Command _layoutPgDownCommand = new Command ();
        public Command LayoutPgDownCommand
        {
            get { return _layoutPgDownCommand; }
        }

        #endregion

        #region Config Options

        public List<string> RootFolders
        {
            get
            {
                List<string>    folders = new List<string> ();

                foreach (BaseItem child in MediaBrowser.Application.CurrentInstance.RootFolder.Children)
                {
                    if (child is Folder)
                        folders.Add (child.Name);
                }

                return (folders);
            }
        }

        public bool IsCoverWallBackdropFolder (string folder)
        {
            return (this.data.CoverWallBackdropFolders.IndexOf (folder) >= 0);
        }

        public bool IsCoverWallScreenSaverFolder (string folder)
        {
            return (this.data.CoverWallScreenSaverFolders.IndexOf (folder) >= 0);
        }

        public void SetCoverWallBackdropFolder (string folder, bool value)
        {
            if (value)
            {
                if (this.data.CoverWallBackdropFolders.IndexOf (folder) == -1)
                {
                    this.data.CoverWallBackdropFolders.Add (folder);
                    Save ();
                }
            }
            else
            {
                if (this.data.CoverWallBackdropFolders.Remove (folder))
                    Save ();
            }
        }

        public void SetCoverWallScreenSaverFolder (string folder, bool value)
        {
            if (value)
            {
                if (this.data.CoverWallScreenSaverFolders.IndexOf (folder) == -1)
                {
                    this.data.CoverWallScreenSaverFolders.Add (folder);
                    Save ();
                }
            }
            else
            {
                if (this.data.CoverWallScreenSaverFolders.Remove (folder))
                    Save ();
            }
        }

        public Choice InstalledStyles
        {
            get
            {
                return this.installedStyles;
            }
        }

        public string ThemeStyle
        {
            get
            {
                return this.data.ThemeStyle;
            }
            set
            {
                if (this.data.ThemeStyle != value)
                {
                    string  sourceFilePath;

                    this.data.ThemeStyle = value;

                    if (value == STYLE_RANDOM)
                    {
                        Random  rand = new Random ();
                        int     index = rand.Next (1, InstalledStyles.Options.Count);

                        value = InstalledStyles.Options[index].ToString ();

                        Logger.ReportInfo ("Subdued - Chose random style: " + value);
                    }

                    sourceFilePath = Path.Combine (stylesFolderPath, value + ".mcml");

                    Logger.ReportInfo ("Subdued - Setting active style: " + sourceFilePath);

                    File.Copy (sourceFilePath, styleFilePath, true);

                    Save ();
                    FirePropertyChanged ("ThemeStyle");
                }
            }
        }

        public string MediaInfoIconsSet
        {
            get
            {
                return this.data.MediaInfoIconsSet;
            }
            set
            {
                if (this.data.MediaInfoIconsSet != value)
                {
                    this.data.MediaInfoIconsSet = value;
                    Save ();
                    FirePropertyChanged ("MediaInfoIconsSet");
                }
            }
        }

        public bool ShowWatchedOnMovies
        {
            get
            {
                return this.data.ShowWatchedOnMovies;
            }
            set
            {
                if (this.data.ShowWatchedOnMovies != value)
                {
                    this.data.ShowWatchedOnMovies = value;
                    Save ();
                    FirePropertyChanged ("ShowWatchedOnMovies");
                }
            }
        }

        public bool ShowEndTime
        {
            get
            {
                return this.data.ShowEndTime;
            }
            set
            {
                if (this.data.ShowEndTime != value)
                {
                    this.data.ShowEndTime = value;
                    Save ();
                    FirePropertyChanged ("ShowEndTime");
                }
            }
        }

        public bool ShowMiniMediaInfo
        {
            get
            {
                return (this.data.ShowMediaInfoMediaImage ||
                        this.data.ShowMediaInfoVideoImage ||
                        this.data.ShowMediaInfoHDImage ||
                        this.data.ShowMediaInfoAspectImage ||
                        this.data.ShowMediaInfoAudioImage ||
                        this.data.ShowMediaInfoAudioChannelImage);
            }
            set
            {
                if (this.data.ShowMiniMediaInfo != value)
                {
                    this.data.ShowMiniMediaInfo = value;
                    Save ();
                    FirePropertyChanged ("ShowMiniMediaInfo");
                }
            }
        }

        public bool ShowMediaInfoMediaImage
        {
            get
            {
                return this.data.ShowMediaInfoMediaImage;
            }
            set
            {
                if (this.data.ShowMediaInfoMediaImage != value)
                {
                    this.data.ShowMediaInfoMediaImage = value;
                    Save ();
                    FirePropertyChanged ("ShowMediaInfoMediaImage");
                }
            }
        }

        //Option to allow configuration of Weather plugin.... On/Off
        public bool ShowWeather
        {
            get
            {
                return this.data.ShowWeather;
            }
            set
            {
                if (this.data.ShowWeather != value)
                {
                    this.data.ShowWeather = value;
                    base.FirePropertyChanged("ShowWeather");
                    this.Save();
                }
            }
        }


        public bool ShowDiscImage
        {
            get
            {
                return this.data.ShowDiscImage;
            }
            set
            {
                if (this.data.ShowDiscImage != value)
                {
                    this.data.ShowDiscImage = value;
                    base.FirePropertyChanged("ShowDiscImage");
                    this.Save();
                }
            }
        }
        public bool ShowMediaInfoVideoImage
        {
            get
            {
                return this.data.ShowMediaInfoVideoImage;
            }
            set
            {
                if (this.data.ShowMediaInfoVideoImage != value)
                {
                    this.data.ShowMediaInfoVideoImage = value;
                    Save ();
                    FirePropertyChanged ("ShowMediaInfoVideoImage");
                }
            }
        }

        public bool ShowMediaInfoHDImage
        {
            get
            {
                return this.data.ShowMediaInfoHDImage;
            }
            set
            {
                if (this.data.ShowMediaInfoHDImage != value)
                {
                    this.data.ShowMediaInfoHDImage = value;
                    Save ();
                    FirePropertyChanged ("ShowMediaInfoHDImage");
                }
            }
        }

        public bool ShowMediaInfoAspectImage
        {
            get
            {
                return this.data.ShowMediaInfoAspectImage;
            }
            set
            {
                if (this.data.ShowMediaInfoAspectImage != value)
                {
                    this.data.ShowMediaInfoAspectImage = value;
                    Save ();
                    FirePropertyChanged ("ShowMediaInfoAspectImage");
                }
            }
        }

        public bool ShowMediaInfoAudioImage
        {
            get
            {
                return this.data.ShowMediaInfoAudioImage;
            }
            set
            {
                if (this.data.ShowMediaInfoAudioImage != value)
                {
                    this.data.ShowMediaInfoAudioImage = value;
                    Save ();
                    FirePropertyChanged ("ShowMediaInfoAudioImage");
                }
            }
        }

        public bool ShowMediaInfoAudioChannelImage
        {
            get
            {
                return this.data.ShowMediaInfoAudioChannelImage;
            }
            set
            {
                if (this.data.ShowMediaInfoAudioChannelImage != value)
                {
                    this.data.ShowMediaInfoAudioChannelImage = value;
                    Save ();
                    FirePropertyChanged ("ShowMediaInfoAudioChannelImage");
                }
            }
        }

        public bool ShowMediaInfoMovieRatingImage
        {
            get
            {
                return this.data.ShowMediaInfoMovieRatingImage;
            }
            set
            {
                if (this.data.ShowMediaInfoMovieRatingImage != value)
                {
                    this.data.ShowMediaInfoMovieRatingImage = value;
                    Save ();
                    FirePropertyChanged ("ShowMediaInfoMovieRatingImage");
                }
            }
        }

        public bool ShowPrevNextButtons
        {
            get
            {
                return this.data.ShowPrevNextButtons;
            }
            set
            {
                if (this.data.ShowPrevNextButtons != value)
                {
                    this.data.ShowPrevNextButtons = value;
                    Save ();
                    FirePropertyChanged ("ShowPrevNextButtons");
                }
            }
        }

        public bool ShowIndexOfCount
        {
            get
            {
                return this.data.ShowIndexOfCount;
            }
            set
            {
                if (this.data.ShowIndexOfCount != value)
                {
                    this.data.ShowIndexOfCount = value;
                    Save ();
                    FirePropertyChanged ("ShowIndexOfCount");
                }
            }
        }

        public bool ShowMasterPCIcon
        {
            get
            {
                return this.data.ShowMasterPCIcon;
            }
            set
            {
                if (this.data.ShowMasterPCIcon != value)
                {
                    this.data.ShowMasterPCIcon = value;
                    Save ();
                    FirePropertyChanged ("ShowMasterPCIcon");
                }
            }
        }

        public bool ShowClockConfigAtTop
        {
            get
            {
                return this.data.ShowClockConfigAtTop;
            }
            set
            {
                if (this.data.ShowClockConfigAtTop != value)
                {
                    this.data.ShowClockConfigAtTop = value;
                    Save ();
                    FirePropertyChanged ("ShowClockConfigAtTop");
                }
            }
        }

        public string ScrollSpeed
        {
            get
            {
                return this.data.ScrollSpeed;
            }
            set
            {
                if (this.data.ScrollSpeed != value)
                {
                    this.data.ScrollSpeed = value;
                    Save ();
                    FirePropertyChanged ("ScrollSpeed");
                }
            }
        }

        public string NowPlayingStyle
        {
            get
            {
                return this.data.NowPlayingStyle;
            }
            set
            {
                if (this.data.NowPlayingStyle != value)
                {
                    this.data.NowPlayingStyle = value;
                    Save ();
                    FirePropertyChanged ("NowPlayingStyle");
                }
            }
        }

        public string TopPanelStyle
        {
            get
            {
                return this.data.TopPanelStyle;
            }
            set
            {
                if (this.data.TopPanelStyle != value)
                {
                    this.data.TopPanelStyle = value;
                    Save ();
                    FirePropertyChanged ("TopPanelStyle");
                }
            }
        }

        public string StarRatingStyle
        {
            get
            {
                return this.data.StarRatingStyle;
            }
            set
            {
                if (this.data.StarRatingStyle != value)
                {
                    this.data.StarRatingStyle = value;
                    Save ();
                    FirePropertyChanged ("StarRatingStyle");
                }
            }
        }

        public string WatchedIndicatorPosition
        {
            get
            {
                return this.data.WatchedIndicatorPosition;
            }
            set
            {
                if (this.data.WatchedIndicatorPosition != value)
                {
                    this.data.WatchedIndicatorPosition = value;
                    Save ();
                    FirePropertyChanged ("WatchedIndicatorPosition");
                }
            }
        }

        public int ThumbSpacing
        {
            get
            {
                return this.data.ThumbSpacing;
            }
            set
            {
                if (this.data.ThumbSpacing != value)
                {
                    this.data.ThumbSpacing = value;
                    Save ();
                    FirePropertyChanged ("ThumbSpacing");
                }
            }
        }

        public int SelThumbGrow
        {
            get
            {
                return this.data.SelThumbGrow;
            }
            set
            {
                if (this.data.SelThumbGrow != value)
                {
                    this.data.SelThumbGrow = value;
                    Save ();
                    FirePropertyChanged ("SelThumbGrow");
                }
            }
        }

        public int CoverArtAdjustment
        {
            get
            {
                return this.data.CoverArtAdjustment;
            }
            set
            {
                if (this.data.CoverArtAdjustment != value)
                {
                    this.data.CoverArtAdjustment = value;
                    Save ();
                    FirePropertyChanged ("CoverArtAdjustment");
                }
            }
        }

        public int UnselectedThumbAlpha
        {
            get
            {
                return this.data.UnselectedThumbAlpha;
            }
            set
            {
                if (this.data.UnselectedThumbAlpha != value)
                {
                    this.data.UnselectedThumbAlpha = value;
                    Save ();
                    FirePropertyChanged ("UnselectedThumbAlpha");
                }
            }
        }

        public bool GraduatedAlpha
        {
            get
            {
                return this.data.GraduatedAlpha;
            }
            set
            {
                if (this.data.GraduatedAlpha != value)
                {
                    this.data.GraduatedAlpha = value;
                    Save ();
                    FirePropertyChanged ("GraduatedAlpha");
                }
            }
        }

        public bool ColorizeStudioImages
        {
            get
            {
                return this.data.ColorizeStudioImages;
            }
            set
            {
                if (this.data.ColorizeStudioImages != value)
                {
                    this.data.ColorizeStudioImages = value;
                    Save ();
                    FirePropertyChanged ("ColorizeStudioImages");
                }
            }
        }

        public bool ColorizeRatingsImages
        {
            get
            {
                return this.data.ColorizeRatingsImages;
            }
            set
            {
                if (this.data.ColorizeRatingsImages != value)
                {
                    this.data.ColorizeRatingsImages = value;
                    Save ();
                    FirePropertyChanged ("ColorizeRatingsImages");
                }
            }
        }

        public bool ColorizeCustomRatingsImages
        {
            get
            {
                return this.data.ColorizeCustomRatingsImages;
            }
            set
            {
                if (this.data.ColorizeCustomRatingsImages != value)
                {
                    this.data.ColorizeCustomRatingsImages = value;
                    Save ();
                    FirePropertyChanged ("ColorizeCustomRatingsImages");
                }
            }
        }

        public bool ColorizePosterOverlayImages
        {
            get
            {
                return this.data.ColorizePosterOverlayImages;
            }
            set
            {
                if (this.data.ColorizePosterOverlayImages != value)
                {
                    this.data.ColorizePosterOverlayImages = value;
                    Save ();
                    FirePropertyChanged ("ColorizePosterOverlayImages");
                }
            }
        }

        public bool EnableNewItemIndicator
        {
            get
            {
                return this.data.EnableNewItemIndicator;
            }
            set
            {
                if (this.data.EnableNewItemIndicator != value)
                {
                    this.data.EnableNewItemIndicator = value;
                    Save ();
                    FirePropertyChanged ("EnableNewItemIndicator");
                }
            }
        }

        public bool EnableQuickPlay
        {
            get
            {
                return this.data.EnableQuickPlay;
            }
            set
            {
                if (this.data.EnableQuickPlay != value)
                {
                    this.data.EnableQuickPlay = value;
                    Save ();
                    FirePropertyChanged ("EnableQuickPlay");
                }
            }
        }

        public bool CoverWallRootBackdrop
        {
            get
            {
                return this.data.CoverWallRootBackdrop;
            }
            set
            {
                if (this.data.CoverWallRootBackdrop != value)
                {
                    this.data.CoverWallRootBackdrop = value;
                    Save ();
                    FirePropertyChanged ("CoverWallRootBackdrop");
                }
            }
        }

        public bool CoverWallScreenSaver
        {
            get
            {
                return this.data.CoverWallScreenSaver;
            }
            set
            {
                if (this.data.CoverWallScreenSaver != value)
                {
                    this.data.CoverWallScreenSaver = value;
                    Save ();
                    FirePropertyChanged ("CoverWallScreenSaver");
                }
            }
        }

        public string CoverWallStyle
        {
            get
            {
                return this.data.CoverWallStyle;
            }
            set
            {
                if (this.data.CoverWallStyle != value)
                {
                    this.data.CoverWallStyle = value;
                    Save ();
                    FirePropertyChanged ("CoverWallStyle");
                }
            }
        }

        public string CoverWallRotation
        {
            get
            {
                return this.data.CoverWallRotation;
            }
            set
            {
                if (this.data.CoverWallRotation != value)
                {
                    this.data.CoverWallRotation = value;
                    Save ();
                    FirePropertyChanged ("CoverWallRotation");
                }
            }
        }

        public string CoverWallScroll
        {
            get
            {
                return this.data.CoverWallScroll;
            }
            set
            {
                if (this.data.CoverWallScroll != value)
                {
                    this.data.CoverWallScroll = value;
                    Save ();
                    FirePropertyChanged ("CoverWallScroll");
                }
            }
        }

        public string CoverWallScrollSpeed
        {
            get
            {
                return this.data.CoverWallScrollSpeed;
            }
            set
            {
                if (this.data.CoverWallScrollSpeed != value)
                {
                    this.data.CoverWallScrollSpeed = value;
                    Save ();
                    FirePropertyChanged ("CoverWallScrollSpeed");
                }
            }
        }

        public int CoverWallSSTimeout
        {
            get
            {
                return this.data.CoverWallSSTimeout;
            }
            set
            {
                if (this.data.CoverWallSSTimeout != value)
                {
                    this.data.CoverWallSSTimeout = value;
                    Save ();
                    FirePropertyChanged ("CoverWallSSTimeout");
                }
            }
        }

                public string CoverflowRotation
        {
            get
            {
                return this.data.CoverflowRotation;
            }
            set
            {
                if (this.data.CoverflowRotation != value)
                {
                    this.data.CoverflowRotation = value;
                    Save ();
                    FirePropertyChanged ("CoverflowRotation");
                }
            }
        }

        public string PosterRotation
        {
            get
            {
                return this.data.PosterRotation;
            }
            set
            {
                if (this.data.PosterRotation != value)
                {
                    this.data.PosterRotation = value;
                    Save ();
                    FirePropertyChanged ("PosterRotation");
                }
            }
        }

        public string ThumbRotation
        {
            get
            {
                return this.data.ThumbRotation;
            }
            set
            {
                if (this.data.ThumbRotation != value)
                {
                    this.data.ThumbRotation = value;
                    Save ();
                    FirePropertyChanged ("ThumbRotation");
                }
            }
        }

        public string ThumbstripRotation
        {
            get
            {
                return this.data.ThumbstripRotation;
            }
            set
            {
                if (this.data.ThumbstripRotation != value)
                {
                    this.data.ThumbstripRotation = value;
                    Save ();
                    FirePropertyChanged ("ThumbstripRotation");
                }
            }
        }

        public int RootEHSHListSize
        {
            get
            {
                return this.data.RootEHSHListSize;
            }
            set
            {
                if (this.data.RootEHSHListSize != value)
                {
                    this.data.RootEHSHListSize = value;
                    Save ();
                    FirePropertyChanged ("RootEHSHListSize");
                }
            }
        }

        public int RootEHSThumbSize
        {
            get
            {
                return this.data.RootEHSThumbSize;
            }
            set
            {
                if (this.data.RootEHSThumbSize != value)
                {
                    this.data.RootEHSThumbSize = value;
                    Save ();
                    FirePropertyChanged ("RootEHSThumbSize");
                }
            }
        }

        public int RootEHSRight
        {
            get
            {
                return this.data.RootEHSRight;
            }
            set
            {
                if (this.data.RootEHSRight != value)
                {
                    this.data.RootEHSRight = value;
                    Save ();
                    FirePropertyChanged ("RootEHSRight");
                }
            }
        }

        public int RootEHSRALLeft
        {
            get
            {
                return this.data.RootEHSRALLeft;
            }
            set
            {
                if (this.data.RootEHSRALLeft != value)
                {
                    this.data.RootEHSRALLeft = value;
                    Save ();
                    FirePropertyChanged ("RootEHSRALLeft");
                }
            }
        }

        public string RootEHSRALPos
        {
            get
            {
                return this.data.RootEHSRALPos;
            }
            set
            {
                if (this.data.RootEHSRALPos != value)
                {
                    this.data.RootEHSRALPos = value;
                    Save ();
                    FirePropertyChanged ("RootEHSRALPos");
                }
            }
        }

        public bool RootEHSShowInfo
        {
            get
            {
                return this.data.RootEHSShowInfo;
            }
            set
            {
                if (this.data.RootEHSShowInfo != value)
                {
                    this.data.RootEHSShowInfo = value;
                    Save ();
                    FirePropertyChanged ("RootEHSShowInfo");
                }
            }
        }

        public bool RootEHSRALBackdrop
        {
            get
            {
                return this.data.RootEHSRALBackdrop;
            }
            set
            {
                if (this.data.RootEHSRALBackdrop != value)
                {
                    this.data.RootEHSRALBackdrop = value;
                    Save ();
                    FirePropertyChanged ("RootEHSRALBackdrop");
                }
            }
        }

        public bool RootEHSEpisodeThumbs
        {
            get
            {
                return this.data.RootEHSEpisodeThumbs;
            }
            set
            {
                if (this.data.RootEHSEpisodeThumbs != value)
                {
                    this.data.RootEHSEpisodeThumbs = value;
                    Save ();
                    FirePropertyChanged ("RootEHSEpisodeThumbs");
                }
            }
        }

        public string RootCoverflowPos
        {
            get
            {
                return this.data.RootCoverflowPos;
            }
            set
            {
                if (this.data.RootCoverflowPos != value)
                {
                    this.data.RootCoverflowPos = value;
                    Save ();
                    FirePropertyChanged ("RootCoverflowPos");
                }
            }
        }

        public int RootDetailsRight
        {
            get
            {
                return this.data.RootDetailsRight;
            }
            set
            {
                if (this.data.RootDetailsRight != value)
                {
                    this.data.RootDetailsRight = value;
                    Save ();
                    FirePropertyChanged ("RootDetailsRight");
                }
            }
        }

        public int RootPosterTop
        {
            get
            {
                return this.data.RootPosterTop;
            }
            set
            {
                if (this.data.RootPosterTop != value)
                {
                    this.data.RootPosterTop = value;
                    Save ();
                    FirePropertyChanged ("RootPosterTop");
                }
            }
        }

        public string ChildCoverflowPos
        {
            get
            {
                return this.data.ChildCoverflowPos;
            }
            set
            {
                if (this.data.ChildCoverflowPos != value)
                {
                    this.data.ChildCoverflowPos = value;
                    Save ();
                    FirePropertyChanged ("ChildCoverflowPos");
                }
            }
        }

        public int ChildDetailsTop
        {
            get
            {
                return this.data.ChildDetailsTop;
            }
            set
            {
                if (this.data.ChildDetailsTop != value)
                {
                    this.data.ChildDetailsTop = value;
                    Save ();
                    FirePropertyChanged ("ChildDetailsTop");
                }
            }
        }

        public int ChildDetailsSep
        {
            get
            {
                return this.data.ChildDetailsSep;
            }
            set
            {
                if (this.data.ChildDetailsSep != value)
                {
                    this.data.ChildDetailsSep = value;
                    Save ();
                    FirePropertyChanged ("ChildDetailsSep");
                }
            }
        }

        public int ChildPosterTop
        {
            get
            {
                return this.data.ChildPosterTop;
            }
            set
            {
                if (this.data.ChildPosterTop != value)
                {
                    this.data.ChildPosterTop = value;
                    Save ();
                    FirePropertyChanged ("ChildPosterTop");
                }
            }
        }

        public int ChildThumbTop
        {
            get
            {
                return this.data.ChildThumbTop;
            }
            set
            {
                if (this.data.ChildThumbTop != value)
                {
                    this.data.ChildThumbTop = value;
                    Save ();
                    FirePropertyChanged ("ChildThumbTop");
                }
            }
        }

        public int ChildThumbSep
        {
            get
            {
                return this.data.ChildThumbSep;
            }
            set
            {
                if (this.data.ChildThumbSep != value)
                {
                    this.data.ChildThumbSep = value;
                    Save ();
                    FirePropertyChanged ("ChildThumbSep");
                }
            }
        }

        public bool ChildThumbBanner
        {
            get
            {
                return this.data.ChildThumbBanner;
            }
            set
            {
                if (this.data.ChildThumbBanner != value)
                {
                    this.data.ChildThumbBanner = value;
                    Save ();
                    FirePropertyChanged ("ChildThumbBanner");
                }
            }
        }

        public int ChildThumbstripTop
        {
            get
            {
                return this.data.ChildThumbstripTop;
            }
            set
            {
                if (this.data.ChildThumbstripTop != value)
                {
                    this.data.ChildThumbstripTop = value;
                    Save ();
                    FirePropertyChanged ("ChildThumbstripTop");
                }
            }
        }

        public bool ChildThumbMiniBackdrop
        {
            get
            {
                return this.data.ChildThumbMiniBackdrop;
            }
            set
            {
                if (this.data.ChildThumbMiniBackdrop != value)
                {
                    this.data.ChildThumbMiniBackdrop = value;
                    Save ();
                    FirePropertyChanged ("ChildThumbMiniBackdrop");
                }
            }
        }

        public bool ChildThumbstripMiniBackdrop
        {
            get
            {
                return this.data.ChildThumbstripMiniBackdrop;
            }
            set
            {
                if (this.data.ChildThumbstripMiniBackdrop != value)
                {
                    this.data.ChildThumbstripMiniBackdrop = value;
                    Save ();
                    FirePropertyChanged ("ChildThumbstripMiniBackdrop");
                }
            }
        }

        public bool ChildSeasonEpisodeOverlay
        {
            get
            {
                return this.data.ChildSeasonEpisodeOverlay;
            }
            set
            {
                if (this.data.ChildSeasonEpisodeOverlay != value)
                {
                    this.data.ChildSeasonEpisodeOverlay = value;
                    Save ();
                    FirePropertyChanged ("ChildSeasonEpisodeOverlay");
                }
            }
        }

        #endregion

        #region Folder Config Options

        private Guid    folderId = Guid.Empty;
        private Guid    parentFolderId = Guid.Empty;

        public Guid FolderId
        {
            get
            {
                return this.folderId;
            }
            set
            {
                if (this.folderId != value)
                {
                    this.folderId = value;

                    if ((this.folderId == Guid.Empty) || !LoadFolder ())
                    {
                        folderData = null;
                    }
                }
            }
        }

        public Guid ParentFolderId
        {
            get
            {
                return this.parentFolderId;
            }
            set
            {
                if (this.parentFolderId != value)
                    this.parentFolderId = value;
            }
        }

        public bool FolderShowAsText
        {
            get
            {
                bool    show = false;

                if (folderData != null)
                    show = folderData.ShowAsText;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.ShowAsText != value))
                {
                    folderData.ShowAsText = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderShowAsText");
                }
            }
        }

        public string FolderOrientation
        {
            get
            {
                string    orientation = "Vertical";

                if (folderData != null)
                    orientation = folderData.Orientation;

                return orientation;
            }
            set
            {
                if ((folderData != null) && (folderData.Orientation != value))
                {
                    folderData.Orientation = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderOrientation");
                }
            }
        }

        public string FolderWrapItemList
        {
            get
            {
                string    wrap = "When Too Big";

                if (folderData != null)
                    wrap = folderData.WrapItemList;

                return wrap;
            }
            set
            {
                if ((folderData != null) && (folderData.WrapItemList != value))
                {
                    folderData.WrapItemList = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderWrapItemList");
                }
            }
        }

        //Choice for selecting Logos/ClearArt & Thumbs
        public string FolderClearLogosList
        {
            get
            {
                string show = "Logo";

                if (folderData != null)
                    show = folderData.FolderClearLogosList;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.FolderClearLogosList != value))
                {
                    folderData.FolderClearLogosList = value;
                    SaveFolder();
                    FirePropertyChanged("FolderClearLogosList");
                }
            }
        }

        public string FolderInfoDisplay
        {
            get
            {
                string  display = "Overview";

                if (folderData != null)
                    display = folderData.FolderInfoDisplay;

                return display;
            }
            set
            {
                if ((folderData != null) && (folderData.FolderInfoDisplay != value))
                {
                    folderData.FolderInfoDisplay = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderInfoDisplay");
                }
            }
        }

        public bool FolderShowBackdrop
        {
            get
            {
                bool    show = true;

                if (folderData != null)
                    show = folderData.ShowBackdrop;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.ShowBackdrop != value))
                {
                    folderData.ShowBackdrop = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderShowBackdrop");
                }
            }
        }

        

        public bool FolderShowBackdropOverlay
        {
            get
            {
                bool    show = false;

                if (folderData != null)
                    show = folderData.ShowBackdropOverlay;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.ShowBackdropOverlay != value))
                {
                    folderData.ShowBackdropOverlay = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderShowBackdropOverlay");
                }
            }
        }

        public bool FolderShowNPV
        {
            get
            {
                bool    show = true;

                if (folderData != null)
                    show = folderData.ShowNPV;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.ShowNPV != value))
                {
                    folderData.ShowNPV = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderShowNPV");
                }
            }
        }

        public bool FolderShowTextBG
        {
            get
            {
                bool    show = true;

                if (folderData != null)
                    show = folderData.ShowTextBG;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.ShowTextBG != value))
                {
                    folderData.ShowTextBG = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderShowTextBG");
                }
            }
        }

        public bool FolderShowThumbsBG
        {
            get
            {
                bool    show = true;

                if (folderData != null)
                    show = folderData.ShowThumbsBG;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.ShowThumbsBG != value))
                {
                    folderData.ShowThumbsBG = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderShowThumbsBG");
                }
            }
        }

        public bool FolderShowTopPanel
        {
            get
            {
                bool    show = true;

                if (folderData != null)
                    show = folderData.ShowTopPanel;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.ShowTopPanel != value))
                {
                    folderData.ShowTopPanel = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderShowTopPanel");
                }
            }
        }

        public bool FolderShowSelectedInfo
        {
            get
            {
                bool    show = true;

                if (folderData != null)
                    show = folderData.ShowSelectedInfo;

                return show;
            }
            set
            {
                if ((folderData != null) && (folderData.ShowSelectedInfo != value))
                {
                    folderData.ShowSelectedInfo = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderShowSelectedInfo");
                }
            }
        }

        public string FolderSelectedInfoStyle
        {
            get
            {
                string  display = "Split";

                if (folderData != null)
                    display = folderData.SelectedInfoStyle;

                return display;
            }
            set
            {
                if ((folderData != null) && (folderData.SelectedInfoStyle != value))
                {
                    folderData.SelectedInfoStyle = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderSelectedInfoStyle");
                }
            }
        }

        public bool FolderRotateBackdrops
        {
            get
            {
                bool    rotate = true;

                if (folderData != null)
                    rotate = folderData.RotateBackdrops;

                return rotate;
            }
            set
            {
                if ((folderData != null) && (folderData.RotateBackdrops != value))
                {
                    folderData.RotateBackdrops = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderRotateBackdrops");
                }
            }
        }

        public bool FolderEnableWatchedIndicators
        {
            get
            {
                bool    enabled = true;

                if (folderData != null)
                    enabled = folderData.EnableWatchedIndicators;

                return enabled;
            }
            set
            {
                if ((folderData != null) && (folderData.EnableWatchedIndicators != value))
                {
                    folderData.EnableWatchedIndicators = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderEnableWatchedIndicators");
                }
            }
        }

        public bool FolderEnableNewItemIndicator
        {
            get
            {
                bool    enabled = true;

                if (folderData != null)
                    enabled = folderData.EnableNewItemIndicator;

                return enabled;
            }
            set
            {
                if ((folderData != null) && (folderData.EnableNewItemIndicator != value))
                {
                    folderData.EnableNewItemIndicator = value;
                    SaveFolder ();
                    FirePropertyChanged ("FolderEnableNewItemIndicator");
                }
            }
        }

        #endregion

        #region Save / Load Configuration

        private void Save ()
        {
            lock (this)
                this.data.Save ();
        }

        private void SaveFolder ()
        {
            if ((folderData != null) && (folderData.FolderId != Guid.Empty.ToString ()))
            {
                lock (this)
                    this.folderData.Save ();
            }
        }

        private bool Load ()
        {
            try
            {
                this.data = ConfigData.FromFile (configFilePath);

                if (this.data.CoverWallScroll == "Undefined")
                {
                    if (this.data.CoverWallStyle == "Off")
                    {
                        this.data.CoverWallRootBackdrop = false;
                        this.data.CoverWallScroll = "Down";
                    }
                    else
                    {
                        if (this.data.CoverWallStyle == "Horizontal")
                            this.data.CoverWallScroll = "Right";
                        else if (this.data.CoverWallStyle == "Vertical")
                            this.data.CoverWallScroll = "Down";
                        else
                            this.data.CoverWallScroll = this.data.CoverWallStyle;

                        this.data.CoverWallRootBackdrop = true;
                    }

                    Save ();
                }

                return true;
            }
            catch (Exception ex)
            {
                MediaCenterEnvironment ev = Microsoft.MediaCenter.Hosting.AddInHost.Current.MediaCenterEnvironment;
                DialogResult r = ev.Dialog (ex.Message + "\nReset to default?", "Error in configuration file", DialogButtons.Yes | DialogButtons.No, 600, true);
                if (r == DialogResult.Yes)
                {
                    if (!Directory.Exists (configFolderPath))
                        Directory.CreateDirectory (configFolderPath);
                    this.data = new ConfigData (configFilePath);
                    Save ();
                    return true;
                }
                else
                    return false;
            }
        }

        private bool LoadFolder ()
        {
            if (folderId != Guid.Empty)
            {
                string  folderFilePath = Path.Combine (displayFolderPath, "Subdued_" + folderId.ToString () + ".xml");

                if (!File.Exists (folderFilePath) && (parentFolderId != Guid.Empty))
                {
                    string  parentFolderFilePath = Path.Combine (displayFolderPath, "Subdued_" + parentFolderId.ToString () + ".xml");

                    if (File.Exists (parentFolderFilePath))
                    {
                        try
                        {
                            File.Copy (parentFolderFilePath, folderFilePath);
                        }
                        catch (Exception)
                        {
                        	// NOP
                        }
                    }
                }

                try
                {
                    this.folderData = FolderConfigData.FromFile (folderId, folderFilePath);

                    if (this.folderData.WrapItemList == "True")
                    {
                        this.folderData.WrapItemList = "When Too Big";
                        SaveFolder ();
                    }
                    else if (this.folderData.WrapItemList == "False")
                    {
                        this.folderData.WrapItemList = "Never";
                        SaveFolder ();
                    }

                    return true;
                }
                catch (Exception)
                {
                    if (!Directory.Exists (configFolderPath))
                        Directory.CreateDirectory (configFolderPath);

                    this.folderData = new FolderConfigData (folderId, folderFilePath);
                    SaveFolder ();
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion

        
    }
}
