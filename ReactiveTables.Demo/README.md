ReactiveTables Demo
================================================

The demo project illustrates all the current functionality included in the ReactiveTables framework and the ReactiveTables.Protobuf add-on.

The sub-demo projects are set up as follows:

1. The real time demo
    - This demo is the simplest demo, it demonstrates how to bind to a normal WPF DataGrid using the INotifyProperty changed interface.  It also shows how to consume data from other threads using the ReactiveBatchedPassThroughTable.  Note that the performance of this demo is constrained by the performance of the DataGrid.
2. The client/server demo
    - To run this demo you also need to launch the server project at the same time.  This demo shows how to stream data from a server to the GUI whilst encoding with Protobuf.  Note that it uses the WPF DataGrid so the same performance caveat applies.
3. The Syncfusion client/server demo
    - This is a syncfusion grid based implementation of the previous demo.  Note that by using the syncfusion grid in full virtual mode (i.e. only the visible cells are consume change notifications from the ReactiveTable) we improve the performance and scalability of the grid.  Allocations throughout the chain are minimised, from encoding, storage, event propagation and rendering.
4. The Syncfusion demo
    - This is a syncfusion grid based implementation fo the real time demo.
5. The Xceed demo
    - This is an Xceed grid based  implementation fo the real time demo. Note that this requires you to have a demo Xceed license and to specify it in the App class.
6. The broker feed demo
    - This is a client/server demo which also incorporates a subscription functionality.  The client can chose which currencies to subscribe/unsubscribe to on the fly.

# FAQ

If you have any questions or suggestions please contact me at <mailto:reactivetables@gmail.com>

# License

ReactiveTables is available under the [Gnu General Public Licence 3.0](http://www.gnu.org/licenses/).

# Release History / Changelog

See the [Releases page](https://bitbucket.org/mendelmonteiro/reactivetables/downloads).
