3.0 Ideas
---------
- Filtering (rating, hd, genre, etc.)
- Flat/grouped views
- Browse/flip-book view
- Show % played bargraph

Issues:
-------------
! Using CoverWall with MB3 and large collections slows Subdued to a snails pace! Find a better way of caching the images
! Returning to item details view while playing doesn't show Now Playing view; backing out and going back in does
- With Poster view, vertical scroll, scroll wrapping doesn't work (?!)
- Use Banners:
    - Switching to 'Use Banners' sometimes shows no images first time (switching off/on will display)
    - Changing thumb size in banner mode doesn't update until config is closed
    - Switching to 'Use Banners' when the item has no banner displays name label as if it had no image

TODO:
-----
! Use primaray image for selected thumb (fix "blurriness")
- NPV view sizes
- Overview + thumb list, overview + text list (how do I know if overview is not just the child list?)

Feature Requests:
-----------------
- Scroll lock position (view setting)
- Media info strip on Album view (audio format)
- Easier navigation to config on EHS (left?)
- Layout % per folder
- Transparency padding (do not affect n thumbnails)
- Selected item on Details show thumb
- Show backdrop on Details instead of thumb
- Max width of backdrop on Thumb

Unsupported:
------------
- Root thumb and thumb strip views
- Remote content overlay

ChangeLog 3.0.1.5 - Release
-----------------
+ Remove subtitle reference (annoyance to users)
  Will look at adding to media Info.

ChangeLog 3.0.1.4 - Release
-----------------
MBC version 3.0.62 Required for new option
Changed Disc image location
Fix Details not showing


Changelog 3.0.1.2 - Dev
-----------------
+ Fixed Bug on MovieDetailView where selecting Cast or Details throws an error.

Changelog 3.0.1.1 - Dev
-----------------
+ Recently Watched Text changes to Media in Progress if the "treat Watched as in Progress" option is checked.

