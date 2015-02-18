using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MediaBrowser.Library.Plugins;
using MediaBrowser.Library.Entities;
using MediaBrowser.Library.Logging;
using MediaBrowser;
using MediaBrowser.Util;
using MediaBrowser.Library.Configuration;
using MediaBrowser.Library;

//******************************************************************************************************************
//  This class is the heart of your plug-in Theme.  It is instantiated by the initial loading logic of MediaBrowser.
//
//  For your project to build you need to be sure the reference to the MediaBrowser assembly is current.  More than
//  likely, it is broken in this template as the template cannot copy binaries, plus it would probably be out of date
//  anyway.  Expand the References tree in the Solution Explorer to the right and make sure you have a good reference
//  to a current version of the mediabrowser.dll.
//
//  For your project to load as a MediaBrowser Plug-in you need to build your release .dll and copy it to the ehome
//  directory (under your windows directory). AND ALSO create a .pgn file with the same name as your dll and put it
//  in c:\program data\mediabrowser\plugins.  The Configurator will do this automatically if provided a valid 
//  plugin_info.xml file and location for your .dll
//******************************************************************************************************************

namespace Subdued
{
    class Plugin : BasePlugin
    {
        static readonly Guid SubduedGuid = new Guid ("f5d36ea5-736f-4860-8162-07588cb7d5b3");

        public static MyConfig config = null;

        public Plugin ()
        {
            Logger.ReportInfo ("Subdued - Creating Theme");

            using (new Profiler ("Subdued - Theme Creation"))
            {
                bool isMC = AppDomain.CurrentDomain.FriendlyName.Contains ("ehExtHost");
                if (isMC)
                {
                    MyConfig.InitDisplayPrefs ();
                    MyConfig.InitStyles ();

                    if (config == null)
                        config = new MyConfig ();
                }
            }
        }

        /// <summary>
        /// The Init method is called when the plug-in is loaded by MediaBrowser.  You should perform all your specific initializations
        /// here - including adding your theme to the list of available themes.
        /// </summary>
        /// <param name="kernel"></param>
        public override void Init (Kernel kernel)
        {
            try
            {
                Logger.ReportInfo ("Subdued - Initializing Theme");

                using (new Profiler ("Subdued - Theme Initialization"))
                {
                    //the AddTheme method will add your theme to the available themes in MediaBrowser.  You need to call it and send it
                    //resx references to your mcml pages for the main "Page" selector and the MovieDetailPage for your theme.
                    //The template should have generated some default pages and values for you here but you will need to create all the 
                    //specific mcml files for the individual views (or, alternatively, you can reference existing views in MB.
                    kernel.AddTheme ("Subdued", "resx://Subdued/Subdued.Resources/Page#PageSubdued", "resx://Subdued/Subdued.Resources/DetailMovieView#SubduedMovieView", config);

                    //The AddConfigPanel method will allow you to extend the config page of MediaBrowser with your own options panel.
                    //You must create this as an mcml UI that fits within the standard config panel area.  It must take Application and 
                    //FocusItem as parameters.  The project template should have generated an example ConfigPage.mcml that you can modify
                    //or, if you don't wish to extend the config, remove it and the following call to AddConfigPanel.  This call is in 
                    //a conditional because we can only perform these operations when in MediaCenter (we can be called from other places)
                    bool isMC = AppDomain.CurrentDomain.FriendlyName.Contains ("ehExtHost");
                    if (isMC)
                    {
                        if (config == null)
                            config = new MyConfig ();

                        config.CheckActiveStyle ();

                        kernel.AddConfigPanel ("Subdued General", "resx://Subdued/Subdued.Resources/ConfigPanel#ConfigPanel", config);
                        kernel.AddConfigPanel ("Subdued Cover Wall", "resx://Subdued/Subdued.Resources/ConfigPanelCoverWall#ConfigPanelCoverWall", config);
                        kernel.AddConfigPanel ("Subdued Views", "resx://Subdued/Subdued.Resources/ConfigPanelViews#ConfigPanelViews", config);
                        kernel.AddInternalIconTheme ("Subdued"); //tells core that we want to use icons in our resource file
                        //If you want to add any context menus they need to be inside this logic as well.
                    }
                    else
                        Logger.ReportInfo ("Not creating menus for Subdued.  Appear to not be in MediaCenter.  AppDomain is: " + AppDomain.CurrentDomain.FriendlyName);

                    //The AddStringData method will allow you to extend the localized strings used by MediaBrowser with your own.
                    //This is useful for adding descriptive text to go along with your theme options.  If you don't have any theme-
                    //specific options or other needs to extend the string data, remove the following call.
                    kernel.StringData.AddStringData (MyStrings.FromFile (MyStrings.GetFileName ("Subdued-")));
                }
            }
            catch (Exception ex)
            {
                Logger.ReportException ("Error adding theme - probably incompatible MB version", ex);
            }

            Logger.ReportInfo ("Subdued - Version " + Version.ToString () + " ready");
        }

        public override string Name
        {
            //provide a short name for your theme - this will display in the configurator list box
            get
            {
                return "Subdued Theme";
            }
        }

        public override string Description
        {
            //provide a longer description of your theme - this will display when the user selects the theme in the plug-in section
            get
            {
                return "A minimalist theme emphasizing a balance between style and functionality.";
            }
        }

        public override string RichDescURL
        {
            //You can return a fully-qualified URI to a resource that displays a rich description of your plugin here
            get { return ""; }
        }

        public override bool InstallGlobally
        {
            get
            {
                //this must return true so we can pass references to our resources to MB
                return true; //we need to be installed in a globally-accessible area (GAC, ehome)
            }
        }

        // Return the lowest version of MediaBrowser with which this plug-in is compatable
        public override System.Version RequiredMBVersion
        {
            get
            {
                return new System.Version (3, 0, 62, 0);
            }
        }

        // Return the highest version of MediaBrowser with which this plug-in has been tested
        public override System.Version TestedMBVersion
        {
            get
            {
                return new System.Version (3, 0, 62, 0);
            }
        }

    }


}
