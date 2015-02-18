using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.MediaCenter;
using Microsoft.MediaCenter.Hosting;
using Microsoft.MediaCenter.UI;
using MediaBrowser;
using MediaBrowser.Code.ModelItems;
using MediaBrowser.Library;
using MediaBrowser.Library.Configuration;
using MediaBrowser.Library.Entities;
using MediaBrowser.Library.Extensions;
using MediaBrowser.Library.ImageManagement;
using MediaBrowser.Library.Logging;
using MediaBrowser.Util;
using Subdued.APICalls;
using Application = MediaBrowser.Application;

namespace Subdued
{
    internal struct LASTINPUTINFO
    {
        public uint cbSize;

        public uint dwTime;
    }

    public class IdleHandler : ModelItem
    {
        [DllImport ("User32.dll")]
        private static extern bool GetLastInputInfo (ref LASTINPUTINFO plii);

        private int idleTimeout = 0;

        public int IdleTimeout
        {
            set { idleTimeout = value; }
            get { return (idleTimeout); }
        }

        public int IdleTimeoutMins
        {
            set { idleTimeout = value * 60; }
        }

        private bool isIdle = false;

        public bool IsIdle
        {
            get { return (isIdle); }
        }

        private Microsoft.MediaCenter.UI.Timer idleTimer = new Microsoft.MediaCenter.UI.Timer ();

        public IdleHandler ()
        {
            idleTimer.AutoRepeat    = true;
            idleTimer.Interval      = 250;
            idleTimer.Tick          += new EventHandler(idleTimer_Tick);

            idleTimer.Start ();
        }

        public void idleTimer_Tick (object sender, EventArgs args)
        {
            if (IdleTimeout > 0)
            {
                uint    curIdleTime = GetIdleTime ();

                if ((curIdleTime < (IdleTimeout * 1000)) && IsIdle)
                {
                    isIdle = false;
                    FirePropertyChanged ("IsIdle");
                }
                else if ((curIdleTime >= (idleTimeout * 1000)) && !IsIdle)
                {
                    isIdle = true;
                    FirePropertyChanged ("IsIdle");
                }
            }
            else if (IsIdle)
            {
                isIdle = false;
                FirePropertyChanged ("IsIdle");
            }
        }

        private static uint GetIdleTime ()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO ();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf (lastInPut);
            GetLastInputInfo (ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }
    }

    public class AppUtil : ModelItem
    {
        private Item                    _currentItem;
        private FolderModel             _currentParent;
        
        private bool                    _enableChildCount = false;
        private Thread                  _childCountThread = null;
        private volatile FolderModel    _currentFolder = null;
        private volatile string         _currentFolderChildCount = "";
        private AutoResetEvent          _eventStartChildCount = new AutoResetEvent (false);
        private AutoResetEvent          _eventStopChildCount = new AutoResetEvent (false);
        private AutoResetEvent          _eventExitChildCount = new AutoResetEvent (false);

        private FolderModel             _currentTopParent = null;
        
      
        public AppUtil ()
        {
        }

        ~AppUtil ()
        {
            if (_childCountThread != null)
            {
                _eventExitChildCount.Set ();
                _childCountThread.Join ();
            }
        }

        public MyConfig ThemeConfig
        {
            get
            {
                if (Plugin.config == null)
                    Plugin.config = new MyConfig ();

                return Plugin.config;
            }
        }

        public bool EnableChildCount
        {
            get
            {
                return _enableChildCount;
            }
            set
            {
                _enableChildCount = value;
            }
        }

        public Item CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                _currentItem = value;

                if (_enableChildCount)
                {
                    _eventStopChildCount.Set ();

                    _currentFolderChildCount = "";
                    FirePropertyChanged ("ChildCountString");

                    if ((_currentItem != null) && _currentItem.IsFolder)
                    {
                        _currentFolder = (FolderModel)_currentItem;

                        if (_childCountThread == null)
                        {
                            _childCountThread = new Thread (new ThreadStart (GetRecursiveChildCountStringProc));
                            _childCountThread.IsBackground = true;
                            _childCountThread.Priority = ThreadPriority.Normal;

                            _childCountThread.Start ();
                        }

                        _eventStartChildCount.Set ();
                    }
                    else
                    {
                        _currentFolder = null;
                    }
                }

                // GameTime changes
                FirePropertyChanged ("Players");
                FirePropertyChanged ("TgdbRating");
                FirePropertyChanged ("ProductionYear");
                FirePropertyChanged ("EsrbRating");
                FirePropertyChanged ("Company");
                FirePropertyChanged ("ConsoleReleaseYear");
                FirePropertyChanged ("CpuBits");
                FirePropertyChanged ("GpuBits");