Changelog 3.0.1.0 - Release
-----------------
+ Added Weather to the HomeScreen.  Please use YahooWeather ID e.g. UKXX0001 on Server instead of City,Country
+ Added DiscArt to Detailed Movie View
+ Added LogoArt, ClearArt, ThumbArt in all views now except RootLayout.
+ Added Spinner menu to View Options for Logo, ClearArt, Thumbs or Off
= Restored custom Poster Custom overlay images - These are to go into %ProgramData%\MediaBrowser-Classic\Plugins\Subdued\Custom Overlays (Create your own Folder
= Restored custom MediaInfo icons - Although, you need to select a Custom IBN folder with the Server. Still Working on this


Changelog 3.0.0.0 (Inline with MB Classic and Mediabrowser 3)
-----------------
+ Added Logos to Coverflow view
+ Added Show Logos checkbox to Subdued Config


Chagelog 2.9.2
--------------
+ Added '(Un)Watched Indicator Position' general option: top-left, bottom-left, bottom-right (default: bottom-right)
+ Added support for custom poster overlay images (watched, unwatched, HD, new)
    + Use %PROGRAMDATA%\MediaBrowser\ImagesByName\Subdued\watched.png, unwatched.png, hd.png, new_bg.png, and new_fg.png
    + Images should be white with transparency to allow the current Style to colorize them
+ Added 'Colorize Poster Overlay Images' general option (default: on)
- Localization strings version updated to 1.0024

Chagelog 2.9.1 (Requires MB 2.6)
--------------------------------
+ Added support for offical IBN MPAA ratings image support
+ Added general option to enable/disable the Quick Play action on the item details view (default: disabled)
+ Added general option to enable/disable display of the movie (MPAA) rating icon (default: enabled)
+ Added Clear Logo to item details view
- Changed alignment of Clear Logo to be left-justified
- Localization strings version updated to 1.0023

Changelog 2.9 (Requires MB 2.5.2)
---------------------------------
+ Added ClearLogo display on Thumb, Thumb Strip, and Details view styles
+ Added '[Random]' option to Theme Style setting; will choose a random style to apply each time MB is started
+ Added support for MBMusic plug-in
- Changed 'NEW' indicator to be visible only if item is in the recently added list and has not been watched
- Fixed 'NEW' indicator visibility conditions when Folder Display is set to 'Thumb List'
- Fixed startup crash associated with showing labels on EHS recent items
- Fixed Cover Wall crash with MBMusic plug-in

Changelog 2.8.8
---------------
+ Added view option to Show Labels under posters on EHS RAL and poster, thumb, and thumbstrip view styles
- Fixed 'NEW' overlay appearing on EHS recent lists
- Fixed jagged edges on HD, watched, and unwatched indicators when using thumb rotation other than 'None'
- Fixed placement of HD, watched, and unwatched indicators to obey 'Cover Art Adjustment'
- Style engine version updated to 6 (added SdI_PosterLabelHeight)

Changelog 2.8.7.1
-----------------
+ Added general option to globally enable/disable display of new item indicator
- Fixed audio format display string on item details page Details panel to show both the codec and profile
- Fixed Media Info icons display on item details page Details panel to always be visible
- Localization strings version updated to 1.0022

Changelog 2.8.7
---------------
+ Added new item indicator overlay for items within a collection that are in its recent list; colorized by current style
+ Added view option to turn on/off new item indicator (default: on)
- Fixed layering order of tiered backdrops: theme bg < initial folder bg < fan art
- Fixed placement of poster overlays when changing thumb size using Live Layout
- Fixed issue where "extra" metadata is displayed after an item is refreshed
- Fixed issue where no Media Info icons are displayed if the pre-2.8.6 'Show Media Info Icons' setting was off
- Fixed minor display issues on item details view 'Details' panel: aspect ratio mismatch, separator character for audio streams
- Fixed path to theme display settings when UserSettingsPath is used
- Fixed required MB version number
- Style engine version updated to 5
- Localization strings version updated to 1.0021

Changelog 2.8.6
---------------
+ Added audio channels Media Info icon
+ Added options to show/hide individual Media Info icons; all icons shown by default
+ Added support for tiered main backdrops (StartupFolder + VF)
+ Added animation to HD, watched, and unwatched indicators for selected item
- Changed HD, watched, and unwatched indicators to better match theme; colorized by current style
- Fixed placement of HD, watched, and unwatched indicators if using view rotation
- Improved highlighting of actors and music tracks in details view
- Localization strings version updated to 1.0020

Changelog 2.8.5 (Requires MB 2.5)
---------------------------------
+ Added support for multi-level context menus
+ Added black background to backdrop NPV to prevent fan art/theme background from showing
- Improved speed and responsiveness of recursively counting a folder's children for display
- Fixed display of studio images to use studios from Series metadata for Episode items
- Fixed layout of Details panel on item details page when all Media Info and Last Watched is displayed

Changelog 2.8.4 (Requires MB 2.3.2)
-----------------------------------
+ Added 'Play All' and 'Shuffle Play' options to Music Support Plugin root context and play menus
+ Added 'Colorize Studio Icons' general option
+ Added 'Wrap Item List' EHS view option
+ Added collection child count display to horizontal EHS with top panel enabled
- Changed folder child count display to include all children recursively, not just the immediate children
- Moved 'Rotate Backdrops on All Levels' from general option to 'Rotate Backdrops' view-specific option
- Moved 'Show (Un)Watched Indicator on Movies' from general option to 'Enable (Un)Watched Indicators' view-specific option
- Changed order of actions on item details page to have 'Resume' at the top so it is the default if available
- Movied studio icons to Overview to Details panel on item details page (max of 3 displayed; only studios with images)
- Localization strings version updated to 1.0019

Changelog 2.8.3.1
-----------------
- Fixed crash when using first/last/prev/next on items details page with Cast or Details panel displayed

Changelog 2.8.3
---------------
+ Added support for custom/non-US ratings images; uses 'IBN\Ratings\Subdued\rated_typeStr_ratingStr.png' (e.g., 'rated_movie_pg.png' or 'rated_pg.png')
+ Added "full" rating image to item details page, and support for custom/non-US images; uses 'IBN\Ratings\Subdued\rated_typeStr_ratingStr_full.png' (e.g., 'rated_movie_pg_full.png' or 'rated_pg_full.png')
+ Added option to enable/disable colorization of custom/non-US ratings images (default: off)
+ Added watched/unwatched indicator to EHS Recent Additions list (obeys all current watched/unwatched settings)
+ Added recent date string ('added on' or 'watched on') to EHS recent item info
+ Added version string informational log entry at completion of theme initialization
- Moved SubduedDisplay folder from Plugins\Configurations to Cache to support shared display state
- Fixed '#' key to no longer cycle view styles if configuration is locked
- Fixed multi-line text justification on horizontal EHS collection names
- Fixed MPAA rating to show 'CS' custom rating text
- Localization strings version updated to 1.0018

Changelog 2.8.2
---------------
+ Added option to turn on/off the season poster under episode thumbnail on details view style and items details page (default: off)
+ Added watched/unwatched indicator to item details page; obeys 'Highlight Unwatched Items' and 'Watched Indicator in Details View' MB options
- Fixed problem with season poster and episode thumbnail overlay when using banners at season level
- Localization strings version updated to 1.0017

Changelog 2.8.1
---------------
+ Added display option to turn off text background on EHS (default: on)
+ Added display option to turn on top panel on horizontal EHS or non-details view style root view (default: off)
- Changed details view style and items details page to display season poster under episode thumbnail
- Fixed positioning of recent item thumbnails and info on horizontal EHS when shown on non-16:9 aspect display
- Adjusted spacing around MediaInfo icons to provide more consistent margins
- Fixed selection problem on poster and thumb view styles when mixing different thumbnail aspect ratios (e.g., posters w/ box set images)
- Fixed focus frame to use bounding box size instead of actual thumbnail size
- Localization strings version updated to 1.0016

Changelog 2.8
-------------
+ Added 'Orientation' view option to EHS: Vertical or Horizontal (default: Vertical)
+ Added support for non-US, or non-standard MPAA rating strings; actual string is displayed instead of custom icon
+ Added ability to adjust EHS Recent panel left position via Live Layout using Fwd/Rew buttons
- Removed all Subdued Views options that are modifiable via Live Layout
- Fixed 'Folder Display' view option to obey 'Thumb Spacing' and 'CoverArt Adjustment' config settings
- Moved extraction of MediaInfo icon set into worker thread to speed up theme initialization
- Localization strings version updated to 1.0015

Changelog 2.7.2.2
-----------------
- Expanded 'Scroll Item List' view option to include: Never, When Too Big, or Always
- Fixed issues with Cover Wall Screen Saver when launching MB directly to a collection via entry point
- Fixed problem with repeat gap on coverflow and thumbstrip views not using 'CoverArt Adjustment' config setting

Changelog 2.7.2.1
-----------------
- Fixed problem with Cover Wall Scroll using 'Down' no matter what the setting (oops)
- Removed gaps around edges of Cover Wall

Changelog 2.7.2
---------------
+ Added Cover Wall Screen Saver feature; set timeout and collections to include
+ Added watched/unwatched indicators to item list when 'Folder Display' view option is set to 'Text List'
+ Added 'Child Thumb Mini Backdrop' option to turn on/off display of mini backdrop on Thumb views (default: off)
- Moved Cover Wall options from Subdued General to Subdued Cover Wall category (sorry, daspannerman)
- Improved calculation of Cover Wall aspect thumbnail aspect ratio when collection contains items with different aspects
- Fixed display of Cover Wall to obey Backdrop Load Delay setting
- Fixed problem with Cover Wall not displaying any covers if item count was exactly one row/column
- Fixed unwatched indicator on Details view style for Series and Season items
- Fixed list centering for Thumb view style
- Localization strings version updated to 1.0014

Changelog 2.7.1
---------------
+ Added 'Cover Wall Scroll Speed' option of Crawl
- Changed 'Cover Wall Style' options to Off, Left, Right, Up, or Down (default: off)
- Fixed Cover Wall thumb aspect/sizing when parent folders used different aspect images than their items (e.g., box sets)
- Fixed crash with Cover Wall and Music Support plugin with playlists enabled
- Localization strings version updated to 1.0013

Changelog 2.7
-------------
+ Added Cover Wall for root view (EHS and normal); options include Style, Rotation, and Scroll Speed
+ Added EHS view option 'EHS Show Recent Backdrop' to enable/disable backdrop of selected recent item (default: on)
+ Added EHS view option 'EHS Show Recent Episode Thumb' to choose between episode thumbs or season posters (default: off)
- Moved 'EHS Show Recent Info' and 'EHS Thumb Size' config options to View Options popup menu
- Allow 'Backdrop Overlay' and 'Show NPV' to be set on EHS View Options
- Fixed thumb spacing to also include CoverArt adjustment
- Localization strings version updated to 1.0012 (1.0011 was used during a beta cycle)

Changelog 2.6
-------------
+ Introducing Live Layout for easy real-time layout of your views and thumb sizing...without leaving the view
+ New child folders now inherit Subdued-specfic view options from their parent folder
- Localization strings version updated to 1.0010

Changelog 2.5.3
---------------
- Now nearly every config setting can be changed WITHOUT needing to restart (only 'Theme Style' and 'MediaInfo Icon Set' still do)
- Localization strings version updated to 1.0009

Changelog 2.5.2
---------------
+ Added 'Unselected Thumb Alpha' (default: 100%) and 'Graduated Alpha' (default: off) general options
- Localization strings version updated to 1.0008

Changelog 2.5.1.1
-----------------
+ Added 720p, 720i, 1080p, and 1080i Media Info icons

Changelog 2.5.1 (Requires MB 2.3.1)
-----------------------------------
+ Added 'Blue Note' (for a better match with Media Center colors/fonts), 'Cold as Ice', 'Neon', and 'Redrum' theme styles
- Fixed layout of MediaInfo icons to wrap if out of room on detail and thumb views
- MPAA rating for TV seasons and episodes are now inherited from the series level
- Fixed MPAA rating to be not visible if the rating string is 'None' (Titan-compliant)
- Removed overlay "mask" from center of selected item focus frame; only visible on thumbs with transparency
- Fixed jagged edges on poster dim overlay when using thumb rotation other than 'None'
- Fixed initial backdrop delay and added support for MB backdrop delay setting
- Fixed year/manufacturer/game count info for GameTime console items appearing on two lines

Changelog 2.5
-------------
+ Added 'MediaInfo Icon Set' general option: Color, Mono, Custom Color, Custom Mono (default: Color)
+ Added 'Star Rating Style' general option: Star Graph, Numeric, Off (default: off)
+ Added general option to turn off first/prev/next/last buttons on item details view; does not disable keyboard/remote shortcuts (default: on)
+ Redesigned MPAA/ESRB ratings, star rating, and MediaInfo icons layout
+ Redesigned album view (used by Music Support plug-in)
- Separated 'Thumb Rotation' general option into Coverflow, Poster, Thumb, and Thumb Strip Rotation view options
- Fixed exception/crash that can occur when migrating display prefs, or backup up styles/strings if destination file exists
- Modified display preferences and styles initialization to only occur at theme (plug-in) initialization
- Fixed thumb strip view not obeying 'Child Thumb Top %' setting
- Style engine version updated to 4
- Localization strings version updated to 1.0007

Changelog 2.4.1
---------------
+ Added 'Show Clock/Config at Top' general option to place clock, busy indicator, and config buttons at top of screen (default: off)
+ Added 'Quick Play' action to item details view which is visible for Movies if a pre-play plugin (like MBIntros) is installed
- Improved thumb layout and scrolling of poster and thumb views, especially when using thumb rotation
- Moved display preference files to SubduedDisplay folder under Plugins\Configurations
- Localization strings version updated to 1.0006

Changelog 2.4
-------------
+ Added 'Thumb Rotation' general option to rotate thumb list on applicable views: none, to back, to right (default: none)
+ Redesigned 'Cast' panel on item details view
- Modified size of top/bottom info panels for root coverflow view when 'Show As Text' is enabled
- Modified layout of top/bottom info panels to prevent title with descending characters from overlapping MediaInfo icons
- Modified width of item details view actions panel to better handle translated strings
- MPAA rating icon will now display 'NR' for any non-empty ratings string that is not recognized
- Fixed problem with repeat gap on coverflow and thumbstrip views not using 'Thumb Spacing' config setting
- Fixed spacing issues with MediaInfo icons when not all icons are used
- Fixed layering issues with thumbstrip view when 'Thumb Grow %' causes thumb to extend into info panel
- Localization strings version updated to 1.0005

Changelog 2.3.1
---------------
- Fixed problem with config menu not showing the proper items with EHS enabled

Changelog 2.3
-------------
+ Added 'Selected Info Style' view option for coverflow and poster views: Split, Top, Bottom, Off (default: Split)
+ Added 'Show Backdrop Overlay' view option to show a full-screen overlay on top of the backdrop (default: off)
+ Added option to config menu to lock/unlock configuration; prevents all settings from being changed until unlocked
+ Added option to show master PC icon to allow Parental Control to be globally enabled/disabled (default: on)
+ Added help descriptions for config settings
- Removed 'Show Selected Item Info' view option; superceded by 'Selected Info Style' 
- Fixed long description auto-scrolling on thumb strip view
- Visiblity of 'Details' action on item details view is no longer controlled by 'Show Media Info' option; always visible
- Fixed a few localization string issues
- Style engine version updated to 3
- Localization strings version updated to 1.0004

Changelog 2.2
-------------
+ Added 'Spy vs. Spy 1' and 'Spy vs. Spy 2' theme styles (What, me worry?)
- Changed first/prev/next/last buttons on item details view to be in a fixed position
- Fixed problem with 'index of count' display disappearing on item details view after pressing fist/prev/next/last button
- Changed display of details view watched/unwatched indicator icon to use text color from style
- Fixed problem with 'Sort By' menu being disabled if using localized MB strings (specifically, translating 'NoneDispPref')
- Style engine version updated to 2

Changelog 2.1
-------------
+ Added 'Show As Text' view option for root coverflow view style
+ Style engine now will create a backup file of any styles that are overwritten because of version mismatch
+ Strings engine now will create a backup of any strings file that is overwritten because of version mismatch
- Changed styles engine to always copy over active style from SubduedStyles folder on startup
- Fixed label used for folder items count to work properly on non-EHS root view and views where 'Group By...' is set
- Fixed problem with poster dim margins using focus frame size instead of CoverArt adjustment
- Localization strings version changed to 1.0003

Changelog 2.0
-------------
+ New support for theme styles! Choose from included styles or create your own:
    + The Original - same as it ever was
    + White Shadow - like The Original, but a lighter shade of pale
+ Added option for CoverArt adjustment of poster focus frame and dim overlay (default: 0)
- Split up Subdued Options into Subdued General and Subdued Views

Changelog 1.6
-------------
+ Localized all display strings (testing help appreciated)
+ Added display of Series TV rating and star rating (if 'Show Media Info/Details' is on)
+ 'Show Media Info' option now controls visibility of 'Details' action on item details view
+ Added initial GameTime support
- Changed display of number of children of selected item to show 'Items' if 'Group By...' is set
- Changed layout of numeric star rating icon to better display whole number ratings
- Changed details and thumb views to show MediInfo icons on a separate line

Changelog 1.5.1
---------------
- Fixed problem with coverflow not obeying new spacing option

Changelog 1.5 (Requires MB 2.3)
-------------------------------
+ Added options to set thumbnail spacing (default: 15) and grow % for focused thumbnail (default: 25%); not applicable to EHS RAL
+ Added numeric star rating icon to media info panels; removed 5-star display
+ Added studios to item detils view 'Details' panel
+ Added display of number of child movies/seasons/episodes/albums/songs of selected item
+ Added support for MB backdrop rotation and transition intervals
- Fixed problem with selection returning to first item when navigating back to a view
- Changed running time display to show span as hours and minutes (if applicable)
- Improved visibility of elements at top of display when 'Top Panel Style' is not 'Double Panel'
- Fixed problem with child items not always displaying when 'Folder Display' is set to 'Overview'
- Fixed info display issues when refreshing an item

Changelog 1.4
-------------
+ Added option to set the size of the EHS RAL thumbnails (default: 200, min: 200, max: 400)
+ Added option to show item end time, with live update while an item is selected (default: off)
- Split 'View Style' config menu into 'View Style' (choice of display style) and 'View Options' (all other view-specific settings)
- Changed navigation of actions on item details view to disable currently visible info (Overview, Cast, Details)
- Moved 'Refresh' and 'Delete' to bottom of list of actions on item details view
- Implemented ebr's fix for navigating to first unwatched item

Changelog 1.3.1
---------------
+ Extended folder view settings to turn off background behind thumbnails and title/info display to Thumb and Thumbstrip views

Changelog 1.3
-------------
+ Added folder view setting for Coverflow and Poster to turn off background behind thumbnails (default: on)
+ Added folder view setting for Coverflow and Poster to turn off title/info display for selected item (default: on)

Changelog 1.2
-------------
+ Added option to choose top panel style: Double Bar, Single Bar, No Bars, Off (default: Double Bar)

Changelog 1.1.1
---------------
- Added back rated R image that was accidentally removed from resources (oops)

Changelog 1.1 (Requires MB 2.2.9)
---------------------------------
+ Added support for MediaInfo IBN; removed theme option to switch between color/mono icons (default: color)
- Fixed problem with EHS RAL thumbnails changing to display parent folder thumbnails
- Tied visiblity of default theme background to MB general option 'Show Theme Background'
- Fixed problem with item details view not obeying overscan settings

Changelog 1.0
-------------
+ Added folder view setting to turn off backdrop if application setting has it on (default: on)
+ Added folder view setting to turn off Now Playing View if application setting has it on (default: on)
+ Added default theme background to prevent the Media Center blue from showing through
+ Added option to set position of left side of EHS recent list
- Changed EHS recent view of TV episodes and music songs to use the poster from the season/album
- Fixed problems with crashes on Vista

Changelog 0.9.5
---------------
- Fixed problem with crashes on empty folders
- Fixed problem with items playing multiple when using external players

Changelog 0.9.4
---------------
+ Added option to turn off EHS recent item info panel (default: on)
+ Added mini details to EHS recent item info panel
- Fixed long description auto-scrolling on movie details view

Changelog 0.9.3.1
-----------------
- Actually fixed mouse interactivity on details view list this time

Changelog 0.9.3
---------------
+ Added MediaInfo icons from chipmonkery and option to display in color (default: off)
- Changed clock display to use locale-based short time string
- Fixed mouse interactivity on details view list; introduced with auto-scrolling of long
  item titles in 0.9.2

Changelog 0.9.2
---------------
+ Folder Info Display (default: overview) and Wrap Long Item List (default: on) options moved
  to folder view settings
+ Added auto-scrolling for long item titles
+ Added option to turn off banner display in info panel of thumb view (default: on)
- Fixed problem with display of EHS Recent item's backdrop if backdrop came from parent folder
- Fixed aspect ratio of poster/thumb display in details view
- Fixed 'Use Banners' on details view to use MB preferred image functionality

Changelog 0.9.1
---------------
+ Added option to choose Now Playing style: Off, Small, Backdrop (default: Backdrop)
+ Added display of selected Recent item's backdrop, if available, on EHS
+ Added option to display info for folders (box sets, series, season, etc.) as overview, child items text
  list, or child items thumb list (default: overview)
+ Added list scroll speed options: fast, medium, slow (default: fast)
- Fixed problem with extra panel showing up on EHS view
- Fixed long description auto-scrolling on details view
- Fixed problem with thumbnails on EHS Recent list not caching

Changelog 0.9.0
---------------
+ Added custom view for music plugin
- Changed watched/unwatched icons on details view
- Fixed bug on config page where Child Coverflow Position would "steal" initial focus
+ Added banner display to info panel of thumb view
- Fixed banner display layout on info panel of details view
- Fixed production year/first aired being cleared upon item refresh (record, CTRL+R)
+ Added option to rotate backdrops on all view levels vs. only movie/episode details (default: on)
+ Replaced text-only Now Playing with standard Now Playing View
- Fixed list display bug when scroll wrapping was turned off
- Fixed cast list display bug on movie details page

Changelog 0.8.7
---------------
+ Added option to turn off scroll wrapping of large lists (default: on)
- Changed details list scrolling to track "normally" instead of keeping selection at top
- Fixed problems with Play and Context menus
+ Skip back ('|<') and skip foward ('>|') remote buttons act as page up/down on all views

Changelog 0.8.6
---------------
- Fixed layout problems with list in details view

Changelog 0.8.5
---------------
- Changed busy indicator to use spinning arrows from Diamond
+ Added context menu ('*') and play menu support
+ Added optional mini-MediaInfo strip next to item date | running time | MPAA (default: off)
+ Added optional (un)watched indicators to movie (non-TV series, season, episode) items (default: off)
+ Added auto-scrolling for long item descriptions
+ Added optional item 'n of m' display next to breadcrumb (default: off)
- Moved clock and busy indicator to left side of sub-status bar
+ Added options to set root and child coverflow positions: middle, bottom (default: bottom)
- Changed focus of initial item to use timer
+ Added option to turn off display of small backdrop image in thumbstrip view (default: on)
+ Added option to set position of recent items on EHS: top, middle, bottom (default: middle)
+ Added first ('|<') and last ('>|') item buttons to item details view
