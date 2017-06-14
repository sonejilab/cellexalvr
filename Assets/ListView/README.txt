# List View Framework #

The List View Framework is a set of core classes which  can be used as-is or extended to create dynamic, scrollable lists in Unity applications.  This repository tracks the development of the package, which is also available on the Unity Asset Store. If you do not wish to contribute to the project, we recommend that you download and import the latest version on the Asset Store for your convenience.

This package is intended for experienced developers who want a starting-point for customization. There are a number of working examples which may provide drop-in functionality, but they were developed as prototypes, and should be treated that way. The wiki contains in-depth explanations of the [Core Classes](https://bitbucket.org/Unity-Technologies/list-view-framework/wiki/Core%20Classes) and [Examples](https://bitbucket.org/Unity-Technologies/list-view-framework/wiki/Examples), but the easiest way to get started is to pick whichever example is closest to your intended use-case and work from there.

The code is set up to be customizable, but not "intelligent" or comprehensive. In other words, this is not a one-size-fits-all solution with lots of options and internal logic to adapt to the type of data set.  Instead, the idea is to extend the core classes into a number of list types which handle different requirements throughout your project.  In this way, we avoid a single, monolithic and complex script file which is hard to read, and at the same time, we can ensure that our lists aren't wasting CPU cycles on unnecessary branching, i.e. if(horizontal) or if(smoothScrolling).  At the same time, developers are free to create their own one-size-fits-all implementation if they have the need to switch options on-the-fly, or if for some other reason their use case demands it.

Examples are best viewed with a 16:9 viewport.

#Usage#
List view implementations will require at a minimum:

1. A GameObject with a ListViewController (or extension) component
2. At least one template prefab with a ListViewItem (or extension) component
3. A data source composed of ListViewData objects

The ListViewController by default contains a simple array of ListViewData objects.  Refer to examples 4 and above for how to utilize different types of data sources. The simplest examples use the Inspector to fill out a list of data, which is nice for example purposes but quickly becomes tedious in production.  One major advantage of the approach used by this project is that not all data must be available at once. See the Dictionary and Web Data examples for how that is done. Certain features require the ability to know the total row count, or other properties of the data-set, but the system is flexible enough to just cut those features if that information does not exist.

#Requirements#
This project was developed on Unity 5.3, but could reasonably work on any version of Unity, since it doesn't rely on particularly new APIs.  The examples make use of the Standard shader, so will require Unity 5+, but you have full control over the template object, so that isn't really a requirement.

The SQLite example has been tested on Windows and OS X.  It can probably work on smartphones as well, but the plugin-fu gets a little messy, so it is left as an exercise for the reader.

#Forks#
Anyone in the community is welcome to create their own forks. Drop us a note to labs@unity3d.com if you find this package useful, we'd love to hear from you!