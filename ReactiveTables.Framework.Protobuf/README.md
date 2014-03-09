ReactiveTables Protobuf
================================================

This module provides class for encoding and decoding ReactiveTables using the protobuf-net ProtocolBuffers implementation.  

Thet two main classes in this module are ProtobufTableEncoder and ProtobufTableDecoder.  The encoder observes a ReactiveTable and then serialises all the changes to a Stream.  The decoder reads from a Stream and will then write to a ReactiveTable as changes are read.

Note that there are still a couple of pending performance issues with this project.  Namely that on each message we need to create instances of the ProtoWriter and ProtoReader classes.  Hopefully Marc will sort these out sometime soon.

# Credits

Obviously both Marc Gravell's implementation [protobuf-net](https://code.google.com/p/protobuf-net/) and Google's [ProtocolBuffers](https://code.google.com/p/protobuf/) deserve all the credit.

# FAQ

If you have any questions or suggestions please contact me at <mailto:reactivetables@gmail.com>

# License

ReactiveTables is available under the [Gnu General Public Licence 3.0](http://www.gnu.org/licenses/).

# Release History / Changelog

See the [Releases page](https://bitbucket.org/mendelmonteiro/reactivetables/downloads).