                // Music changes
                FirePropertyChanged ("Duration");
            }
        }

        public FolderModel CurrentParent
        {
            get
            {
                return (_currentParent);
            }
            set
            {
                _currentParent      = value;
                _currentTopParent   = null;

                if (_currentParent != null)
                {
                    FolderModel curParent = _currentParent;

                    while (true)
                    {
                        if ((curParent.PhysicalParent == null) || curParent.PhysicalParent.IsRoot)
                        {
                            _currentTopParent = curParent;
                            break;
                        }
                        else
                        {
                            curParent = curParent.PhysicalParent;
                        }
                    }
                }

                FirePropertyChanged ("CurrentTopParent");
            }
        }

        public FolderModel CurrentTopParent
        {
            get
            {
                return _currentTopParent;
            }
        }

        public void GetRecursiveChildCountStringProc ()
        {
            FolderModel         folder = null;
            AutoResetEvent[]    events = new AutoResetEvent[3];
            int                 triggered = -1;
            bool                exitThread = false;

            events[0] = _eventExitChildCount;
            events[1] = _eventStopChildCount;
            events[2] = _eventStartChildCount;

            while (true)
            {
                triggered = WaitHandle.WaitAny (events);

                switch (triggered)
                {
                    case 0: // Exit
                        exitThread = true;
                        break;

                    case 1: // Stop
                        // NOP: Nothing to do since we are idle
                        break;

                    case 2: // Start
                        folder = _currentFolder;

                        if (folder != null)
                        {
                            using (new Profiler ("Subdued - GetRecursiveChildCountString for '" + folder.Name + "'"))
                            {
                                if (!_eventStopChildCount.WaitOne (0))
                                {
                                    Item    item;
                                    int     countMovies = 0;
                                    int     countSeries = 0;
                                    int     countSeasons = 0;
                                    int     countEpisodes = 0;
                                    int     countArtists = 0;
                                    int     countAlbums = 0;
                                    int     countSongs = 0;
                                    int     countConsoles = 0;
                                    int     countGames = 0;
                                    int     countTrailers = 0;
                                    int     countItems = 0;
                                    bool    stopped = false;
                                    string  result = "";

                                    foreach (BaseItem baseItem in folder.Folder.RecursiveChildren)
                                    {
                                        if (_eventStopChildCount.WaitOne (0))
                                        {
                                            stopped = true;
                                            break;
                                        }

                                        item = ItemFactory.Instance.Create (baseItem);

                                        if (item != null)
                                        {
                                            switch (item.ItemTypeString.ToLower ())
                                            {
                                                case "movie":
                                                    countMovies++;
                                                    break;

                                                case "series":
                                                    countSeries++;
                                                    break;

                                                case "season":
                                                    countSeasons++;
                                                    break;

                                                case "episode":
                                                    countEpisodes++;
                                                    break;

                                                case "musicfolder":
                                                    countArtists++;
                                                    break;

                                                case "artistalbum":
                                                case "musicalbum":
                                                    countAlbums++;
                                                    break;

                                                case "song":
                                                    countSongs++;
                                                    break;

                                                case "consolefolder":
                                                    countConsoles++;
                                                    break;

                                                case "gameitem":
                                                    countGames++;
                                                    break;

                                                default:
                                                    if (item.ItemTypeString.ToLower ().IndexOf ("trailer") >= 0)
                                                        countTrailers++;
                                                    else if (item.IsPlayable)
                                                        countItems++;
                                                    break;
                                            }
                                        }
                                    }

                                    if (countMovies > 0)
                                        result += countMovies.ToString () + " " + ((countMovies > 1) ? Kernel.Instance.StringData.GetString ("MoviesStr") : Kernel.Instance.StringData.GetString ("MovieStr"));

                                    if (countTrailers > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countTrailers.ToString () + " " + ((countTrailers > 1) ? Kernel.Instance.StringData.GetString ("TrailersStr") : Kernel.Instance.StringData.GetString ("TrailerStr"));
                                    }

                                    if (countSeries > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countSeries.ToString () + " " + ((countSeries > 1) ? Kernel.Instance.StringData.GetString ("SeriesPluralStr") : Kernel.Instance.StringData.GetString ("SeriesStr"));
                                    }

                                    if (countSeasons > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countSeasons.ToString () + " " + ((countSeasons > 1) ? Kernel.Instance.StringData.GetString ("SeasonsStr") : Kernel.Instance.StringData.GetString ("SeasonStr"));
                                    }

                                    if (countEpisodes > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countEpisodes.ToString () + " " + ((countEpisodes > 1) ? Kernel.Instance.StringData.GetString ("EpisodesStr") : Kernel.Instance.StringData.GetString ("EpisodeStr"));
                                    }

                                    if (countArtists > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countArtists.ToString () + " " + ((countArtists > 1) ? Kernel.Instance.StringData.GetString ("ArtistsStr") : Kernel.Instance.StringData.GetString ("ArtistStr"));
                                    }

                                    if (countAlbums > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countAlbums.ToString () + " " + ((countAlbums > 1) ? Kernel.Instance.StringData.GetString ("AlbumsStr") : Kernel.Instance.StringData.GetString ("AlbumStr"));
                                    }

                                    if (countSongs > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countSongs.ToString () + " " + ((countSongs > 1) ? Kernel.Instance.StringData.GetString ("SongsStr") : Kernel.Instance.StringData.GetString ("SongStr"));
                                    }

                                    if (countConsoles > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countConsoles.ToString () + " " + ((countConsoles > 1) ? Kernel.Instance.StringData.GetString ("ConsolesStr") : Kernel.Instance.StringData.GetString ("ConsolesStr"));
                                    }

                                    if (countGames > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countGames.ToString () + " " + ((countGames > 1) ? Kernel.Instance.StringData.GetString ("GamesStr") : Kernel.Instance.StringData.GetString ("GameStr"));
                                    }

                                    if (countItems > 0)
                                    {
                                        if (!String.IsNullOrEmpty (result))
                                            result += " | ";

                                        result += countItems.ToString () + " " + ((countItems > 1) ? Kernel.Instance.StringData.GetString ("ItemsStr") : Kernel.Instance.StringData.GetString ("ItemStr"));
                                    }

                                    if (_eventStopChildCount.WaitOne (0))
                                        stopped = true;

                                    if (!stopped)
                                    {
                                        _currentFolderChildCount = result;
                                        Microsoft.MediaCenter.UI.Application.DeferredInvoke (_ => FireChildCountStringPropertyChanged ());
                                    }
                                }
                            }
                        }
                        break;
                }

                if (exitThread)
                    break;
            }
        }

        public void FireChildCountStringPropertyChanged ()
        {
            if (_enableChildCount)
                FirePropertyChanged ("ChildCountString");
        }

        public string ChildCountString
        {
            get
            {
                return _currentFolderChildCount;
            }
        }

        public string GetDynamicProperty (string propertyName)
        {
            try
            {
                return CurrentItem.DynamicProperties[propertyName] as string;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private int GetDynamicPropertyAsInt (string PropertyName)
        {
            try
            {
                return Convert.ToInt32(CurrentItem.DynamicProperties[PropertyName]);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private Single GetDynamicPropertyAsSingle (string PropertyName)
        {
            try
            {
                return Convert.ToSingle (CurrentItem.DynamicProperties[PropertyName]);
            }
            catch
            {
                return 0;
            }
        }

        public float GetUnselectedAlpha (int index, int selected, float fullAlpha, int stepAlpha, bool graduated)
        {
            float   result = 1;

            if (graduated)
            {
                int iter = 0;

                for (result = fullAlpha, iter = 0; iter < Math.Abs (selected - index); iter++)
                {
                    result = result * ((float)stepAlpha / 100F);

                    if (result <= 0.01F)
                        break;
                }
            }
            else if (index != selected)
            {
                result = (float)stepAlpha / 100F;
            }
            else
            {
                result = fullAlpha;
            }

            return (result);
        }

        public string FormatIndexOfCount (int index, int count)
        {
            return (string.Format (Kernel.Instance.StringData.GetString ("IndexOfCountFormat"), index + 1, count));
        }

        public string FormatFirstAired (string firstAired)
        {
            string  result;

            try
            {
                result = DateTime.Parse (firstAired).ToShortDateString ();
            }
            catch (Exception)
            {
                result = firstAired;
            }

            return (result);
        }

        public string FormatRunningTime (int runningTime, bool showEndTime)
        {
            String  formatted;

            if (runningTime > 0)
            {
                if (runningTime >= 60)
                {
                    TimeSpan    span = new TimeSpan (0, runningTime, 0);

                    if (span.Hours > 1)
                        formatted = span.Hours.ToString () + " " + Kernel.Instance.StringData.GetString ("HoursStr");
                    else
                        formatted = span.Hours.ToString () + " " + Kernel.Instance.StringData.GetString ("HourStr");

                    if (span.Minutes > 0)
                        formatted += " " + span.Minutes.ToString () + " " + Kernel.Instance.StringData.GetString ("MinutesStr");
                }
                else
                {
                    formatted = runningTime.ToString () + " " + Kernel.Instance.StringData.GetString ("MinutesStr");
                }

                if (showEndTime)
                    formatted += " | " + DateTime.Now.AddMinutes (runningTime).ToShortTimeString ();
            }
            else
            {
                formatted = "";
            }

            return (formatted);
        }

        public int Negate (int number)
        {
            return (-number);
        }

        public int Add2 (int numberA, int numberB)
        {
            return (numberA + numberB);
        }

        public Inset CalcShowLabelsMargins (int labelHeight, int coverArtAdj)
        {
            Inset   result = new Inset (0, 0, 0, labelHeight - coverArtAdj);

            return (result);
        }

        public Size CalcActualReferenceSize (Size thumbSize, bool showLabel, int labelHeight, int coverArtAdj)
        {
            Size    sizeActual = thumbSize;

            if (showLabel)
                sizeActual.Height += (labelHeight - coverArtAdj);

            return (sizeActual);
        }

        public Size GetScaledReferenceSize (Size sizeOrig, Single scale)
        {
            Size    sizeNew = new Size ();

            sizeNew.Width   = (int)((Single)sizeOrig.Width * scale);
            sizeNew.Height  = (int)((Single)sizeOrig.Height * scale);

            return (sizeNew);
        }

        public Size GetBoundedReferenceSize (Size sizeOrig, Size bounds)
        {
            Size    sizeNew = new Size ();
            Single  ratioWidth = (Single)bounds.Width / (Single)sizeOrig.Width;
            Single  ratioHeight = (Single)bounds.Height / (Single)sizeOrig.Height;

            if (((bounds.Width > 0) && (bounds.Height > 0) && (ratioWidth <= ratioHeight)) || ((bounds.Width > 0) && (bounds.Height == 0)))
            {
                sizeNew.Width   = (int)((Single)sizeOrig.Width * ratioWidth);
                sizeNew.Height  = (int)((Single)sizeOrig.Height * ratioWidth);
            }
            else if (((bounds.Width > 0) && (bounds.Height > 0) && (ratioHeight < ratioWidth)) || ((bounds.Height > 0) && (bounds.Width == 0)))
            {
                sizeNew.Width   = (int)((Single)sizeOrig.Width * ratioHeight);
                sizeNew.Height  = (int)((Single)sizeOrig.Height * ratioHeight);
            }

            return (sizeNew);
        }

        public Size BuildSize (int width, int height)
        {
            return (new Size (width, height));
        }

        public Inset BuildInset (int left, int top, int right, int bottom)
        {
            return (new Inset (left, top, right, bottom));
        }

        public void LoadBackdropImage (Item item)
        {
            // Referencing item.BackdropImage causes it to be loaded if
            // it doesn't already exist
            Image   image = item.BackdropImage;
        }

        public void LoadDisplayPrefs (FolderModel folder)
        {
            if (folder != null)
            {
                // Referencing folder.DisplayPrefs causes it to be loaded if
                // it doesn't already exist
                Guid    id = folder.DisplayPrefs.Id;
            }
        }

        //Yahoo Weather Main Images Override - not yet finished
        public Image GetWeatherImage(string imageType)
        {
            string imagePath = Path.Combine(ApplicationPaths.AppPluginPath, "Subdued\\CustomWeather\\" + imageType + "{0}");
            string resx = null;
            Image image = null;

            if (File.Exists(imagePath))
                resx = "file://" + imagePath;
            else
                resx = string.Format("resx://Subdued/Subdued.Resources/Weather/_{0}");

            image = new Image(resx);

            return (image);
        }

        public bool IsWeatherImageCustom(string imageType)
        {
            string imagePath = Path.Combine(ApplicationPaths.AppPluginPath, "Subdued\\CustomWeather\\" + imageType + "{0}");
            bool result = false;

            if (File.Exists(imagePath))
                result = true;

            return (result);

        }

        //********************Custom PosterItem Images - cheesegeezer*******************************
        //Cheap Workaroud
        public Image GetOverlayImage (string imageType)
        {
            string imagePath = Path.Combine(ApplicationPaths.AppPluginPath, "Subdued\\CustomOverlays\\" + imageType + ".png");
            string  resx = null;
            Image   image = null;            

            if (File.Exists (imagePath))
                resx = "file://" + imagePath;
            else
                resx = "resx://Subdued/Subdued.Resources/" + imageType;

            image = new Image (resx);

            return (image);
        }

        public bool IsOverlayImageCustom (string imageType)
        {
            string imagePath = Path.Combine(ApplicationPaths.AppPluginPath, "Subdued\\CustomOverlays\\" + imageType + ".png");
            bool    result = false;

            if (File.Exists (imagePath))
                result = true;
                 
            return (result);
            
        }
        //End Custom PosterItem Images - cheesegeezer

        private string BuildRatingImagePath (string ratingString, string itemType, bool fullImage)
        {
            var path = Path.Combine(Config.Instance.ImageByNameLocation, "MediaInfo\\Subdued\\rated_");

            if (!String.IsNullOrEmpty(itemType))
                path += itemType + "_";

            // Replace with underscore
            ratingString = ratingString.Replace(' ', '_');
            ratingString = ratingString.Replace('-', '_');
            ratingString = ratingString.Replace('.', '_');
            ratingString = ratingString.Replace(',', '_');
            ratingString = ratingString.Replace('/', '_');
            ratingString = ratingString.Replace('\\', '_');
            ratingString = ratingString.Replace('^', '_');
            ratingString = ratingString.Replace('*', '_');

            // Remove
            ratingString = ratingString.Replace("!", "");
            ratingString = ratingString.Replace("@", "");
            ratingString = ratingString.Replace("#", "");
            ratingString = ratingString.Replace("$", "");
            ratingString = ratingString.Replace("%", "");
            ratingString = ratingString.Replace("(", "");
            ratingString = ratingString.Replace(")", "");
            ratingString = ratingString.Replace("[", "");
            ratingString = ratingString.Replace("]", "");
            ratingString = ratingString.Replace("<", "");
            ratingString = ratingString.Replace(">", "");
            ratingString = ratingString.Replace("{", "");
            ratingString = ratingString.Replace("}", "");
            ratingString = ratingString.Replace(":", "");
            ratingString = ratingString.Replace(";", "");
            ratingString = ratingString.Replace("'", "");
            ratingString = ratingString.Replace("\"", "");
            ratingString = ratingString.Replace("?", "");
            ratingString = ratingString.Replace("=", "");

            // Special strings
            ratingString = ratingString.Replace("&", "_and_");
            ratingString = ratingString.Replace("+", "_plus_");

            ratingString = ratingString.TrimEnd('_');

            path += ratingString;

            if (fullImage)
                path += "_full";

            path += ".png";

            return (path);
        }

        private string TranslateItemType (string itemType)
        {
            if ((itemType == "series") || (itemType == "season") || (itemType == "episode"))
                itemType = "tv";
            else if ((itemType == "consolefolder") || (itemType == "gameitem"))
                itemType = "game";
            else if (itemType.IndexOf ("trailer") >= 0)
                itemType = "movie";

            return (itemType);
        }

        public Image GetRatingImage (string ratingString, string itemType, bool fullImage)
        {
            string imagePath = Path.Combine(Config.Instance.ImageByNameLocation, "MediaInfo\\Subdued\\rated_");
            Image   image = null;
            string  resx = "";
            bool    customFound = false;

            ratingString    = ratingString.ToLower ();
            itemType        = itemType.ToLower ();
            itemType        = TranslateItemType (itemType);
            imagePath       = BuildRatingImagePath (ratingString, itemType, fullImage);

            Logger.ReportVerbose ("Subdued - GetRatingImage checking for " + imagePath);

            if (File.Exists (imagePath))
            {
                resx        = "file://" + imagePath;
                customFound = true;
              
            image = new Image (resx);

            return (image);
            }
            else if (fullImage)
            {
                imagePath = BuildRatingImagePath (ratingString, itemType, false);

                Logger.ReportVerbose ("Subdued - GetRatingImage checking for " + imagePath);

                if (File.Exists (imagePath))
                {
                    resx = "file://" + imagePath;
                    customFound = true;
                }
            }

            if (!customFound)
            {
                imagePath = BuildRatingImagePath (ratingString, "", fullImage);

                Logger.ReportVerbose ("Subdued - GetRatingImage checking for " + imagePath);

                if (File.Exists (imagePath))
                {
                    resx = "file://" + imagePath;
                    customFound = true;
                }
                else if (fullImage)
                {
                    imagePath = BuildRatingImagePath (ratingString, "", false);

                    Logger.ReportVerbose ("Subdued - GetRatingImage checking for " + imagePath);

                    if (File.Exists (imagePath))
                    {
                        resx = "file://" + imagePath;
                        customFound = true;
                    }
                }
            }

            if (!customFound)
            {
                switch (itemType)
                {
                    case "movie":
                        switch (ratingString)
                        {
                            case "g":
                                if (fullImage)
                                    resx = "resx://Subdued/Subdued.Resources/rated_movie_g_full";
                                else
                                    resx = "resx://Subdued/Subdued.Resources/rated_g";
                                break;

                            case "pg":
                                if (fullImage)
                                    resx = "resx://Subdued/Subdued.Resources/rated_movie_pg_full";
                                else
                                    resx = "resx://Subdued/Subdued.Resources/rated_pg";
                                break;

                            case "pg-13":
                                if (fullImage)
                                    resx = "resx://Subdued/Subdued.Resources/rated_movie_pg_13_full";
                                else
                                    resx = "resx://Subdued/Subdued.Resources/rated_pg_13";
                                break;

                            case "r":
                                if (fullImage)
                                    resx = "resx://Subdued/Subdued.Resources/rated_movie_r_full";
                                else
                                    resx = "resx://Subdued/Subdued.Resources/rated_r";
                                break;

                            case "nc-17":
                                if (fullImage)
                                    resx = "resx://Subdued/Subdued.Resources/rated_movie_nc_17_full";
                                else
                                    resx = "resx://Subdued/Subdued.Resources/rated_nc_17";
                                break;

                            case "ur":
                            case "u":
                            case "nr":
                            case "unrated":
                            case "not yet rated":
                                if (fullImage)
                                    resx = "resx://Subdued/Subdued.Resources/rated_movie_nr_full";
                                else
                                    resx = "resx://Subdued/Subdued.Resources/rated_nr";
                                break;
                        }
                        break;

                    case "tv":
                        switch (ratingString)
                        {
                            case "tv-g":
                                resx = "resx://Subdued/Subdued.Resources/rated_tv_g";
                                break;

                            case "tv-ma":
                                resx = "resx://Subdued/Subdued.Resources/rated_tv_ma";
                                break;

                            case "tv-y7":
                                resx = "resx://Subdued/Subdued.Resources/rated_tv_y7";
                                break;

                            case "tv-y":
                                resx = "resx://Subdued/Subdued.Resources/rated_tv_y";
                                break;

                            case "tv-14":
                                resx = "resx://Subdued/Subdued.Resources/rated_tv_14";
                                break;

                            case "tv-pg":
                                resx = "resx://Subdued/Subdued.Resources/rated_tv_pg";
                                break;

                            case "tv-v7-fv":
                                resx = "resx://Subdued/Subdued.Resources/rated_tv_y7_fv";
                                break;
                        }
                        break;

                    case "game":
                        switch (ratingString)
                        {
                            case "ec":
                                resx = "resx://Subdued/Subdued.Resources/esrb_ec";
                                break;

                            case "e":
                                resx = "resx://Subdued/Subdued.Resources/esrb_e";
                                break;

                            case "10+":
                                resx = "resx://Subdued/Subdued.Resources/esrb_e10";
                                break;

                            case "t":
                                resx = "resx://Subdued/Subdued.Resources/esrb_t";
                                break;

                            case "m":
                                resx = "resx://Subdued/Subdued.Resources/esrb_m";
                                break;

                            case "ao":
                                resx = "resx://Subdued/Subdued.Resources/esrb_ao";
                                break;

                            case "rp":
                                resx = "resx://Subdued/Subdued.Resources/esrb_rp";
                                break;

                            case "ka":
                                resx = "resx://Subdued/Subdued.Resources/esrb_ka";
                                break;
                        }
                        break;
                }
            }

            if (!String.IsNullOrEmpty (resx))
                image = new Image (resx);

            return (image);
        }

        public bool HasRatingImage (string ratingString, string itemType, bool fullImage, bool customOnly, bool has)
        {
            bool    result = false;

            if (!String.IsNullOrEmpty (ratingString) && (ratingString.ToLower () != "none"))
            {
                string  imagePath = "";
                bool    customFound = false;

                ratingString    = ratingString.ToLower ();
                itemType        = itemType.ToLower ();
                itemType        = TranslateItemType (itemType);
                imagePath       = BuildRatingImagePath (ratingString, itemType, fullImage);

                if (File.Exists (imagePath))
                {
                    result      = true;
                    customFound = true;
                }
                else if (fullImage)
                {
                    imagePath = BuildRatingImagePath (ratingString, itemType, false);

                    if (File.Exists (imagePath))
                    {
                        result      = true;
                        customFound = true;
                    }
                }

                if (!customFound)
                {
                    imagePath = BuildRatingImagePath (ratingString, "", fullImage);

                    if (File.Exists (imagePath))
                    {
                        result = true;
                        customFound = true;
                    }
                    else if (fullImage)
                    {
                        imagePath = BuildRatingImagePath (ratingString, "", false);

                        if (File.Exists (imagePath))
                        {
                            result = true;
                            customFound = true;
                        }
                    }
                }

                if (!customFound && !customOnly)
                {
                    switch (itemType)
                    {
                        case "movie":
                            switch (ratingString)
                            {
                                case "g":                                
                                case "pg":
                                case "pg-13":
                                case "r":
                                case "nc-17":
                                case "ur":
                                case "u":
                                case "nr":
                                case "unrated":
                                case "not yet rated":
                                    result = true;
                                    break;
                            }
                            break;

                        case "tv":
                            switch (ratingString)
                            {
                                case "tv-g":
                                case "tv-ma":
                                case "tv-y7":
                                case "tv-y":
                                case "tv-14":
                                case "tv-pg":
                                case "tv-v7-fv":
                                    result = true;
                                    break;
                            }
                            break;

                        case "game":
                            switch (ratingString)
                            {
                                case "ec":
                                case "e":
                                case "10+":
                                case "t":
                                case "m":
                                case "ao":
                                case "rp":
                                case "ka":
                                    result = true;
                                    break;
                            }
                            break;
                    }
                }

                if (!has)
                    result = !result;
            }

            return (result);
        }
          

        //Studio Item
        public List<Item> GetStudioImages (Item item, int max)
        {
            List<Item> images = new List<Item> ();

            if (item != null)
            {
                int count = 0;

                if ((item.StudioItems.Count == 0) && (item.ItemTypeString == "Episode") && (item.Series != null))
                {
                    foreach (StudioItemWrapper sir in item.Series.StudioItems)
                    {
                        if (sir.Item.HasPrimaryImage)
                        {
                            images.Add (sir.Item);
                            count++;

                            if ((max > 0) && (count == max))
                                break;
                        }
                    }
                }
                else
                {
                    foreach (StudioItemWrapper sir in item.StudioItems)
                    {
                        if (sir.Item.HasPrimaryImage)
                        {
                            images.Add (sir.Item);
                            count++;

                            if ((max > 0) && (count == max))
                                break;
                        }
                    }
                }
            }

            return (images);
        }

        public Item GetQuickListItem (FolderModel folder, int index)
        {
            Item    item = null;

            if ((folder != null) && (index >= 0) && (index < folder.QuickListItems.Count))
                item = folder.QuickListItems[index];

            return (item);
        }

        private Item GetCoverWallItem (BaseItem baseItem)
        {
            Item        result = null;
            Item        item = null;
            FolderModel parent = null;

            if ((baseItem != null) && !String.IsNullOrEmpty (baseItem.PrimaryImagePath))
            {
                item = ItemFactory.Instance.Create (baseItem);

                if (item != null)
                {
                    if (baseItem.Parent != null)
                        parent = ItemFactory.Instance.Create (baseItem.Parent) as FolderModel;

                    if ((item.ItemTypeString == "Series") ||
                        (item.ItemTypeString == "ArtistAlbum") ||
                        (item.ItemTypeString == "MusicAlbum") ||
                        (item.IsPlayable &&
                            (item.ItemTypeString != "Episode") &&
                            (item.ItemTypeString != "Song") &&
                            ((parent == null) || (parent.ItemTypeString != "Series"))))
                    {
                        result = item;
                    }
                }
            }

            return (result);
        }

        public Size GetCoverWallThumbSize (FolderModel folder, int multipleOf, bool screenSaver, bool recursive)
        {
            Size    thumbSize = new Size (0, 0);

            if (folder != null)
            {
                using (new Profiler ("Subdued - GetCoverWallThumbSize for '" + ((screenSaver) ? "Screen Saver" : folder.Name) + "'"))
                {
                    List<Folder>    folders = new List<Folder> ();
                    float           aspect = 1F;
                    float           totalAspect = 0F;
                    Size            refSize = new Size (0, 0);
                    int             nCount = 0;

                    if (screenSaver)
                    {
                        foreach (BaseItem child in MediaBrowser.Application.CurrentInstance.RootFolder.Children)
                        {
                            if ((child is Folder) && ThemeConfig.IsCoverWallScreenSaverFolder (child.Name))
                                folders.Add (child as Folder);
                        }
                    }
                    else
                    {
                        if (ThemeConfig.IsCoverWallBackdropFolder (folder.Name))
                            folders.Add (folder.Folder);
                    }

                    foreach (Folder curFolder in folders)
                    {
                        foreach (BaseItem baseItem in curFolder.RecursiveChildren)
                        {
                            if (GetCoverWallItem (baseItem) != null)
                            {
                                aspect = baseItem.PrimaryImage.Aspect;

                                if (aspect == 0)
                                    aspect = 1;

                                totalAspect += aspect;
                                nCount++;
                            }
                        }
                    }

                    if (nCount > 0)
                    {
                        aspect = totalAspect / (float)nCount;

                        if (aspect >= 1)
                        {
                            thumbSize.Width = 220;
                            thumbSize.Height = (int)((float)(thumbSize.Width) * aspect);
                        }
                        else
                        {
                            thumbSize.Height = 220;
                            thumbSize.Width = (int)((float)(thumbSize.Height) / aspect);
                        }
                    }

                    Logger.ReportInfo ("Subdued - GetCoverWallThumbSize for '" + ((screenSaver) ? "Screen Saver" : folder.Name) + "' checked " + nCount + " items");
                }
            }

            return (thumbSize);
        }

        public Size GetCoverWallActualThumbSize (Size thumbSize)
        {
            return (GetBoundedReferenceSize (thumbSize, new Size (220, 220)));
        }

        public List<Item> GetCoverWallChildren (FolderModel folder, int multipleOf, bool screenSaver, bool recursive)
        {
            List<Item>  playable = new List<Item> ();

            if (folder != null)
            {
                using (new Profiler ("Subdued - GetCoverWallChildren for '" + ((screenSaver) ? "Screen Saver" : folder.Name) + "'"))
                {
                    List<Folder>    folders = new List<Folder> ();
                    Item            item;

                    if (screenSaver)
                    {
                        foreach (BaseItem child in MediaBrowser.Application.CurrentInstance.RootFolder.Children)
                        {
                            if ((child is Folder) && ThemeConfig.IsCoverWallScreenSaverFolder (child.Name))
                                folders.Add (child as Folder);
                        }
                    }
                    else
                    {
                        if (ThemeConfig.IsCoverWallBackdropFolder (folder.Name))
                            folders.Add (folder.Folder);
                    }

                    foreach (Folder curFolder in folders)
                    {
                        foreach (BaseItem baseItem in curFolder.RecursiveChildren)
                        {
                            item = GetCoverWallItem (baseItem);

                            if (item != null)
                                playable.Add (item);
                        }
                    }

                    Logger.ReportInfo ("Subdued - GetCoverWallChildren for '" + ((screenSaver) ? "Screen Saver" : folder.Name) + "' added " + playable.Count + " items");

                    if ((playable.Count > 0) && (multipleOf > 1))
                    {
                        int groups = playable.Count / multipleOf;
                        int extra = playable.Count % multipleOf;

                        if ((groups <= 1) || (extra > 0))
                        {
                            Random  rand = new Random ();
                            int     nCount = playable.Count;
                            int     nPos = 0;

                            extra = multipleOf - extra;

                            if (groups == 0)
                                extra += multipleOf;

                            for (nPos = 0; nPos < extra; nPos++)
                                playable.Add (playable[rand.Next (0, nCount)]);
                        }
                    }

                    ShuffleGenericList (playable);
                }
            }

            return (playable);
        }

        private static void ShuffleGenericList<T> (IList<T> list)
        {
            //generate a Random instance
            Random rnd = new Random ();
            //get the count of items in the list
            int i = list.Count ();
            //do we have a reference type or a value type
            T val = default (T);

            //we will loop through the list backwards
            while (i >= 1)
            {
                //decrement our counter
                i--;
                //grab the next random item from the list
                var nextIndex = rnd.Next (i, list.Count ());
                val = list[nextIndex];
                //start swapping values
                list[nextIndex] = list[i];
                list[i] = val;
            }
        }

        public ActorItemWrapper GetActor (Item item, int index)
        {
            ActorItemWrapper    actor = null;

            if ((item != null) && (index >= 0) && (index < item.Actors.Count))
                actor = item.Actors[index];

            return (actor);
        }

        public ChapterItem GetChapters(Item item, int index)
        {
            ChapterItem chapter = null;

            if ((item != null) && (index >= 0) && (index < item.Chapters.Count))
                chapter = item.Chapters[index];

            return (chapter);
        }

        public Item GetSpecials (Item item, int index)
        {
            Item special = null;

            if ((item != null) && (index >= 0) && (index < item.SpecialFeatures.Count))
                special = item.SpecialFeatures[index];

            return (special);
        }

        public string GetMediaDetailsString (Item item)
        {
            string  details = "";

            if ((item != null) && (item.MediaInfo != null))
            {
                string  videoDetails = "";
                string  audioDetails = "";

                if (!String.IsNullOrEmpty (item.MediaInfo.VideoCodecString))
                {
                    videoDetails += Kernel.Instance.StringData.GetString ("VideoFormatLabel") + ": " + item.MediaInfo.VideoCodecString;

                    if (item.MediaInfo.VideoBitRate >= 10000)
                        videoDetails += " @ " + (item.MediaInfo.VideoBitRate / 1000).ToString () + " " + Kernel.Instance.StringData.GetString ("KBsStr");
                    else if (item.MediaInfo.VideoBitRate > 0)
                        videoDetails += " @ " + item.MediaInfo.VideoBitRate.ToString () + " " + Kernel.Instance.StringData.GetString ("KBsStr");
                }

                if (!String.IsNullOrEmpty (item.MediaInfo.VideoResolutionString))
                {
                    if (!String.IsNullOrEmpty (videoDetails))
                        videoDetails += "\n";

                    videoDetails += Kernel.Instance.StringData.GetString ("VideoResolutionLabel") + ": " + item.MediaInfo.VideoResolutionString;

                    if (!String.IsNullOrEmpty (item.AspectRatioString))
                        videoDetails += " (" + item.AspectRatioString + ")";

                    if (!String.IsNullOrEmpty (item.MediaInfo.VideoFrameRateString))
                        videoDetails += " @ " + item.MediaInfo.VideoFrameRateString;
                }

                if (!String.IsNullOrEmpty (videoDetails))
                    details += videoDetails;

                if (!String.IsNullOrEmpty (item.MediaInfo.SubtitleString))
                {
                    if (!String.IsNullOrEmpty (details))
                        details += "\n\n";

                    details += Kernel.Instance.StringData.GetString ("SubtitlesLabel") + ": " + item.MediaInfo.SubtitleString.Replace (" / ", ", ");
                }

                if (!String.IsNullOrEmpty (item.MediaInfo.AudioProfileString))
                {
                    audioDetails += Kernel.Instance.StringData.GetString ("AudioFormatLabel") + ": " + item.MediaInfo.AudioProfileString;

                    if (!String.IsNullOrEmpty (item.MediaInfo.AudioChannelString))
                        audioDetails += " (" + item.MediaInfo.AudioChannelString + " " + Kernel.Instance.StringData.GetString ("ChannelsStr").ToLower () + ")";

                    if (item.MediaInfo.AudioBitRate >= 10000)
                        audioDetails += " @ " + (item.MediaInfo.AudioBitRate / 1000).ToString () + " " + Kernel.Instance.StringData.GetString ("KBsStr");
                    else if (item.MediaInfo.AudioBitRate > 0)
                        audioDetails += " @ " + item.MediaInfo.AudioBitRate.ToString () + " " + Kernel.Instance.StringData.GetString ("KBsStr");
                }

                if (!String.IsNullOrEmpty (item.MediaInfo.AudioStreamString))
                {
                    if (!String.IsNullOrEmpty (audioDetails))
                        audioDetails += "\n";

                    audioDetails += Kernel.Instance.StringData.GetString ("AudioStreamsLabel") + ": " + item.MediaInfo.AudioStreamString.Replace (" / ", ", ");
                }

                if (!String.IsNullOrEmpty (audioDetails))
                {
                    if (!String.IsNullOrEmpty (details))
                        details += "\n\n";

                    details += audioDetails;
                }

                if (!String.IsNullOrEmpty (item.LastPlayedString))
                {
                    if (!String.IsNullOrEmpty (details))
                        details += "\n\n";

                    details += Kernel.Instance.StringData.GetString ("LastWatchedLabel") + ": " + item.LastPlayedString;
                }
            }

            if (String.IsNullOrEmpty (details))
                details = Kernel.Instance.StringData.GetString ("NoMediaDetailsLabel");

            return (details);
        }

        public Guid GetFolderPrefsId (FolderModel folder)
        {
            Guid    id = Guid.Empty;

            if (folder != null)
            {
                id = folder.Id;

                if (Config.Instance.EnableSyncViews)
                {
                    if (folder.BaseItem is Folder && folder.BaseItem.GetType () != typeof (Folder))
                    {
                        id = folder.BaseItem.GetType ().FullName.GetMD5 ();
                    }
                }

            }

            return (id);
        }

        public bool IsItemInList (List<Item> list, BaseItem item)
        {
            bool    result = false;

            if (item is Folder)
            {
                Folder folder = (Folder)item;

                foreach (BaseItem child in folder.Children)
                {
                    if (IsItemInList (list, child))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (Item curItem in list)
                {
                    if (curItem is FolderModel)
                    {
                        FolderModel curFolder = (FolderModel)curItem;

                        if (IsItemInGroupedList (curFolder.Folder.Children, item))
                        {
                            result = true;
                            break;
                        }
                    }
                    else if (curItem.Id == item.Id)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return (result);
        }

        private bool IsItemInGroupedList (IList<BaseItem> list, BaseItem item)
        {
            bool    result = false;

            foreach (BaseItem curItem in list)
            {
                if (curItem.Id == item.Id)
                {
                    result = true;
                    break;
                }
            }

            return (result);
        }

        public float CalcRALTopLayout (int thumbHeight, bool showLabel, int labelHeight, int coverArtAdj)
        {
            float   result = Math.Min (0.95F, 0.7F + ((float)(CalcRALActualThumbHeight (thumbHeight, showLabel, labelHeight, coverArtAdj) - 100) * 0.0015F));

            return (result);
        }

        public int CalcRALMiddleLayout (int titleHeight, int thumbHeight, bool showLabel, int labelHeight, int coverArtAdj)
        {
            int result = -(titleHeight + (CalcRALActualThumbHeight (thumbHeight, showLabel, labelHeight, coverArtAdj) / 2));

            return (result);
        }

        public float CalcRALBottomLayout (int thumbHeight, bool showLabel, int labelHeight, int coverArtAdj)
        {
            float   result = Math.Max (0.12F, (0.35F - ((float)(CalcRALActualThumbHeight (thumbHeight, showLabel, labelHeight, coverArtAdj) - 100) * 0.0015F)));

            return (result);
        }

        public int CalcCycleBottomLayout (int titleHeight, int thumbHeight, bool showLabel, int labelHeight, int coverArtAdj)
        {
            int result = -(titleHeight + CalcRALActualThumbHeight (thumbHeight, showLabel, labelHeight, coverArtAdj) + 20 - 3);

            return (result);
        }

        public int CalcChildBottomLayoutTop (int titleHeight, int thumbHeight, bool showLabel, int labelHeight, int coverArtAdj)
        {
            int result = -(titleHeight + CalcRALActualThumbHeight (thumbHeight, showLabel, labelHeight, coverArtAdj) + 20);

            return (result);
        }

        public int CalcChildBottomLayoutBottom (int thumbHeight, bool showLabel, int labelHeight, int coverArtAdj)
        {
            int result = -(CalcRALActualThumbHeight (thumbHeight, showLabel, labelHeight, coverArtAdj) + 20);

            return (result);
        }

        public int CalcChildBGLayoutBottom (int thumbHeight, bool showLabel, int labelHeight, int coverArtAdj)
        {
            int result = CalcRALActualThumbHeight (thumbHeight, showLabel, labelHeight, coverArtAdj) + 20;

            return (result);
        }

        private int CalcRALActualThumbHeight (int thumbHeight, bool showLabel, int labelHeight, int coverArtAdj)
        {
            int result = thumbHeight;

            if (showLabel && (labelHeight > 0))
                result += (labelHeight - coverArtAdj);

            return (result);
        }

        public Vector3 CalcActualGrowPct (int growPct)
        {
            Vector3 scale = new Vector3 (1F, 1F, 1F);
            Single  actualPct  = new Single ();

            actualPct = (Single)(growPct + 100) / 100F;

            scale.X = actualPct;
            scale.Y = actualPct;

            return (scale);
        }

        public Vector3 CalcRALActualGrowPct (int thumbSize, int avail)
        {
            Vector3 scale = new Vector3 (1F, 1F, 1F);
            Single  actualPct  = new Single ();

            actualPct = 1.0F + ((Single)avail / (Single)thumbSize);

            scale.X = actualPct;
            scale.Y = actualPct;

            return (scale);
        }

        public Inset CalcActualGrow (Size thumbSize, int growPct, Vector3 centerPointPct, bool absolute)
        {
            Inset   actualGrow = new Inset (0, 0, 0, 0);

            actualGrow.Left     = (int)(((double)(thumbSize.Width * growPct) / 100F) * centerPointPct.X);
            actualGrow.Right    = (int)(((double)(thumbSize.Width * growPct) / 100F) * (1F - centerPointPct.X));
            actualGrow.Top      = (int)(((double)(thumbSize.Height * growPct) / 100F) * centerPointPct.Y);
            actualGrow.Bottom   = (int)(((double)(thumbSize.Height * growPct) / 100F) * (1F - centerPointPct.Y));

            if (!absolute)
            {
                actualGrow.Left = -actualGrow.Left;
                actualGrow.Top  = -actualGrow.Top;
            }

            return (actualGrow);
        }

        public Size CalcActualGrowSize (Size thumbSize, int growPct)
        {
            Size    actualGrow = new Size ((int)((double)(thumbSize.Width * growPct) / 100F),
                                           (int)((double)(thumbSize.Height * growPct) / 100F));

            return (actualGrow);
        }

        public int CalcFocusFrameAdj (int frameSize, int coverArtAdj, bool inverse)
        {
            int result = frameSize - coverArtAdj;

            if (inverse)
                result = -result;

            return (result);
        }

        public int CalcCoverflowBottomOffset (int bgOffset, string style, int infoHeight, int titleInfoHeight, bool isRoot, bool showAsText)
        {
            int result = -bgOffset;

            if (style == "Split")
            {
                result -= infoHeight;
            }
            else if (style == "Bottom")
            {
                if (isRoot && showAsText)
                    result -= infoHeight;
                else
                    result -= titleInfoHeight;
            }

            if (isRoot)
                result -= 40;

            return (result);
        }

        public float CalcRotateToRightExtension (int sepPct)
        {
            return (0.94F + ((float)sepPct * .0036F));
        }

        public Size CalcCoverWallReferenceSize (Size sizeOrig, int coverArtAdj, string orientation, string rotation, int rows, int columns)
        {
            Size    bounds = new Size (0,0);

            if (orientation == "Horizontal")
            {
                bounds.Height = (808 / rows) + (coverArtAdj * 2);

                if (rotation == "To Back")
                    bounds.Height = (int)((float)bounds.Height * 1.08F);
            }
            else if (orientation == "Vertical")
            {
                bounds.Width = (1406 / columns) + (coverArtAdj * 2);
            }

            return (GetBoundedReferenceSize (sizeOrig, bounds));
        }

        public int CalcCoverWallRepeatGap (int coverArtAdj)
        {
            return (-(2 * coverArtAdj));
        }

        public Size CalcActualThumbSpacing (int spacing, int coverArtAdj)
        {
            int nSpacing = spacing - (2 * coverArtAdj);

            return (new Size (nSpacing, nSpacing));
        }

        public int CalcActualThumbSpacingInt (int spacing, int coverArtAdj)
        {
            return (spacing - (2 * coverArtAdj));
        } 

        public void AskToQuit()
        {
            MediaCenterEnvironment mediaCenterEnvironment = AddInHost.Current.MediaCenterEnvironment;
            const string text = "Are you sure you want to quit MediaBrowser?";
            const string caption = "Quit MediaBrowser";
            if (mediaCenterEnvironment.Dialog(text, caption, DialogButtons.Cancel | DialogButtons.Ok, 0, true) == DialogResult.Ok)
            {
                Application.CurrentInstance.BackOut();
            }
        }

        // GameTime support
        public string Players
        {
            get
            {
                string players = GetDynamicProperty("Players");

                if (players != null)
                    return players;

                return "";
            }
        }

        public Single TgdbRating
        {
            get
            {
                Single rating = GetDynamicPropertyAsSingle ("TgdbRating");

                if (rating > 0)
                    return rating;

                return 0;
            }
        }

        public string EsrbRating
        {
            get
            {
                string rating = GetDynamicProperty("EsrbRating");

                if (rating != null)
                    return rating;

                return "";
            }
        }

        public string ConsoleReleaseYear
        {
            get
            {
                int year = GetDynamicPropertyAsInt("ReleaseYear");

                if (year != 0)
                    return year.ToString();

                return "";
            }
        }

        public string Company
        {
            get
            {
                string company = GetDynamicProperty("Manufacturer");

                if (company != null)
                    return company;

                return "";
            }
        }

        public string CpuBits
        {
            get
            {
                string cpu = GetDynamicProperty("CPU");

                if (cpu != null)
                    return cpu;

                return "";
            }
        }

        public string GpuBits
        {
            get
            {
                string gpu = GetDynamicProperty("GPU");

                if (gpu != null)
                    return gpu;

                return "";
            }
        }

        // Music support
        public string Duration
        {
            get
            {
                string duration = GetDynamicProperty ("Duration");

                if (duration != null)
                    return duration;

                return "";
            }
        }

        public void LogVerbose (String message)
        {
            Logger.ReportVerbose (message);
        }

        public void LogInfo (String message)
        {
            Logger.ReportInfo (message);
        }

        public void LogWarning (String message)
        {
            Logger.ReportWarning (message);
        }

        public void LogError (String message)
        {
            Logger.ReportError (message);
        }

        public string ImageUrl { get; set; }

        #region NextUp

        public FolderModel SelectedChild
        {
            get { return Application.CurrentInstance.CurrentFolder.SelectedChild as FolderModel; }
        }

        public bool IsTVShowFolder
        {
            get
            {
                return SelectedChild != null && SelectedChild.CollectionType == "tvshows";
            }
        }

        public ArrayListDataSet GetNextUpEpisodes()
        {
            return GetAPIItems.GetNextUpSet();
        }

        #endregion

    }
}
